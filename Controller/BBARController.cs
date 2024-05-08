using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBM.Data.DB2.Core;
using System.Data;
using System.Configuration;
using Basic;
using SQLUI;
using Oracle.ManagedDataAccess.Client;
using HIS_DB_Lib;
using System.Text.Json.Serialization;

namespace DB2VM
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARController : ControllerBase
    {
        public class POST_Order_API
        {
            [JsonPropertyName("prescriptionID")]
            public string 處方編號 { get; set; }
            [JsonPropertyName("prescriptionTime")]
            public string 處方日時 { get; set; }
            [JsonPropertyName("stockNo")]
            public string 處方庫台 { get; set; }
            [JsonPropertyName("drugNo")]
            public string 處方領藥號 { get; set; }
            [JsonPropertyName("patientNo")]
            public string 病歷號 { get; set; }
            [JsonPropertyName("patientName")]
            public string 病患姓名 { get; set; }
            [JsonPropertyName("visitInfo")]
            public string 就診資訊 { get; set; }
            [JsonPropertyName("nursingStation")]
            public string 護理站 { get; set; }
            [JsonPropertyName("bedNo")]
            public string 床位號碼 { get; set; }
            [JsonPropertyName("pharmacistCode")]
            public string 藥師代號 { get; set; }
            [JsonPropertyName("pharmacistName")]
            public string 藥師姓名 { get; set; }
            [JsonPropertyName("computerNo")]
            public string 電腦號碼 { get; set; }

            [JsonPropertyName("prescriptionType")]
            public string 處方類型 { get; set; }
            [JsonPropertyName("sendCommand1")]
            public string 指令欄位_0 { get; set; }

            [JsonPropertyName("visitNo")]
            public string 門急住編號 { get; set; }
            [JsonPropertyName("patientSex")]
            public string 性別 { get; set; }
            [JsonPropertyName("patientAge")]
            public string 年齡 { get; set; }
            [JsonPropertyName("patientBirthday")]
            public string 生日 { get; set; }
            [JsonPropertyName("departmentCode")]
            public string 科別代號 { get; set; }
            [JsonPropertyName("departmentName")]
            public string 科別名稱 { get; set; }
            [JsonPropertyName("doctorCode")]
            public string 醫師代號 { get; set; }
            [JsonPropertyName("doctorName")]
            public string 醫師姓名 { get; set; }
            [JsonPropertyName("diagnosisCode")]
            public string 診斷碼 { get; set; }
            [JsonPropertyName("diagnosisName")]
            public string 診斷名稱 { get; set; }

            [JsonPropertyName("drugList")]
            public List<drugClass> drugList { get; set; }

            public class drugClass
            {
                [JsonPropertyName("drugCode")]
                public string 藥品代碼 { get; set; }
                [JsonPropertyName("drugName")]
                public string 藥品學名 { get; set; }
                [JsonPropertyName("drugCName")]
                public string 藥品中文名 { get; set; }
                [JsonPropertyName("drugSpec")]
                public string 藥品規格 { get; set; }
                [JsonPropertyName("dosage")]
                public double 次劑量 { get; set; }
                [JsonPropertyName("dosageUnit")]
                public string 次劑量單位 { get; set; }
                [JsonPropertyName("frequency")]
                public string 頻率 { get; set; }
                [JsonPropertyName("dosageTime")]
                public string 使用時間 { get; set; }
                [JsonPropertyName("route")]
                public string 途徑 { get; set; }
                [JsonPropertyName("qty")]
                public double 總量 { get; set; }
                [JsonPropertyName("qtyUnit")]
                public string 總量單位 { get; set; }
                [JsonPropertyName("drugQrCode")]
                public string 處方藥品辨識碼 { get; set; }
                [JsonPropertyName("sendCommand1")]
                public string 廠商指令欄位_1 { get; set; }
                [JsonPropertyName("sendCommand2")]
                public string 廠商指令欄位_2 { get; set; }
                [JsonPropertyName("sendCommand3")]
                public string 廠商指令欄位_3 { get; set; }
            }
        }

        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";
        static public string API_Server = "http://127.0.0.1:4433/api/serversetting";
        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

        [HttpGet]
        async public Task<string> Get(string? BarCode)
        {
            returnData returnData = new returnData();
            try
            {
                MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
               

                return returnData.JsonSerializationt(true);
            }
            catch
            {
                return "醫令串接異常";
            }

        }

        [Route("OutTakeMed")]
        [HttpGet]
        async public Task<string> GET_OutTakeMed([FromBody] POST_Order_API pOST_Order_API)
        {
            return $"{DateTime.Now.ToDateTimeString()} - OK!";
        }

        [Route("OutTakeMed/{value}")]
        [HttpPost]
        async public Task<string> POST_OutTakeMed([FromBody] POST_Order_API pOST_Order_API,string value)
        {
            returnData returnData = new returnData();
            returnData.Method = "POST_OutTakeMed";
            try
            {
                if(pOST_Order_API == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"傳入資料錯誤";
                    return returnData.JsonSerializationt();
                }
                if (pOST_Order_API.drugList == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"傳入處方藥品訊息錯誤";
                    return returnData.JsonSerializationt();
                }

                if (pOST_Order_API.drugList.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = $"傳入處方無藥品訊息";
                    return returnData.JsonSerializationt();
                }
                string 目標台名稱 = value;
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind(目標台名稱, "調劑台", "一般資料");
                if (serverSettingClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = $"找無Server資料";
                    return returnData.JsonSerializationt();
                }
              
                ServerSettingClass serverSettingClass = serverSettingClasses[0];
                string Server = serverSettingClass.Server;
                string DB = serverSettingClass.DBName;
                string UserName = serverSettingClass.User;
                string Password = serverSettingClass.Password;
                uint Port = (uint)serverSettingClass.Port.StringToInt32();

                string OP_Type = pOST_Order_API.指令欄位_0;
                MyTimerBasic myTimerBasic = new MyTimerBasic(50000);

                List<class_OutTakeMed_data> class_OutTakeMed_Datas = new List<class_OutTakeMed_data>();
                for (int i = 0; i < pOST_Order_API.drugList.Count; i++)
                {
                    class_OutTakeMed_data class_OutTakeMed_Data = new class_OutTakeMed_data();
                    class_OutTakeMed_Data.開方時間 = pOST_Order_API.處方日時;
                    class_OutTakeMed_Data.領藥號 = pOST_Order_API.處方領藥號;
                    class_OutTakeMed_Data.病人姓名 = pOST_Order_API.病患姓名;
                    class_OutTakeMed_Data.病歷號 = pOST_Order_API.病歷號;
                    class_OutTakeMed_Data.病人姓名 = pOST_Order_API.病患姓名;
                    class_OutTakeMed_Data.類別 = pOST_Order_API.處方類型;
                    class_OutTakeMed_Data.電腦名稱 = pOST_Order_API.電腦號碼;
                    class_OutTakeMed_Data.護理站 = pOST_Order_API.護理站;
                    class_OutTakeMed_Data.操作人 = pOST_Order_API.藥師姓名;
                    class_OutTakeMed_Data.藥品碼 = pOST_Order_API.drugList[i].藥品代碼;
                    class_OutTakeMed_Data.交易量 = (pOST_Order_API.drugList[i].總量 * -1).ToString();
                    class_OutTakeMed_Data.功能類型 = OP_Type;
                    class_OutTakeMed_Data.PRI_KEY = $"{DateTime.Now.ToDateTimeString_6()},{class_OutTakeMed_Data.開方時間},{class_OutTakeMed_Data.領藥號},{class_OutTakeMed_Data.病歷號},{class_OutTakeMed_Data.藥品碼}";

                    class_OutTakeMed_Datas.Add(class_OutTakeMed_Data);

                }
                string url = $"http://127.0.0.1:4433/api/OutTakeMed/{目標台名稱}";
                string json_result = Basic.Net.WEBApiPostJson(url, class_OutTakeMed_Datas.JsonSerializationt());
                returnData.TimeTaken = myTimerBasic.ToString();
                returnData.Data = pOST_Order_API;
                returnData.Result = $"成功轉換醫令共{class_OutTakeMed_Datas.Count}筆";
                Logger.LogAddLine($"OutTakeMed");
                Logger.Log($"OutTakeMed", $"{ returnData.JsonSerializationt(true)}");
                Logger.LogAddLine($"OutTakeMed");
                return json_result;
            }
            catch (Exception e)
            {
                returnData.Code = -200;
                returnData.Data = null;
                returnData.Result = $"{e.Message}";
                Logger.Log($"OutTakeMed", $"[異常] { returnData.Result}");
                return returnData.JsonSerializationt();
            }
        }
    }
}
