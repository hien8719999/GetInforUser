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
    public class CommonJSChrome
    {
        public static bool CheckLiveCookie(ChromeDriver chrome, string URL = "https://www.facebook.com")
        {
            bool isLive = false;
            try
            {
                int typeWeb = 0;//1-www, 2-m, 0-ko co
                if (chrome.Url.StartsWith("https://www.facebook") || chrome.Url.StartsWith("https://facebook"))
                    typeWeb = 1;
                else if (chrome.Url.StartsWith("https://m.facebook"))
                    typeWeb = 2;

                if (typeWeb == 0)
                {
                    chrome.Navigate().GoToUrl(URL);

                    if (chrome.Url.StartsWith("https://www.facebook") || chrome.Url.StartsWith("https://facebook"))
                        typeWeb = 1;
                    else if (chrome.Url.StartsWith("https://m.facebook"))
                        typeWeb = 2;
                }

                string body = "";
                if (typeWeb == 1)
                    body = (string)chrome.ExecuteScript("async function CheckLiveCookie() { var output = '0|0'; try { var response = await fetch('https://www.facebook.com/me'); if (response.ok) { var body = await response.text(); if (body.includes('id=\"code_in_cliff\"')||body.includes('name=\"new\"')||body.includes('name=\"c\"')) output = '1|0'; else if (!body.includes('checkpointSubmitButton') && !body.includes('checkpointBottomBar') && !body.includes('https://www.facebook.com/communitystandards/') && !body.includes('captcha_response') && !body.includes('FB:ACTION:OPEN_NT_SCREEN') && body.match(new RegExp('\"USER_ID\":\"(.*?)\"'))[1] == (document.cookie + ';').match(new RegExp('c_user=(.*?);'))[1]) output = '1|1'; } } catch {} return output; }; var c = await CheckLiveCookie(); return c");
                else
                {
                    body = (string)chrome.ExecuteScript("async function CheckLiveCookie() { var output = '0|0'; try { var response = await fetch('https://m.facebook.com/me'); if (response.ok) { var body = await response.text(); if (body.includes('id=\"code_in_cliff\"')||body.includes('name=\"new\"')||body.includes('changeemail')||body.includes('name=\"c\"')) output = '1|0'; else if (!body.includes('checkpointSubmitButton') && !body.includes('checkpointBottomBar') && !body.includes('captcha_response') && !body.includes('FB:ACTION:OPEN_NT_SCREEN') && body.match(new RegExp('\"USER_ID\":\"(.*?)\"'))[1] == (document.cookie+';').match(new RegExp('c_user=(.*?);'))[1]) output = '1|1'; } } catch {} return output; }; var c = await CheckLiveCookie();  return c;");
                    if (body.Split('|')[0] == "0")
                        body = (string)chrome.ExecuteScript("async function CheckLiveCookie() { var output = '0|0'; try { var response = await fetch('https://www.facebook.com/me'); if (response.ok) { var body = await response.text(); if (body.includes('id=\"code_in_cliff\"')||body.includes('name=\"new\"')||body.includes('name=\"c\"')) output = '1|0'; else if (!body.includes('checkpointSubmitButton') && !body.includes('checkpointBottomBar') && !body.includes('https://www.facebook.com/communitystandards/') && !body.includes('captcha_response') && !body.includes('FB:ACTION:OPEN_NT_SCREEN') && !body.includes('/help/203305893040179') && body.match(new RegExp('\"USER_ID\":\"(.*?)\"'))[1] == (document.cookie + ';').match(new RegExp('c_user=(.*?);'))[1]) output = '1|1'; } } catch {} return output; }; var c = await CheckLiveCookie(); return c");
                }

                if (body.Split('|')[0] == "1")
                    isLive = true;
            }
            catch
            {
            }
            return isLive;
        }

        public static string GetTokenEAAG(ChromeDriver chrome)
        {
            string token = "";
            try
            {
                if (!chrome.Url.Contains("https://business.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://business.facebook.com/");
                token = (string)chrome.ExecuteScript("async function GetTokenEaag() { var output = ''; try { var response = await fetch('https://business.facebook.com/business_locations/'); if (response.ok) { var body = await response.text(); output=body.match(new RegExp('EAAG(.*?)\"'))[0].replace('\"',''); } } catch {} return output; }; var c = await GetTokenEaag(); return c;");
            }
            catch
            {
            }
            return token;
        }
        public static string CheckNumberAds(ChromeDriver chrome)
        {
            try
            {
                if (!chrome.Url.Contains("https://www.facebook.com"))
                    chrome.Navigate().GoToUrl("https://www.facebook.com");
                string html = (string)chrome.ExecuteScript("async function CheckNumberAds() { var output = ''; try { var response = await fetch('https://www.facebook.com/ads/manager/accounts/'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await CheckNumberAds(); return c;");

                MatchCollection AdsID = Regex.Matches(html, "/ads/manage/campaigns/\\?act=(.*?)\"");
                List<string> lstAds = new List<string>();
                if (AdsID.Count > 0)
                {
                    string ads_Id = "";
                    for (int i = 0; i < AdsID.Count; i++)
                    {
                        ads_Id = AdsID[i].Groups[1].Value;
                        if (!lstAds.Contains(ads_Id))
                            lstAds.Add(ads_Id);
                    }
                }
                bool isCoChiTieu = false;
                MatchCollection matches = Regex.Matches(html, "lastRow(.*?)/td>");
                string money = "", money1 = "", money2 = "";
                for (int i = 0; i < matches.Count; i++)
                {
                    money = Regex.Match(matches[i].Groups[1].Value, ">(.*?)<").Groups[1].Value;
                    money = WebUtility.HtmlDecode(money);
                    money = CommonCSharp.ConvertToUTF8(money);
                    money2 = money == "" ? "0" : money;
                    if (money != "")
                    {
                        for (int j = 0; j < money.Length; j++)
                        {
                            if (char.IsDigit(money[j]))
                                money1 += money[j];
                        }
                        if (Convert.ToInt32(money1) > 0)
                        {
                            isCoChiTieu = true;
                            break;
                        }
                    }
                }
                return lstAds.Count + "|" + isCoChiTieu + "|" + money2;
            }
            catch
            {
                return 0 + "|" + false + "|" + 0;
            }
        }
        public static List<string> GetMyListUidNameFriend(ChromeDriver chrome, string token)
        {
            List<string> listFriend = new List<string>();

            if (!chrome.Url.Contains("https://graph.facebook.com/"))
                chrome.Navigate().GoToUrl("https://graph.facebook.com/");
            string getListFriend = (string)chrome.ExecuteScript("async function GetListUidNameFriend() { var output = ''; try { var response = await fetch('https://graph.facebook.com/me/friends?limit=5000&fields=id,name&access_token=" + token + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await GetListUidNameFriend(); return c;");

            JObject objFriend = JObject.Parse(getListFriend);
            if (objFriend["data"].Count() > 0)
            {
                for (int i = 0; i < objFriend["data"].Count(); i++)
                {
                    string uidFr = objFriend["data"][i]["id"].ToString();
                    string nameFr = objFriend["data"][i]["name"].ToString();
                    listFriend.Add(uidFr + "|" + nameFr);
                }
            }
            return listFriend;
        }
        public static List<string> GetMyListComments(ChromeDriver chrome, int numberMonth)
        {
            List<string> lstComment = new List<string>();
            try
            {
                if (!chrome.Url.Contains("https://mbasic.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://mbasic.facebook.com/");
                string link_mau = "https://mbasic.facebook.com/{0}/allactivity/?category_key=commentscluster&timestart={1}&timeend={2}";

                string uid = chrome.ExecuteScript("return (document.cookie + ';').match(new RegExp('c_user=(.*?);'))[1]").ToString();

                string timeStart = "";
                string timeEnd = "";

                string link = "";
                string htmlActivity = "";

                MatchCollection matchCollection = null;

                List<string> lstLink = new List<string>();
                for (int i = 0; i < numberMonth; i++)
                {
                    DateTime dateFrom = DateTime.Now.AddMonths(2 - i);
                    DateTime dateTo = DateTime.Now.AddMonths(1 - i);
                    timeStart = CommonCSharp.ConvertDatetimeToTimestamp(new DateTime(dateFrom.Year, dateFrom.Month, 1)).ToString();
                    timeEnd = CommonCSharp.ConvertDatetimeToTimestamp(new DateTime(dateTo.Year, dateTo.Month, 1)).ToString();
                    link = string.Format(link_mau, uid, timeStart, timeEnd);
                    lstLink.Add(link);
                }

                for (int k = 0; k < lstLink.Count; k++)
                {
                    link = lstLink[k];

                    bool isContinue = false;
                    do
                    {
                        isContinue = false;
                        //chrome.GotoURL(link);
                        htmlActivity = RequestGet(chrome, link, "https://mbasic.facebook.com/");

                        htmlActivity = WebUtility.HtmlDecode(htmlActivity);
                        matchCollection = Regex.Matches(htmlActivity, "<span>(.*?)</h4>");
                        for (int i = 0; i < matchCollection.Count; i++)
                        {
                            string text = matchCollection[i].Groups[1].Value;
                            text = text.Substring(0, text.LastIndexOf('<'));
                            MatchCollection match = Regex.Matches(text, "<(.*?)>");
                            for (int j = 0; j < match.Count; j++)
                                text = text.Replace(match[j].Value, "");
                            if (text != "" && !lstComment.Contains(text))
                                lstComment.Add(text);
                        }

                        if (Regex.IsMatch(htmlActivity, $"/{uid}/allactivity/\\?category_key=commentscluster&timeend(.*?)\""))
                        {
                            link = "https://mbasic.facebook.com" + Regex.Match(htmlActivity, $"/{uid}/allactivity/\\?category_key=commentscluster&timeend(.*?)\"").Value.Replace("\"", "");
                            isContinue = true;
                        }
                    } while (isContinue);
                }
            }
            catch
            { }
            return lstComment;
        }
        public static List<string> GetMyListUidMessage(ChromeDriver chrome)
        {
            List<string> lstMessage = new List<string>();
            try
            {
                if (!chrome.Url.Contains("https://mbasic.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://mbasic.facebook.com/");
                string htmlMessage = (string)chrome.ExecuteScript("async function GetListUidNameFriend() { var output = ''; try { var response = await fetch('https://mbasic.facebook.com/messages/'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await GetListUidNameFriend(); return c;");
                int moreAcc = 1;
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
                            uid = WebUtility.HtmlDecode(uid);
                            if (!lstMessage.Contains(uid))
                                lstMessage.Add(uid);
                        }
                        catch { }
                    }
                    linkReadMes = Regex.Match(htmlMessage, "/messages/.pageNum=(.*?)\"").Value.Replace("amp;", "");
                    htmlMessage = (string)chrome.ExecuteScript("async function GetListUidNameFriend() { var output = ''; try { var response = await fetch('https://mbasic.facebook.com" + linkReadMes + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await GetListUidNameFriend(); return c;");

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
        public static List<string> BackupImageOne(ChromeDriver chrome, string uidFr, string nameFr, string token, int countImage = 20)
        {
            List<string> listImageBackup = new List<string>();
            try
            {
                if (!chrome.Url.Contains("https://graph.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://graph.facebook.com/");
                string htmlImage = (string)chrome.ExecuteScript("async function GetListUidNameFriend() { var output = ''; try { var response = await fetch('https://graph.facebook.com/" + uidFr + "/photos?fields=images&limit=" + countImage + "&access_token=" + token + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await GetListUidNameFriend(); return c;");
                JObject objImg = JObject.Parse(htmlImage);
                int stt = 0;
                if (objImg != null && htmlImage.Contains("images"))
                {
                    for (int j = 0; j < objImg["data"].Count(); j++)
                    {
                        stt = objImg["data"][j]["images"].ToList().Count - 1;
                        listImageBackup.Add(uidFr + "*" + nameFr + "*" + objImg["data"][j]["images"][stt]["source"] + "|" + objImg["data"][j]["images"][stt]["width"] + "|" + objImg["data"][j]["images"][stt]["height"]);
                    }
                }
            }
            catch { }

            return listImageBackup;
        }
        public static string RequestGet(ChromeDriver chrome, string url, string website)
        {
            try
            {
                if (!chrome.Url.Contains(website))
                    chrome.Navigate().GoToUrl(website);
                string rq = (string)chrome.ExecuteScript("async function RequestGet() { var output = ''; try { var response = await fetch('" + url + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await RequestGet(); return c;");
                return rq;
            }
            catch { }

            return "";
        }
        public static string ScanMailCookie(ChromeDriver chrome, string website = "https://www.facebook.com")
        {
            try
            {
                List<string> listImageBackup = new List<string>();
                if (!chrome.Url.Contains(website))
                    chrome.Navigate().GoToUrl(website);
                string script1 = "async function ScanMailCookie() { var output = ''; try { var fb_dtsg = document.documentElement.innerHTML.match(new RegExp('name=\"fb_dtsg\" value=\"(.*?)\"'))[1]; var q = 'me(){friends{count,nodes{id,name,email_addresses,friends{count},subscribers{count},birthday,hometown,registration_time}}}'; var data = 'fb_dtsg=' + fb_dtsg + '&q=' + q; var response = await fetch('https://www.facebook.com/api/graphql/', { method: 'POST', body: data, headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await ScanMailCookie(); return c;";
                string script2 = "async function ScanMailCookie() { var output = ''; try { var fb_dtsg = document.documentElement.innerHTML.match(new RegExp('name=\"fb_dtsg\" value=\"(.*?)\"'))[1]; var q = 'me(){friends{count,nodes{email_addresses,birthday,hometown}}}'; var data = 'fb_dtsg=' + fb_dtsg + '&q=' + q; var response = await fetch('https://www.facebook.com/api/graphql/', { method: 'POST', body: data, headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await ScanMailCookie(); return c;";

                string x = (string)chrome.ExecuteScript(script1);
                JObject objUser = new JObject();

                try
                {
                    objUser = JObject.Parse(x);
                }
                catch
                {
                    x = (string)chrome.ExecuteScript(script2);
                    objUser = JObject.Parse(x);
                }
                return x;
            }
            catch { }

            return "";
        }
        public static string GetBirthday(ChromeDriver chrome, string token, string uid)
        {
            string output = "";
            try
            {
                if (!chrome.Url.Contains("https://graph.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://graph.facebook.com/");
                string rq = (string)chrome.ExecuteScript("async function RequestGet() { var output = ''; try { var response = await fetch('" + "https://graph.facebook.com/" + uid + "?fields=id,name,birthday&access_token=" + token + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await RequestGet(); return c;");
                JObject json = JObject.Parse(rq);
                return json["id"].ToString() + "|" + json["name"].ToString() + "|" + json["birthday"].ToString();
            }
            catch
            {
            }

            return output;
        }



        public static bool SendRequestAddFriend(ChromeDriver chrome, string uid)
        {
            try
            {
                if (!chrome.Url.Contains("https://mbasic.facebook.com/"))
                    chrome.Navigate().GoToUrl("https://mbasic.facebook.com/");

                var body = RequestGet(chrome, "https://mbasic.facebook.com/" + uid, "https://mbasic.facebook.com/");
                var url = Regex.Match(body, "/a/mobile/friends/profile_add_friend.php(.*?)\"").Groups[1].Value;
                url = WebUtility.HtmlDecode(url);
                if (!string.IsNullOrEmpty(url))
                {
                    body = RequestGet(chrome, "https://mbasic.facebook.com/a/mobile/friends/profile_add_friend.php" + url, "https://mbasic.facebook.com/");
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

        public static bool RemoveMessage(ChromeDriver chrome, string uid)
        {
            try
            {
                if (!chrome.Url.Contains("https://www.facebook.com"))
                    chrome.Navigate().GoToUrl("https://www.facebook.com");
                string js = "async function ScanMailCookie() { var output = ''; try { var uid_me = require([\"CurrentUserInitialData\"]).USER_ID; var uid_xoa = '" + uid + "'; var rev=require([\"SiteData\"]).client_revision; var spinT = require([\"SiteData\"]).__spin_t; var hsi = require([\"SiteData\"]).hsi; var jazoest = document.documentElement.innerHTML.match(new RegExp('name=\"jazoest\" value=\"(.*?)\"'))[1]; var fb_dtsg = require([\"DTSGInitData\"]).token; var data = 'ids[0]=' + uid_xoa + '&__user=' + uid_me + '&__a=1&__dyn=&__csr=&__req=1p&__beoa=0&__pc=PHASED%3ADEFAULT&dpr=1&__rev='+rev+'&__s=vmpfbx%3A367nnd%3Aejl3g0&__hsi='+hsi+'&__comet_req=0&fb_dtsg='+fb_dtsg+'&jazoest='+jazoest+'&__spin_r='+rev+'&__spin_b=trunk&__spin_t=' + spinT; var response = await fetch('https://www.facebook.com/ajax/mercury/delete_thread.php', { method: 'POST', body: data, headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'referer': 'https://www.facebook.com/messages' } }); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await ScanMailCookie(); return c;";
                var body = (string)chrome.ExecuteScript(js);
            }
            catch
            {
            }

            return false;
        }

        public static string CheckPageState(ChromeDriver chrome)
        {
            string result = "";
            try
            {
                string js = "var x=document.readyState; return x;";
                result = (string)chrome.ExecuteScript(js);
            }
            catch
            {
            }

            return result;
        }

    }
}
