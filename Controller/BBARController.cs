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
        static string API_SERVER = $"{ConfigurationManager.AppSettings["API_SERVER"]}";
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
        [Route("OutTakeMed")]
        [HttpPost]
        async public Task<string> POST_OutTakeMed([FromBody] POST_Order_API pOST_Order_API)
        {
            returnData returnData = new returnData();
            try
            {    
                MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
             
                return returnData.JsonSerializationt();
            }
            catch (Exception e)
            {
                return returnData.JsonSerializationt();
            }
        }
    }
}
