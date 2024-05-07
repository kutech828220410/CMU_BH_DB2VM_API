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
using System.Text;
using HIS_DB_Lib;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;
namespace DB2VM.Controller
{


    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBCMController : ControllerBase
    {
        public class DrugInfoClass
        {
            [JsonPropertyName("medCode")]
            public string 藥碼 { get; set; }
            [JsonPropertyName("drugName")]
            public string 藥名 { get; set; }
            [JsonPropertyName("genericName1")]
            public string 商品名1 { get; set; }
            [JsonPropertyName("genericName2")]
            public string 商品名2 { get; set; }
            [JsonPropertyName("stockUnitName")]
            public string 包裝單位 { get; set; }
            [JsonPropertyName("highAlert")]
            public string 高警訊 { get; set; }
            [JsonPropertyName("controlDrug")]
            public string 管制級別 { get; set; }

        }
      
        static public string API_Server = "http://127.0.0.1:4433/api/serversetting";
        static private MySqlSslMode SSLMode = MySqlSslMode.None;
        [HttpGet]
        async public Task<string> Get(string? Code)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
            returnData returnData = new returnData();
            try
            {
                List<ServerSettingClass> serverSettingClasses = ServerSettingClassMethod.WebApiGet($"{API_Server}");
                serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "VM端");
                if (serverSettingClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = $"找無Server資料!";
                    return returnData.JsonSerializationt();
                }

                ServerSettingClass serverSettingClass = serverSettingClasses[0];
                string Server = serverSettingClass.Server;
                string DB = serverSettingClass.DBName;
                string UserName = serverSettingClass.User;
                string Password = serverSettingClass.Password;
                uint Port = (uint)serverSettingClass.Port.StringToInt32();

                SQLControl sQLControl_UDSDBBCM = new SQLControl(Server, DB, "medicine_page_cloud", UserName, Password, Port, SSLMode);

                string json_med = Basic.Net.WEBApiGet("https://his-staging.cmuh.org.tw/webapi/drugStockManager/drugStock/getDrugStockBasics/2P11");
                List<DrugInfoClass> drugInfoClasses = json_med.JsonDeserializet<List<DrugInfoClass>>();

                if(Code.StringIsEmpty() == false)
                {
                    drugInfoClasses = (from temp in drugInfoClasses
                                       where temp.藥碼.Trim() == Code
                                       select temp).ToList();
                }

                List<object[]> list_BBCM = sQLControl_UDSDBBCM.GetAllRows("medicine_page_cloud");
                List<object[]> list_BBCM_buf = new List<object[]>();
                List<object[]> list_BBCM_Add = new List<object[]>();
                List<object[]> list_BBCM_Replace = new List<object[]>();

                List<medClass> medClasses = list_BBCM.SQLToClass<medClass, enum_雲端藥檔>();
                List<medClass> medClasses_Add = new List<medClass>();
                List<medClass> medClasses_Replace = new List<medClass>();



                Dictionary<string, List<medClass>> keyValuePairs = medClass.CoverToDictionaryByCode(medClasses);
                for (int i = 0; i < drugInfoClasses.Count; i++)
                {
                    string 藥碼 = drugInfoClasses[i].藥碼.Trim();
                    string 藥名 = drugInfoClasses[i].藥名.Trim();
                    string 商品名1 = drugInfoClasses[i].商品名1.Trim();
                    string 包裝單位 = drugInfoClasses[i].包裝單位.Trim();
                    string 管制級別 = drugInfoClasses[i].管制級別.Trim();
                    string 高警訊 = drugInfoClasses[i].高警訊.Trim();
                    if (高警訊.ToUpper() == "Y") 高警訊 = "True";
                    else 高警訊 = "False";
                    List<medClass> medClasse_buf = medClass.SortDictionaryByCode(keyValuePairs, 藥碼);
                    if (medClasse_buf.Count > 0)
                    {
                        medClass medClass = medClasse_buf[0];
                        bool flag_replace = false;
                        if (藥名 != medClasse_buf[0].藥品名稱) flag_replace = true;
                        if (商品名1 != medClasse_buf[0].藥品學名) flag_replace = true;
                        if (包裝單位 != medClasse_buf[0].包裝單位) flag_replace = true;
                        if (管制級別 != medClasse_buf[0].管制級別) flag_replace = true;
                        if (高警訊 != medClasse_buf[0].警訊藥品) flag_replace = true;
                        medClass.藥品碼 = 藥碼;
                        medClass.藥品名稱 = 藥名;
                        medClass.藥品學名 = 商品名1;
                        medClass.包裝單位 = 包裝單位;
                        medClass.管制級別 = 管制級別;
                        medClass.警訊藥品 = 高警訊;
                        if (flag_replace) medClasses_Replace.Add(medClass);
                    }
                    else
                    {
                        medClass medClass = new medClass();
                        medClass.GUID = Guid.NewGuid().ToString();
                        medClass.藥品碼 = 藥碼;
                        medClass.藥品名稱 = 藥名;
                        medClass.藥品學名 = 商品名1;
                        medClass.包裝單位 = 包裝單位;
                        medClass.管制級別 = 管制級別;
                        medClass.警訊藥品 = 高警訊;
                        medClasses_Add.Add(medClass);
                    }
                }
                list_BBCM_Replace = medClasses_Replace.ClassToSQL<medClass, enum_雲端藥檔>();
                list_BBCM_Add = medClasses_Add.ClassToSQL<medClass, enum_雲端藥檔>();
                if (list_BBCM_Replace.Count > 0) sQLControl_UDSDBBCM.UpdateByDefulteExtra(null, list_BBCM_Replace);
                if (list_BBCM_Add.Count > 0) sQLControl_UDSDBBCM.AddRows(null, list_BBCM_Add);
                returnData.Code = 200;
                returnData.Result = $"取得藥檔完成! 共<{drugInfoClasses.Count}>筆 ,新增<{list_BBCM_Add.Count}>筆,修改<{list_BBCM_Replace.Count}>筆";
                returnData.TimeTaken = myTimerBasic.ToString();
                return returnData.JsonSerializationt(true);
            }
            catch(Exception e)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception : {e.Message}";
                return returnData.JsonSerializationt();
            }
          
        }
    }
}
