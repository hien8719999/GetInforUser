using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Template_MinSoftware;

namespace Common
{
    public class CommonOther
    {
        #region Generator.email

        public static string CreateMailGenerator(string tenMail = "")
        {
            string duoiMail = "";

            int dem = 0;
            while (duoiMail == "")
            {
                dem++;
                duoiMail = GetDuoiMail();
                if (dem == 10)
                    return "";
            }

            if (tenMail == "")
            {
                tenMail = Common.CommonCSharp.CreateRandomString(10);
            }
            return tenMail + duoiMail;
        }

        public static string GetDuoiMail()
        {
            Common.RequestHttp request = new Common.RequestHttp();

            string respond = request.RequestGet("https://generator.email/");
            MatchCollection collection = Regex.Matches(respond, "change_dropdown_list\\(this.innerHTML\\)\" id=\"(.*?)\"");
            List<string> lstDuoiMail = new List<string>();

            string duoiMail = "";
            for (int i = 0; i < collection.Count; i++)
            {
                duoiMail = collection[i].Groups[1].Value;
                if (Common.CommonCSharp.CheckBasicString(duoiMail) && !Common.CommonCSharp.IsContainNumber(duoiMail) && !duoiMail.Contains("-") && (duoiMail.EndsWith(".com") || duoiMail.EndsWith(".org") || duoiMail.EndsWith(".info")))
                    lstDuoiMail.Add(duoiMail);
            }

            if (lstDuoiMail.Count <= 0)
                return "";

            Random rd = new Random();
            return "@" + lstDuoiMail[rd.Next(0, lstDuoiMail.Count)];
        }

        private string GetOtpGenerator(string mail, int luot, int timeOut = 30, string proxy = "")
        {
            RequestHttp request = new RequestHttp();

            string otp = "";
            string pattern = "/" + mail.Split('@')[1] + "/" + mail.Split('@')[0] + "/(.*?)\"";
            int timeStart = Environment.TickCount;
            while (true)
            {
                if (Environment.TickCount - timeStart > timeOut * 1000)
                {
                    break;
                }
                string respond = request.RequestGet("https://generator.email/" + mail, proxy);
                if (luot == 1)
                {
                    otp = Regex.Match(respond, "<table(.*?)table>", RegexOptions.Singleline).Value;
                    otp = Regex.Match(otp, "<table(.*?)table>", RegexOptions.Singleline).Groups[1].Value;
                    otp = Regex.Match(otp, "<span(.*?)span>", RegexOptions.Singleline).Groups[1].Value;
                    otp = Regex.Match(otp, @"\d{4}", RegexOptions.Singleline).Value.Trim();
                    if (otp != "")
                        break;
                }
                else if (luot == 2)
                {
                    var url_mes = Regex.Matches(respond, pattern);
                    if (url_mes.Count > 0)
                    {
                        string url = url_mes[0].Value.TrimEnd('\"');
                        respond = request.RequestGet("https://generator.email" + url, proxy);
                        otp = Regex.Match(respond, "<table(.*?)table>", RegexOptions.Singleline).Value;
                        otp = Regex.Match(otp, "<table(.*?)table>", RegexOptions.Singleline).Groups[1].Value;
                        otp = Regex.Match(otp, "<span(.*?)span>", RegexOptions.Singleline).Groups[1].Value;
                        otp = Regex.Match(otp, @"\d{7}", RegexOptions.Singleline).Value.Trim();
                        if (otp != "")
                            break;
                    }
                }
            }

            return otp;
        }
        public static bool DelAllMail(string mail)
        {
            Common.RequestHttp request = new Common.RequestHttp();

            string respond = request.RequestGet("https://generator.email/" + mail);
            string delll = Regex.Match(respond, "delll: \"(.*?)\"").Groups[1].Value;
            string Data = "delll=" + delll;
            respond = request.RequestPost("https://generator.email/del_mail.php", Data);

            if (respond.Contains("successfully"))
                return true;
            return false;
        }
        #endregion

        public static string CheckCountry(string hometown)
        {
            Common.RequestHttp request = new Common.RequestHttp();
            string respond = request.RequestGet("https://minsoftware.xyz/minsoftware/api1.php/GetCodeCheckCountry");
            string code = respond.Replace("\"", "");

            string country = "";
            string jsonGetLocation = request.RequestPost("https://www.mapdevelopers.com/data.php?operation=geocode&address=" + hometown + "&region=US&code="+code).ToString();
            JObject objLoca = JObject.Parse(jsonGetLocation);
            string nameCountry = objLoca["data"]["country"].ToString();
            if (nameCountry != "")
                country = nameCountry;
            return country;
        }

