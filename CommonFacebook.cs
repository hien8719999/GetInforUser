using HttpRequest;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public class CommonFacebook
    {
        #region ChromeSelenium
        public static bool LoginFacebookUsingCookie(ChromeDriver chrome, string cookie, string userAgent)
        {
            bool isSuccess = false;
            try
            {
                if (!chrome.Url.Contains("https://www.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://www.facebook.com/");
                CommonChrome.AddCookieIntoChrome(chrome, cookie);
                chrome.Navigate().Refresh();
                isSuccess = CommonJSChrome.CheckLiveCookie(chrome);
            }
            catch
            {
            }
            return isSuccess;
        }

        public static bool LoginFacebookUsingUidPass(ChromeDriver chrome, string uid, string pass)
        {
            bool isSuccess = false;
            try
            {
                if (!chrome.Url.Contains("https://www.facebook.com/login"))
                    chrome.Navigate().GoToUrl("https://www.facebook.com/login");
                CommonCSharp.DelayTime(1);
                CommonChrome.SendKeysChrome(chrome, 1, "email", uid, 0.1);
                CommonChrome.SendKeysChrome(chrome, 1, "pass", pass, 0.1);
                CommonChrome.ClickChrome(chrome, 2, "login");
                CommonCSharp.DelayTime(1);
                isSuccess = CommonJSChrome.CheckLiveCookie(chrome);
            }
            catch
            {
            }
            return isSuccess;
        }
        #endregion

        #region Request
        public static List<string> GetListGroupFromCookie(string cookie, string userAgent = "", string proxy = "")
        {
            List<string> listGroup = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet(cookie, userAgent, proxy);
                string html = request.RequestGet("https://mbasic.facebook.com/groups/?seemore");
                MatchCollection linkGr = Regex.Matches(html, "<a href=\"/groups/[0-9]+\\?refid=27");
                for (int i = 0; i < linkGr.Count; i++)
                {
                    try
                    {
                        string idgr = Regex.Match(linkGr[i].Value, "groups/(.*?)\\?refid=27").Groups[1].Value.ToString();
                        if (idgr != "")
                            listGroup.Add(idgr);
                    }
                    catch { }
                }
            }
            catch
            {
            }

            return listGroup;
        }
        /// <summary>
        /// X|Y => X:0-die, 1-live; Y:0-noveri, 1-veri
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="ua"></param>
        /// <param name="proxy"></param>
        /// <returns>X|Y => X:0-die, 1-live; Y:0-noveri, 1-veri</returns>
        public static string CheckLiveCookie(string cookie, string userAgent, string proxy)
        {
            string output = "0|0";
            string uid = Regex.Match(cookie + ";", "c_user=(.*?);").Groups[1].Value;
            try
            {
                RequestXNet request = new RequestXNet(cookie, userAgent, proxy);
                if (uid != "")
                {
                    string html = request.RequestGet("https://www.facebook.com/me").ToString();
                    if (html.Contains("id=\"code_in_cliff\"") || html.Contains("name=\"new\"") || html.Contains("name=\"c\"") || html.Contains("changeemail"))
                        output = "1|0";
                    else if (Regex.Match(html, "\"USER_ID\":\"(.*?)\"").Groups[1].Value.Trim() == uid.Trim() && !html.Contains("checkpointSubmitButton") && !html.Contains("checkpointBottomBar") && !html.Contains("captcha_response") && !html.Contains("https://www.facebook.com/communitystandards/") && !html.Contains("/help/203305893040179") && !html.Contains("FB:ACTION:OPEN_NT_SCREEN"))
                        output = "1|1";
                }
            }
            catch
            { }

            return output;
        }
        public static List<string> GetMyListUidNameFriend(string cookie, string useragent, string token, string port911 = "")
        {
            List<string> listFriend = new List<string>();
            try
            {
                string uid = Regex.Match(cookie + ";", "c_user=(.*?);").Groups[1].Value;
                RequestXNet request = new RequestXNet(cookie, useragent, port911);
                request.request.AddHeader("Authorization", "OAuth " + token);
                //string getListFriend = request.RequestGet("https://graph.facebook.com/me/friends?pretty=0&limit=5000&fields=id,name&access_token=" + token);
                string getListFriend = request.RequestGet("https://graph.facebook.com/?ids=" + uid + "&fields=friends{id,name}");
                JObject objFriend = JObject.Parse(getListFriend);

                var temp = objFriend[uid]["friends"];
                if (temp["data"].Count() > 0)
                {
                    for (int i = 0; i < temp["data"].Count(); i++)
                    {
                        string uidFr = temp["data"][i]["id"].ToString();
                        string nameFr = temp["data"][i]["name"].ToString();
                        listFriend.Add(uidFr + "|" + nameFr);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return listFriend;
        }
        public static List<string> GetListUidNameFriendOfUid(string token, string uid, string port911 = "")
        {
            List<string> listFriend = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet("", "", port911);
                request.request.AddHeader("Authorization", "OAuth " + token);
                string getListFriend = request.RequestGet("https://graph.facebook.com/?ids=" + uid + "&fields=friends{id,name}");
                JObject objFriend = JObject.Parse(getListFriend);

                var temp = objFriend[uid]["friends"];
                if (temp["data"].Count() > 0)
                {
                    for (int i = 0; i < temp["data"].Count(); i++)
                    {
                        string uidFr = temp["data"][i]["id"].ToString();
                        string nameFr = temp["data"][i]["name"].ToString();
                        listFriend.Add(uidFr);
                    }
                }
            }
            catch
            {
            }

            return listFriend;
        }
        public static List<string> BackupImageOne(string cookie, string ua, string uidFr, string nameFr, string token, string port911 = "", int countImage = 20)
        {
            List<string> listImageBackup = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet(cookie, ua, port911);
                request.request.AddHeader("Authorization", "OAuth " + token);

                //string htmlImage = request.RequestGet("https://graph.facebook.com/" + uidFr + "/photos?fields=images&limit=" + countImage + "&access_token=" + token);
                string url = "https://graph.facebook.com/?ids=" + uidFr + "&pretty=0&fields=id,name,photos.limit(" + countImage + "){images}";
                string htmlImage = request.RequestGet(url);
                JObject objImg = JObject.Parse(htmlImage);
                int stt = 0;
                if (objImg != null && htmlImage.Contains("images"))
                {
                    var temp = objImg[uidFr]["photos"];
                    for (int j = 0; j < temp["data"].Count(); j++)
                    {
                        stt = temp["data"][j]["images"].ToList().Count - 1;
                        listImageBackup.Add(uidFr + "*" + nameFr + "*" + temp["data"][j]["images"][stt]["source"] + "|" + temp["data"][j]["images"][stt]["width"] + "|" + temp["data"][j]["images"][stt]["height"]);
                    }
                }
            }
            catch (Exception ex) { }

            return listImageBackup;
        }
        public static List<string> GetMyListComments(string cookie, string proxy)
        {
            List<string> lstComment = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet(cookie, "", proxy);
                string htmlActivity = request.RequestGet("https://mbasic.facebook.com/me/allactivity?log_filter=cluster_116");
                string text = "";
                string linkReadCmt = "";
                MatchCollection matchCollection = null;
                do
                {
                    htmlActivity = WebUtility.HtmlDecode(htmlActivity);
                    matchCollection = Regex.Matches(htmlActivity, "<span>(.*?)</h4>");
                    for (int i = 0; i < matchCollection.Count; i++)
                    {
                        text = matchCollection[i].Groups[1].Value;
                        text = text.Substring(0, text.LastIndexOf('<'));
                        MatchCollection match = Regex.Matches(text, "<(.*?)>");
                        for (int j = 0; j < match.Count; j++)
                        {
                            text = text.Replace(match[j].Value, "");
                        }
                        if (!lstComment.Contains(text))
                            lstComment.Add(text);
                    }
                    linkReadCmt = Regex.Match(htmlActivity, "/allactivity.category_key(.*?)more_\\d").Value;
                    htmlActivity = request.RequestGet("http://mbasic.facebook.com/me" + linkReadCmt);
                } while (linkReadCmt != "");
            }
            catch
            { }
            return lstComment;
        }
        public static List<string> GetMyListUidMessage(string cookie, string proxy)
        {
            List<string> lstMessage = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet(cookie, "", proxy);
                int moreAcc = 1;
                string htmlMessage = request.RequestGet("https://mbasic.facebook.com/messages/");

                string linkReadMes = "";
                string uid = "";
                do
                {
                    MatchCollection matchComments = Regex.Matches(htmlMessage, "#fua\">(.*?)<");
                    for (int c = 0; c < matchComments.Count; c++)
                    {
                        try
                        {
                            uid = matchComments[c].Groups[1].Value.Replace("\"", "");
                            uid = Html.DecodeHtml(uid);
                            if (!lstMessage.Contains(uid))
                                lstMessage.Add(uid);
                        }
                        catch { }
                    }
                    linkReadMes = Regex.Match(htmlMessage, "/messages/.pageNum=(.*?)\"").Value.Replace("amp;", "");
                    htmlMessage = request.RequestGet("https://mbasic.facebook.com" + linkReadMes);
                    moreAcc++;
                    if (moreAcc >= 5)
                    {
                        break;
                    }
                } while (linkReadMes != "");
            }
            catch
            { }
            return lstMessage;
        }
        public static string GetMyBirthday(string token, string proxy = "")
        {
            string output = "";
            try
            {
                RequestXNet request = new RequestXNet("", "", proxy);
                string rq = request.RequestGet("https://graph.facebook.com/me?fields=id,name,birthday&access_token=" + token);
                JObject json = JObject.Parse(rq);
                return json["id"].ToString() + "|" + json["birthday"].ToString() + "|" + json["name"].ToString();
            }
            catch
            {
            }

            return output;
        }
        public static string GetBirthdayOfUid(string token, string uid, string proxy = "")
        {
            string output = "";
            try
            {
                RequestXNet request = new RequestXNet("", "", proxy);
                string rq = request.RequestGet("https://graph.facebook.com/" + uid + "?fields=id,name,birthday&access_token=" + token);
                JObject json = JObject.Parse(rq);
                string ngaysinh = "";
                try
                {
                    ngaysinh = json["birthday"].ToString();
                }
                catch
                {
                }
                return json["id"].ToString() + "|" + ngaysinh + "|" + json["name"].ToString();
            }
            catch
            {
            }

            return output;
        }
        public static bool CheckLiveWall(string uid, string token = "", string proxy = "")
        {
            bool output = false;
            RequestXNet request = new RequestXNet("", "", proxy);
            try
            {
                string html = request.RequestGet("https://graph.facebook.com/" + uid + "?access_token=" + token).ToString();
                if (JObject.Parse(html)["id"].ToString() == uid)
                    output = true;
            }
            catch { }
            return output;
        }

        public static bool CheckLiveToken(string cookie, string useragent, string token, string proxy = "")
        {
            bool output = false;
            RequestXNet request = new RequestXNet(cookie, useragent, proxy);
            try
            {
                string html = request.RequestGet("https://graph.facebook.com/me?access_token=" + token).ToString();
                if (JObject.Parse(html)["id"].ToString() != "")
                    output = true;
            }
            catch { }
            return output;
        }

        public static string GetFbdtsg(string cookie, string userAgent = "")
        {
            try
            {
                string rq = new RequestXNet(cookie, userAgent).RequestGet("https://m.facebook.com/ajax/dtsg/?__ajax__=true").ToString();
                string token = Regex.Match(rq, "\"token\":\"(.*?)\"").Groups[1].Value;
                return token;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 0-Không xác định, 1-live, 2-checkpoint, 3-sai pass, 4-sai email, 5-có 2fa
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pass"></param>
        /// <returns>0-Không xác định, 1-live, 2-checkpoint, 3-sai pass, 4-sai email, 5-có 2fa</returns>
        public static string CheckFacebookAccount(string email, string pass, string userAgent = "")
        {
            string output = "";
            try
            {
                string str = "email=" + WebUtility.UrlEncode(email) + "&pass=" + WebUtility.UrlEncode(pass);

                RequestXNet request = new RequestXNet("", userAgent);
                string html = request.RequestPost("https://mbasic.facebook.com/login/device-based/regular/login/?refsrc=https%3A%2F%2Fmbasic.facebook.com%2F&lwv=100&refid=8", str).ToString();
                if (html.Contains("id=\"checkpointSubmitButton\""))
                {
                    if (html.Contains("id=\"approvals_code\""))
                    {
                        output = "5|";
                        //Gửi request nhập mã 2fa
                    }
                    else
                    {
                        output = "2|";

                        //Check dạng cp
                        request = new RequestXNet("", userAgent);
                        request.RequestGet("https://www.facebook.com").ToString();
                        html = request.RequestPost("https://www.facebook.com/login/device-based/regular/login/?login_attempt=1&lwv=100", str).ToString();

                        string fb_dtsg = Regex.Match(html, "name=\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
                        string jazoest = Regex.Match(html, "name=\"jazoest\" value=\"(.*?)\"").Groups[1].Value;
                        string nh = Regex.Match(html, "name=\"nh\" value=\"(.*?)\"").Groups[1].Value;
                        string __rev = Regex.Match(html, "\"__spin_r\":(.*?),").Groups[1].Value;
                        string __spin_t = Regex.Match(html, "\"__spin_t\":(.*?),").Groups[1].Value;

                        string data = "jazoest=" + jazoest + "&fb_dtsg=" + fb_dtsg + "&nh=" + nh + "&submit[Continue]=Ti%E1%BA%BFp%20t%E1%BB%A5c&__user=0&__a=1&__dyn=7xe6Fo4OQ1PyUhxOnFwn84a2i5U4e1Fx-ewSwMxW0DUeUhw5cx60Vo1upE4W0OE2WxO0SobEa87i0n2US1vw4Ugao881FU3rw&__csr=&__req=5&__beoa=0&__pc=PHASED%3ADEFAULT&dpr=1&__rev=" + __rev + "&__s=op5tkm%3A2d4a9m%3A37z92b&__hsi=6789153697588537525-0&__spin_r=" + __rev + "&__spin_b=trunk&__spin_t=" + __spin_t;
                        html = request.RequestPost("https://www.facebook.com/checkpoint/async?next=https%3A%2F%2Fwww.facebook.com%2F", data);
                        html = request.RequestGet("https://www.facebook.com/checkpoint/?next");

                        var coll = Regex.Matches(html, "verification_method\" value=\"(.*?)\"");
                        if (coll.Count > 0)
                        {
                            for (int i = 0; i < coll.Count; i++)
                            {
                                output += CheckCheckpoint(coll[i].Groups[1].Value) + "-";
                            }
                            output = output.TrimEnd('-');
                        }
                        else
                        {
                            if (html.Contains("/checkpoint/dyi/?referrer=disabled_checkpoint"))
                                output += CheckCheckpoint("vhh");
                            else if (html.Contains("captcha-recaptcha"))
                                output += CheckCheckpoint("72h");
                            else if (html.Contains("name=\"submit[Log Out]\""))
                                output += "không thể xmdt";
                        }
                    }
                }
                else if (html.Contains("login_error"))
                {
                    //string check = Regex.Match(html, "login_error(.*?)</a>").Groups[1].Value;
                    //if (Regex.Match(check, "href=\"(.*?)\"").Groups[1].Value.Contains("/login/identify/"))
                    //{
                    //    output = "4|";
                    //}
                    //else if (Regex.Match(check, "href=\"(.*?)\"").Groups[1].Value.Contains("/recover/initiate/"))
                    //{
                    //    output = "3|";
                    //}
                    if (html.Contains("m_login_email"))
                    {
                        output = "3|";
                    }
                    else
                    {
                        output = "0|";
                    }
                }
                else
                {
                    output += "1|" + request.GetCookie();
                }
            }
            catch
            {
                output = "0|";
            }

            return output;
        }

        public static string GetCookieFromFacebookAccount(string email, string pass, string userAgent = "")
        {
            string str = "email=" + WebUtility.UrlEncode(email) + "&pass=" + WebUtility.UrlEncode(pass);
            RequestXNet request = new RequestXNet("", userAgent);

            //Cách 1
            //request.RequestGet("https://www.facebook.com").ToString();
            //request.RequestPost("https://www.facebook.com/login.php", str, "application/x-www-form-urlencoded").ToString();

            //Cách 2
            request.RequestPost("https://mbasic.facebook.com/login/device-based/regular/login/?refsrc=https%3A%2F%2Fmbasic.facebook.com%2F&lwv=100&refid=8", str).ToString();
            string Cookie = request.GetCookie();
            return Cookie;
        }

        public static string GetNameByUID(string uid, string token)
        {
            try
            {
                Common.RequestXNet request = new RequestXNet();
                string html = request.RequestGet("https://graph.facebook.com/" + uid + "?fields=name&access_token=" + token);
                JObject json = JObject.Parse(html);
                string name = json["name"].ToString();
                return name;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Send request add friend
        /// </summary>
        /// <param name="port"></param>
        /// <param name="cookie"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static bool SendRequestAddFriend(string cookie, string uid, string proxy = "")
        {
            try
            {
                //request.AddHeader("DNT", "1");
                RequestXNet request = new RequestXNet(cookie, "", proxy);

                var body = request.RequestGet("https://mbasic.facebook.com/" + uid).ToString();
                var url = Regex.Match(body, "/a/mobile/friends/profile_add_friend.php(.*?)\"").Groups[1].Value;
                url = WebUtility.HtmlDecode(url);
                if (!string.IsNullOrEmpty(url))
                {
                    body = request.RequestGet("https://mbasic.facebook.com/a/mobile/friends/profile_add_friend.php" + url).ToString();
                    url = Regex.Match(body, "/a/mobile/friends/profile_add_friend.php(.*?)\"").Groups[1].Value;
                    return string.IsNullOrEmpty(url);
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool AcceptRequestAddFriend(string cookie, string uid, string proxy = "")
        {
            try
            {
                RequestXNet request = new RequestXNet(cookie, "", proxy);

                var body = request.RequestGet("https://mbasic.facebook.com/" + uid).ToString();
                var url = Regex.Match(body, "/a/mobile/friends/profile_add_friend.php(.*?)\"").Groups[1].Value;
                url = WebUtility.HtmlDecode(url);
                if (!string.IsNullOrEmpty(url))
                {
                    body = request.RequestGet("https://mbasic.facebook.com/a/mobile/friends/profile_add_friend.php" + url).ToString();
                    url = Regex.Match(body, "/a/mobile/friends/profile_add_friend.php(.*?)\"").Groups[1].Value;
                    return string.IsNullOrEmpty(url);
                }
            }
            catch
            {
            }

            return false;
        }
        public static string GetTokenEaagFromCookie(string cookie, string proxy = "")
        {
            RequestXNet request = new RequestXNet(cookie, "", proxy);

            string GetDataToken = request.RequestGet("https://business.facebook.com/business_locations/");
            string Token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
            return Token;
        }
        public static List<string> GetListAdsID(string token, string proxy = "")
        {
            List<string> lst = new List<string>();
            try
            {
                RequestXNet request = new RequestXNet("", "", proxy);

                var body = request.RequestGet("https://graph.facebook.com/v4.0/me/personal_ad_accounts?fields=id&limit=99999&access_token=" + token).ToString();
                JObject json = JObject.Parse(body);
                foreach (var item in json["data"])
                {
                    lst.Add(item["id"].ToString());
                }
            }
            catch
            {
            }

            return lst;
        }

        public static int ShareAds(string token, string toUid, string proxy = "")
        {
            int countAds = 0;
            try
            {
                RequestXNet request = new RequestXNet("", "", proxy);
                string body = "";

                //get list ads_id from token
                List<string> lst = GetListAdsID(token, proxy);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        body = request.RequestGet("https://graph.facebook.com/v4.0/" + lst[i] + "/users?uid=" + toUid + "&role=1001&access_token=" + token + "&format=json&method=post").ToString();
                        if (Convert.ToBoolean(JObject.Parse(body)["success"]))
                            countAds++;
                    }
                }
            }
            catch
            {
            }

            return countAds;
        }
        /// <summary>
        /// Tắt thông báo mới nhất
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static bool DisableNotify(string cookie, string proxy = "")
        {
            try
            {
                RequestXNet request = new RequestXNet(cookie, "", proxy);

                var body = request.RequestGet("https://mbasic.facebook.com/notifications.php").ToString();
                var url = Regex.Match(body, "/a/notifications.php(.*?)\"").Value.TrimEnd('"');
                url = WebUtility.HtmlDecode(url);
                if (!string.IsNullOrEmpty(url))
                {
                    body = request.RequestGet("https://mbasic.facebook.com/" + url).ToString();
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
        #endregion

        #region Other
        public static string CheckCheckpoint(string value)
        {
            string stt = "";
            switch (value)
            {
                case "3":
                    stt = "Ảnh";
                    break;
                case "2":
                    stt = "Ngày sinh";
                    break;
                case "20":
                    stt = "Tin nhắn";
                    break;
                case "4":
                case "34":
                    stt = "Otp";
                    break;
                case "14":
                    stt = "Thiết bị";
                    break;
                case "26":
                    stt = "Nhờ bạn bè";
                    break;
                case "18":
                    stt = "Bình luận";
                    break;
                case "72h":
                    stt = "72h";
                    break;
                case "vhh":
                    stt = "Vô hiệu hóa";
                    break;
                case "id_upload":
                    stt = "Up ảnh";
                    break;
                default:
                    break;
            }
            return stt;
        }
        public static string ConvertToStandardCookie(string cookie)
        {
            try
            {
                string c1 = Regex.Match(cookie, "c_user=(.*?);").Value;
                string c2 = Regex.Match(cookie, "xs=(.*?);").Value;
                string c3 = Regex.Match(cookie, "fr=(.*?);").Value;
                string c4 = Regex.Match(cookie, "datr=(.*?);").Value;
                return c1 + c2 + c3 + c4;
            }
            catch
            {
                return cookie;
            }
        }
        #endregion
    }
}
