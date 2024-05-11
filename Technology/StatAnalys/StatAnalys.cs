using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RBS_Core.Features.Technology.StatAnalys;
using RBS_Core.Helpers;
using RBS_Core.Models;
using RBS_Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController
    {
        public PartialViewResult StatAnalys()
        {
            return PartialView("StatAnalys");
        }

        public ContentResult GetStatCastplant()
        {
            var sqlText = @"select * from table (RBS_BL.PG_SPC.GET_stat_castplant(:asmelter_id))";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":asmelter_id", Value = RBS_Helpers.Parametrs.PlantId, OracleDbType = OracleDbType.Decimal }
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatSeries()
        {
            var sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_series)";

            var table = OracleOperation.GetData(sqlText, null, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatMarks(int seriesId, int castPlantId, DateTime date_Beg, DateTime date_End)
        {
            const string query = @"select * from table(RBS_BL.PG_SPC.GET_stat_marks(:aseries_id, :castplant_id_, :D_B_, :D_E_))";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":aseries_id", Value = seriesId, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":castplant_id_", Value = castPlantId, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":D_B_", Value = date_Beg, OracleDbType = OracleDbType.Date },
                new OracleParameter {ParameterName = ":D_E_", Value = date_End, OracleDbType = OracleDbType.Date }
            };

            var sqlResult = OracleOperation.GetData(query, param);

            return Content(JsonConvert.SerializeObject(sqlResult));
        }

        public ContentResult GetStatStds(int castplant_id, int mark_id, DateTime date_Beg, DateTime date_End)
        {
            const string query = @"select * from table(RBS_BL.PG_SPC.GET_stat_stds(:castplant_id_, :mark_id_, :D_B_, :D_E_))";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":castplant_id_", Value = castplant_id, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":mark_id_", Value = mark_id, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":D_B_", Value = date_Beg, OracleDbType = OracleDbType.Date },
                new OracleParameter {ParameterName = ":D_E_", Value = date_End, OracleDbType = OracleDbType.Date }
            };

            var result = OracleOperation.GetData(query, param);

            return Content(JsonConvert.SerializeObject(result));
        }

        public ContentResult GetStatSize(int std_id)
        {
            const string query = @"select * from table(RBS_BL.PG_SPC.GET_stat_size(:std_id_))";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":std_id_", Value = std_id, OracleDbType = OracleDbType.Int32 }
            };

            var result = OracleOperation.GetData(query, param);

            return Content(JsonConvert.SerializeObject(result));
        }

        public ContentResult GetStatMelts(int castplant_id, int mark_id, int stds_id, DateTime D_B, DateTime D_E)
        {
            var sqlText = @"select t1.* from table(RBS_BL.PG_SPC.GET_stat_melts (:castplant_id_, :mark_id_, :stds_id_, :D_B_, :D_E_)) t1";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":castplant_id_", Value = castplant_id },
                new OracleParameter {ParameterName = ":mark_id_", Value = mark_id },
                new OracleParameter {ParameterName = ":stds_id_", Value = stds_id },
                new OracleParameter {ParameterName = ":D_B_", Value = D_B },
                new OracleParameter {ParameterName = ":D_E_", Value = D_E }
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatResult(int casthouse, DateTime d_b, DateTime d_e, int std_id, int mark_id, int melt_id, int typeId, int series_id, int smena_id, string size_id)
        {
            var sqlText = @"select * FROM table(RBS_BL.PG_SPC.GET_stat_result(:casthouse_id_, :D_B_, :D_E_, :std_id_, :mark_id_, :melt_id_, :pTypeId, :pSamplingId, :pSeries, :pSmenaId, :size_id_))";

            DataTable table;
            try
            {
                var sqlParam = new List<DbParameter>
                {
                    new OracleParameter { ParameterName = ":casthouse_id_", Value = casthouse, OracleDbType = OracleDbType.Int32 },
                    new OracleParameter { ParameterName = ":D_B_", Value = d_b, OracleDbType = OracleDbType.Date },
                    new OracleParameter { ParameterName = ":D_E_", Value = d_e, OracleDbType = OracleDbType.Date },
                    new OracleParameter { ParameterName = ":std_id_", Value = std_id, OracleDbType = OracleDbType.Int32 },
                    new OracleParameter { ParameterName = ":mark_id_", Value = mark_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":melt_id_", Value = melt_id, OracleDbType = OracleDbType.Int32, IsNullable = true},
                    new OracleParameter { ParameterName = ":pTypeId", Value = typeId == 1 || typeId == 2 ? 16 : typeId, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSamplingId", Value = typeId == 2 ? 2 : -1, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSeries", Value = series_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSmenaId", Value = smena_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":size_id_", Value = size_id, OracleDbType = OracleDbType.Varchar2}
                };

                table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);
            }
            catch
            {
                var sqlDelete = @"begin RBS_BL.PG_SPC.ClearTable; commit; end;";
                OracleOperation.RunSql(sqlDelete, null, Static.ConnectionRepository.RBS_USER.Default);

                System.Threading.Thread.Sleep(8000);

                var sqlParam1 = new List<DbParameter>
                {
                    new OracleParameter { ParameterName = ":casthouse_id_", Value = casthouse, OracleDbType = OracleDbType.Int32 },
                    new OracleParameter { ParameterName = ":D_B_", Value = d_b, OracleDbType = OracleDbType.Date },
                    new OracleParameter { ParameterName = ":D_E_", Value = d_e, OracleDbType = OracleDbType.Date },
                    new OracleParameter { ParameterName = ":std_id_", Value = std_id, OracleDbType = OracleDbType.Int32 },
                    new OracleParameter { ParameterName = ":mark_id_", Value = mark_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":melt_id_", Value = melt_id, OracleDbType = OracleDbType.Int32, IsNullable = true},
                    new OracleParameter { ParameterName = ":pTypeId", Value = typeId == 1 || typeId == 2 ? 16 : typeId, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSamplingId", Value = typeId == 2 ? 2 : -1, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSeries", Value = series_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":pSmenaId", Value = smena_id, OracleDbType = OracleDbType.Int32},
                    new OracleParameter { ParameterName = ":size_id_", Value = size_id, OracleDbType = OracleDbType.Varchar2}
                };

                table = OracleOperation.GetData(sqlText, sqlParam1, Static.ConnectionRepository.RBS_USER.Default);
            }

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatTable(int idStat, double avg, double l_min, double l_max, int session, int samplingId, int casthouseId, double sigmaPlus, double sigmaMinus)
        {
            var sqlText = @"select t1.*, :avg_ avg_t1, :L_MIN_ l_min_t1, :L_MAX l_max_t1, :SIGMA_PLUS sigma_plus, :SIGMA_MINUS sigma_minus from table(RBS_BL.PG_SPC.GET_stat_table (:aid_stat_el, :session_, :samplingId, :castHouseId)) t1";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":L_MAX", Value = l_max },
                new OracleParameter {ParameterName = ":L_MIN_", Value = l_min },
                new OracleParameter {ParameterName = ":avg_", Value = avg },
                new OracleParameter {ParameterName = ":aid_stat_el", Value = idStat, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":session_", Value = session /*HttpContext.Session.Get<int?>("Sessions")*/ },
                new OracleParameter {ParameterName = ":samplingId", Value = samplingId == 1 || samplingId == 2 ? 16 : samplingId},
                new OracleParameter {ParameterName = ":castHouseId", Value = casthouseId},
                new OracleParameter {ParameterName = ":SIGMA_PLUS", Value = sigmaPlus},
                new OracleParameter {ParameterName = ":SIGMA_MINUS", Value = sigmaMinus}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatTableChartInMelt(int idStat, double avg, double l_min, double l_max, int session, int idMelt, int samplingId)
        {
            var sqlText = @"select t1.*, :avg_ avg_t1, :L_MIN_ l_min_t1, :L_MAX l_max_t1 from table(RBS_BL.PG_SPC.GET_stat_table_chart_in_melt (:aid_stat_el, :session_, :samplingId, :meltId)) t1";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":L_MAX", Value = l_max },
                new OracleParameter {ParameterName = ":L_MIN_", Value = l_min },
                new OracleParameter {ParameterName = ":avg_", Value = avg },
                new OracleParameter {ParameterName = ":aid_stat_el", Value = idStat, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":session_", Value = session /*HttpContext.Session.Get<int?>("Sessions")*/ },
                new OracleParameter {ParameterName = ":samplingId", Value = samplingId == 1 || samplingId == 2 ? 16 : samplingId},
                new OracleParameter {ParameterName = ":meltId", Value = idMelt}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        public ContentResult GetStatTablePPM(int idStat, int session, double sigmaPlus, double sigmaMinus)
        {
            var sqlText = $@"select t1.*, :SIGMA_PLUS sigma_plus, :SIGMA_MINUS sigma_minus from table(RBS_BL.PG_SPC.GET_stat_table_PPM  (:aid_stat_el, :session_)) t1";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":aid_stat_el", Value = idStat },
                new OracleParameter {ParameterName = ":session_", Value = session },
                new OracleParameter {ParameterName = ":SIGMA_PLUS", Value = sigmaPlus},
                new OracleParameter {ParameterName = ":SIGMA_MINUS", Value = sigmaMinus}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            return Content(JsonConvert.SerializeObject(table));
        }

        private DataTable GetTable(int idStat, int session, int samplingId, int casthouseId)
        {
            var sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table (:aid_stat_el, :session_, :samplingId, :castHouseId)) t1";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":aid_stat_el", Value = idStat, OracleDbType = OracleDbType.Decimal },
                new OracleParameter {ParameterName = ":session_", Value = session },
                new OracleParameter {ParameterName = ":samplingId", Value = samplingId == 1 || samplingId == 2 ? 16 : samplingId },
                new OracleParameter {ParameterName = ":castHouseId", Value = casthouseId}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            table.TableName = "Стат.анализ";

            table.Columns.Remove("ID_STAT_EL");
            table.Columns.Remove("SORTNUM");
            table.Columns.Remove("ID_MELT");

            table.Columns["NAME_EL"].ColumnName = "Наименование элемента";
            table.Columns["VALUE_Y"].ColumnName = "Значение";
            table.Columns["L_MIN"].ColumnName = "Мин.";
            table.Columns["L_MAX"].ColumnName = "Макс.";
            table.Columns["NOTE"].ColumnName = "Номер плавки";

            return table;
        }

        public IActionResult GetExcel(int idStat, int session, int samplingId, int casthouseId)
        {
            var wb = new XLWorkbook();

            var dataTable = GetTable(idStat, session, samplingId, casthouseId);

            wb.Worksheets.Add(dataTable);

            var stream = new MemoryStream();
            wb.SaveAs(stream);

            string handle = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(handle, Convert.ToBase64String(stream.ToArray()));

            return new JsonResult(new
            {
                Success = true,
                FileGuid = handle,
                MimeType = System.Net.Mime.MediaTypeNames.Application.Octet,
                FileName = $"Стат.анализ - {"построение"}.xlsx"
            });
        }

        private DataTable GetTableMiniTab(int session, string spec, int samplingId, int casthouseId)
        {
            string sqlText = null;

            switch (samplingId)
            {
                case 1:
                    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Express (:session_, :spec, :casthouseId)) t1";
                    break;
                //case 2:
                //    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Product (:session_, :spec)) t1";
                //    break;
                //throw new Exception("Выгрузка по товарным пробам еще не реализована");
                case 2:
                    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Allow (:session_, :spec, :casthouseId)) t1";
                    break;
                default:
                    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Other (:session_, :spec, :casthouseId)) t1";
                    break;
            }


            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":session_", Value = session },
                new OracleParameter {ParameterName = ":spec", Value = spec },
                new OracleParameter {ParameterName = ":castHouseId", Value = casthouseId}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            var data2 = table.AsEnumerable().Select(x => new {
                MeltName = x.Field<string>("melt_name"),
                El = x.Field<string>("NAME_EL"),
                Val = x.Field<decimal>("VALUE"),
                ProdAn = x.Field<string>("PROD_AN"),
                Spec = x.Field<string>("SPEC"),
                Date_Beg = x.Field<DateTime?>("Date_Beg").ToString(),
                Date_End = x.Field<DateTime?>("Date_End").ToString(),
                Smena = x.Field<string>("Smena"),
                Size_Name = x.Field<string>("Size_Name"),
                User_Name = x.Field<string>("User_Name")
            });

            DataTable pivotDataTable = data2.XToPivotTable(
                 item => item.El,
                item => new { item.Date_Beg, item.Date_End, item.MeltName, item.Spec, item.Smena, item.ProdAn, item.Size_Name, item.User_Name },
                items => items.Any() ? items.Select(x => x.Val).FirstOrDefault() : 0);

            //DataTable pivotDataTable = data2.ToPivotTable(
            //     item => item.El,
            //    item => item.MeltName,
            //    items => items.Any() ? items.Select(x => x.Val).FirstOrDefault() : 0);


            pivotDataTable.TableName = "Выгрузка MiniTab";

            pivotDataTable.Columns["Date_Beg"].ColumnName = "Дата отправки";
            pivotDataTable.Columns["Date_End"].ColumnName = "Дата анализа";
            pivotDataTable.Columns["meltName"].ColumnName = "№ Плавки";
            pivotDataTable.Columns["SPEC"].ColumnName = "Сплав/спецификация";
            pivotDataTable.Columns["PRODAN"].ColumnName = "Номер пробы";
            pivotDataTable.Columns["Smena"].ColumnName = "Смена";
            pivotDataTable.Columns["Size_Name"].ColumnName = "Сечение";
            pivotDataTable.Columns["User_Name"].ColumnName = "Пользователь";

            return pivotDataTable;
        }

        private DataTable GetTableMiniTabTechnParam(int session)
        {
            string sqlText = @"select * from table(RBS_BL.PG_SPC.GET_MiniTab_Techn_Param (:session_)) t1";


            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":session_", Value = session }
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            var data2 = table.AsEnumerable().Select(x => new {
                MeltName = x.Field<string>("melt_name"),
                El = x.Field<string>("NAME_EL"),
                Val = x.Field<decimal?>("VALUE"),
                Date_Begin = x.Field<DateTime?>("DATE_BEGIN").ToString(),
                Dt_Begin = x.Field<DateTime?>("DT_BEGIN").ToString(),
                Dt_End = x.Field<DateTime?>("DT_END").ToString(),
                Size_Name = x.Field<string>("Size_Name").ToString(),
            });

            DataTable pivotDataTable = data2.XToPivotTable(
                 item => item.El,
                item => new { item.Date_Begin, item.Dt_Begin, item.Dt_End, item.MeltName, item.Size_Name },
                items => items.Any() ? items.Select(x => x.Val).FirstOrDefault() : 0);

            pivotDataTable.TableName = "Выгрузка MiniTab";

            pivotDataTable.Columns["DT_BEGIN"].ColumnName = "Начало плавки";
            pivotDataTable.Columns["DT_END"].ColumnName = "Конец плавки";
            pivotDataTable.Columns["meltName"].ColumnName = "№ Плавки";
            pivotDataTable.Columns["DATE_BEGIN"].ColumnName = "Дата";
            pivotDataTable.Columns["Size_Name"].ColumnName = "Сечение";

            return pivotDataTable;
        }

        private DataTable GetTableMiniTabProduct(int session, string spec, int samplingId, int chProd, int casthouseId)
        {
            string sqlText = null;
            switch (chProd)
            {
                case 1:
                    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Product (:session_, :spec, :casthouseId)) t1";
                    break;
                case 2:
                    sqlText = @"select * from table(RBS_BL.PG_SPC.GET_stat_table_MiniTab_Chem (:session_, :spec, :casthouseId)) t1";
                    break;
            }


            var sqlParam = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":session_", Value = session },
                new OracleParameter {ParameterName = ":spec", Value = spec },
                new OracleParameter {ParameterName = ":castHouseId", Value = casthouseId}
            };

            var table = OracleOperation.GetData(sqlText, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            var data2 = table.AsEnumerable().Select(x => new {
                MeltName = x.Field<string>("melt_name"),
                El = x.Field<string>("NAME_EL"),
                Val = x.Field<decimal?>("VALUE"),
                ProdAn = x.Field<string>("PROD_AN"),
                Spec = x.Field<string>("SPEC"),
                Date_Beg = x.Field<DateTime?>("Date_Beg").ToString(),
                Date_End = x.Field<DateTime?>("Date_End").ToString(),
                Smena = x.Field<string>("Smena"),
                Size_Name = x.Field<string>("Size_Name"),
                User_Name = x.Field<string>("User_Name")
            });

            DataTable pivotDataTable = data2.XToPivotTable(
                 item => item.El,
                item => new { item.Date_Beg, item.Date_End, item.MeltName, item.Spec, item.Smena, item.ProdAn, item.Size_Name, item.User_Name },
                items => items.Any() ? items.Select(x => x.Val).FirstOrDefault() : 0);


            pivotDataTable.TableName = "Выгрузка MiniTab";

            pivotDataTable.Columns["Date_Beg"].ColumnName = "Дата отправки";
            pivotDataTable.Columns["Date_End"].ColumnName = "Дата анализа";
            pivotDataTable.Columns["meltName"].ColumnName = "№ Плавки";
            pivotDataTable.Columns["SPEC"].ColumnName = "Сплав/спецификация";
            pivotDataTable.Columns["PRODAN"].ColumnName = "Номер пробы";
            pivotDataTable.Columns["Smena"].ColumnName = "Смена";
            pivotDataTable.Columns["Size_Name"].ColumnName = "Сечение";
            pivotDataTable.Columns["User_Name"].ColumnName = "Пользователь";

            if (chProd == 1 && Parametrs.PlantId == 4)
            {
                string[] array = pivotDataTable.Columns.OfType<DataColumn>().Select(k => k.ToString()).ToArray();

                if (!array.Contains("Температура воды"))
                {
                    pivotDataTable.Columns.Add("Температура воды");
                }

                if (!array.Contains("Скорость литья"))
                {
                    pivotDataTable.Columns.Add("Скорость литья");
                }

                if (!array.Contains("Температура металла в миксере"))
                {
                    pivotDataTable.Columns.Add("Температура металла в миксере");
                }

                if (!array.Contains("Температура металла в разливочной чаше"))
                {
                    pivotDataTable.Columns.Add("Температура металла в разливочной чаше");
                }

                if (!array.Contains("Температура заготовки"))
                {
                    pivotDataTable.Columns.Add("Температура заготовки");
                }
            }

            return pivotDataTable;
        }

        public IActionResult GetExcelTableMiniTab(int session, string spec, int samplingId, int chProd, int casthouseId)
        {
            var wb = new XLWorkbook();

            DataTable dataTable = null;

            if (samplingId == 12)
            {
                dataTable = GetTableMiniTabProduct(session, spec, samplingId, chProd, casthouseId);
            }
            else if (samplingId == 1000)
            {
                dataTable = GetTableMiniTabTechnParam(session);
            }
            else
            {
                dataTable = GetTableMiniTab(session, spec, samplingId, casthouseId);
            }

            wb.Worksheets.Add(dataTable);

            var stream = new MemoryStream();
            wb.SaveAs(stream);

            string handle = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(handle, Convert.ToBase64String(stream.ToArray()));

            return new JsonResult(new
            {
                Success = true,
                FileGuid = handle,
                MimeType = System.Net.Mime.MediaTypeNames.Application.Octet,
                FileName = $"Стат.анализ - {"MiniTab"}.xlsx"
                //FileName = samplingId == 1 ? $"Стат.анализ - {"MiniTab все экспресс пробы"}.xlsx" : samplingId == 3 ? $"Стат.анализ - {"MiniTab разрешающие экспресс пробы"}.xlsx" : samplingId == 2 ? $"Стат.анализ - {"MiniTab товарные пробы"}.xlsx" : ""
            });
        }

        public ContentResult DataParam(int mark_id, int castequipment_Id, int ts, int? minval, int? maxval)
        {
            var result = true;

            var strSql = @"begin RBS_BL.PG_SPC.DATAPARAM(:pMARK_ID, :pCASTEQUIPMENT_ID, :pTS, :pMINVAL, :pMAXVAL, :pUSERID); end;";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter { ParameterName = ":pMARK_ID", Value = mark_id},
                new OracleParameter { ParameterName = ":pCASTEQUIPMENT_ID", Value = castequipment_Id},
                new OracleParameter { ParameterName = ":pTS", Value = ts},
                new OracleParameter { ParameterName = ":pMINVAL", Value = minval},
                new OracleParameter { ParameterName = ":pMAXVAL", Value = maxval},
                new OracleParameter { ParameterName = ":pUSERID", Value = HttpContext.Session.Get<UserModel>("UserInfo").UserId},
            };

            var sqlResult = OracleOperation.RunSql(strSql, sqlParam, Static.ConnectionRepository.RBS_USER.Default);

            if (!sqlResult)
            {
                result = false;
            }

            return Content(result.ToString());
        }
    }
}
