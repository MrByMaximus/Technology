using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using Sibinfosoft.Utils;
using RBS_Core.Helpers;
using RBS_Core.Models;
using RBS_Helpers;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController
    {
        public PartialViewResult RealTime()
        {
            var area = RouteData.Values["area"]?.ToString() ?? "RBS";

            var date = HttpContext.Session.Get<DateTime?>("Date");
            if (date != null)
            {
                HttpContext.Session.Set("Date", (DateTime)date);
            }
            else
            {
                HttpContext.Session.Set("Date", DateTime.Now);
            }


            return PartialView("RealTime");
        }

        public JsonResult GetTechnologyRealTimeData(string array)
        {

            var tags = array.Split(',').ToList();


            string output = string.Join(",", tags);

            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var query = @"begin :cur := rbs_bl.rbs_technology.get_technology_real_time_data(p_module_id => :p_module_id, p_plant_id => :p_plant_id, p_params => :p_params); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},               
                new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
                new OracleParameter {ParameterName = ":p_plant_id", Value = RBS_Helpers.Parametrs.PlantId},
                new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Clob}
            };

            var connection = Static.ConnectionRepository.RBS_TECHN.Default;
            if (RBS_Helpers.Parametrs.Plant == Plant.IAZ && moduleId == 101)
            {
                connection = Static.ConnectionRepository.RBS_USER.Default;
            }

            var tableData = OracleOperation.GetFunctionRefCursor(query, param, connection);


            //dynamic jsonData = new ExpandoObject();
            //model.jsonData = tableData;

            return Json(tableData);
        }
    }
}