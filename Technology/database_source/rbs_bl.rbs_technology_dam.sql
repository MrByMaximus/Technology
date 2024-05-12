create or replace package rbs_bl.rbs_technology_dam as
    /**
    Obcject:    get_technology_tree
    Type:       procedure
    Purpose:    Получение данных для дерева обурудования/параметров
    
    PARAMETERS:
        p_type:         тип данных (0 - оборудование, 1 - параметры технологии)
        p_module_id:    id модуля (для фильтра дерева по подразделения)
        p_plant_id:     id завода RBS
        p_smelter_id:   id завода ЦА
        
    RETURN sys_refcursor:
        если p_type = 0, то
            equipment_id        ид оборудования
            equipment_name      имя оборудования
            level               уровень оборудования в дереве
            num_order           сортировка
            has_tech_param      1 - есть параметры у оборудования, 0 - нет
        если p_type = 1, то
            parameter_id        ид параметра
            equipment_id        ид оборудования параметра
            param_short_name    короткое наименование параметра
            equipment_path      путь к параметру, для заголовка таблицы
            unit                еденица измерения параметра
            tag_id              ид тега параметра (для получения данных технологии)
            num_order           сортировка
    **/
    procedure get_technology_tree (p_type number, p_module_id number, p_plant_id number default null, p_smelter_id number default null, p_cur out sys_refcursor);
    
    
    /**
    Obcject:    get_technology_data
    Type:       procedure
    Purpose:    Получение данных технологии
    
    PARAMETERS:
        p_type:         тип данных (0 - минутные, 1 - часовые)
        p_module_id:    id модуля (для фильтра дерева по подразделения)
        p_plant_id:     id завода RBS
        p_smelter_id:   id завода ЦА
        p_date_begin:   дата начала периода
        p_date_end:     дата окончания периода
        p_params:       список ид параметров для данных (через запятую)
        
    RETURN sys_refcursor:
            date_stamp  дата параметра
            date_time   дата параметра (char dd.mm.yy hh24:mi)
            [p_params]  колонки, равные входящим ид параметрам, т.е если входящий p_params 448,449 то будет 2 колонки с именами 448 и 449
                        необходим порядок, такой же какой в p_params
    **/
    procedure get_technology_data (p_type number, p_module_id number, p_plant_id number default null, p_smelter_id number default null, p_date_begin date, p_date_end date, p_params clob, p_cur out sys_refcursor);
    
    /**
    Obcject:    get_technology_limits
    Type:       procedure
    Purpose:    Получение данных по границам технологических параметров
    
    PARAMETERS:
        p_params:       список ид параметров для данных (через запятую)
        
    RETURN sys_refcursor:
            id_param    дата параметра
            min_err     минимальная граница ошибки     
            max_err     максимальная граница ошибки
            min_war     минимальная граница предупреждения
            max_war     максимальная граница предупреждения
    **/
    procedure get_technology_limits (p_params varchar2, p_cur out sys_refcursor);
    procedure get_technology_limits_clob (p_params clob, p_cur out sys_refcursor);
    
                            
end rbs_technology_dam;
/

