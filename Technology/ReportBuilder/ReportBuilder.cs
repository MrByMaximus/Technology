using System.Data;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Sibinfosoft.Utils;
using RBS_Core.Helpers;
using RBS_Core.Helpers.Utils;
using RBS_Core.Models;
using RBS_Helpers;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController
    {
        public PartialViewResult ReportBuilder()
        {
            var date = HttpContext.Session.Get<DateTime?>("Date");
            if (date != null)
            {
                HttpContext.Session.Set("Date", (DateTime) date);
            }
            else
            {
                HttpContext.Session.Set("Date", DateTime.Now);
            }


            var dateBegin = HttpContext.Session.Get<DateTime?>("DateBegin");
            if (dateBegin != null)
            {
                HttpContext.Session.Set("DateBegin", (DateTime)dateBegin);
            }
            else
            {
                HttpContext.Session.Set("DateBegin", DateTime.Now);
            }

            var dateEnd = HttpContext.Session.Get<DateTime?>("DateEnd");
            if (dateEnd != null)
            {
                HttpContext.Session.Set("DateEnd", (DateTime)dateEnd);
            }
            else
            {
                HttpContext.Session.Set("DateEnd", DateTime.Now);
            }


            return PartialView("ReportBuilder");
        }

        #region Избранное

        public async Task<JsonResult> GetFavorites()
        {
            var subModuleId = HttpContext.Session.Get<int?>("SubModuleId");

            var userId = HttpContext.Session.Get<UserModel>("UserInfo")?.UserId;

            #region query

            const string query = @"
                  select f.id,
                         f.module_id,
                         f.user_id,
                         f.favorite_name,
                         f.favorite
                    from rbs_base.sys_favorites f
                   where     1=1
                         and f.module_id = :p_module_id 
                         and f.user_id = :p_user_id
                order by f.favorite_name
            ";

            #endregion

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_module_id", Value = subModuleId},
                new OracleParameter {ParameterName = ":p_user_id", Value = userId},
            };

            var table = await OracleOperation.GetDataAsync(query, param);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<ContentResult> SaveFavorites(int subModuleId, int userId, string array, string name)
        {
            #region подготовка данных

            var list = new List<dynamic>();

            var jsonArray = JArray.Parse(array);
            foreach (var jObject in jsonArray.Children<JObject>())
            {
                string? id = null;
                string? key = null;
                string? text = null;
                foreach (var property in jObject.Properties())
                {
                    switch (property.Name)
                    {
                        case "id":
                            id = (property.Value).ToString();
                            break;
                        case "key":
                            key = (property.Value).ToString();
                            break;
                        case "text":
                            text = (property.Value).ToString().Replace("\\\\n", " - ");
                            break;
                    }
                }
                if (id != null)
                {
                    dynamic model = new ExpandoObject();
                    model.id = id;
                    model.key = key;
                    model.text = text;

                    list.Add(model);
                }
            }

            var json = JsonConvert.SerializeObject(list);

            #endregion
            
            const string strSql = @"begin rbs_base.sys_favorites_add(:p_module_id, :p_user_id, :p_favorite_name, :p_favorite); end;";
            
            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_module_id", Value = subModuleId},
                new OracleParameter {ParameterName = ":p_user_id", Value = userId},
                new OracleParameter {ParameterName = ":p_favorite_name", Value = name},
                new OracleParameter {ParameterName = ":p_favorite", Value = json, OracleDbType = OracleDbType.Clob},
            };
            
            var sqlResult = await OracleOperation.RunSqlAsync(strSql, sqlParam);

            return Content(sqlResult.ToString());
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<ContentResult> DeleteFavorites(string id)
        {
            const string strSql = @"begin rbs_base.sys_favorites_delete(:p_favorite_id); end;";

            var sqlParam = new List<DbParameter>(1)
            {
                new OracleParameter {ParameterName = ":p_favorite_id", Value = Convert.ToInt32(id)}
            };

            var sqlResult = await OracleOperation.RunSqlAsync(strSql, sqlParam);

            return Content(sqlResult.ToString());
        }

        #endregion


        public async Task<JsonResult> GetTechnologyTree()
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 33 || moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
                if (moduleId == 938)
                {
                    moduleId = 33;
                }
                if (moduleId == 939)
                {
                    moduleId = 101;
                }
                if (moduleId == 943)
                {
                    moduleId = 305;
                }
            }
            if (Parametrs.Plant == Plant.SILICON)
            {
                package = "rbs_technology_old";
            }

            var query = $@"begin :cur := rbs_bl.{package}.get_technology_tree(p_type => :p_type, p_module_id => :p_module_id, p_plant_id => :p_plant_id, p_smelter_id => :p_smelter_id); end;";
            
            var paramEquipment = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_type", OracleDbType = OracleDbType.Int32, Value = 0},
                new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                new OracleParameter {ParameterName = ":p_plant_id", Value = Static.CurrentPlant?.Id, IsNullable = true},
                new OracleParameter {ParameterName = ":p_smelter_id", Value = Static.CurrentPlant?.SmelterId, IsNullable = true},
            };
            var paramParameters = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_type", OracleDbType = OracleDbType.Int32, Value = 1},
                new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                new OracleParameter {ParameterName = ":p_plant_id", Value = Static.CurrentPlant?.Id, IsNullable = true},
                new OracleParameter {ParameterName = ":p_smelter_id", Value = Static.CurrentPlant?.SmelterId, IsNullable = true},
            };


            #region Equipment

            var equipmentTable = await OracleOperation.GetFunctionRefCursorAsync(query, paramEquipment, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            #endregion

            #region Parameters

            var parametersTable = await OracleOperation.GetFunctionRefCursorAsync(query, paramParameters, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            #endregion

            var list = new List<dynamic>();

            var minLevel = Operation.GetValueOrZero<int>(equipmentTable.Compute("MIN(level)", "level <> 1"));
            var rootValue = equipmentTable.Select($"level={minLevel}").First()["parent_id"];


            foreach (var equip in equipmentTable.Select(null, "num_order"))
            {
                dynamic modelEquip = new ExpandoObject();
                modelEquip.id = equip["equipment_id"].ToString() ?? "";

                if (CheckChildWithParams(modelEquip.id, equipmentTable, parametersTable))
                {
                    modelEquip.name = (equip["equipment_name"].ToString() ?? "").Replace('.', '_');
                    modelEquip.parent_id = equip["parent_id"].ToString() ?? "";
                    //modelEquip.num_order = equip["num_order"].ToString();
                    modelEquip.num_order = 0;
                    modelEquip.seq = equipmentTable.Columns.Contains("seq") ? equip["seq"] : -1;

                    list.Add(modelEquip);

                    foreach (var param in parametersTable.Select($"equipment_id={modelEquip.id}", "num_order, param_short_name"))
                    {
                        dynamic modelParam = new ExpandoObject();
                        modelParam.id = $"{modelEquip.id}_{param["parameter_id"]}";
                        modelParam.tag_id = param["tag_id"];
                        modelParam.name = (param["param_short_name"].ToString() ?? "").Replace('.', '_');
                        modelParam.parent_id = modelEquip.id;


                        //modelParam.num_order = param["num_order"].ToString();
                        modelParam.num_order = 1;
                        modelParam.seq = parametersTable.Columns.Contains("seq") ? param["seq"] : -1;


                        dynamic data = new ExpandoObject();
                        var unit = string.IsNullOrWhiteSpace(param["unit"].ToString()) ? "" : ", " + param["unit"];

                        //data.type = "last";
                        data.tag = param["tag_id"].ToString() ?? "";
                        data.text = (param["equipment_path"] + "\\n" + param["param_short_name"] + unit).Replace('.', '_');
                        data.type = parametersTable.Columns.Contains("value_type") ? param["value_type"] : -1;

                        modelParam.data = data;

                        list.Add(modelParam);
                    }
                }
            }
            

            dynamic model = new ExpandoObject();
            model.data = list;
            model.root_value = rootValue;

            return Json(model, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }


        private bool CheckChildWithParams(string id, DataTable equipmentTable, DataTable parametersTable)
        {
            var currentEquip = equipmentTable.Select($"equipment_id={id}").FirstOrDefault();
            if (currentEquip == null)
            {
                return false;
            }

            var hasParam = Operation.GetValueOrZero<int>(currentEquip["has_tech_param"]);
            if (hasParam == 1)
            {
                var find = parametersTable.Select($"equipment_id={id}");
                if (find.Length > 0)
                {
                    return true;
                }
            }

            var result = equipmentTable.Select($"parent_id={id}");
            if (result.Length == 0)
            {
                return false;
            }
            
            var res = false;
            foreach (var row in result)
            {
                var equipId = row["equipment_id"].ToString() ?? "";
                if (CheckChildWithParams(equipId, equipmentTable, parametersTable))
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        //private void FillTree(ref dynamic item, ref DataTable equipmentTable, ref DataTable parametersTable)
        //{
        //    var list = new List<dynamic>();
            
        //    foreach (var equipment in equipmentTable.Select($"parent_id={item.id}", "num_order"))
        //    {
        //        dynamic model = new ExpandoObject();
        //        model.id = equipment["equipment_id"].ToString();
        //        model.text = equipment["equipment_name"].ToString().Replace('.', '_');
        //        model.hasTechParam = Operation.GetValueOrNull<int>(equipment["has_tech_param"]);
        //        model.items = new List<dynamic>();

        //        if (equipmentTable.Select($"parent_id={model.id}").Any())
        //        {
        //            FillTree(ref model, ref equipmentTable, ref parametersTable);
        //        }

        //        var countItems = ((List<dynamic>) model.items).Count;

        //        if (model.hasTechParam == 1 || countItems > 0)
        //            list.Add(model);
        //    }

        //    foreach (var model in list)
        //    {

        //        foreach (var row in parametersTable.Select($"equipment_id={model.id}", "num_order, param_short_name"))
        //        {
        //            dynamic param = new ExpandoObject();
        //            param.id = $"{model.id}_{row["parameter_id"]}";
        //            param.text = row["param_short_name"].ToString().Replace('.', '_');

        //            var unit = string.IsNullOrWhiteSpace(row["unit"].ToString()) ? "" : ", " + row["unit"];

        //            dynamic data = new ExpandoObject();
        //            data.type = "last";
        //            data.tag = row["tag_id"];
        //            data.text = (row["equipment_path"] + "\\n" + row["param_short_name"] + unit).Replace('.', '_');
        //            param.data = data;

        //            ((List<dynamic>)model.items).Add(param);
        //        }
        //    }

        //    item.items = list;
        //}
        
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetTechnologyCollection(string array, DateTime dateBegin, DateTime dateEnd, int cStep, int valuesMode)
        {
            HttpContext.Session.Set("DateBegin", dateBegin);
            HttpContext.Session.Set("DateEnd", dateEnd);


            var tags = array.Split(',').ToArray();
            
            var output = string.Join(",", array);

            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");


            var connection = Static.ConnectionRepository.RBS_TECHN.Default;
            if (RBS_Helpers.Parametrs.Plant == Plant.IAZ && (moduleId == 101 || moduleId == 305 || moduleId == 938 || moduleId == 939 || moduleId == 943))
            {
                connection = Static.ConnectionRepository.RBS_USER.Default;
            }

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
                if (moduleId == 938)
                {
                    moduleId = 33;
                }
                if (moduleId == 939)
                {
                    moduleId = 101;
                }
                if (moduleId == 943)
                {
                    moduleId = 305;
                }
            }

            var query = $@"begin :cur := rbs_bl.{package}.get_technology_data(p_type => :p_type, p_module_id => :p_module_id, p_plant_id => :p_plant_id, p_date_begin => :p_date_begin, p_date_end => :p_date_end, p_params => :p_params, p_values_mode => :p_values_mode); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_type", Value = cStep, OracleDbType = OracleDbType.Int32},
                new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                new OracleParameter {ParameterName = ":p_plant_id", Value = Static.CurrentPlant?.Id, IsNullable = true},
                new OracleParameter {ParameterName = ":p_date_begin", Value = dateBegin, OracleDbType = OracleDbType.Date},
                new OracleParameter {ParameterName = ":p_date_end", Value = dateEnd, OracleDbType = OracleDbType.Date},
                new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Clob},
                new OracleParameter {ParameterName = ":p_values_mode",  Value = valuesMode}
            };


            var oracleOperation = GetOracleOperation();
            oracleOperation.FetchSize *= 8;

            var tableData = await oracleOperation.GetFunctionRefCursorAsync(query, param, connection);
            
            oracleOperation.FetchSize /= 8;

            var totalHours = (dateEnd - dateBegin).TotalHours;

            for (int i = 0; i < totalHours + 1; i++)
            {
                var date = dateBegin.AddHours(i);
                var newDate = date.ToString("dd.MM.yy HH:mm");
                DataRow[] rows = tableData.Select($"date_time='{newDate}'");
                if (rows.Length == 0)
                {
                    var row = tableData.NewRow();
                    row["date_time"] = date.ToString("dd.MM.yy HH:mm");
                    /*row["date_stamp"] = date.ToString("dd.MM.yyyy HH:mm:ss");*/
                    row["date_stamp"] = date;
                    tableData.Rows.Add(row);
                }
            }

            var dtView = new DataView(tableData) {Sort = "date_stamp"};
            tableData = dtView.ToTable();

            foreach (var tag in tags)
            {
                tableData.Columns[tag].ColumnName = $"c_{tag}";
            }
            tableData.Columns["date_time"].ColumnName = "date";
            tableData.Columns.Remove("date_stamp");
            tableData.AcceptChanges();

            #region границы

            if (Parametrs.Plant == Plant.SAZ)
            {
                query = @"begin :cur := rbs_bl.rbs_technology.get_technology_limits(p_module_id => :p_module_id, p_params => :p_params); end;";

                param = new List<DbParameter>
                {
                    new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                    new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                    new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Varchar2},
                };
            }
            else
            {
                query = $@"begin :cur := rbs_bl.{package}.get_technology_limits_clob(p_module_id => :p_module_id, p_params => :p_params); end;";

                param = new List<DbParameter>
                {
                    new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                    new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                    new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Clob}
                };
            }

            var tableLimits = await OracleOperation.GetFunctionRefCursorAsync(query, param, connection);

            var listLimits = new List<dynamic>();

            foreach (var row in tableLimits.AsEnumerable())
            {
                dynamic item = new ExpandoObject();
                item.ParamId = Convert.ToString(row["id_param"]);
                item.ParamName = Convert.ToString(row["id_param"]);
                item.min_err = Operation.GetValueOrNull<double>(row["min_err"]);
                item.min_war = Operation.GetValueOrNull<double>(row["min_war"]);
                item.max_war = Operation.GetValueOrNull<double>(row["max_war"]);
                item.max_err = Operation.GetValueOrNull<double>(row["max_err"]);

                listLimits.Add(item);
            }

            #endregion

            dynamic model = new ExpandoObject();
            model.jsonData = tableData;
            model.jsonLimits = listLimits;

            model.jsonSummary = GenerateSummary(tags, tableData, listLimits);

            return Json(model);
        }

        private dynamic GenerateSummary(string[] tags, DataTable data, List<dynamic> list)
        {
            dynamic model = new ExpandoObject();
            if (data.Rows.Count == 0)
            {
                return model;
            }
            foreach (var tag in tags)
            {
                var values = data.AsEnumerable().Select(r => double.TryParse(r[$"c_{tag}"].ToString(), out var result) ? (double?)result : null).Where(val => val.HasValue).ToArray();
                if (values.Length == 0)
                {
                    continue;
                }

                var tagName = tag;
                //мин. знач.
                var name = $"min_{tagName}";
                var min = values.Min();
                ((IDictionary<string, object>)model)[name] = min;

                //макс. знач.
                name = $"max_{tagName}";
                var max = values.Max();
                ((IDictionary<string, object>)model)[name] = max;

                //сред. знач.
                name = $"avg_{tagName}";
                var avg = values.Average();
                ((IDictionary<string, object>)model)[name] = avg;

                //стандарт.откл
                name = $"stdev_{tagName}";
                double? stdev = null;
                if (values.Length > 1)
                {
                    double sumOfSquaresOfDifferences = Convert.ToDouble(values.Select(v => (v - avg) * (v - avg)).Sum());
                    stdev = Math.Sqrt(sumOfSquaresOfDifferences / (values.Length - 1));
                }
                ((IDictionary<string, object>)model)[name] = stdev;

                //кол-во знач.
                name = $"cnt_{tagName}";
                int cnt = values.Length;
                ((IDictionary<string, object>)model)[name] = cnt;

                //откл по ТР <
                name = $"cnt_min_err_{tagName}";
                var limit = list.FirstOrDefault(vv => vv.ParamId == tag);
                int? cntMinErr = null;
                if (limit != null && limit.min_err != null)
                {
                    cntMinErr = values.Count(v => v < Convert.ToDouble(limit.min_err));
                }

                ((IDictionary<string, object>)model)[name] = cntMinErr;

                //откл по ТР >
                name = $"cnt_max_err_{tagName}";
                //int cntMaxErr = enumerable.Count(v => v.ParamId == tag.Item1 && (double)v.Value > Convert.ToDouble((list.First(vv => vv.ParamId == tag.Item1)).max_err));
                //var maxErr = list.First(vv => vv.ParamId == tag.Item1).max_err;
                int? cntMaxErr = null;
                if (limit != null && limit.max_err != null)
                {
                    cntMaxErr = (int?)values.Count(v => v > Convert.ToDouble(limit.max_err));
                }

                ((IDictionary<string, object>)model)[name] = cntMaxErr;

                //Кол-во отклонений
                name = $"cnt_err_{tagName}";
                int? cntErr = cntMinErr + cntMaxErr;
                ((IDictionary<string, object>)model)[name] = cntErr;

                //% откл
                name = $"perc_err_{tagName}";
                var percErr = cntErr == 0 ? 0.0 : (Operation.GetValueOrNull<double>(cntErr) * 100 / cnt);
                ((IDictionary<string, object>)model)[name] = percErr;
            }

            return model;
        }

    }

    public static class LinqExtenions
    {

        public static Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>> Pivot<TSource, TFirstKey, TSecondKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TFirstKey> firstKeySelector, Func<TSource, TSecondKey> secondKeySelector, Func<IEnumerable<TSource>, TValue> aggregate)
        {
            var retVal = new Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>();

            var l = source.ToLookup(firstKeySelector);
            foreach (var item in l)
            {
                var dict = new Dictionary<TSecondKey, TValue>();
                retVal.Add(item.Key, dict);
                var subdict = item.ToLookup(secondKeySelector);
                foreach (var subitem in subdict)
                {
                    dict.Add(subitem.Key, aggregate(subitem));
                }
            }

            return retVal;
        }
    }
}