        #region https://rentcode.co/
        public static string CheckBalance(string apikey)
        {
            string result = "";
            RequestXNet request = new RequestXNet();
            string rq = request.RequestGet("https://api.rentcode.net/api/ig/balance?apiKey=" + Uri.EscapeDataString(apikey));
            
            JObject js = JObject.Parse(rq);
            if (Convert.ToBoolean(js["success"]))
            {
                try
                {
                    result = js["results"]["balance"].ToString();
                }
                catch
                {
                }
            }
            return result==""?"": Convert.ToInt32(result).ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="id_service">3-facebook</param>
        /// <param name="id_provider">null/0: không chọn nhà mạng; 1: Viettel; 2: Vina Phone; 3: Mobiphone; 4: vnmb; 5: cambodia</param>
        /// <returns></returns>
        public static string GetPhoneRentcode(string apikey, int id_service=3, int id_provider=2, int timeOut=60)
        {
            string result = "";
            RequestXNet request = new RequestXNet();

            //create order
            string data = "{ \"serviceProviderId\": "+id_service+", \"networkProvider\": "+id_provider+" }";
            string rq = "";

            string id_order = "";
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    rq = request.RequestPost("https://api.rentcode.net/api/ig/create-request?apiKey=" + Uri.EscapeDataString(apikey), data, "application/json");
                    id_order = JObject.Parse(rq)["results"]["id"].ToString();
                    if (id_order != "")
                        break;
                }
                catch
                {
                }
            }

            if (id_order != "")
            {
                //get phone
                int timeStart = Environment.TickCount;
                while (true)
                {
                    if (Environment.TickCount - timeStart > timeOut * 1000)
                        break;

                    rq = request.RequestGet("https://api.rentcode.net/api/ig/orders/" + id_order + "/check-status?apiKey=" + Uri.EscapeDataString(apikey));
                    JObject js = JObject.Parse(rq);
                    if (Convert.ToBoolean(js["success"]))
                    {
                        try
                        {
                            result = js["results"]["phoneNumber"].ToString();
                            if (result != "")
                                break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return id_order+"|"+result;
        }
        public static string GetOTPRentcode(string apikey, string id_order, int timeOut = 60)
        {
            string result = "";
            RequestXNet request = new RequestXNet();

            //create order
            string data = "{ \"pageIndex\": 0, \"pageSize\": 0, \"sortColumnName\": \"string\", \"isAsc\": true, \"searchObject\": { \"additionalProp1\": { }, \"additionalProp2\": { }, \"additionalProp3\": { } } }";
            string rq = "";

            //get phone
            int timeStart = Environment.TickCount;
            while (true)
            {
                if (Environment.TickCount - timeStart > timeOut * 1000)
                    break;

                rq = request.RequestPost("https://api.rentcode.net/api/ig/orders/"+id_order+"/results?apiKey=" + Uri.EscapeDataString(apikey), data, "application/json");
                JObject js = JObject.Parse(rq);
                if (Convert.ToInt32(js["total"])>0)
                {
                    try
                    {
                        result = js["results"][0]["message"].ToString();
                        result = Regex.Match(result, "\\d{6}").Value;
                        if (result != "")
                            break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }
        public static bool CancelRentcode(string apikey, string id_order)
        {
            bool result = false;
            RequestXNet request = new RequestXNet();

            //create order
            string rq = "";
            for (int i = 0; i < 5; i++)
            {
                rq = request.RequestPost("https://api.rentcode.net/api/ig/orders/" + id_order + "/cancel?apiKey=" + Uri.EscapeDataString(apikey));
                JObject js = JObject.Parse(rq);
                if (Convert.ToBoolean(js["success"]))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        #endregion

        #region 
        public static bool ChangeIP(int typeChangeIp, string profileDcom = "")
        {
            bool isSuccess = false;
            try
            {
                string ip_old = CommonCSharp.CheckIP();
                string ip_new = "";
                if (typeChangeIp == 0)
                {
                    //Ko đổi ip
                }
                else if (typeChangeIp == 2)
                {
                    //Change ip hma
                    //Click nút đổi ip HMA
                    Auto.ClickControlPoint("HMA VPN", "Chrome_RenderWidgetHostHWND", "Chrome Legacy Window", 355, 285);
                    Thread.Sleep(5000);
                    Auto.ClickControlPoint("HMA VPN", "Chrome_RenderWidgetHostHWND", "Chrome Legacy Window", 355, 285);
                    Thread.Sleep(15000);
                    int timeStart = Environment.TickCount;
                    //do
                    //{

                    //    if (Environment.TickCount - timeStart > 20000)
                    //        break;
                    //} while (ip_new == ip_old);
                    ip_new = CommonCSharp.CheckIP();
                    if (ip_new != ip_old)
                        isSuccess = true;
                }
                else if (typeChangeIp == 1)
                {
                    CommonCSharp.resetDcom(profileDcom);
                    ip_new = CommonCSharp.CheckIP();
                    if (ip_new != ip_old)
                        isSuccess = true;
                }

            }
            catch (Exception ex)
            {
                CommonCSharp.ExportError(null, ex.ToString());
            }
            return isSuccess;
        }
        #endregion
    }
}