create or replace package body rbs_bl.rbs_technology_dam
as
   ----------------------------------------------------------------------------
   --  Private variables
   ----------------------------------------------------------------------------
   
   

   ----------------------------------------------------------------------------
   --  Private procedures/functions
   ----------------------------------------------------------------------------



   ----------------------------------------------------------------------------
   --  Public procedures/functions
   ----------------------------------------------------------------------------
   
   procedure get_technology_tree (p_type         number,
                                 p_module_id    number,
                                 p_plant_id     number default null,
                                 p_smelter_id   number default null,
                                 p_cur out sys_refcursor)
   is
   begin
   
      if p_type = 0
      then
         open p_cur for
                select eq.equipment_id,
                       eq.parent_id,
                       eq.equipment_name,
                       level,
                       eq.num_order,
                       eq.has_tech_param
                  from rbs_technology.ref_equipment eq
                 where     1 = 1
                       and sysdate between eq.date_begin
                                       and nvl (eq.date_end, sysdate)
            connect by prior eq.equipment_id = eq.parent_id
            start with eq.equipment_id =
                          (select equipment_id
                             from rbs_technology.module_equipment
                            where     1 = 1
                                  and module_id = p_module_id
                                  --AND plant_id = p_plant_id
                                  and sysdate between date_begin
                                                  and nvl (date_end,
                                                           sysdate));
      else
         open p_cur for
            select p.parameter_id,
                   eq.equipment_id equipment_id,
                   p.param_short_name,
                      (select equ.equipment_short_name
                         from rbs_technology.ref_equipment equ
                        where equ.equipment_id = eq.parent_id)
                   || '-'
                   || (select equ.equipment_short_name
                         from rbs_technology.ref_equipment equ
                        where equ.equipment_id = eq.equipment_id)
                      equipment_path,
                   (select uu.unit_sumbol
                      from rbs_technology.ref_units uu
                     where uu.unit_id = p.unit_id and uu.unit_id != 0)
                      unit,
                   ept.tag_id,
                   p.num_order
              from (    select eq.*
                          from rbs_technology.ref_equipment eq
                         where     1 = 1
                               and sysdate between eq.date_begin
                                               and nvl (eq.date_end, sysdate)
                    connect by prior eq.equipment_id = eq.parent_id
                    start with eq.equipment_id =
                                  (select equipment_id
                                     from rbs_technology.module_equipment
                                    where     1 = 1
                                          and module_id = p_module_id
                                          --AND plant_id = p_plant_id
                                          and sysdate between date_begin
                                                          and nvl (
                                                                 date_end,
                                                                 sysdate))) eq,
                   rbs_technology.ref_parameters p,
                   rbs_technology.equipment_groups_parameter egp,
                   rbs_technology.equipment_parametr_tag ept
             where     1 = 1
                   and sysdate between ept.date_begin
                                   and nvl (ept.date_end, sysdate)
                   and eq.equipment_group_id = egp.equipment_group_id
                   and egp.parameter_id = p.parameter_id
                   and p.parameter_id = ept.parametr_id
                   and eq.equipment_id = ept.equipment_id;
      end if;
      
   end get_technology_tree;


   procedure get_technology_data (p_type         number,
                                 p_module_id    number,
                                 p_plant_id     number default null,
                                 p_smelter_id   number default null,
                                 p_date_begin   date,
                                 p_date_end     date,
                                 p_params       clob,
                                 p_cur out sys_refcursor)
   is
      v_step         number;
      v_level_factor number;
      v_clob         clob;
   begin
      if p_type = 0
      then
         v_step := trunc ((p_date_end - p_date_begin) * 24 * 60 * 60 / 120);
         v_level_factor := 120;
      else
         v_step := 3600;
         v_level_factor := trunc((p_date_end-p_date_begin) * 24);
      end if;

      if p_type = 0
      then
         v_clob := '
            select *
              from (with t1 
                         as (    select :p_date_begin + (level - 1) * '|| v_step||' / 24 / 60 / 60 as date_beg,
                                        :p_date_begin + (level) * '|| v_step ||' / 24 / 60 / 60 as date_end
                                   from dual
                             --connect by Level <= 120
                             connect by level <= '|| v_level_factor ||'),
                         t2
                         as (  select t1.date_end date_stamp,
                                      to_char (t1.date_end, '||chr(39)||'dd.mm.yy hh24:mi'||chr (39)||') date_time,
                                      r.id_param,
                                      ROUND (avg(r.val), 2) val_avg
                                 from t1, 
                                      ds_bratsk_techn.values_tbl r
                                where     r.id_param in ('|| p_params ||')
                                      and r.d_start >= t1.date_beg and r.d_finis <=  t1.date_end
                                      and r.d_start <  t1.date_end and r.d_finis > t1.date_beg
                             group by t1.date_end, 
                                      to_char (t1.date_end, '||chr(39)||'dd.mm.yy hh24:mi'||chr(39)||'),
                                      r.id_param )
                    select * from t2) pivot (sum (val_avg) for id_param in ('|| p_params ||'))';
                    
         open p_cur for v_clob using p_date_begin, p_date_begin;
      else
         v_clob := '
                select *
                  from (  select r.dtime date_stamp,
                                 TO_CHAR (r.dtime, '||chr(39)||'dd.mm.yy hh24:mi'||chr (39)||') date_time,
                                 r.id_param,
                                 ROUND (r.val_avg, 2) val_avg
                            from ds_bratsk_techn.values_tbl_agr r
                           where     r.id_param in ('|| p_params ||')
                                 and r.dtime between :p_date_begin and :p_date_end
                        ) pivot (SUM (val_avg) for id_param in ('|| p_params ||')) order by date_stamp';
         
         open p_cur for v_clob using p_date_begin, p_date_end;
                                 
      end if;
      
      
   end get_technology_data;
   

   procedure get_technology_limits (p_params varchar2, p_cur out sys_refcursor)
   is
   begin
      open p_cur for
         select x.id_param,
                pg.min_err,
                pg.min_war,
                pg.max_war,
                pg.max_err
           from (select * from ds_bratsk_techn.params_gran where sysdate between d_from and d_to) pg
                  right join
                     (   select to_number (trim (regexp_substr (p_params, '[^,]+', 1, level))) id_param
                           from dual
                        connect by regexp_substr (p_params, '[^,]+', 1, level) is not null) x
                     on pg.id_param = x.id_param;

   end get_technology_limits;
   
   procedure get_technology_limits_clob (p_params clob, p_cur out sys_refcursor)
   is
   begin    
      open p_cur for '
         select pg.id_param,
                pg.min_err,
                pg.min_war,
                pg.max_war,
                pg.max_err
           from ds_bratsk_techn.params_gran pg
          where     sysdate between pg.d_from and pg.d_to
                and pg.id_param in ( '||p_params||' )';

   end get_technology_limits_clob;
   
end rbs_technology_dam;
/

grant EXECUTE on rbs_bl.rbs_technology_dam to rbs_user;
