using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using RBS_Core.Helpers;
using Sibinfosoft.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController
    {
        public PartialViewResult GraphOpers()
        {
            return PartialView("GraphOpers");
        }

        public JsonResult GetTechnology(string array)
        {
            var tags = array.Split(',').ToList();

            string output = string.Join(",", tags);

            var query = @"begin :cur := rbs_bl.rbs_technology.get_technology_data_online(p_params => :p_params); end;";
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Clob}
            };

            var tableData = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHN.Default);

            return Json(tableData);
        }

        public JsonResult GetTechnologyLim(string array)
        {
            var tags = array.Split(',').ToList();

            string output = string.Join(",", tags);

            var query = @"begin :cur := rbs_bl.rbs_technology.get_technology_limits(p_params => :p_params); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_params", Value = output, OracleDbType = OracleDbType.Varchar2}
            };

            var tableLimits = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHN.Default);

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

            return Json(listLimits);
        }
    }
}
