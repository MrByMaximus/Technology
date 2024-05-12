create or replace package rbs_bl.rbs_technology as
    /**
    Obcject:    get_technology_tree
    Type:       function
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
    function get_technology_tree (p_type number, p_module_id number, p_plant_id number default null, p_smelter_id number default null) return sys_refcursor;
    
    
    /**
    Obcject:    get_technology_data
    Type:       function
    Purpose:    Получение данных технологии
    
    PARAMETERS:
        p_type:         тип данных (0 - минутные, 1 - часовые)
        p_module_id:    id модуля (для фильтра дерева по подразделения)
        p_plant_id:     id завода RBS
        p_smelter_id:   id завода ЦА
        p_date_begin:   дата начала периода
        p_date_end:     дата окончания периода
        p_params:       список ид параметров для данных (через запятую)
        p_values_mode:  режим получения данных (0 - усредненный режим, 1 - последнее значение)
        
    RETURN sys_refcursor:
            date_stamp  дата параметра
            date_time   дата параметра (char dd.mm.yy hh24:mi)
            [p_params]  колонки, равные входящим ид параметрам, т.е если входящий p_params 448,449 то будет 2 колонки с именами 448 и 449
                        необходим порядок, такой же какой в p_params
    **/
    function get_technology_data (p_type number, p_module_id number, p_plant_id number default null, p_smelter_id number default null, p_date_begin date, p_date_end date, p_params clob, p_values_mode  number default 0) return sys_refcursor;
    
    function get_technology_data_online (p_params   clob) return sys_refcursor;
    
    /**
    Obcject:    get_technology_limits
    Type:       function
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
    function get_technology_limits (p_module_id number default null, p_params varchar2) return sys_refcursor;
    function get_technology_limits_clob (p_module_id number default null, p_params clob) return sys_refcursor;

    
    /**
    Obcject:    get_plc_mnemo
    Type:       function
    Purpose:    Получение данных по мнемосхемам
    
    PARAMETERS:
        p_module_id:    id модуля
        --
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo(p_module_id number default null) return sys_refcursor;      
                       
    /**
    Obcject:    get_plc_mnemo_detail
    Type:       function
    Purpose:    Получение подробных данных по мнемосхемам (расположение)
    
    PARAMETERS:
        --
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_detail(p_id_mnemo number default null) return sys_refcursor; 
                
    /**
    Obcject:    get_plc_mnemo_blob
    Type:       function
    Purpose:    Получение файла-схемы
    
    PARAMETERS:
        p_blob_id:      ид файлы
        
    RETURN blob:
            --
    **/
    function get_plc_mnemo_blob(p_blob_id number) return sys_refcursor;
                       
    /**
    Obcject:    get_plc_mnemo_data
    Type:       function
    Purpose:    Получение данных по выбраным параметрам
    
    PARAMETERS:
        p_params:       список ид параметров для данных (через запятую)
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_data (p_params varchar2) return sys_refcursor;
                 
    /**
    Obcject:    upload_plc_mnemo_blob
    Type:       procedure
    Purpose:    Загрузка файла схемы миксера
    
    PARAMETERS:
        p_mixer_id:     id миксера
        p_file:         файл
        
    **/
    procedure upload_plc_mnemo_blob (p_mixer_id number, p_file blob, p_user_id number);
    
    
    type user_mail_record  is record(
        emailid     varchar2(1), 
        frommail    varchar2(100),
        tomail      varchar2(100),
        ccmail      varchar2(100),
        datecreate  date,
        datesend    date,
        important   number,
        status      number,
        subject     varchar2(200),
        body        clob,
        bccmail     varchar2(1),
        moduleid    number,
        attachments varchar2(1),
        userid      number,
        expanded    varchar2(10),
        enabled     varchar2(10)
    );        
    type user_mail_table   is table of user_mail_record;
    
    
    /**
    Obcject:    get_plc_mnemo_check_temp
    Type:       function
    Purpose:    Получение оповещений о превышении температуры
    
    PARAMETERS:
        p_temp_limit_min:  min для проверки температуры
        p_temp_limit_max:  max для проверки температуры
        p_temp_check_log:  0 - не проверять запись в логах по gage_ids, 1 - проверять. Проверка за последний час
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_check_temp(p_temp_limit_min number default null, p_temp_limit_max number default null, p_temp_check_log number default 0) return sys_refcursor;
    
    
    /**
    Obcject:    get_plc_mnemo_no_data
    Type:       function
    Purpose:    Получение оповещений об отсутствии данных с точек
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_no_data return sys_refcursor;
    
    
    /**
    Obcject:    get_plc_mnemo_mailing_group
    Type:       function
    Purpose:    Получение списка групп рассылки
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_mailing_group return sys_refcursor;
    
    
    /**
    Obcject:    get_plc_mnemo_group_users
    Type:       function
    Purpose:    Получение списка людей рассылки
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_group_users return sys_refcursor;
    
    
    /**
    Obcject:    get_mixer_gages_state
    Type:       function
    Purpose:    Получение списка точек миксера
    
    PARAMETERS:
        p_mixer_id ид миксера
        
    RETURN sys_refcursor:
            --
    **/
    function get_mixer_gages_state(p_mixer_id number) return sys_refcursor;
    
    
    /**
    Obcject:    change_gages_state
    Type:       procedure
    Purpose:    Отключение/включение рассылки по точкам
    
    PARAMETERS:
        p_state:        0 - отключение, 1 - включение
        p_date_begin:   дата начала
        p_date_end:     дата окончания
        p_request:      номер заявки
        p_note:         примечание
        p_array:        массив gage_id
        p_user_id:      пользователь
        
    **/
    procedure change_gages_state (p_state number, p_date_begin date, p_date_end date, p_request varchar2, p_note varchar2, p_array varchar2, p_user_id number);
       
    
    /**
    Obcject:    get_disabled_gages
    Type:       function
    Purpose:    Получение списка отключенных для рассылки точек миксера
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_disabled_gages return sys_refcursor; 
    
    
    /**
    Obcject:    set_gage_enabled
    Type:       procedure
    Purpose:    Включение рассылки по точке
    
    PARAMETERS:
        plc_mail_stop_id    ид        
    **/
    procedure set_gage_enabled (plc_mail_stop_id number);
    
    
     /**
    Obcject:    set_gage_disabled
    Type:       procedure
    Purpose:    Включение рассылки по точке
    
    PARAMETERS:
        plc_mail_stop_id    ид        
    **/
    procedure set_gage_disabled (plc_mail_stop_id number);

    
    type plc_check_record  is record(
        group_id number,
        pict_blob blob,
        id_plc_mnemo number,
        parent number,
        mixer_name varchar2(400),
        gage_ids varchar2(4000),
        gage_ids_hit varchar2(4000),
        gage_ids_name varchar2(4000),
        mixer_tag_name nvarchar2(4000),
        gage_ids_json nvarchar2(4000)
    );        
    type plc_check_table is table of plc_check_record;
    
    /**
    Obcject:    get_plc_mnemo_check_growth
    Type:       function
    Purpose:    Проверка роста температуры в точках миксера в течении n дней
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_check_growth return sys_refcursor;
    
    /**
    Obcject:    get_plc_mnemo_check_stddev
    Type:       function
    Purpose:    Проверка по стандартному отклонению температуры в точках миксера 
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_check_stddev return sys_refcursor;
    
    /**
    Obcject:    get_plc_mnemo_check_sharp_jump
    Type:       function
    Purpose:    Проверка по обнаружению резких скачков температуры
    
    PARAMETERS:
        
    RETURN sys_refcursor:
            --
    **/
    function get_plc_mnemo_check_sharp_jump return sys_refcursor;


   procedure mnemo_check_log_save(p_id_event number, p_tag_ids clob);
    
    /**
    Obcject:    mnemo_check_mixer_temp
    Type:       function
    Purpose:    Получение оповещений о превышении температуры
    
    PARAMETERS:
        p_id_event:        id события
        p_check_minutes:   за какой период проверять данные (в минутах)
        p_check_log:       0 - не проверять запись в логах по gage_ids, 1 - проверять. Проверка за последний час
        
    RETURN sys_refcursor:
            --
    **/
    function mnemo_check_mixer_temp(p_id_event number, p_check_minutes number, p_check_log number) return sys_refcursor;


end rbs_technology;
/

