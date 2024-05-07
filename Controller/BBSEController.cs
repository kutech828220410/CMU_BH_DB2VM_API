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

namespace DB2VM_API
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBSEController: ControllerBase
    {

        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        static string API_SERVER = $"{ConfigurationManager.AppSettings["API_SERVER"]}";

        private SQLControl sQLControl_person_page = new SQLControl(MySQL_server, MySQL_database, "person_page", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

        public class PharmacistClass
        {
            [JsonPropertyName("EMP_CODE")]
            public string 員工職編 { get; set; }
            [JsonPropertyName("EMP_NAME")]
            public string 員工姓名 { get; set; }
        }
        [Route("{value}")]
        [HttpGet]
        async public Task<string> Get(string value)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
            List<object[]> list_v_hisdrugdia = new List<object[]>();
            returnData returnData = new returnData();

            //ServiceReference.ADCMedicineCabinetWCFServiceClient client = new ADCMedicineCabinetWCFServiceClient();
            //List<EMPCodeParam> eMPCodeParams = new List<EMPCodeParam>();
            //EMPCodeParam eMPCodeParam = new EMPCodeParam();
            //if(value.StringIsInt32()) eMPCodeParam.EMP_CODE = value.StringToInt32().ToString();
            //eMPCodeParam.EMP_CODE = value;
            //eMPCodeParams.Add(eMPCodeParam);
            //string json_Pharmacist = await client.PharmacistAsync(eMPCodeParams.ToArray());
            //List<PharmacistClass> pharmacistClasses = json_Pharmacist.JsonDeserializet<List<PharmacistClass>>();
            //if(pharmacistClasses == null)
            //{
            //    returnData.Code = -200;
            //    returnData.Result = $"取得員工資料失敗! {json_Pharmacist}";
            //    return returnData.JsonSerializationt();
            //}
            //returnData.Code = 200;
            //returnData.Data = pharmacistClasses;
            //returnData.Result = "取得員工資料成功!";
            return returnData.JsonSerializationt();
        }
        [Route("api_login")]
        [HttpPost]
        async public Task<string> api_login([FromBody] returnData returnData)
        {
            bool flag_api_call = false;
            bool flag_new_ID = false;
            string ID = "";
            string Password = "";
            sessionClass sessionClass_temp = returnData.Data.ObjToClass<sessionClass>();
            if (returnData.Value.StringIsEmpty())
            {
                ID = sessionClass_temp.ID;
                Password = sessionClass_temp.Password;
                flag_api_call = false;
            }
            else
            {
                ID = returnData.Value;
                flag_api_call = true;
            }
         
            List<object[]> list_person_list = sQLControl_person_page.GetRowsByDefult(null, (int)enum_人員資料.ID, ID);
            if(list_person_list.Count == 0 && ID.ToUpper() != "admin".ToUpper())
            {
                string json_Pharmacist = await Get(ID);
                returnData returnData_Pharmacist = json_Pharmacist.JsonDeserializet<returnData>();
                if(returnData_Pharmacist.Code == -200)
                {
                    return returnData_Pharmacist.JsonSerializationt();
                }
                List<PharmacistClass> pharmacistClasses = returnData_Pharmacist.Data.ObjToClass<List<PharmacistClass>>();
                if(pharmacistClasses == null || pharmacistClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = "取得員工資料API異常!";
                    return returnData.JsonSerializationt();
                }
                object[] value = new object[new enum_人員資料().GetLength()];
                value[(int)enum_人員資料.GUID] = Guid.NewGuid();
                value[(int)enum_人員資料.ID] = ID;
                value[(int)enum_人員資料.姓名] = pharmacistClasses[0].員工姓名;
                value[(int)enum_人員資料.一維條碼] = ID;
                value[(int)enum_人員資料.顏色] = System.Drawing.Color.BlueViolet.ToColorString();
                value[(int)enum_人員資料.密碼] = ID;
                value[(int)enum_人員資料.權限等級] = "01";
                sQLControl_person_page.AddRow(null, value);
                list_person_list = sQLControl_person_page.GetRowsByDefult(null, (int)enum_人員資料.ID, ID);
                flag_new_ID = true;
                if (list_person_list.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.Result = "新增人員資料庫異常!";
                    return returnData.JsonSerializationt();
                }
                sessionClass sessionClass = new sessionClass();
                sessionClass.ID = list_person_list[0][(int)enum_人員資料.ID].ObjectToString();
                sessionClass.Password = list_person_list[0][(int)enum_人員資料.密碼].ObjectToString();
                returnData.Data = sessionClass;
                string json_login_in = returnData.JsonSerializationt();
                string json_login = Basic.Net.WEBApiPostJson($"{API_SERVER}/api/session/login", json_login_in);
                returnData returnData_login = json_login.JsonDeserializet<returnData>();
                if (flag_new_ID)
                {
                    returnData_login.Result += "\n從院方API新增人員資料";
                }
                return returnData_login.JsonSerializationt();
            }
            else
            {
                sessionClass sessionClass = sessionClass_temp;
                if (ID.ToUpper() != "admin".ToUpper())
                {
                    sessionClass.ID = list_person_list[0][(int)enum_人員資料.ID].ObjectToString();
                    if (flag_api_call) sessionClass.Password = list_person_list[0][(int)enum_人員資料.密碼].ObjectToString();
                    else sessionClass.Password = Password;
                }
                else
                {
                    sessionClass.ID = "admin";
                    sessionClass.Password = "66437068";
                }
                returnData.Data = sessionClass;
                string json_login_in = returnData.JsonSerializationt();
                string json_login = Basic.Net.WEBApiPostJson($"{API_SERVER}/api/session/login", json_login_in);
                returnData returnData_login = json_login.JsonDeserializet<returnData>();
                return returnData_login.JsonSerializationt();
            }
          
        }
    }
}
