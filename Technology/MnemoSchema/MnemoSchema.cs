using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RBS_Core.Helpers;
using RBS_Core.Helpers.Utils;
using RBS_Core.Models;
using RBS_Helpers;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController
    {
        public PartialViewResult MnemoSchema()
        {
            return PartialView("MnemoSchema");
        }


        [HttpGet]
        public JsonResult GetMnemo()
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

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

            var query = $@"begin :cur := rbs_bl.{package}.get_plc_mnemo(p_module_id => :p_module_id); end;";
            
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_module_id", Value = moduleId},
            };
            
            var table = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }

        private byte[] GetMnemo(string blobId)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            var query = $@"begin :cur := rbs_bl.{package}.get_plc_mnemo_blob(:p_blob_id); end;";
            
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_blob_id", Value = blobId},
            };
            
            var table = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            byte[] result = { };
            if (table.Rows.Count > 0)
            {
                var resultByte = table.Rows[0][0];
                if (resultByte is not DBNull)
                {
                    result = (byte[])(resultByte);
                }
            }

            return result;
        }

        #region Download/upload svg

        [HttpGet]
        public FileResult GetMnemoFile(string blobId)
        {
            return File(GetMnemo(blobId), "image/svg+xml", "scheme.svg");
        }

        
        [HttpGet]
        public JsonResult DownloadMnemoFile(string blobId)
        {
            var file = GetMnemo(blobId);

            var result = Convert.ToBase64String(file);
            string handle = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(handle, result.Replace("\"", ""));
            
            var json = new JsonResult(new
            {
                Success = true,
                FileGuid = handle,
                MimeType = "image/svg",
                FileName = Uri.EscapeUriString($"scheme_{blobId}.svg"),
            });

            return json;
        }
        
        [HttpPost]
        [AllowAnonymous]
        public ContentResult UploadMnemoFile(string mixerId)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var file = Request.Form.Files[0];

            if (file == null)
            {
                return Content(false.ToString());
            }

            //string fileName = file.FileName;

            byte[] byteArray;
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byteArray = ms.GetBuffer();
            }

            #region strSql

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            string sql = $@"begin rbs_bl.{package}.upload_plc_mnemo_blob(p_mixer_id => :p_mixer_id, p_file => :p_file, p_user_id => :p_user_id); end;";

            #endregion

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_mixer_id", Value = mixerId},
                //new OracleParameter {ParameterName = ":p_file_name", Value = fileName},
                new OracleParameter {ParameterName = ":p_file", Value = byteArray, OracleDbType = OracleDbType.Blob},
                new OracleParameter {ParameterName = ":p_user_id", Value = HttpContext.Session.Get<UserModel>("UserInfo").UserId}
            };

            var result = OracleOperation.RunSql(sql, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Content(result.ToString());
        }

        #endregion


        
        [HttpGet]
        public JsonResult GetMnemoDetail(string idMnemo)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            var query = $@"begin :cur := rbs_bl.{package}.get_plc_mnemo_detail(p_id_mnemo => :p_id_mnemo); end;";
            
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_id_mnemo", Value = idMnemo},
            };
            
            var table = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }
        [HttpGet]
        public JsonResult GetMnemoData(string parameters)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            var query = $@"begin :cur := rbs_bl.{package}.get_plc_mnemo_data(:p_params); end;";
            
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_params", Value = parameters},
            };
            
            var table = OracleOperation.GetFunctionRefCursor(query, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }

        [HttpGet]
        public async Task<JsonResult> GetMixerGagesState(string mixerId)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            string query = $@"begin :cur := rbs_bl.{package}.get_mixer_gages_state(p_mixer_id => :p_mixer_id); end;";
            
            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor},
                new OracleParameter {ParameterName = ":p_mixer_id", Value = mixerId},
            };
            
            var table = await OracleOperation.GetFunctionRefCursorAsync(query, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ContentResult> ChangeGagesState(int state, DateTime dateBegin, DateTime dateEnd, string request, string note, string array)
        {
            var moduleId = HttpContext.Session.Get<int?>("SubModuleId");

            #region strSql

            var package = "rbs_technology";
            if (Parametrs.Plant == Plant.IAZ)
            {
                if (moduleId == 101 || moduleId == 305)
                {
                    package = "rbs_technology_old";
                }
            }

            string sql = $@"begin rbs_bl.{package}.change_gages_state(p_state => :p_state, p_date_begin => :p_date_begin, p_date_end => :p_date_end, p_request => :p_request, p_note => :p_note, p_array => :p_array, p_user_id => :p_user_id); end;";

            #endregion

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_state", Value = state, OracleDbType = OracleDbType.Int32},
                new OracleParameter {ParameterName = ":p_date_begin", Value = dateBegin, OracleDbType = OracleDbType.Date},
                new OracleParameter {ParameterName = ":p_date_end", Value = dateEnd, OracleDbType = OracleDbType.Date},
                new OracleParameter {ParameterName = ":p_request", Value = request, OracleDbType = OracleDbType.Varchar2},
                new OracleParameter {ParameterName = ":p_note", Value = note, OracleDbType = OracleDbType.Varchar2},
                new OracleParameter {ParameterName = ":p_array", Value = array, OracleDbType = OracleDbType.Varchar2},
                new OracleParameter {ParameterName = ":p_user_id", Value = HttpContext.Session.Get<UserModel>("UserInfo").UserId}
            };

            var result = await OracleOperation.RunSqlAsync(sql, param, Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            return Content(result.ToString());
        }



        [HttpGet]
        public async Task<JsonResult> GetTagPredictionType()
        {
            var userInfo = HttpContext.Session.Get<UserModel>("UserInfo");

            if (userInfo.Roles.Count(r => r.RoleId == 3201) == 0)
            {
                return Json(null);
            }

            var test = userInfo.Roles.Count(r => r.RoleId == 3201);

            #region strSql

            string sql = $@"begin :cur := rbs_bl.rbs_technology.get_tag_prediction_type(); end;";

            #endregion

            var param = new List<DbParameter>
            {
                new OracleParameter { ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor },
            };

            var table = await OracleOperation.GetFunctionRefCursorAsync(sql, param, Static.ConnectionRepository.GENDB_DWFOUNDRY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }

        [HttpGet]
        public async Task<JsonResult> GetTagPrediction(string equipmentId, int showAll, string type)
        {
            var userInfo = HttpContext.Session.Get<UserModel>("UserInfo");

            if (userInfo.Roles.Count(r => r.RoleId == 3201) == 0)
            {
                return Json(null);
            }

            var test = userInfo.Roles.Count(r => r.RoleId == 3201);

            #region strSql

            string sql = $@"begin :cur := rbs_bl.rbs_technology.get_tag_prediction(p_equipment_id => :p_equipment_id, p_show_all => :p_show_all, p_type => :p_type); end;";

            #endregion

            var param = new List<DbParameter>
            {
                new OracleParameter { ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor },
                new OracleParameter { ParameterName = ":p_equipment_id", Value = equipmentId },
                new OracleParameter { ParameterName = ":p_show_all", Value = showAll },
                new OracleParameter { ParameterName = ":p_type", Value = type, IsNullable = true},
            };

            var table = await OracleOperation.GetFunctionRefCursorAsync(sql, param, Static.ConnectionRepository.GENDB_DWFOUNDRY.Default);

            return Json(table, new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() });
        }

    }
}