create or replace package body rbs_bl.rbs_technology
as
   ----------------------------------------------------------------------------
   --  Private variables
   ----------------------------------------------------------------------------
   g_no_data_minute  number := 30;  --за сколько минут считается что данные не поступают с оборудования
   g_temp_limit      number := 250; --нижняя граница предельной температуры в допуске
   g_temp_limit_max  number := 1200;--верхняя граница предельной температуры в допуске (все что больше считается ошибочным)

   ----------------------------------------------------------------------------
   --  Private procedures/functions
   ----------------------------------------------------------------------------



   ----------------------------------------------------------------------------
   --  Public procedures/functions
   ----------------------------------------------------------------------------
   
   function get_technology_tree (p_type         number,
                                 p_module_id    number,
                                 p_plant_id     number default null,
                                 p_smelter_id   number default null)
      return sys_refcursor
   is
      v_smelter_id   number;
      v_id_struct    number;
      v_child_cnt    number;--есть ли дети в xcommon.struct, для получение дерева без верхнего уровня
      plsql_block    varchar2(500);
      
      v_cur sys_refcursor;
   begin
      
      select ss.id_smelterdefault
        into v_smelter_id
        from xcommon.setupsystem ss
       where ss.id_setupsystem=1;
       
      --для БрАЗ, если модуль технологии ДАМ (33) то данные берем из не целевых таблиц (X**) 
      if v_smelter_id = 1 and p_module_id = 33
      then
         --пришлось сделать через execute immediate, чтобы пакет был единым на всех заводах
         plsql_block := 'begin rbs_bl.rbs_technology_dam.get_technology_tree(p_type => :p_type, p_module_id => :p_module_id, p_plant_id => :p_plant_id, p_smelter_id => :p_smelter_id, p_cur => :p_cur); end;';
         execute immediate plsql_block using p_type, p_module_id, p_plant_id, p_smelter_id, v_cur;
         
      else
   
         if p_type = 0 then
            
            select sm.id_struct
              into v_id_struct 
              from xtech.structmodule sm 
             where sm.module_id = p_module_id;
         
            select count(*)
              into v_child_cnt 
              from xcommon.struct s 
             where s.parent = v_id_struct;
          
            open v_cur for
            
               with lo as (
                   select s.id_struct,
                          --(case when :p_culture = 'ru' then s.nameshort else s.nameshortengl end) name,
                          nvl(s.nameshort, s.name) name,
                          s.parent,
                          s.id_structtype,
                          s.seq,
                          level lvl
                     from xcommon.struct s 
                    where 1=1
                          and sysdate between nvl(s.datebeg, sysdate-1) and nvl(s.dateend, sysdate+1)
                          --and s.id_structtype in (4) 
                    --start with s.parent in (select ss.id_smelterdefault from xcommon.setupsystem ss where ss.id_setupsystem = 1)
                    --start with s.id_struct = v_id_struct
                    start with decode(v_child_cnt, 0, s.id_struct, s.parent) = v_id_struct
                  connect by prior s.id_struct = s.parent 
               )
               select l.id_struct as equipment_id,
                      l.parent parent_id,
                      --0 as "level",
                      (case when l.lvl = 1 then -1 else 0 end) as "level",
                      to_char(l.name) as equipment_name,
                      l.id_struct as num_order,
                      nvl(l.seq,-1) seq,
                      0 as has_tech_param
                 from lo l
                             
                      union all
                           
               select e.id_equip equipment_id,
                      nvl (e.parent, e.id_struct) parent_id,
                      level as "level",
                      to_char(e.description) equipment_name,
                      e.id_equip num_order,
                      nvl(e.seq,-1) seq,
                      1 has_tech_param
                 from xcequip.equip e
                where e.id_struct in (select l.id_struct from lo l)
                      and e.id_equip <> e.id_struct -- исключаем возможность дублирования id для дерева
                      and e.id_equip not in (select l.id_struct from lo l) -- исключаем возможность дублирования id для дерева
                      and e.id_equiptypeclass <> 52 -- исключаем какие-то весы
               start with e.parent is null
               connect by prior e.id_equip = e.parent;

         else
         
            open v_cur for
            
               select z1.id_el || '' || rownum parameter_id,
                      z1.id_equip equipment_id,
                      to_char (z2.name) param_short_name,
                      (select to_char (z0.name) from xcequip.equip z0 where z1.id_equip = z0.id_equip) equipment_path,
                      to_char (null) unit,
                      z2.id_typevalue as value_type,
                      to_char (z1.id_tag) tag_id,
                      to_char (z1.id_tag) num_order,
                      --to_number (regexp_replace(z1.id_el, 1,'',1,1)) as seq,
                      z1.seq
                 from xtech.tag z1, xcommon.el z2
                where z1.id_el = z2.id_el;
                      -- and z2.proc_measure_class_id = 1;
                      
         end if;
         
      end if;

      return v_cur;
   end get_technology_tree;


   function get_technology_data (p_type         number,
                                 p_module_id    number,
                                 p_plant_id     number default null,
                                 p_smelter_id   number default null,
                                 p_date_begin   date,
                                 p_date_end     date,
                                 p_params       clob,
                                 p_values_mode  number default 0)
      return sys_refcursor
   is
      v_smelter_id   number;
      v_minutes      number;
      v_step         number;
      v_level_factor number;
      v_clob         clob;
      plsql_block    varchar2(500);
      
      get_value_text varchar2(500);
      
      v_cur sys_refcursor;
   begin
   
      select ss.id_smelterdefault
        into v_smelter_id
        from xcommon.setupsystem ss
       where ss.id_setupsystem=1;
       
      --для БрАЗ, если модуль технологии ДАМ (33) то данные берем из не целевых таблиц (X**)
      if v_smelter_id = 1 and p_module_id = 33
      then
         --пришлось сделать через execute immediate, чтобы пакет был единым на всех заводах
         plsql_block := 'begin rbs_bl.rbs_technology_dam.get_technology_data(p_type => :p_type, p_module_id => :p_module_id, p_plant_id => :p_plant_id, p_date_begin => :p_date_begin, p_date_end => :p_date_end, p_params => :p_params, p_cur => :p_cur); end;';
         execute immediate plsql_block using p_type, p_module_id, p_plant_id, p_date_begin, p_date_end, p_params, v_cur;
         
      else
         
         if p_type = 0
         then
            if p_values_mode = 0
            then
               v_minutes := 120;
            else
               select (p_date_end - p_date_begin) * 1440
                 into v_minutes
                 from dual;
            end if;
            
            v_step := trunc ((p_date_end - p_date_begin) * 24 * 60 * 60 / v_minutes);
            v_level_factor := v_minutes;
         else
            v_step := 3600;
            v_level_factor := trunc((p_date_end-p_date_begin) * 24);
         end if;
         
         if p_values_mode = 0
         then
            get_value_text := 'round (avg (a.value), nvl (t.rnd, 1))';
         else
            get_value_text := 'max(nvl(a.valuestr, round(a.value, nvl (t.rnd, 1)))) keep (dense_rank last order by a.datestamp)';
         end if;
         
         v_clob:= '
            select *
              from (with t1
                         as (    select :p_date_begin + (level - 1) * '|| v_step||' / 24 / 60 / 60 as date_beg,
                                        :p_date_begin + (level) * '|| v_step ||' / 24 / 60 / 60 as date_end
                                   from dual
                             --connect by Level <= 120
                             connect by level <= '|| v_level_factor ||'),
                         t2
                         as (  select d.date_beg,
                                      a.id_tag gage_id,
                                      --nvl(max(a.valuestr), to_char(round(avg(a.Value), nvl(t.rnd, 1)))) val
                                      --round (avg (a.value), nvl (t.rnd, 1)) val
                                      '||get_value_text||' val
                                 from t1 d, xtech.tagvalue_ar a, xtech.tag t
                                where     a.id_tag in ('|| p_params ||') --aGage_ID
                                      and a.datestamp between d.date_beg and d.date_end
                                      and a.id_tag = t.id_tag
                             group by d.date_beg, a.id_tag, t.rnd)
                    select date_beg date_stamp,
                           to_char (date_beg,'|| chr (39) ||'dd.mm.yy hh24:mi'|| chr (39) ||') date_time,
                           to_char (gage_id) id_param,
                           val
                      from t2) pivot (max (val) for id_param in ('|| p_params ||'))
         ';      
         open v_cur for v_clob using p_date_begin, p_date_begin;
         
      end if;
      
      return v_cur;
   end get_technology_data;
   
   
   function get_technology_data_online (p_params clob)
      return sys_refcursor
   is
      v_step         number;
      v_date_begin   date;
      v_date_end     date;
      v_clob         clob;
      
      v_cur sys_refcursor;
   begin
      v_step := 60;
      v_date_begin := TRUNC (SYSDATE, 'mi') - 30 / 1440;
      v_date_end := TRUNC (SYSDATE, 'mi');
      
      --dbms_output.put_line('v_date_begin=' || to_char(v_date_begin, 'dd.mm.yyyy hh24:mi:ss'));
      --dbms_output.put_line('v_date_begin=' || to_char(v_date_begin, 'dd.mm.yyyy hh24:mi:ss'));

      v_clob:= '
           select *
             from (with t1
                        as (    select :v_date_begin + (level - 1) * '|| v_step ||' / 24 / 60 / 60 as date_beg,
                                       :v_date_begin + (level) * '|| v_step ||' / 24 / 60 / 60 as date_end
                                  from dual
                            connect by level <= 30),
                        t2
                        as (  select d.date_beg,
                                     a.id_tag gage_id,
                                     nvl (round (avg (a.value), nvl (t.rnd, 1)), max (a.valuestr)) val
                                from t1 d, xtech.tagvalue_ar a, xtech.tag t
                               where     a.id_tag in ('|| p_params ||')  --aGage_ID
                                     and a.datestamp between d.date_beg and d.date_end
                                     and a.id_tag = t.id_tag
                            group by d.date_beg, a.id_tag, t.rnd)
                   select date_beg date_stamp,
                          to_char(date_beg, '|| chr (39) ||'hh24:mi'|| chr (39) ||') date_time,
                          to_char(gage_id) id_param,
                          val
                     from t2) pivot (max (val) for id_param in ('|| p_params ||'))
         order by 1
      ';
     
      open v_cur for v_clob using v_date_begin, v_date_begin;

      return v_cur;
   end get_technology_data_online;
   

   function get_technology_limits (p_module_id number default null, p_params varchar2)
      return sys_refcursor
   is
      v_smelter_id   number;
      v_mark_id      number;
      v_mark_par_id  number;
      v_ccount       number;
      plsql_block    varchar2(500);
      
      v_cur sys_refcursor;
   begin
   
      select ss.id_smelterdefault
        into v_smelter_id
        from xcommon.setupsystem ss
       where ss.id_setupsystem=1;
       
      --для БрАЗ, если модуль технологии ДАМ (33) то данные берем из не целевых таблиц (X**)
      if v_smelter_id = 1 and p_module_id = 33
      then
         --пришлось сделать через execute immediate, чтобы пакет был единым на всех заводах
         plsql_block := 'begin rbs_bl.rbs_technology_dam.get_technology_limits(p_params => :p_params, p_cur => :p_cur); end;';
         execute immediate plsql_block using p_params, v_cur;
      else
         v_ccount := 0;

         open v_cur for
            select to_char (id_tag) id_param,
                   to_char (null) min_err,
                   to_char (null) min_war,
                   to_char (null) max_err,
                   to_char (null) max_war
              from xtech.tag
             where id_tag in (    select to_number (trim (regexp_substr (p_params, '[^,]+', 1, level))) id_param
                                    from dual
                              connect by regexp_substr (p_params, '[^,]+', 1, level) is not null);
      end if;
      
      return v_cur;
   end get_technology_limits;
   
   function get_technology_limits_clob (p_module_id number default null, p_params clob) return sys_refcursor
   is
      plsql_block varchar2(500);
      
      v_smelter_id   number;
      
      v_cur sys_refcursor;
   begin
   
      select ss.id_smelterdefault
        into v_smelter_id
        from xcommon.setupsystem ss
       where ss.id_setupsystem=1;
       
      --для БрАЗ, если модуль технологии ДАМ (33) то данные берем из не целевых таблиц (X**)
      if v_smelter_id = 1 and p_module_id = 33
      then
         --пришлось сделать через execute immediate, чтобы пакет был единым на всех заводах
         plsql_block := 'begin rbs_bl.rbs_technology_dam.get_technology_limits_clob(p_params => :p_params, p_cur => :p_cur); end;';
         execute immediate plsql_block using p_params, v_cur;
      else
         open v_cur for '
            select to_char(id_tag) id_param,
                   to_char(null) min_err,
                   to_char(null) min_war,
                   to_char(null) max_err,
                   to_char(null) max_war
              from xtech.tag
             where id_tag in ( '||p_params||' )';
      end if;
      
      return v_cur;
   end get_technology_limits_clob;
   
   
   function get_plc_mnemo(p_module_id number default null) return sys_refcursor
   is
      v_id_app number;
      
      v_cur sys_refcursor;
   begin
      begin
         select a.id_app
           into v_id_app
           from xcommon.app a
          where a.module_id = p_module_id
          fetch first 1 rows only;
          
         exception when no_data_found then null;
      end;
      
   
      open v_cur for
         select to_char(a.id_mnemo) as id,
                to_char(a.parent) as id_parent,
                a.name,
                to_char(a.id_mnemobackground) id_blob,
                x.id_equip as equipment_id
           from xtech.mnemo a
                left join (select distinct d.id_mnemo, 
                                  min(t.id_equip) id_equip
                             from xtech.mnemodetail d, xtech.tag t
                            where     1 = 1 
                                  and d.id_tag = t.id_tag
                            group by d.id_mnemo
                          ) x
                   on a.id_mnemo = x.id_mnemo
                start with a.parent is null and a.id_app = v_id_app
              connect by prior a.id_mnemo = a.parent                   
          order by to_number(regexp_replace(a.name, '[^0-9]', ''));
      return v_cur;
   end get_plc_mnemo;
   
   function get_plc_mnemo_blob(p_blob_id number) return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for
         select a.picture pict
           from xtech.mnemobackground a
          where a.id_mnemobackground = p_blob_id;
      return v_cur;
   end get_plc_mnemo_blob;
    
   function get_plc_mnemo_detail(p_id_mnemo number default null) return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for
         select to_char(m.id_mnemodetail) as id,
                to_char(m.id_mnemo) as id_mnemo,
                to_char(m.id_tag) gage_id,
                nvl(pm.name, '---') name
           from xtech.mnemodetail m,
                xtech.tag gr,
                xcommon.el pm
          where     1 = 1
                and m.id_mnemo = p_id_mnemo
                and m.id_tag = gr.id_tag
                and gr.id_el = pm.id_el;
      return v_cur;
   end get_plc_mnemo_detail;
    
   function get_plc_mnemo_data (p_params varchar2) return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for
         select to_char(t.id_tag) gage_id, -- 'FM99999999999999990.9999'
                a.datestamp date_stamp,
                nvl(a.valuestr, trim(trailing '.' from to_char(round(a.value, nvl(t.rnd, 1)), 'FM99999999999999990.9999'))) value,
                --(case when a.value < 200 then '#66bb6a' else '#e57373' end) color
                /*(case when round((sysdate - CAST(a.datestamp as date)) * 24 * 60, 2) > g_no_data_minute then '#2196f3'--нет данных > n минут (синим)
                      when a.value < 200 then '#66bb6a'--в допуске (зеленый)
                      else '#e57373' --не в допуске (зеленый)
                end) color*/
                /*цвета по US 13102*/
                (
                 case 
                      --Отклонение по СГП план\факт за сутки
                      when a.id_tag = 1100000000000000000127 and a.value > (select valuemin from xtech.limit where id_tag = 1100000000000000000127 and id_event = 10 and valuemin is not null) then '#34c759'
                      when a.id_tag = 1100000000000000000127 and a.value < (select valuemin from xtech.limit where id_tag = 1100000000000000000127 and id_event = 10 and valuemin is not null) then '#ff6961'
                      --when a.id_tag = 1100000000000000000127 then 'transparent'
                      
                      --Миксер 1. Угол наклона
                      when a.id_tag = 1100000000000000000136 and (select value from xtech.tagvalue where id_tag = 1100000000000000000086) > (select valuemin from xtech.limit where id_tag = 1100000000000000000086 and id_event = 9 and valuemin is not null) then '#34c759'
                      --Миксер 2. Угол наклона
                      when a.id_tag = 1100000000000000000135 and (select value from xtech.tagvalue where id_tag = 1100000000000000000087) > (select valuemin from xtech.limit where id_tag = 1100000000000000000087 and id_event = 9 and valuemin is not null) then '#34c759'
                      
                      --Событие по превышении температуры в границах 205 - 250 в точках миксера
                      when l.id_event = 1 and round((sysdate - CAST(a.datestamp as date)) * 24 * 60, 2) > 30 then '#2196f3' --нет данных > n минут (синим)
                      
                      when l.valuemin is not null and a.value >= l.valuemin then '#ff6961'--красный
                      when l.valuemin is not null and a.value < l.valuemin then '#34c759'--зеленый '#30db5b', '#34c759'
                      
                      else 'transparent'
                end) color
           from xtech.tag t
                left join xtech.tagvalue a on t.id_tag = a.id_tag
                left join xtech.limit l on a.id_tag = l.id_tag and l.id_event = 1
          where t.id_tag in (select to_number(trim(regexp_substr (p_params,'[^,]+', 1, level))) id_param
                                 from dual
                              connect by regexp_substr (p_params, '[^,]+', 1, level) is not null);
      return v_cur;
   end get_plc_mnemo_data;
    
   procedure upload_plc_mnemo_blob (p_mixer_id number, p_file blob, p_user_id number)
   is
      v_id_plc_mnemoblob   number;
   begin
      select m.id_mnemobackground
        into v_id_plc_mnemoblob
        from xtech.mnemo m
       where m.id_mnemo = p_mixer_id;
           
      if v_id_plc_mnemoblob is null
      then
         insert into xtech.mnemobackground (picture)
              values (p_file)
           returning id_mnemobackground into v_id_plc_mnemoblob;
      else
         update xtech.mnemobackground m 
            set m.picture = p_file
          where m.id_mnemobackground = v_id_plc_mnemoblob;
      end if;
           
      update xtech.mnemo m
         set m.id_mnemobackground = v_id_plc_mnemoblob
       where m.id_mnemo = p_mixer_id;
           
   end upload_plc_mnemo_blob;
    
    --только ИРКАЗ (удалить после перехода на X)
   function get_plc_mnemo_check_temp(p_temp_limit_min number default null, p_temp_limit_max number default null, p_temp_check_log number default 0) return sys_refcursor
   is
      v_cur               sys_refcursor;
        
      v_period            date := sysdate - g_no_data_minute/(24*60);--n минут
      v_temp_limit        number := nvl(p_temp_limit_min, g_temp_limit);--оповещение, если температура равна или выше этого значения
      v_temp_limit_max    number := nvl(p_temp_limit_max, g_temp_limit_max);--ограничение по ошибочным значениям
   begin
      open v_cur for 
         with plc_mnemo_la as (
                select x.name,
                       listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
                  from (select distinct m.name,
                               c.description mixer_name,
                               mp.name parent_name,
                               m.parent, 
                               m.id_mnemo,
                               (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                    when 1 then 81
                                    when 2 then 82
                                    when 3 then 83
                               end) group_id,
                               m.id_mnemobackground blob_id
                          from xtech.mnemodetail d,
                               xtech.mnemo m,
                               xtech.mnemo mp,
                               xtech.tag r,
                               xcequip.equip c
                         where 1=1
                           and d.id_mnemo = m.id_mnemo
                           and d.id_tag = r.id_tag
                           and r.id_equip = c.id_equip
                           and mp.id_mnemo = m.parent
                           and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                       ) x 
              group by x.name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
              ),
              gage_log_check as (
                   select distinct TRIM (regexp_substr (xx.gage_ids, '[^,]+', 1, level)) gage_id
                     from (select listagg (x.gage_ids, ',') within group (order by 1) gage_ids
                             from (select distinct json_value (l.description, '$.gage_ids') as gage_ids
                                     from xaudit.LOG l
                                    where     1 = 1
                                          and l.id_app = 22
                                          and l.datetimestamp >= SYSDATE - 1 / 24
                                          and l.methodlocation = 'api/Lo/Technology/ProcessThermocoupleCheckTemperature'
                                          and json_value (l.parameter, '$.limitMin') = p_temp_limit_min
                                          and json_value (l.parameter, '$.limitMax') = p_temp_limit_max
                                          and p_temp_check_log = 1
                                  ) x
                            where x.gage_ids is not null) xx
               connect by regexp_substr (xx.gage_ids, '[^,]+', 1, level) is not null
              ),
              plc_mnemo_gage as (
                select d.id_tag,
                       m.name description,
                       d.id_mnemo
                  from xtech.mnemodetail d,
                       xtech.tag r,
                       xcommon.el m
                 where 1=1                    
                   and d.id_tag = r.id_tag
                   and m.id_el = r.id_el
                   and d.id_tag not in (select ss.id_tag
                                          from xtech.tagrepair ss
                                         where 1 = 1
                                           and sysdate between ss.datebeg and ss.dateend
                                           and ss.isenabled = 1--запись активна/актуальна
                                           and ss.datebeg <> ss.dateend
                                       )
                   and not exists (select * from gage_log_check glc where d.id_tag = glc.gage_id)
                   and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                   and r.id_el >= 30524 and r.id_el <= 30551
              ),
              gage_data as (
                select a.id_tag,
                       a.value
                  from xtech.tagvalue a
                 where 1=1
                   and cast(a.datestamp as date) >= v_period
                   and a.value between v_temp_limit and v_temp_limit_max
              )
         select x.group_id,
                b.picture pict_blob,
                to_char(x.id_mnemo) id_plc_mnemo,
                x.parent,
                x.mixer_name,
                (select listagg(gg.id_tag,',') within group (order by gg.id_tag)
                   from plc_mnemo_gage gg
                  where gg.id_mnemo = x.id_mnemo
                ) gage_ids,
                x.gage_ids_hit
           from (select l.group_id,
                        l.id_mnemo,
                        l.parent,
                        l.mixer_name,
                        l.blob_id,
                        listagg(d.id_tag,',') within group (order by d.id_tag) gage_ids_hit
                   from plc_mnemo_la l,
                        plc_mnemo_gage g,
                        gage_data d
                  where 1=1
                    and l.id_mnemo = g.id_mnemo
                    and g.id_tag = d.id_tag
                  group by l.group_id,
                           l.id_mnemo,
                           l.parent,
                           l.mixer_name,
                           l.blob_id
                ) x,
                xtech.mnemobackground b
          where 1=1
            and x.blob_id = b.id_mnemobackground
          order by x.group_id,
                   x.blob_id;

      return v_cur;
      
   end get_plc_mnemo_check_temp;
       
   function get_plc_mnemo_no_data return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for
         with plc_mnemo_la as (
                select x.name,
                       listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
                  from (select distinct m.name,
                               c.description mixer_name,
                               mp.name parent_name,
                               m.parent,
                               m.id_mnemo,
                               (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                    when 1 then 81
                                    when 2 then 82
                                    when 3 then 83
                               end) group_id,
                               m.id_mnemobackground blob_id
                          from xtech.mnemodetail d,
                               xtech.mnemo m,
                               xtech.mnemo mp,
                               xtech.tag r,
                               xcequip.equip c
                         where 1=1
                           and d.id_mnemo = m.id_mnemo
                           and d.id_tag = r.id_tag
                           and r.id_equip = c.id_equip
                           and mp.id_mnemo = m.parent
                           and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                       ) x 
              group by x.name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id 
              ),
              plc_mnemo_gage as (
                select d.id_tag,  
                       m.name description,
                       d.id_mnemo
                  from xtech.mnemodetail d,
                       xtech.tag r,
                       xcommon.el m
                 where 1=1                    
                   and d.id_tag = r.id_tag
                   and m.id_el = r.id_el
                   and d.id_tag not in (select ss.id_tag
                                          from xtech.tagrepair ss
                                         where 1 = 1
                                           and sysdate between ss.datebeg and ss.dateend
                                           and ss.isenabled = 1--запись активна/актуальна
                                           and ss.datebeg <> ss.dateend
                                       )
                   and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                   and r.id_el >= 30524 and r.id_el <= 30551
              ),
              gage_data as (
                select a.id_tag,
                       a.value
                  from xtech.tagvalue a,
                       xtech.limit li
                 where     1 = 1
                       and a.id_tag = li.id_tag
                       and li.id_event = 3
                       and (li.isdel is null or li.isdel = 0)
                       and li.datebeg <= sysdate 
                       and round((sysdate - cast(a.datestamp as date)) * 24 * 60, 2) > li.valuemin
              )
                           
         select x.group_id,
                b.picture pict_blob,
                to_char(x.id_mnemo) id_plc_mnemo,
                to_char(x.parent) parent,
                x.mixer_name,
                x.gage_ids,
                null gage_ids_hit
           from (select distinct l.group_id,
                        l.id_mnemo,
                        l.parent,
                        l.mixer_name,
                        /*(select listagg(gg.id_tag, ',') within group (order by gg.id_tag)
                           from plc_mnemo_gage gg
                          where gg.id_mnemo = g.id_mnemo
                        ) gage_ids,*/
                         --bug 19368
                         (select listagg(dd.id_tag,',') within group (order by dd.id_tag)
                            from xtech.mnemodetail dd
                           where dd.id_mnemo = g.id_mnemo
                         ) gage_ids,
                         --
                        l.blob_id
                   from plc_mnemo_la l,
                        plc_mnemo_gage g,
                        gage_data d
                  where 1 = 1
                    and l.id_mnemo = g.id_mnemo
                    and g.id_tag = d.id_tag
                ) x,
                xtech.mnemobackground b
          where 1=1
            and x.blob_id = b.id_mnemobackground
          order by x.group_id,
                   x.blob_id;

      return v_cur;
      
   end get_plc_mnemo_no_data;
                 
   function get_plc_mnemo_mailing_group return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for
         select x.group_id,
                'ЛО'||to_number(regexp_replace(x.name, '[^0-9]', '')) lo,
                (select p.plant_name_short
                   from rbs_base.sys_ref_plants p
                  where p.castplant_id = (select ss.id_smelterdefault
                                            from xcommon.setupsystem ss
                                           where ss.id_setupsystem = 1)) plant_name
           from (select g.group_id,
                        g.name,
                        (select count(*)
                           from rbs_base.group_user u
                          where u.group_id = g.group_id) cnt_user 
                   from rbs_base.ref_groups g 
                  where g.group_id in (
                                       81, 82, 83,    -- мнемосхемы термопар (базовый первоначально)
                                       421, 462, 463, -- прогноз
                                       642, 643, 644, -- рассылка по ст. откл.
                                       663, 664, 665  -- рассылка по резким скачкам
                                      )
                ) x
          where 1=1
            and x.cnt_user > 0; 

      return v_cur;
   end get_plc_mnemo_mailing_group;
    
    
   function get_plc_mnemo_group_users return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for 
         select g.group_id,
                p.user_plant_id,
                u.email
           from rbs_base.group_user g,
                rbs_base.sys_ref_users u,
                rbs_base.sys_user_plants p,
                rbs_base.sys_ref_plants rp
          where 1=1
            and g.group_id in (
                               81, 82, 83,    -- мнемосхемы термопар (базовый первоначально)
                               421, 462, 463, -- прогноз
                               642, 643, 644, -- рассылка по ст. откл.
                               663, 664, 665  -- рассылка по резким скачкам
                              )
            and p.plant_id = rp.plant_id
            and rp.castplant_id = (select ss.id_smelterdefault
                                     from xcommon.setupsystem ss
                                    where ss.id_setupsystem = 1)
            and g.user_plant_id = p.user_plant_id
            and p.user_id = u.user_id
            and u.email is not null; 

      return v_cur;
   end get_plc_mnemo_group_users;
    
    
   function get_mixer_gages_state(p_mixer_id number) return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for 
         with stop_mail_data as (
                select ss.id_tag,
                       ss.description,
                       ss.request,
                       ss.datebeg,
                       ss.dateend,
                       0 state
                  from xtech.tagrepair ss
                 where 1=1
                   and ss.isenabled = 1--запись активна/актуальна
                   and ss.dateend > sysdate
                   and ss.datebeg <> ss.dateend
              )
         select to_char(d.id_tag) gage_id,
                m.name gage_name,
                to_char(d.id_mnemo) id_plcmnemo,
                nvl((select (case when sysdate between s.datebeg and s.dateend then 0 else 1 end)--проверяем что активные записи попадают в текущий переиод
                      from stop_mail_data s where s.id_tag = d.id_tag)
                    , 1) state,
                (select s.description from stop_mail_data s where s.id_tag = d.id_tag) description,
                (select s.request from stop_mail_data s where s.id_tag = d.id_tag) request,
                (select s.datebeg from stop_mail_data s where s.id_tag = d.id_tag) date_begin,
                (select s.dateend from stop_mail_data s where s.id_tag = d.id_tag) date_end
           from xtech.mnemodetail d,
                xtech.tag r,
                xcommon.el m
          where 1=1
            and d.id_mnemo = p_mixer_id
            and d.id_tag = r.id_tag
            and m.id_el = r.id_el
            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
            and r.id_el >= 30524 and r.id_el <= 30551
          order by to_number(regexp_replace(m.name, '[^0-9]', '')); 
        
      return v_cur;
   end get_mixer_gages_state;
    
   procedure change_gages_state (p_state number, p_date_begin date, p_date_end date, p_request varchar2, p_note varchar2, p_array varchar2, p_user_id number)
   is
   begin
        
      for i in (
         select to_number(trim(regexp_substr (p_array,'[^,]+', 1, level))) gage_id
           from dual
        connect by regexp_substr (p_array, '[^,]+', 1, level) is not null
      )
      loop
         --ставим всем активным записям (isenabled = 1) статус 0
         update xtech.tagrepair s
            set s.dtedit = sysdate,
                s.isenabled = 0,
                s.user_id = p_user_id
          where s.id_tag = i.gage_id
            and s.isenabled = 1;--запись активна/актуальна
         
         --p_state = 0 это значит точка отключается, и ставим isenabled = 1, т.е. запись активна
         if p_state = 0 then
            insert into xtech.tagrepair (id_tag, description, isenabled, request, dtcreate, datebeg, dateend, dtedit, user_id)
                 values (i.gage_id, p_note, 1, p_request, sysdate, p_date_begin, p_date_end, sysdate, p_user_id);
         end if;
      end loop;
   
   end change_gages_state;
    
   function get_disabled_gages return sys_refcursor
   is
      v_cur sys_refcursor;
   begin
      open v_cur for 
         with no_data_gages as (
                select a.id_tag,
                       0 state
                  from xtech.tagvalue a
                 where round((sysdate - cast(a.datestamp as date)) * 24 * 60, 2) > 30
               
              )
         select to_char(s.id_tagrepair) id_plcmailstop,
                (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                     when 1 then 81
                     when 2 then 82
                     when 3 then 83
                end) group_id,
                to_char(m.id_mnemo) id_plc_mnemo,
                to_char(m.parent) parent,
                m.name mixer_name,
                e.name gage_name,
                to_char(s.id_tag) gage_id,
                s.datebeg date_mail_stop,
                s.dateend date_mail_start,
                s.description,
                s.request,
                s.datebeg,
                s.dateend,
                nvl(gg.state, 1) has_data,
                rbs_base.get_fio(s.user_id, 2) fio
           from xtech.mnemo m,
                xtech.mnemo mp,
                xtech.mnemodetail d,
                xtech.tag r,
                xcommon.el e,
                xtech.tagrepair s,
                no_data_gages gg
          where 1=1
            and mp.id_mnemo = m.parent
            and m.id_mnemo = d.id_mnemo
            and e.id_el = r.id_el
            and d.id_tag = r.id_tag
            and r.id_tag = s.id_tag
            and s.id_tag = gg.id_tag(+)
            and s.isenabled = 1--запись активна/актуальна
            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
            and r.id_el >= 30524 and r.id_el <= 30551
          order by d.id_mnemo,
                   d.id_tag;

      return v_cur;
   end get_disabled_gages;
    
    
   procedure set_gage_enabled (plc_mail_stop_id number) 
   is
   begin        
      update xtech.tagrepair s
         set s.isenabled = 0,
             s.datebeg = sysdate
       where s.id_tagrepair = plc_mail_stop_id;
   end set_gage_enabled;
    
   procedure set_gage_disabled (plc_mail_stop_id number) 
   is
   begin        
      update xtech.tagrepair s
         set s.dateend = sysdate
       where s.id_tagrepair = plc_mail_stop_id;
   end set_gage_disabled;
    

