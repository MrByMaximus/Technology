using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oracle.ManagedDataAccess.Client;
using RBS_Core.Helpers;
using RBS_Core.Models;
using Sibinfosoft.Utils;
using Sibinfosoft.Utils.DbOperation;
using Svg;

namespace RBS_Core.Areas.Aluminum.Lo.Technology
{
    [AllowAnonymous]
    public class ProcessThermocoupleController : Controller
    {
        [Route("api/Lo/Technology/CheckMail")]
        public async Task<ActionResult> CheckMail(string path)
        {
            var result = string.Empty;
            
            var baseUrl = HttpContext.Request.Host.ToString();
            var http = HttpContext.Request.IsHttps ? "https" : "http";
            var url = $"{http}://{baseUrl}/{path}";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var model = JsonConvert.DeserializeObject<dynamic>(content, new ExpandoObjectConverter());
                        if (model != null && model.Emails?.Count > 0)
                        {
                            result = model?.Emails[0].Body?.ToString();
                        }
                    }
                }
                else
                {
                    result = response.StatusCode.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "Нет данных";
            }
            
            return Content(result, "text/html; charset=UTF-8");
        }


        #region Рассылка по росту температуры в точках миксера в течении n дней (sysdate-3)

        [Route("api/Lo/Technology/ProcessThermocoupleCheckTemperatureGrowth")]
        public async Task<string> CheckTemperatureGrowth()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckTemperatureGrowth";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_check_growth(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion

            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += @"
            <br/><br/>
            <div class='body-end'>
                <u><span>Ответственному сотруднику рекомендуется сделать следующие действия:</span></u>
                    <ol style='padding: 0;'>
		                <li style='margin:0;'>При возможности проверить показания датчиков мобильным пирометром;</li>
		                <li>Выполнить осмотр футеровки подины миксера на предмет отсутствия повреждений;</li>
	   	                <li>При отсутствии повреждений футеровки - подать заявку в службу ИСО, к заявке приложить скриншоты из письма;</li>
	   	                <ol type='a'>
			                <li>Дождаться обратной связи от службы ИСО:</li>
			                <ol type='i'>
				                <li>Если неисправность в зоне ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
				                <li>Если неисправность в не зоны ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), к заявке приложить скриншоты;</li>
                                    <ol type='1'>
					                    <li>Заполнить все обязательные поля (поля «Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП»);</li>
					                    <li>Приложить скриншоты.</li>
				                    </ol>
                                    <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
			                </ol>
		                </ol>
	                </ol>

	                <div><u><span>ВАЖНО! При остановке технологического процесса и/или срыва сменного плана производства по предприятию, по причине срабатывания системы контроля температуры подины миксера о превышении показателей температуры, организуйте:</span></u></div>
	                <ul style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Создание сообщения о происшествии в Системе информирования о ЧП согласно ""Регламенту РАМ-РГ-3.5-05 информирования руководства о чрезвычайных происшествиях в работе Компании"" (Распоряжение № РАМ-21-Р921 от 28.10.2021);</li>
		                <li>Фиксацию сообщения о внеплановой остановке оборудования в системе SAP согласно распоряжения № РАМ-21-Р480 от 31.05.2021;</li>
		                <li>Проведение расследования ЧП с согласованием ""Акта технического расследования инцидента"" в PayDox согласно распоряжения № РАМ-19-Р337 от 19.08.2019.</li>
	                </ul>
                    <br/>
                    <div><u><span>Рассылка осуществляется в случае если за последние 3 дня есть точки где наблюдается стабильный рост температуры.</span></u></div>
                    <br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	            </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, наблюдается стабильный рост температуры в течении 3-х дней!", body, "Рост температуры в течении 3-х дней.");


            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            //получаем список gage_ids чтобы их записать в логи для последующей проверки что по ним уже была рассылка (по хорошему как-то переделать)
            string gageIds = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids"].ToString()));
            string gageIdsHit = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_hit"].ToString()));
            string gageIdsJson = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_json"].ToString()));

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message, gage_ids = gageIds, gage_ids_hit = gageIdsHit };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);
            await LogTechEvent(2, $"[{gageIdsJson}]");


            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }

            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion

        #region Рассылка по превышении температуры в точках миксера

        [Route("api/Lo/Technology/ProcessThermocoupleCheckTemperature/{tempLimitMin?}/{tempLimitMax?}/{tempCheckLog?}")]
        public async Task<string> CheckTemperature(int? tempLimitMin, int? tempLimitMax, int? tempCheckLog)
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckTemperature";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_check_temp(p_temp_limit_min => :p_temp_limit_min, p_temp_limit_max => :p_temp_limit_max, p_temp_check_log => :p_temp_check_log); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
                new OracleParameter {ParameterName = ":p_temp_limit_min", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = tempLimitMin},
                new OracleParameter {ParameterName = ":p_temp_limit_max", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = tempLimitMax},
                new OracleParameter {ParameterName = ":p_temp_check_log", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = tempCheckLog},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion

            var limitMin = tempLimitMin ?? 250;
            var limitMax = tempLimitMax ?? 1200;
            
            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"
            <br/><br/>
            <div class='body-end'>
                <u><span>Ответственному сотруднику рекомендуется сделать следующие действия:</span></u>
                    <ol style='padding: 0;'>
		                <li style='margin:0;'>При возможности проверить показания датчиков мобильным пирометром;</li>
		                <li>Выполнить осмотр футеровки подины миксера на предмет отсутствия повреждений;</li>
	   	                <li>При отсутствии повреждений футеровки - подать заявку в службу ИСО, к заявке приложить скриншоты из письма;</li>
	   	                <ol type='a'>
			                <li>Дождаться обратной связи от службы ИСО:</li>
			                <ol type='i'>
				                <li>Если неисправность в зоне ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
				                <li>Если неисправность в не зоны ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), к заявке приложить скриншоты;</li>
                                    <ol type='1'>
					                    <li>Заполнить все обязательные поля (поля «Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП»);</li>
					                    <li>Приложить скриншоты.</li>
				                    </ol>
                                    <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
			                </ol>
		                </ol>
	                </ol>

	                <div><u><span>ВАЖНО! При остановке технологического процесса и/или срыва сменного плана производства по предприятию, по причине срабатывания системы контроля температуры подины миксера о превышении показателей температуры, организуйте:</span></u></div>
	                <ul style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Создание сообщения о происшествии в Системе информирования о ЧП согласно ""Регламенту РАМ-РГ-3.5-05 информирования руководства о чрезвычайных происшествиях в работе Компании"" (Распоряжение № РАМ-21-Р921 от 28.10.2021);</li>
		                <li>Фиксацию сообщения о внеплановой остановке оборудования в системе SAP согласно распоряжения № РАМ-21-Р480 от 31.05.2021;</li>
		                <li>Проведение расследования ЧП с согласованием ""Акта технического расследования инцидента"" в PayDox согласно распоряжения № РАМ-19-Р337 от 19.08.2019.</li>
	                </ul>
                    <br/>
                    <div><u><span>Рассылка осуществляется если за последние 30 минут, значение температуры попало в пределы от {limitMin} + до {limitMax}</span></u></div>
                    <br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	            </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, превышение показателей!", body, "Превышение температуры в миксере.");

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }
            
            //получаем список gage_ids чтобы их записать в логи для последующей проверки что по ним уже была рассылка (по хорошему как-то переделать)
            string gageIds = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids"].ToString()));
            string gageIdsHit = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_hit"].ToString()));

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message, gage_ids = gageIds, gage_ids_hit = gageIdsHit };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;
            var runParams = new { limitMin = tempLimitMin, limitMax = tempLimitMax };

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), JsonConvert.SerializeObject(runParams));

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }

            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }



        [Route("api/Lo/Technology/MnemoCheckMixerTemp/{idEvent}/{checkMinutes}/{checkLog}")]
        public async Task<string> MnemoCheckMixerTemp(int idEvent, int checkMinutes, int checkLog)
        {
            var methodLocation = "api/Lo/Technology/MnemoCheckMixerTemp";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            #region Проверка превышений температуры подин

            const string query = @"begin :cur := rbs_bl.rbs_technology.mnemo_check_mixer_temp(p_id_event => :p_id_event, p_check_minutes => :p_check_minutes, p_check_log => :p_check_log); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
                new OracleParameter {ParameterName = ":p_id_event", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = idEvent},
                new OracleParameter {ParameterName = ":p_check_minutes", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = checkMinutes},
                new OracleParameter {ParameterName = ":p_check_log", OracleDbType = OracleDbType.Int32, IsNullable = true, Value = checkLog},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion


            var limitMin = 250;
            var limitMax = 1200;

            switch (idEvent)
            {
                case 1:
                    limitMin = 205;
                    limitMax = 250;
                    break;
                case 4:
                    limitMin = 250;
                    limitMax = 1200;
                    break;
            }

            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"
            <br/><br/>
            <div class='body-end'>
                <u><span>Ответственному сотруднику рекомендуется сделать следующие действия:</span></u>
                    <ol style='padding: 0;'>
		                <li style='margin:0;'>При возможности проверить показания датчиков мобильным пирометром;</li>
		                <li>Выполнить осмотр футеровки подины миксера на предмет отсутствия повреждений;</li>
	   	                <li>При отсутствии повреждений футеровки - подать заявку в службу ИСО, к заявке приложить скриншоты из письма;</li>
	   	                <ol type='a'>
			                <li>Дождаться обратной связи от службы ИСО:</li>
			                <ol type='i'>
				                <li>Если неисправность в зоне ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
				                <li>Если неисправность в не зоны ответственности ИСО:</li>
				                <ol type='1'>
					                <li>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), к заявке приложить скриншоты;</li>
                                    <ol type='1'>
					                    <li>Заполнить все обязательные поля (поля «Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП»);</li>
					                    <li>Приложить скриншоты.</li>
				                    </ol>
                                    <li>Узнать причину неисправности и сроки по её устранению;</li>
					                <li>Написать письмо в ответ на группу рассылки с указанием причины и сроков по её устранению.</li>
				                </ol>
			                </ol>
		                </ol>
	                </ol>

	                <div><u><span>ВАЖНО! При остановке технологического процесса и/или срыва сменного плана производства по предприятию, по причине срабатывания системы контроля температуры подины миксера о превышении показателей температуры, организуйте:</span></u></div>
	                <ul style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Создание сообщения о происшествии в Системе информирования о ЧП согласно ""Регламенту РАМ-РГ-3.5-05 информирования руководства о чрезвычайных происшествиях в работе Компании"" (Распоряжение № РАМ-21-Р921 от 28.10.2021);</li>
		                <li>Фиксацию сообщения о внеплановой остановке оборудования в системе SAP согласно распоряжения № РАМ-21-Р480 от 31.05.2021;</li>
		                <li>Проведение расследования ЧП с согласованием ""Акта технического расследования инцидента"" в PayDox согласно распоряжения № РАМ-19-Р337 от 19.08.2019.</li>
	                </ul>
                    <br/>
                    <div><u><span>Рассылка осуществляется, если за последние {checkMinutes} минут значение точек температур попало в диапазон аварийных границ</span></u></div>
                    <br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	            </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, превышение показателей!", body, "Превышение температуры в миксере.");

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            //получаем список gage_ids чтобы их записать в логи для последующей проверки что по ним уже была рассылка (по хорошему как-то переделать)
            string gageIds = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids"].ToString()));
            string gageIdsHit = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_hit"].ToString()));
            string gageIdsJson = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_json"].ToString()));

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message, gage_ids = gageIds, gage_ids_hit = gageIdsHit };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;
            var runParams = new { idEvent, checkMinutes, checkLog, limitMin, limitMax = limitMax };

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), JsonConvert.SerializeObject(runParams));
            await LogTechEvent(idEvent, $"[{gageIdsJson}]");

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }

            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion

        #region Рассылка по отсутствию данных с точек (записи с датой больше 30 минут от sysdate)

        [Route("api/Lo/Technology/ProcessThermocoupleCheckNoData")]
        public async Task<string> CheckNoData()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckNoData";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();


            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_no_data(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);
            
            #endregion


            var currentPlant = Static.Plants.FirstOrDefault(v => v.Current);

            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"
            <br/><br/>
            <div class='body-end'>
                <span>К инструкции приложена «Схема взаимодействия сотрудников при отсутствии данных по температуре подин миксеров» (см. файл).</br><u>Ответственному сотруднику рекомендуется сделать следующие действия:</u></span>
                    <ol type='1'>
		                <li>Подать заявку в службу ИСО в SAP TOPO, к заявке приложить скриншоты из письма.</li>
	   	                <li>Дождаться обратной связи от службы ИСО:</li>
	   	                    <ol type='a'>
			                    <li>Если неисправность <u>в не зоны</u> ответственности ИСО:</br>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), заполнить следующие поля:</li>
			                        <ul type='disc'>
				                        <li>«Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП: Портал Технология».</li>
				                        <li>Приложить скриншоты.</li>
				                        <li>IP Адрес.</li>
				                        <li>MAC адрес.</li>				                
				                        <li>Причина отсутствия данных: Данные от датчика до термодата поступают, а от термодата до сервера не поступают.</li>				                
			                        </ul>
			                    <li>Если неисправность <u>в зоне</u> ответственности ИСО:</br>Узнать причину неисправности и сроки по её устранению.</li>
		                    </ol>
                        <li>Перейти по ссылке на неисправный миксер (в письме).</li>
                        <li>Отключить рассылку по неисправным точкам\миксеру, либо в случае кап. ремонта (<a href='http://education.{currentPlant?.PlantCode}.mes.rual.ru/305'>видео инструкция</a>):</li>
                            <ol type='1'>
				                <li>Выбрать Завод, ЛО, Миксер.</li>
				                <li>Нажать справа кнопку “Статус рассылки”.</li>
				                <li>Выбрать неисправные точки температуры.</li>
				                <li>Нажать на кнопку “Изменить статус”.</li>				                
				                <li>Выставить время и дату отключения\включения рассылки..</li>				                
				                <li>Указать № заявки.</li>				                
				                <li>Указать причину неисправности.</li>				                
			                </ol>
	                </ol>
                    <br/>
                    <div><u><span>Рассылка осуществляется при наличии точек, по которым отсутствует обновления данных более чем 30 минут с момента последнего значения, либо температура больше 1200.</span></u></div>
                    <br/>
					<br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	        </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, отсутствуют данные по точкам!", body, "Отсутствие данных по температуре подины.");

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            //получаем список gage_ids чтобы их записать в логи для последующей проверки что по ним уже была рассылка (по хорошему как-то переделать)
            string gageIds = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids"].ToString()));
            string gageIdsHit = string.Join(",", checkTable.AsEnumerable().Select(r => r["gage_ids_hit"].ToString()));

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message, gage_ids = gageIds, gage_ids_hit = gageIdsHit };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }

            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion

        #region Рассылка об отключение/включение рассылки по точкам

        [Route("api/Lo/Technology/ProcessThermocoupleCheckGagesState")]
        public async Task<string> CheckDisabledGages()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckGagesState";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            #region Таблица с отключенными точками миксера

            var query = @"begin :cur := rbs_bl.rbs_technology.get_disabled_gages(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var checkTable = await oracleOperation.GetFunctionRefCursorAsync(query, param);

            #endregion


            var listEmails = new List<XEmailRequest>();

            #region strSql

            const string enabledSql = @"begin rbs_bl.rbs_technology.set_gage_enabled(plc_mail_stop_id => :p_plc_mail_stop_id); end;";
            const string disabledSql = @"begin rbs_bl.rbs_technology.set_gage_disabled(plc_mail_stop_id => :p_plc_mail_stop_id); end;";

            #endregion

            #region Таблица групп рассылки

            query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_mailing_group(); end;";

            param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var groupsTable = await oracleOperation.GetFunctionRefCursorAsync(query, param);

            #endregion

            #region Таблица людей по группам рассылки

            query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_group_users(); end;";

            param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var usersTable = await oracleOperation.GetFunctionRefCursorAsync(query, param);

            #endregion


            var currentPlant = Static.Plants.First(v => v.Current);

            foreach (var groupRow in groupsTable.AsEnumerable())
            {
                var groupId = Convert.ToInt32(groupRow["group_id"]);
                var loName = groupRow["lo"].ToString();
                var plantName = groupRow["plant_name"].ToString();

                var tableRows = new List<string>();

                foreach (var row in checkTable.Select($"group_id={groupId}"))
                {
                    var placeId = row["parent"].ToString();
                    var mixerId = row["id_plc_mnemo"].ToString();
                    var mixerName = row["mixer_name"].ToString();

                    //var mixerName = row["mixer_name"].ToString();
                    var mixer = $"<a href=\"{currentPlant.Url}/Lo/Technology?module_id=305&mixer_id={mixerId}&place_id={placeId}\">{mixerName}</a>";

                    var tmp = Convert.ToDateTime(row["datebeg"]);
                    var dateBegin = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0);
                    tmp = Convert.ToDateTime(row["dateend"]);
                    var dateEnd = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0);

                    var dateSendMailStop = Operation.GetValueOrNull<DateTime>(row["date_mail_stop"]);
                    var dateSendMailStart = Operation.GetValueOrNull<DateTime>(row["date_mail_start"]);


                    if (DateTime.Now >= dateBegin && dateSendMailStop == null)
                    {
                        //рассылка об отключении

                        var sqlParam = new List<DbParameter>
                        {
                            new OracleParameter { ParameterName = ":p_plc_mail_stop_id", Value = row["id_plcmailstop"]  }
                        };

                        var res = await oracleOperation.RunSqlAsync(disabledSql, sqlParam);
                        if (res)
                        {
                            tableRows.Add($"<tr><td>{row["fio"]}</td><td>{mixer}</td><td>{row["gage_name"]}</td><td>{row["request"]}</td><td>{dateBegin:dd.MM.yyyy HH:mm}</td><td>{dateEnd:dd.MM.yyyy HH:mm}</td><td>{row["description"]}</td><td style='color: red;'>Отключена</td><td>Плановое отключение</td></tr>");
                        }
                    }

                    if (DateTime.Now >= dateEnd && dateSendMailStart == null)
                    {
                        //рассылка о включении

                        var sqlParam = new List<DbParameter>
                        {
                            new OracleParameter { ParameterName = ":p_plc_mail_stop_id", Value = row["id_plcmailstop"]  }
                        };

                        var res = await oracleOperation.RunSqlAsync(enabledSql, sqlParam);
                        if (res)
                        {
                            tableRows.Add($"<tr><td>{row["fio"]}</td><td>{mixer}</td><td>{row["gage_name"]}</td><td>{row["request"]}</td><td>{dateBegin:dd.MM.yyyy HH:mm}</td><td>{dateEnd:dd.MM.yyyy HH:mm}</td><td>{row["description"]}</td><td style='color: green;'>Включена</td><td>Плановое включение</td></tr>");
                        }
                    }
                }

                if (tableRows.Count > 0)
                {
                    string toMail = string.Join(";", usersTable.Select($"group_id={groupId}").AsEnumerable().Select(r => r["email"]).ToArray());

                    //string toMail = "Viktor.Chetvertakov@rusal.com;Pavel.Melnikov@rusal.com;Igor.Yakishchik@rusal.com";

                    var html = $@"
                        <table border='1' cellpadding='5' style='font-family: Segoe UI; font-size: 13px; border-spacing: 0; border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>
                            <tr style='background: #ececec;'><th>Пользователь</th><th>Миксер</th><th>Точка</th><th>Номер заявки</th><th>Плановая дата отключения</th><th>Плановая дата включения</th><th>Комментарий</th><th>Статус</th><th>Причина</th></tr>
                            {string.Join(Environment.NewLine, tableRows)}
                        </table>
                        ";

                    listEmails.Add(new XEmailRequest
                    {
                        ToMail = toMail,
                        Subject = $"{plantName} ({loName}). Изменение рассылки по точкам.",
                        Body = html
                    });
                }
            }


            var runTimeResult = runTime.Result();


            var description = new { ip = ipAddress, runtime = runTimeResult, desc = "" };
            var value = 1;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);


            dynamic model = new ExpandoObject();
            model.Emails = listEmails;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion


        #region Рассылка по стандартному отклонению температуры в точках миксера 

        [Route("api/Lo/Technology/ProcessThermocoupleCheckSharpJump")]
        public async Task<string> CheckSharpJump()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckSharpJump";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            //заготовочка
            var currentPlant = Static.Plants.FirstOrDefault(v => v.Current);
            int? smelterId = null;//currentPlant?.SmelterId == 0 ? null : currentPlant?.SmelterId;

            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_check_sharp_jump(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion


            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"
            <br/><br/>
            <div class='body-end'>
                <span><u>Ответственному сотруднику рекомендуется сделать следующие действия:</u></span>
                    <ol type='1'>
		                <li>Подать заявку в службу ИСО в SAP TOPO, к заявке приложить скриншоты из письма.</li>
	   	                <li>Дождаться обратной связи от службы ИСО:</li>
	   	                    <ol type='a'>
			                    <li>Если неисправность <u>в не зоны</u> ответственности ИСО:</br>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), заполнить следующие поля:</li>
			                        <ul type='disc'>
				                        <li>«Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП: Портал Технология».</li>
				                        <li>Приложить скриншоты.</li>
				                        <li>IP Адрес.</li>
				                        <li>MAC адрес.</li>				                
				                        <li>Причина отсутствия данных: Данные от датчика до термодата поступают, а от термодата до сервера не поступают.</li>				                
			                        </ul>
			                    <li>Если неисправность <u>в зоне</u> ответственности ИСО:</br>Узнать причину неисправности и сроки по её устранению.</li>
		                    </ol>
                        <li>Перейти по ссылке на неисправный миксер (в письме).</li>
                        <li>Отключить рассылку по неисправным точкам\миксеру, либо в случае кап. ремонта (<a href='http://education.{currentPlant?.PlantCode}.mes.rual.ru/305'>видео инструкция</a>):</li>
                            <ol type='1'>
				                <li>Выбрать Завод, ЛО, Миксер.</li>
				                <li>Нажать справа кнопку “Статус рассылки”.</li>
				                <li>Выбрать неисправные точки температуры.</li>
				                <li>Нажать на кнопку “Изменить статус”.</li>				                
				                <li>Выставить время и дату отключения\включения рассылки..</li>				                
				                <li>Указать № заявки.</li>				                
				                <li>Указать причину неисправности.</li>				                
			                </ol>
	                </ol>
                    <br/>
                    <div><u><span>Рассылка осуществляется, если за последние 30 минут значение температуры резко увеличивается на 40 и не более 200 градусов.</span></u></div>
                    <br/>
					<br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	        </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, зафиксирован резкий скачок температуры!", body, "Зафиксирован резкий скачок температуры.", true);

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }


            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion


        #region Рассылка по прогнозам превышения температуры в миксерах

        [Route("api/Lo/Technology/ProcessThermocoupleCheckPrediction")]
        public async Task<string> CheckPrediction()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckPrediction";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            //заготовочка
            var currentPlant = Static.Plants.FirstOrDefault(v => v.Current);
            int? smelterId = null;//currentPlant?.SmelterId == 0 ? null : currentPlant?.SmelterId;

            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_prediction(p_smelter_id => :p_smelter_id, p_type => :p_type); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_smelter_id", Value = smelterId, IsNullable = true},
                new OracleParameter {ParameterName = ":p_type", Value = null, IsNullable = true},
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.GENDB_DWFOUNDRY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion
            

            var body = "<style type=\"text/css\"> .body {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"<br/><br/>";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, спрогнозировано превышение температуры подины миксера (выше 205°C)", body, "Спрогнозировано превышение температуры подины миксера.", false);

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }


            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion


        #region Рассылка по стандартному отклонению температуры в точках миксера 

        [Route("api/Lo/Technology/ProcessThermocoupleCheckStdDev")]
        public async Task<string> CheckStdDev()
        {
            var methodLocation = "api/Lo/Technology/ProcessThermocoupleCheckStdDev";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            OperationRunTime runTime = new OperationRunTime();
            runTime.Start();

            //заготовочка
            var currentPlant = Static.Plants.FirstOrDefault(v => v.Current);
            int? smelterId = null;//currentPlant?.SmelterId == 0 ? null : currentPlant?.SmelterId;

            #region Таблица с миксерами для рассылки

            const string query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_check_stddev(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":p_smelter_id", Value = smelterId, IsNullable = true},
                new OracleParameter {ParameterName = ":p_type", Value = null, IsNullable = true},
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            var (checkTable, checkExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

            #endregion


            var body = "<style type=\"text/css\"> .body-end {color:#7F7F7F;font-family:\"Calibri\",sans-serif;font-size:11.0pt; border-top: 1px solid #7F7F7F; padding-top: 10px; margin-top: 30px;} li {padding: 1px 10px;}</style>";
            body += $@"
            <br/><br/>
            <div class='body-end'>
                <span><u>Ответственному сотруднику рекомендуется сделать следующие действия:</u></span>
                    <ol type='1'>
		                <li>Подать заявку в службу ИСО в SAP TOPO, к заявке приложить скриншоты из письма.</li>
	   	                <li>Дождаться обратной связи от службы ИСО:</li>
	   	                    <ol type='a'>
			                    <li>Если неисправность <u>в не зоны</u> ответственности ИСО:</br>Подать заявку в ДПА под № 63003 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=e24402bc-6443-490e-9ee1-42c7d9721730'>перейти по ссылке на заявку</a>), заполнить следующие поля:</li>
			                        <ul type='disc'>
				                        <li>«Направление» и «Наименование ПО» заполняются значениями «Литейное производство - Сервис сбора данных с АСУТП: Портал Технология».</li>
				                        <li>Приложить скриншоты.</li>
				                        <li>IP Адрес.</li>
				                        <li>MAC адрес.</li>				                
				                        <li>Причина отсутствия данных: Данные от датчика до термодата поступают, а от термодата до сервера не поступают.</li>				                
			                        </ul>
			                    <li>Если неисправность <u>в зоне</u> ответственности ИСО:</br>Узнать причину неисправности и сроки по её устранению.</li>
		                    </ol>
                        <li>Перейти по ссылке на неисправный миксер (в письме).</li>
                        <li>Отключить рассылку по неисправным точкам\миксеру, либо в случае кап. ремонта (<a href='http://education.{currentPlant?.PlantCode}.mes.rual.ru/305'>видео инструкция</a>):</li>
                            <ol type='1'>
				                <li>Выбрать Завод, ЛО, Миксер.</li>
				                <li>Нажать справа кнопку “Статус рассылки”.</li>
				                <li>Выбрать неисправные точки температуры.</li>
				                <li>Нажать на кнопку “Изменить статус”.</li>				                
				                <li>Выставить время и дату отключения\включения рассылки..</li>				                
				                <li>Указать № заявки.</li>				                
				                <li>Указать причину неисправности.</li>				                
			                </ol>
	                </ol>
                    <br/>
                    <div><u><span>Рассылка осуществляется, если за последние 24 часа у какой-нибудь точки температуры наблюдается нестандартное поведение по сравнению с остальными точками температуры на миксере.</span></u></div>
                    <br/>
					<br/>
	                <div><u><span>Для отписки от рассылки необходимо:</span></u></div>
	                <ol style='margin-top: 0.3em;'>
		                <li style='margin:0;'>Подать заявку в SD c № 63001 (<a href='http://request.int.rual.ru/Pages/User/Template/View.aspx?tmpl=616bf863-b502-48ba-9c87-272808589d49'>перейти по ссылке на заявку</a>);</li>
		                <li>Указать в заявке причину отключения от рассылки;</li>
		                <li>Указать ФИО сотрудника, на которого необходимо перенаправить рассылку.</li>
	                </ol>
	        </div>
            ";

            var (list, exception) = await GetEmails(checkTable, "ВНИМАНИЕ, зафиксировано нестандартное поведение точек температуры!", body, "Зафиксировано нестандартное поведение точек температуры.", true);

            var runTimeResult = runTime.Result();

            var message = string.Empty;
            if (checkExceptionOut.exception != null)
            {
                message = string.Join(Environment.NewLine, message, checkExceptionOut.exception.Message);
            }
            if (exception != null)
            {
                message = string.Join(Environment.NewLine, message, exception.Message);
            }

            var description = new { ip = ipAddress, runtime = runTimeResult, desc = message };
            var value = checkExceptionOut.exception == null && exception == null ? 1 : 0;

            await Log(methodLocation, value, JsonConvert.SerializeObject(description), string.Empty);

            if (checkExceptionOut.exception != null)
            {
                throw checkExceptionOut.exception;
            }
            if (exception != null)
            {
                throw exception;
            }


            dynamic model = new ExpandoObject();
            model.Emails = list;
            model.Status = 0;
            model.IsLog = true;
            model.Message = "Отправка сообщения";

            return JsonConvert.SerializeObject(model);
        }

        #endregion



        private async Task<(List<XEmailRequest> list, Exception? exception)> GetEmails(DataTable? checkTable, string bodyStart, string bodyEnd, string subjectEnd, bool showSmelter = true)
        {
            (List<XEmailRequest> list, Exception? exception) result = (new List<XEmailRequest>(), null);


            if (checkTable != null && checkTable.Rows.Count > 0)
            {
                var currentPlant = Static.Plants.First(v => v.Current);

                var smelterInSubject = showSmelter;//currentPlant.SmelterId != 0;


                var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

                #region Таблица групп рассылки

                var query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_mailing_group(); end;";

                var param = new List<DbParameter>
                {
                    new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
                };

                var (groupsTable, groupsExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

                if (groupsExceptionOut.exception != null)
                {
                    result.exception = groupsExceptionOut.exception;

                    return result;
                }

                #endregion

                #region Таблица людей по группам рассылки

                query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_group_users(); end;";

                param = new List<DbParameter>
                {
                    new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
                };

                var (usersTable, usersExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

                if (usersExceptionOut.exception != null)
                {
                    result.exception = usersExceptionOut.exception;

                    return result;
                }

                #endregion

                try
                {
                    foreach (var groupRow in groupsTable.AsEnumerable())
                    {
                        var groupId = Convert.ToInt32(groupRow["group_id"]);
                        var loName = groupRow["lo"].ToString();
                        var plantName = groupRow["plant_name"].ToString();

                        var bodyText = string.Empty;

                        var listAttachments = new List<XAttachment>();

                        var addEmail = false;
                        XImportant important = XImportant.Normal;
                        if (checkTable.Columns.Contains("cnt_log"))
                        {
                            var cntLog = checkTable.Select($"group_id={groupId}").Max(row => row["cnt_log"]);
                            if (cntLog != null)
                            {
                                var maxCountLog = Convert.ToInt32(cntLog);
                                if (maxCountLog >= 3)
                                {
                                    important = XImportant.High;
                                }
                            }
                        }
                        

                        foreach (var equipmentRow in checkTable.Select($"group_id={groupId}"))
                        {
                            var placeId = equipmentRow["parent"].ToString();
                            var mixerId = equipmentRow["id_plc_mnemo"].ToString();
                            var mixerName = equipmentRow["mixer_name"].ToString();

                            var predictionData = equipmentRow.Table.Columns.Contains("prediction_data") ? equipmentRow["prediction_data"].ToString() : string.Empty;

                            var plant = currentPlant;
                            if (predictionData?.Length > 0)
                            {
                                var smelterId = Convert.ToInt32(equipmentRow["smelter_id"].ToString());
                                plant = Static.Plants.First(v => v.SmelterId == smelterId);
                            }

                            var mixerTagName = equipmentRow.Table.Columns.Contains("mixer_tag_name") ? equipmentRow["mixer_tag_name"].ToString() : string.Empty;
                            if (mixerTagName?.Length > 0)
                            {
                                mixerName += $". {mixerTagName}";
                            }

                            var href = $"{plant.Url}/Lo/Technology?module_id=305";

                            if (equipmentRow.Table.Columns.Contains("equipment_id"))
                            {
                                href += $"&equipment_id={equipmentRow["equipment_id"]}";
                            }
                            else
                            {
                                href += $"&mixer_id={mixerId}&place_id={placeId}";
                            }

                            bodyText += $"<br/><b>&nbsp;&nbsp;&nbsp;&nbsp;{plantName}. {mixerName}</b>. Перейти по ссылке: <a style=\"color: #616161;\" href=\"{href}\">{mixerName}</a><br>";

                            var gages = equipmentRow.Table.Columns.Contains("gage_ids_name") ? equipmentRow["gage_ids_name"].ToString() : string.Empty;
                            if (gages?.Length > 0)
                            {
                                bodyText += $@"<ul style='margin:0;padding:0;'>";
                                foreach (var gage in gages.Split('|'))
                                {
                                    bodyText += $@"<li style='margin:2px 0 2px 50px;padding:0;'>{gage.Trim()}</li>";
                                }
                                bodyText += $@"</ul>";
                            }


                            if (predictionData?.Length > 0)
                            {
                                var predictions = JsonConvert.DeserializeObject<List<ExpandoObject>>(predictionData);

                                bodyText += $@"<ul style='margin:0;padding:0;'>";

                                if (predictions != null)
                                {
                                    foreach (dynamic prediction in predictions)
                                    {
                                        var tagName = prediction.tag_name?.ToString();

                                        DateTime? datePrediction = null;
                                        if (DateTime.TryParse(prediction.date_prediction?.ToString(), out DateTime datePredict))
                                        {
                                            datePrediction = datePredict;
                                        }

                                        DateTime? dateValuePrediction = null;
                                        if (DateTime.TryParse(prediction.date_value_prediction?.ToString(), out DateTime dateValue))
                                        {
                                            dateValuePrediction = dateValue;
                                        }

                                        var days = prediction.days_before_fail?.ToString();


                                        bodyText += $@"
                                    <li style='margin:0 0 0 30px;padding:0;'>{tagName}
                                        <br/>&nbsp;&nbsp;&nbsp;Дата формирования прогноза: {datePrediction?.ToShortDateString()}
                                        <br/>&nbsp;&nbsp;&nbsp;Дата спрогнозированного превышения температуры: {dateValuePrediction?.ToShortDateString()}
                                        <br/>&nbsp;&nbsp;&nbsp;Дней до даты спрогнозированного превышения температуры: {days}
                                    </li>";
                                    }
                                }

                                bodyText += $@"</ul>";

                                addEmail = true;
                            }

                            if (equipmentRow["pict_blob"] != DBNull.Value)
                            {
                                var pictBlob = (byte[])equipmentRow["pict_blob"];
                                if (pictBlob.Length > 0)
                                {

                                    var gageIds = equipmentRow["gage_ids"].ToString();

                                    Stream stream = new MemoryStream(pictBlob);
                                    var svgDocument = SvgDocument.Open<SvgDocument>(stream);

                                    #region Данные температуры по точкам

                                    query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_data(p_params => :p_params); end;";

                                    param = new List<DbParameter>
                                    {
                                        new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
                                        new OracleParameter {ParameterName = ":p_params", Value = gageIds},
                                    };

                                    var (tempTable, tempExceptionOut) = await oracleOperation.GetFunctionRefCursorAsync(query, param, true);

                                    if (tempExceptionOut.exception != null)
                                    {
                                        result.exception = tempExceptionOut.exception;
                                    }

                                    #endregion

                                    foreach (var tempRow in tempTable.AsEnumerable())
                                    {
                                        var gageId = tempRow["gage_id"].ToString();
                                        var color = ColorTranslator.FromHtml(tempRow["color"].ToString() ?? string.Empty);
                                        var value = tempRow["value"].ToString() ?? string.Empty;

                                        var element = svgDocument.GetElementById($"gage{gageId}");

                                        if (element == null || element.Children.Count == 0) continue;

                                        StyleSvgGage(element, color, value);
                                    }

                                    var fileStore = new XFileStore
                                    {
                                        Name = $"{plantName}. {mixerName}.png",
                                        FileFormat = new XFileFormat
                                        {
                                            FileFormatId = 43
                                        }
                                    };

                                    using (var image = svgDocument.Draw())
                                    {
                                        //image.Save($@"C:\tmp\{loName} - {mixerName}.png", ImageFormat.Png);
                                        using (var ms = new MemoryStream())
                                        {
                                            image.Save(ms, ImageFormat.Png);
                                            fileStore.ValueBlob = Convert.ToBase64String(ms.ToArray());
                                            fileStore.SizeBlob = ms.Length;
                                        }
                                    }

                                    listAttachments.Add(new XAttachment
                                    {
                                        FileStore = fileStore
                                    });

                                    addEmail = true;
                                }
                            }
                        }

                        if (addEmail)
                        {
                            string toMail = string.Join(";", usersTable.Select($"group_id={groupId}").AsEnumerable().Select(r => r["email"]).ToArray()); ;

                            bodyText = $"<span style=\"font-weight: bold; color: #b20838;\">{bodyStart}</span><br>" + bodyText + bodyEnd;

                            //bodyText = bodyText + $"<br><a style=\"color: #757575; font-size: 10pt;\" href=\"{currentPlant.Url}/PersonalArea/Unsubscribe?groupId={groupId}\">отписаться от рассылки</a>";

                            var subject = string.Empty;
                            if (smelterInSubject)
                            {
                                subject = $"{plantName}. ";
                                if (!string.IsNullOrWhiteSpace(loName))
                                {
                                    subject += $"({loName}). ";
                                }
                            }
                            subject += $"{subjectEnd}";

                            var bodyResult = "<style type=\"text/css\"> .body {font-family:\"Calibri\",sans-serif;font-size:11.0pt}</style>";
                            bodyResult += $@"<div class='body'>{bodyText}</div>";
                            
                            result.list.Add(new XEmailRequest
                            {
                                ToMail = toMail,
                                Subject = subject,
                                Body = bodyResult,
                                Attachments = listAttachments,
                                Important = important
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    result.exception = e;
                }
            }

            return result;
        }

        private void StyleSvgGage(SvgElement element, Color color, string value)
        {
            foreach (var gageChild in element.Children)
            {
                if (gageChild.GetType() == typeof(SvgRectangle))
                {
                    gageChild.Fill = new SvgColourServer(color);
                }
                else if (gageChild.GetType() == typeof(SvgText))
                {
                    ((SvgText)gageChild).Text = value;
                }
                else
                {
                    StyleSvgGage(gageChild, color, value);
                }
            }
        }
        
        private List<XEmailRequest> GetRestartedGagesMail()
        {
            OracleDbOperation OracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_TECHNOLOGY.Default);

            #region Таблица с отключенными точками миксера

            var query = @"begin :cur := rbs_bl.rbs_technology.get_disabled_gages(); end;";

            var param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var checkTable = OracleOperation.GetFunctionRefCursor(query, param);

            #endregion


            var listEmails = new List<XEmailRequest>();

            #region strSql

            const string enabledSql = @"begin rbs_bl.rbs_technology.set_gage_enabled(plc_mail_stop_id => :p_plc_mail_stop_id); end;";

            #endregion

            #region Таблица групп рассылки

            query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_mailing_group(); end;";

            param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var groupsTable = OracleOperation.GetFunctionRefCursor(query, param);

            #endregion

            #region Таблица людей по группам рассылки

            query = @"begin :cur := rbs_bl.rbs_technology.get_plc_mnemo_group_users(); end;";

            param = new List<DbParameter>
            {
                new OracleParameter {ParameterName = ":cur", Direction = ParameterDirection.ReturnValue, OracleDbType = OracleDbType.RefCursor, IsNullable = true},
            };

            var usersTable = OracleOperation.GetFunctionRefCursor(query, param);

            #endregion


            var currentPlant = Static.Plants.First(v => v.Current);

            foreach (var groupRow in groupsTable.AsEnumerable())
            {
                var groupId = Convert.ToInt32(groupRow["group_id"]);
                var loName = groupRow["lo"].ToString();
                var plantName = groupRow["plant_name"].ToString();

                var tableRows = new List<string>();

                foreach (var row in checkTable.Select($"group_id={groupId}"))
                {
                    var placeId = row["parent"].ToString();
                    var mixerId = row["id_plc_mnemo"].ToString();
                    var mixerName = row["mixer_name"].ToString();

                    //var mixerName = row["mixer_name"].ToString();
                    var mixer = $"<a href=\"{currentPlant.Url}/Lo/Technology?module_id=305&mixer_id={mixerId}&place_id={placeId}\">{mixerName}</a>";

                    var tmp = Convert.ToDateTime(row["datebeg"]);
                    var dateBegin = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0);
                    tmp = Convert.ToDateTime(row["dateend"]);
                    var dateEnd = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0);

                    //проверяем есть ли данные
                    if (DateTime.Now >= dateBegin && DateTime.Now <= dateEnd)
                    {
                        var hasData = Convert.ToInt32(row["has_data"]);
                        if (hasData == 1)
                        {
                            //рассылка о включении

                            var sqlParam = new List<DbParameter>
                            {
                                new OracleParameter { ParameterName = ":p_plc_mail_stop_id", Value = row["id_plcmailstop"]  }
                            };

                            var res = OracleOperation.RunSql(enabledSql, sqlParam);
                            if (res)
                            {
                                tableRows.Add($"<tr><td>{row["fio"]}</td><td>{mixer}</td><td>{row["gage_name"]}</td><td>{row["request"]}</td><td>{dateBegin:dd.MM.yyyy HH:mm}</td><td>{dateEnd:dd.MM.yyyy HH:mm}</td><td>{row["description"]}</td><td style='color: green;'>Включена</td><td>Автоматическое включение при появлении данных</td></tr>");
                            }
                        }
                    }
                }

                if (tableRows.Count > 0)
                {
                    string toMail = string.Join(";", usersTable.Select($"group_id={groupId}").AsEnumerable().Select(r => r["email"]).ToArray());

                    //string toMail = "Viktor.Chetvertakov@rusal.com;Pavel.Melnikov@rusal.com;Igor.Yakishchik@rusal.com";

                    var html = $@"
                        <table border='1' cellpadding='5' style='font-family: Segoe UI; font-size: 13px; border-spacing: 0; border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt;'>
                            <tr style='background: #ececec;'><th>Пользователь</th><th>Миксер</th><th>Точка</th><th>Номер заявки</th><th>Плановая дата отключения</th><th>Плановая дата включения</th><th>Комментарий</th><th>Статус</th><th>Причина</th></tr>
                            {string.Join(Environment.NewLine, tableRows)}
                        </table>
                    ";

                    listEmails.Add(new XEmailRequest
                    {
                        ToMail = toMail,
                        Subject = $"{plantName} ({loName}). Изменение рассылки по точкам.",
                        Body = html
                    });
                }
            }

            return listEmails;
        }

        private async Task Log(string? methodLocation, int? value, string description, string parameter)
        {
            const string strSql = @"
                begin
                   xaudit_app.LOG.insert_log (p_id_app            => :p_id_app,
                                              p_datetimestamp     => :p_datetimestamp,
                                              p_id_notifystatus   => :p_id_notifystatus,
                                              p_id_action         => :p_id_action,
                                              p_id_audittype      => :p_id_audittype,
                                              p_methodlocation    => :p_methodlocation,
                                              p_parameter         => :p_parameter,
                                              p_id_refer          => :p_id_refer,
                                              p_val               => :p_val,
                                              p_description       => :p_description,
                                              p_user_id           => :p_user_id);
                end;
            ";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter { ParameterName = ":p_id_app", Value = 22, OracleDbType = OracleDbType.Decimal },
                new OracleParameter { ParameterName = ":p_datetimestamp", Value = DateTime.Now, OracleDbType = OracleDbType.Date},
                new OracleParameter { ParameterName = ":p_id_notifystatus", Value = 1, OracleDbType = OracleDbType.Decimal },
                new OracleParameter { ParameterName = ":p_id_action", Value = 900, OracleDbType = OracleDbType.Decimal },
                new OracleParameter { ParameterName = ":p_id_audittype", Value = 1, OracleDbType = OracleDbType.Decimal },
                new OracleParameter { ParameterName = ":p_methodlocation", Value = methodLocation, OracleDbType = OracleDbType.NVarchar2 },
                new OracleParameter { ParameterName = ":p_parameter", Value = parameter, OracleDbType = OracleDbType.NClob },
                new OracleParameter { ParameterName = ":p_id_refer", Value = null, OracleDbType = OracleDbType.Decimal, IsNullable = true},
                new OracleParameter { ParameterName = ":p_val", Value = value, OracleDbType = OracleDbType.Decimal },
                new OracleParameter { ParameterName = ":p_description", Value = description, OracleDbType = OracleDbType.NClob },
                new OracleParameter { ParameterName = ":p_user_id", Value = null, OracleDbType = OracleDbType.Int32, IsNullable = true },
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_USER.Default);

            var (success, exceptionOut) = await oracleOperation.RunSqlAsync(strSql, sqlParam, true);
            if (exceptionOut.exception != null)
            {
                //todo как то логировать эту ошибку
            }
        }

        private async Task LogTechEvent(int idEvent, string tags)
        {
            const string strSql = @"begin rbs_bl.rbs_technology.mnemo_check_log_save (p_id_event => :p_id_event, p_tag_ids => :p_tag_ids); end;";

            var sqlParam = new List<DbParameter>
            {
                new OracleParameter { ParameterName = ":p_id_event", Value = idEvent, OracleDbType = OracleDbType.Int32 },
                new OracleParameter { ParameterName = ":p_tag_ids", Value = tags, OracleDbType = OracleDbType.Clob},
            };

            var oracleOperation = new OracleDbOperation(Static.ConnectionRepository.RBS_USER.Default);

            var (success, exceptionOut) = await oracleOperation.RunSqlAsync(strSql, sqlParam, true);
            if (exceptionOut.exception != null)
            {
                Static.ExceptionHandlerServer.CatchError(exceptionOut.exception, $"Ошибка LogTechEvent (idEvent = {idEvent}, tags = {tags})", true, false);
            }
        }
    }
}