/*
   function get_plc_mnemo_check_growth return sys_refcursor
   is
      v_cur    sys_refcursor;
      
      v_date   date := trunc(sysdate,'hh');
      v_period date := v_date - 3;--начало проверки за последние n дней
   begin
      open v_cur for
         with plc_mnemo_la as (
                select x.name,
                       listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
                  from (select distinct m.name,
                               c.description mixer_name,
                               mp.name parent_name,
                               m.parent, 
                               m.id_mnemo,
                               (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                    when 1 then 81
                                    when 2 then 82
                                    when 3 then 83
                               end) group_id,
                               m.id_mnemobackground blob_id
                          from xtech.mnemodetail d,
                               xtech.mnemo m,
                               xtech.mnemo mp,
                               xtech.tag r,
                               xcequip.equip c
                         where 1=1
                           and d.id_mnemo = m.id_mnemo
                           and d.id_tag = r.id_tag
                           and r.id_equip = c.id_equip
                           and mp.id_mnemo = m.parent
                           and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                       ) x 
              group by x.name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
              ),
              plc_mnemo_gage as (
                select d.id_tag,
                       m.name description,
                       d.id_mnemo
                  from xtech.mnemodetail d,
                       xtech.tag r,
                       xcommon.el m
                 where 1=1                    
                   and d.id_tag = r.id_tag
                   and m.id_el = r.id_el
                   and d.id_tag not in (select ss.id_tag
                                          from xtech.tagrepair ss
                                         where 1 = 1
                                           and sysdate between ss.datebeg and ss.dateend
                                           and ss.isenabled = 1--запись активна/актуальна
                                           and ss.datebeg <> ss.dateend
                                       )
                   --and not exists (select * from gage_log_check glc where d.id_tag = glc.id_tag)
                   and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                   and r.id_el >= 30524 and r.id_el <= 30551
              ),
              t1 as (
                select v_period + (level - 1) * 3600 / 24 / 60 / 60 as date_beg,
                       v_period + (level) * 3600 / 24 / 60 / 60 as date_end
                  from dual
               connect by level <= 72
              ),
              gage_data as (
                select x.id_tag, 
                       --round (REGR_SLOPE(x.date_beg-v_date, x.val), 6) slope,
                       round (REGR_SLOPE(x.val, x.date_beg-v_date), 6) slope,
                       --round (REGR_SLOPE(TO_NUMBER(TO_CHAR(x.date_beg, 'J')), x.val), 6) slope,
                       count(*) cnt
                  from (
                        select a.id_tag,
                               d.date_beg,
                               avg (a.value) val
                          from t1 d, 
                               plc_mnemo_gage t,
                               xtech.tagvalue_ar a
                         where     1 = 1 
                               and a.datestamp between d.date_beg and d.date_end
                               and a.id_tag = t.id_tag
                         group by a.id_tag, d.date_beg
                         order by d.date_beg
                       ) x
                 group by x.id_tag
              )
              
         select x.group_id,
                b.picture pict_blob,
                to_char(x.id_mnemo) id_plc_mnemo,
                x.parent,
                x.mixer_name,
                (select listagg(dd.id_tag,',') within group (order by dd.id_tag)
                   from xtech.mnemodetail dd
                  where dd.id_mnemo = x.id_mnemo
                ) gage_ids,
                x.gage_ids_hit,
                gage_ids_json
           from (select l.group_id,
                        l.id_mnemo,
                        l.parent,
                        l.mixer_name,
                        l.blob_id,
                        listagg(d.id_tag,',') within group (order by d.id_tag) gage_ids_hit,
                        listagg('{tag_id:'||d.id_tag||', value:"'||d.slope||'"}',',') within group (order by d.id_tag) gage_ids_json
                   from plc_mnemo_la l,
                        plc_mnemo_gage g,
                        gage_data d,
                        xtech.limit li
                  where 1=1
                    and l.id_mnemo = g.id_mnemo
                    and g.id_tag = d.id_tag
                    and d.id_tag = li.id_tag
                    and li.id_event = 2
                    and (li.isdel is null or li.isdel = 0)
                    and li.datebeg <= sysdate 
                    and d.slope > li.valuemin
                    and d.slope < li.valuemax
                    and d.cnt > 54
                    
                  group by l.group_id,
                           l.id_mnemo,
                           l.parent,
                           l.mixer_name,
                           l.blob_id
                ) x,
                xtech.mnemobackground b
          where 1=1
            and x.blob_id = b.id_mnemobackground
          order by x.group_id,
                   x.blob_id;        
      return v_cur;
      
   end get_plc_mnemo_check_growth;
*/
   function get_plc_mnemo_check_growth return sys_refcursor
   is
      v_date_begin   date := sysdate-3;--начало проверки за последние n дней
      v_date         date := trunc(sysdate,'hh');
        
      v_cur          sys_refcursor;

      v_prev_date    date;
      v_last_date    date;
      v_count_data   number;
      v_count_diff   number;
      v_count_name   varchar2(250);
      
      v_valueslop    number;
      v_valuegoal    number;
      v_valuemin     number;
      v_valuemax     number;

      v_tag_count_data     number;

      v_plc_check_record   plc_check_record;
      v_plc_check_table    plc_check_table := plc_check_table();
             
      cursor plc_mnemo_la is 
         select xx.name,
                xx.mixer_name,
                xx.id_mnemo,
                xx.parent,
                xx.group_id,
                xx.blob_id,
                (select b.picture
                   from xtech.mnemobackground b where b.id_mnemobackground = xx.blob_id) pict_blob,
                (select listagg(d.id_tag,',') within group (order by d.id_tag)
                   from xtech.mnemodetail d,
                        xtech.tag r,
                        xcommon.el m
                  where 1=1
                    and d.id_tag = r.id_tag
                    and m.id_el = r.id_el
                    and d.id_tag not in (select ss.id_tag
                                           from xtech.tagrepair ss
                                          where 1 = 1
                                            and sysdate between ss.datebeg and ss.dateend
                                            and ss.isenabled = 1--запись активна/актуальна
                                            and ss.datebeg <> ss.dateend
                                        )
                    and d.id_mnemo = xx.id_mnemo
                    and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                    and r.id_el >= 30524 and r.id_el <= 30551
                ) gage_ids
           from (
                 select x.name,
                        listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                        x.parent_name,
                        x.parent,
                        x.id_mnemo,
                        x.group_id,
                        x.blob_id
                   from (select distinct m.name,
                                c.description mixer_name,
                                mp.name parent_name,
                                m.parent, 
                                m.id_mnemo,
                                (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                     when 1 then 81
                                     when 2 then 82
                                     when 3 then 83
                                end) group_id,
                                m.id_mnemobackground blob_id
                           from xtech.mnemodetail d,
                                xtech.mnemo m,
                                xtech.mnemo mp,
                                xtech.tag r,
                                xcequip.equip c
                          where 1=1
                            and d.id_mnemo = m.id_mnemo
                            and d.id_tag = r.id_tag
                            and r.id_equip = c.id_equip
                            and mp.id_mnemo = m.parent
                            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                            and r.id_el >= 30524 and r.id_el <= 30551
                        ) x
                  group by x.name,
                           x.parent_name,
                           x.parent,
                           x.id_mnemo,
                           x.group_id,
                           x.blob_id
                ) xx;

      cursor plc_mnemo_gage(id_plc_mnemo number) is
         select d.id_tag,  
                m.name description
           from xtech.mnemodetail d,
                xtech.tag r,
                xcommon.el m
          where 1=1
            and d.id_mnemo = id_plc_mnemo
            and d.id_tag = r.id_tag
            and m.id_el = r.id_el
            and d.id_tag not in (select ss.id_tag
                                   from xtech.tagrepair ss
                                  where 1=1
                                    and sysdate between ss.datebeg and ss.dateend
                                    and ss.isenabled = 1--запись активна/актуальна
                                    and ss.datebeg <> ss.dateend
                                )
            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
            and r.id_el >= 30524 and r.id_el <= 30551;

      /*cursor gage_data(p_gage_id number, p_date_begin date) is
         with t1 as (    
                select p_date_begin + (level - 1) * 3600 / 24 / 60 / 60 as date_beg,
                       p_date_begin + (level) * 3600 / 24 / 60 / 60 as date_end
                  from dual
               connect by level <= 73
              ),
              t2 as (
                select datestamp date_beg,
                       value,
                       coalesce(lead(datestamp) over(partition by id_tag order by datestamp),
                                sysdate) date_end,
                       d.date_end ddate_end,
                       d.date_beg ddate_beg,
                       a.id_tag
                  from xtech.tagvalue_ar a,
                       t1 d
                 where a.id_tag in (p_gage_id)
                   and a.datestamp >= d.date_beg - 1
                   and a.datestamp <= d.date_end
                   and a.datestamp >= p_date_begin
                   and a.value < g_temp_limit_max
                   and isdel = 0
                 group by a.id_tag,
                          datestamp,
                          value,
                          d.date_end,
                          d.date_beg
              )
         select a.ddate_beg date_beg,
                a.id_tag,
                round(sum(value * (least (cast(a.date_end as date), a.ddate_end) - greatest(cast(a.date_beg as date), a.ddate_beg))) / 
                      nullif(sum((least (cast(a.date_end as date), a.ddate_end) - greatest(cast(a.date_beg as date), a.ddate_beg))), 0), 1) val
           from t2 a
          where a.date_end >= a.ddate_beg
            and a.date_end <= a.ddate_end + 1
          group by a.ddate_beg, a.id_tag
          order by val;
      */        
   begin        
      for la in plc_mnemo_la
      loop        
         for gage in plc_mnemo_gage(la.id_mnemo)
         loop
           with t1 as (
                   select v_date_begin + (level - 1) * 3600 / 24 / 60 / 60 as date_beg,
                          v_date_begin + (level) * 3600 / 24 / 60 / 60 as date_end
                     from dual
                  connect by level <= 73
                 )
            select count(*),--кол-во данные
                    sum(case when xx.dif >= 0 then 1 else 0 end) + 1, --если различие дат с предыдущими значениями положительное, то это рост\
                    max(xx.nameshort),
                    round(REGR_SLOPE(xx.val, xx.date_beg-v_date), 6), --расчет угла наклона линейного тренда
                    max(xx.valuegoal),
                    max(xx.valuemin),
                    max(xx.valuemax)
              into v_count_data,
                   v_count_diff,
                   v_count_name,
                   v_valueslop,
                   v_valuegoal,
                   v_valuemin,
                   v_valuemax
              from (
                    select x.date_beg,
                        x.id_tag,
                        x.nameshort,
                        x.val,
                        x.valuegoal,
                        x.valuemin,
                        x.valuemax,
                        (x.date_beg - lag(x.date_beg) over (order by x.val, x.date_beg)) dif --разница даты с предыдущей датой значения
                   from (--берем исходные данные по точкам агрегированные по часам
                         select d.date_beg,
                                a.id_tag,
                                t.nameshort,
                                round (avg (a.value), nvl (t.rnd, 1)) val,
                                li.valuegoal,
                                li.valuemin,
                                li.valuemax
                           from t1 d, 
                                xtech.tag t,
                                xtech.tagvalue_ar a,
                                xtech.limit li
                          where     a.id_tag in (gage.id_tag)
                                and a.datestamp between d.date_beg and d.date_end
                                and a.id_tag = t.id_tag
                                and sysdate between nvl(t.datebeg, sysdate) and nvl(t.dateend, sysdate)
                                and li.id_tag = t.id_tag
                                and li.id_event = 2
                                and (li.isdel is null or li.isdel = 0)
                                and li.datebeg <= sysdate
                          group by d.date_beg, a.id_tag, t.rnd, t.nameshort
                                ,li.valuegoal, li.valuemin,li.valuemax
                          order by val, d.date_beg
                           ) x
                   ) xx; 
            /*
            v_count_data := 0;
            v_prev_date := null;
            v_last_date := null; 
            
            for g_data in gage_data(gage.id_tag, v_date_begin)
            loop
               v_last_date := g_data.date_beg;
               
               exit when v_prev_date is not null and v_prev_date > v_last_date;
               
               v_count_data := v_count_data + 1; 

               v_prev_date := v_last_date;
            end loop;
            
            if v_prev_date = v_last_date and v_count_data > 54--было 54, хз почему */
            /*
            select li.valuemin
              into v_tag_count_data
              from xtech.limit li
             where     1 = 1
                   and li.id_tag = gage.id_tag
                   and li.id_event = 2
                   and (li.isdel is null or li.isdel = 0)
                   and li.datebeg <= sysdate;
            */       
            
            if v_count_data = v_count_diff and v_count_data > v_valuegoal--минимально допустимое количество значений с оборудования за период sysdate-3 
                and v_valueslop > v_valuemin and v_valueslop < v_valuemax--Проверка границ у угла наклона
            then
               v_plc_check_record.group_id := la.group_id;
               v_plc_check_record.pict_blob := la.pict_blob;
               v_plc_check_record.id_plc_mnemo := la.id_mnemo;
               v_plc_check_record.parent := la.parent;
               v_plc_check_record.mixer_name := la.mixer_name;
               v_plc_check_record.gage_ids := la.gage_ids;
               v_plc_check_record.gage_ids_hit := null;
               v_plc_check_record.gage_ids_json := null;
               v_plc_check_record.gage_ids_name := lower(to_char(v_count_name));
               
               v_plc_check_table.extend;
               v_plc_check_table(v_plc_check_table.count) := v_plc_check_record;
               
               exit;
               
            end if;
         end loop;
      end loop;
        
      open v_cur for
         select group_id,
                to_char(parent) parent,
                pict_blob,
                to_char(id_plc_mnemo) id_plc_mnemo,
                mixer_name,
                gage_ids,
                gage_ids_hit,
                gage_ids_json
           from table(v_plc_check_table); 
        
      return v_cur;
   end get_plc_mnemo_check_growth;

   function get_plc_mnemo_check_stddev return sys_refcursor
   is
      v_date_begin   date := sysdate-1;--начало проверки за последние n дней
      v_date_end     date := sysdate;--начало проверки за последние n дней
      
      --v_tag_limit    number := 7;--брать ст. отклонения не больше значения
      --v_tag_factor   number := 3;--множитель среднего для поиска отклонений
      
      v_tag_name     nvarchar2(250);
      v_avg_tag      number;
      
      v_cur          sys_refcursor;


      v_plc_check_record  plc_check_record;
      v_plc_check_table   plc_check_table := plc_check_table();
             
      cursor plc_mnemo_la is 
         select xx.name,
                xx.mixer_name,
                xx.id_mnemo,
                xx.parent,
                xx.group_id,
                xx.blob_id,
                (select b.picture
                   from xtech.mnemobackground b where b.id_mnemobackground = xx.blob_id) pict_blob,
                (select listagg(d.id_tag,',') within group (order by d.id_tag)
                   from xtech.mnemodetail d,
                        xtech.tag r,
                        xcommon.el m
                  where 1=1
                    and d.id_tag = r.id_tag
                    and m.id_el = r.id_el
                    and d.id_tag not in (select ss.id_tag
                                           from xtech.tagrepair ss
                                          where 1 = 1
                                            and sysdate between ss.datebeg and ss.dateend
                                            and ss.isenabled = 1--запись активна/актуальна
                                            and ss.datebeg <> ss.dateend
                                        )
                    and d.id_mnemo = xx.id_mnemo
                    and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                    and r.id_el >= 30524 and r.id_el <= 30551
                ) gage_ids
           from (
                 select x.name,
                        listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                        x.parent_name,
                        x.parent,
                        x.id_mnemo,
                        x.group_id,
                        x.blob_id
                   from (select distinct m.name,
                                c.description mixer_name,
                                mp.name parent_name,
                                m.parent, 
                                m.id_mnemo,
                                (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                     when 1 then 642
                                     when 2 then 643
                                     when 3 then 644
                                end) group_id,
                                m.id_mnemobackground blob_id
                           from xtech.mnemodetail d,
                                xtech.mnemo m,
                                xtech.mnemo mp,
                                xtech.tag r,
                                xcequip.equip c
                          where 1=1
                            and d.id_mnemo = m.id_mnemo
                            and d.id_tag = r.id_tag
                            and r.id_equip = c.id_equip
                            and mp.id_mnemo = m.parent
                            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                            and r.id_el >= 30524 and r.id_el <= 30551
                        ) x
                  group by x.name,
                           x.parent_name,
                           x.parent,
                           x.id_mnemo,
                           x.group_id,
                           x.blob_id
                ) xx;

   begin        
      for la in plc_mnemo_la
      loop         
         v_tag_name := null;
         v_avg_tag := null;
         
         begin
            select avg(x.val) * avg(x.tag_stddev_factor)
              into v_avg_tag
              from (
                    select a.id_tag,
                           max(li.valuemin) as tag_stddev_factor,
                           max(li.valuemax) as tag_stddev_limit,
                           round(stddev (a.value), 2) val
                      from xtech.tag t, 
                           xtech.tagvalue_ar a,
                           xtech.mnemodetail d,
                           xtech.limit li,
                           xcommon.el m
                     where     1 = 1 
                           and d.id_mnemo = la.id_mnemo
                           and a.id_tag = t.id_tag
                           and a.datestamp between v_date_begin and v_date_end
                           and a.id_tag = t.id_tag
                           and sysdate between nvl (t.datebeg, sysdate) and nvl (t.dateend, sysdate)
                           and d.id_tag = t.id_tag
                           and m.id_el = t.id_el
                           and d.id_tag not in (select ss.id_tag
                                                  from xtech.tagrepair ss
                                                 where 1=1
                                                   and sysdate between ss.datebeg and ss.dateend
                                                   and ss.isenabled = 1--запись активна/актуальна
                                                   and ss.datebeg <> ss.dateend
                                               )
                           and sysdate between nvl(t.datebeg, sysdate) and nvl(t.dateend, sysdate)
                           and t.id_el >= 30524 and t.id_el <= 30551
                           and a.id_tag = li.id_tag
                           and li.id_event = 7
                           and (li.isdel is null or li.isdel = 0)
                           and li.datebeg <= sysdate
                  group by a.id_tag
      
                  ) x
            where     1 = 1
                  and x.val < x.tag_stddev_limit;
                  
                  
            for i in (
                  select x.id_tag,
                         x.val
                    from (
                          select a.id_tag,
                                 max(li.valuemin) as tag_stddev_factor,
                                 max(li.valuemax) as tag_stddev_limit,
                                 round(stddev (a.value), 2) val
                            from xtech.tag t, 
                                 xtech.tagvalue_ar a,
                                 xtech.mnemodetail d,
                                 xtech.limit li,
                                 xcommon.el m
                           where     1 = 1 
                                 and d.id_mnemo = la.id_mnemo
                                 and a.id_tag = t.id_tag
                                 and a.datestamp between v_date_begin and v_date_end
                                 and a.id_tag = t.id_tag
                                 and sysdate between nvl (t.datebeg, sysdate) and nvl (t.dateend, sysdate)
                                 and d.id_tag = t.id_tag
                                 and m.id_el = t.id_el
                                 and d.id_tag not in (select ss.id_tag
                                                        from xtech.tagrepair ss
                                                       where 1=1
                                                         and sysdate between ss.datebeg and ss.dateend
                                                         and ss.isenabled = 1--запись активна/актуальна
                                                         and ss.datebeg <> ss.dateend
                                                     )
                                 and sysdate between nvl(t.datebeg, sysdate) and nvl(t.dateend, sysdate)
                                 and t.id_el >= 30524 and t.id_el <= 30551
                                 and a.id_tag = li.id_tag
                                 and li.id_event = 7
                                 and (li.isdel is null or li.isdel = 0)
                                 and li.datebeg <= sysdate
                        group by a.id_tag
            
                        ) x
                  where     1 = 1
                        and x.val < x.tag_stddev_limit
            )
            loop
               if i.val > v_avg_tag
               then
                  
                  select t.description
                    into v_tag_name
                    from xtech.tag t
                   where t.id_tag = i.id_tag;
                  
                  v_plc_check_record.group_id := la.group_id;
                  v_plc_check_record.pict_blob := la.pict_blob;
                  v_plc_check_record.id_plc_mnemo := la.id_mnemo;
                  v_plc_check_record.parent := la.parent;
                  v_plc_check_record.mixer_name := la.mixer_name;
                  v_plc_check_record.gage_ids := la.gage_ids;
                  v_plc_check_record.mixer_tag_name := v_tag_name;

                  v_plc_check_table.extend;
                  v_plc_check_table(v_plc_check_table.count) := v_plc_check_record;
                  
               end if;
            end loop;
                
            exception when no_data_found then null;
         end;      
         
      end loop;

      open v_cur for
         select group_id,
                to_char(parent) parent,
                pict_blob,
                to_char(id_plc_mnemo) id_plc_mnemo,
                mixer_name,
                gage_ids,
                mixer_tag_name
           from table(v_plc_check_table); 
        
      return v_cur;
      
   end get_plc_mnemo_check_stddev; 
   
   
   function get_plc_mnemo_check_sharp_jump return sys_refcursor
   is
      v_date         date := sysdate;--to_date('26.03.2023 11:00', 'dd.mm.yyyy hh24:mi');--sysdate;
      v_date_begin   date := v_date - interval '30' minute;
      v_date_end     date := v_date;
      
      v_cur          sys_refcursor;

      v_count_jump   number;
      v_tag_name     nvarchar2(250);
      
      v_tag_jump_min number;
      v_tag_jump_max number;

      v_plc_check_record  plc_check_record;
      v_plc_check_table   plc_check_table := plc_check_table();
             
      cursor plc_mnemo_la is 
         select xx.name,
                xx.mixer_name,
                xx.id_mnemo,
                xx.parent,
                xx.group_id,
                xx.blob_id,
                (select b.picture
                   from xtech.mnemobackground b where b.id_mnemobackground = xx.blob_id) pict_blob,
                (select listagg(d.id_tag,',') within group (order by d.id_tag)
                   from xtech.mnemodetail d,
                        xtech.tag r,
                        xcommon.el m
                  where 1=1
                    and d.id_tag = r.id_tag
                    and m.id_el = r.id_el
                    and d.id_tag not in (select ss.id_tag
                                           from xtech.tagrepair ss
                                          where 1 = 1
                                            and sysdate between ss.datebeg and ss.dateend
                                            and ss.isenabled = 1--запись активна/актуальна
                                            and ss.datebeg <> ss.dateend
                                        )
                    and d.id_mnemo = xx.id_mnemo
                    and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                    and r.id_el >= 30524 and r.id_el <= 30551
                ) gage_ids
           from (
                 select x.name,
                        listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                        x.parent_name,
                        x.parent,
                        x.id_mnemo,
                        x.group_id,
                        x.blob_id
                   from (select distinct m.name,
                                c.description mixer_name,
                                mp.name parent_name,
                                m.parent, 
                                m.id_mnemo,
                                (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                     when 1 then 663
                                     when 2 then 664
                                     when 3 then 665
                                end) group_id,
                                m.id_mnemobackground blob_id
                           from xtech.mnemodetail d,
                                xtech.mnemo m,
                                xtech.mnemo mp,
                                xtech.tag r,
                                xcequip.equip c
                          where 1=1
                            and d.id_mnemo = m.id_mnemo
                            and d.id_tag = r.id_tag
                            and r.id_equip = c.id_equip
                            and mp.id_mnemo = m.parent
                            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                            and r.id_el >= 30524 and r.id_el <= 30551
                        ) x
                  group by x.name,
                           x.parent_name,
                           x.parent,
                           x.id_mnemo,
                           x.group_id,
                           x.blob_id
                ) xx;

      cursor plc_mnemo_gage(id_plc_mnemo number) is
         select d.id_tag,  
                m.name description
           from xtech.mnemodetail d,
                xtech.tag r,
                xcommon.el m
          where 1=1
            and d.id_mnemo = id_plc_mnemo
            and d.id_tag = r.id_tag
            and m.id_el = r.id_el
            and d.id_tag not in (select ss.id_tag
                                   from xtech.tagrepair ss
                                  where 1=1
                                    and sysdate between ss.datebeg and ss.dateend
                                    and ss.isenabled = 1--запись активна/актуальна
                                    and ss.datebeg <> ss.dateend
                                )
            and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
            and r.id_el >= 30524 and r.id_el <= 30551;
            
   begin        
      for la in plc_mnemo_la
      loop        
         for gage in plc_mnemo_gage(la.id_mnemo)
         loop
            
            select li.valuemin, li.valuemax
              into v_tag_jump_min, v_tag_jump_max
              from xtech.limit li
             where     1 = 1
                   and li.id_tag = gage.id_tag
                   and li.id_event = 6
                   and (li.isdel is null or li.isdel = 0)
                   and li.datebeg <= sysdate;
            
            select count (*)
              into v_count_jump
              from (  select x.datestamp,
                             x.id_tag,
                             x.val,
                             (x.val - lag (x.val) over (order by x.datestamp)) dif --разница значений с предыдущей датой
                        from (--берем исходные данные по точкам без агрегации
                              select a.datestamp,
                                     a.id_tag,
                                     a.value val
                                from xtech.tag t, 
                                     xtech.tagvalue_ar a
                               where     1 = 1
                                     and a.id_tag in (gage.id_tag)
                                     and a.datestamp between v_date_begin and v_date_end
                                     and a.id_tag = t.id_tag
                                     and sysdate between nvl (t.datebeg, sysdate) and nvl (t.dateend, sysdate)
                             ) x
                    order by x.datestamp
                   ) xx
             where xx.dif between v_tag_jump_min and v_tag_jump_max;         
            
            
            if v_count_jump > 3--проверка на кол-во скачков температуры у точки миксера 
            then
               select t.description
                 into v_tag_name
                 from xtech.tag t
                where t.id_tag = gage.id_tag;
                  
               v_plc_check_record.group_id := la.group_id;
               v_plc_check_record.pict_blob := la.pict_blob;
               v_plc_check_record.id_plc_mnemo := la.id_mnemo;
               v_plc_check_record.parent := la.parent;
               v_plc_check_record.mixer_name := la.mixer_name;
               v_plc_check_record.gage_ids := la.gage_ids;
               v_plc_check_record.mixer_tag_name := v_tag_name;

               v_plc_check_table.extend;
               v_plc_check_table(v_plc_check_table.count) := v_plc_check_record;
               
               --exit;
               
            end if;
         end loop;
      end loop;
        
      open v_cur for
         select group_id,
                to_char(parent) parent,
                pict_blob,
                to_char(id_plc_mnemo) id_plc_mnemo,
                mixer_name,
                gage_ids,
                mixer_tag_name
           from table(v_plc_check_table); 
        
      return v_cur;
   end get_plc_mnemo_check_sharp_jump;
   
   
   
   procedure mnemo_check_log_save(p_id_event number, p_tag_ids clob)
   is
   begin
      for i in (
         select t.tag_id,
                t.val
           from json_table(p_tag_ids, '$'
                  columns (nested path '$[*]'
                     columns (
                              tag_id   number PATH '$.tag_id',
                              val      number PATH '$.value'
                             )
                          )
                ) t
          where t.tag_id is not null
      )
      loop
         insert into xtech.logevent (id_event, id_tag, value, datebeg)
                             values (p_id_event, i.tag_id, i.val, sysdate);
      end loop;
      
      
   end mnemo_check_log_save;
   
   
   function mnemo_check_mixer_temp(p_id_event number, p_check_minutes number, p_check_log number) return sys_refcursor
   is
      v_cur       sys_refcursor;
              
      v_period    date := sysdate - p_check_minutes/(24*60);--n минут
   begin
      
      open v_cur for 
         with plc_mnemo_la as (
                select x.name,
                       listagg(to_char(x.mixer_name), ', ') within group (order by x.mixer_name) mixer_name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
                  from (select distinct m.name,
                               c.description mixer_name,
                               mp.name parent_name,
                               m.parent, 
                               m.id_mnemo,
                               (case to_number(regexp_replace(mp.name, '[^0-9]', ''))
                                    when 1 then 81
                                    when 2 then 82
                                    when 3 then 83
                               end) group_id,
                               m.id_mnemobackground blob_id
                          from xtech.mnemodetail d,
                               xtech.mnemo m,
                               xtech.mnemo mp,
                               xtech.tag r,
                               xcequip.equip c
                         where 1=1
                           and d.id_mnemo = m.id_mnemo
                           and d.id_tag = r.id_tag
                           and r.id_equip = c.id_equip
                           and mp.id_mnemo = m.parent
                           and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                       ) x 
              group by x.name,
                       x.id_mnemo,
                       x.parent,
                       x.group_id,
                       x.blob_id
              ),
              gage_log_check as (
               select le.id_tag 
                 from xtech.logevent le
                where     1 = 1
                      and le.id_event = p_id_event
                      and le.dtedit >= sysdate - 1 / 24
                      and p_check_log = 1
              ),
              plc_mnemo_gage as (
                select d.id_tag,
                       r.nameshort description,
                       d.id_mnemo
                  from xtech.mnemodetail d,
                       xtech.tag r,
                       xcommon.el m
                 where 1=1                    
                   and d.id_tag = r.id_tag
                   and m.id_el = r.id_el
                   and d.id_tag not in (select ss.id_tag
                                          from xtech.tagrepair ss
                                         where 1 = 1
                                           and sysdate between ss.datebeg and ss.dateend
                                           and ss.isenabled = 1--запись активна/актуальна
                                           and ss.datebeg <> ss.dateend
                                       )
                   and not exists (select * from gage_log_check glc where d.id_tag = glc.id_tag)
                   and sysdate between nvl(r.datebeg, sysdate) and nvl(r.dateend, sysdate)
                   and r.id_el >= 30524 and r.id_el <= 30551
              ),
              gage_log_count as (
               select le.id_tag,
                      count(*) cnt_log
                 from xtech.logevent le
                where     1 = 1
                      and le.id_event = p_id_event
                      and le.dtedit >= sysdate - 6 / 24
                group by le.id_tag
              ),
              gage_data as (
                select a.id_tag,
                       max(a.value) value,
                       max(li.valuemin) valuemin,
                       max(li.valuemax) valuemax
                  from xtech.tagvalue_ar a,
                       xtech.limit li
                 where 1=1
                       and a.datestamp >= v_period
                       and a.id_tag = li.id_tag
                       and li.id_event = p_id_event
                       and (li.isdel is null or li.isdel = 0)
                       and li.datebeg <= sysdate 
                       and a.value between li.valuemin and li.valuemax
                 group by a.id_tag
              )
         select x.group_id,
                b.picture pict_blob,
                to_char(x.id_mnemo) id_plc_mnemo,
                x.parent,
                x.mixer_name,
                /*(select listagg(gg.id_tag,',') within group (order by gg.id_tag)
                   from plc_mnemo_gage gg
                  where gg.id_mnemo = x.id_mnemo
                ) gage_ids,*/
                --bug 19368
                (select listagg(dd.id_tag,',') within group (order by dd.id_tag)
                   from xtech.mnemodetail dd
                  where dd.id_mnemo = x.id_mnemo
                ) gage_ids,
                --
                x.gage_ids_hit,
                x.gage_ids_json,
                x.gage_ids_name,
                x.cnt_log
           from (select l.group_id,
                        l.id_mnemo,
                        l.parent,
                        l.mixer_name,
                        l.blob_id,
                        listagg(d.id_tag,',') within group (order by d.id_tag) gage_ids_hit,
                        listagg('{tag_id:'||d.id_tag||', value:"'||d.value||'"}',',') within group (order by d.id_tag) gage_ids_json,
                        listagg(''||lower(to_char(g.description))||'&nbsp;–&nbsp;<span style="color:white;background:#ff3b30;border-radius:2px;">&nbsp;'||rtrim(to_char(d.value, 'FM99990.09'), '.')||'&nbsp;</span> ('||d.valuemin||' - '||d.valuemax||')','|') within group (order by d.id_tag) gage_ids_name,
                        nvl(max(lc.cnt_log), 0) cnt_log
                   from plc_mnemo_la l,
                        plc_mnemo_gage g,
                        gage_data d,
                        gage_log_count lc
                  where 1=1
                    and l.id_mnemo = g.id_mnemo
                    and g.id_tag = d.id_tag
                    and g.id_tag = lc.id_tag(+)
                  group by l.group_id,
                           l.id_mnemo,
                           l.parent,
                           l.mixer_name,
                           l.blob_id
                ) x,
                xtech.mnemobackground b
          where 1=1
            and x.blob_id = b.id_mnemobackground
          order by x.group_id,
                   x.blob_id;

      return v_cur;
      
   end mnemo_check_mixer_temp;
   
   
    
end rbs_technology;
/grant EXECUTE on rbs_bl.rbs_technology to rbs_user;
