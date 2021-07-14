using HttpRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RequestHttp
    {
        public RequestHttp(string cookie = "", string userAgent = "")
        {
            //Set UserAgent
            if (userAgent == "")
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
            else
                UserAgent = userAgent;

            request = new RequestHTTP();
            request.SetSSL(System.Net.SecurityProtocolType.Tls12);
            request.SetKeepAlive(true);
            request.SetDefaultHeaders(new string[]
            {
                "content-type: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                "user-agent: "+UserAgent
            });

            if (cookie != "")
            {
                AddCookie(cookie);
            }
        }

        private RequestHTTP request;
        private string UserAgent;

        public string RequestGet(string url, string proxy = "")
        {
            if (proxy != "") {
                if (proxy.Contains(":"))
                    return request.Request("GET", url, null, null, true, new System.Net.WebProxy(proxy.Split(':')[0], Convert.ToInt32(proxy.Split(':')[1]))).ToString();
                else
                    return request.Request("GET", url, null, null, true, new System.Net.WebProxy("127.0.0.1", Convert.ToInt32(proxy))).ToString();
            }
            else
                return request.Request("GET", url).ToString();
        }
        public string RequestPost(string url, string data = "", string proxy = "")
        {
            if (proxy != "")
            {
                if (proxy.Contains(":"))
                    return request.Request("POST", url, null, Encoding.ASCII.GetBytes(data), true, new System.Net.WebProxy(proxy.Split(':')[0], Convert.ToInt32(proxy.Split(':')[1]))).ToString();
                else
                    return request.Request("POST", url, null, Encoding.ASCII.GetBytes(data), true, new System.Net.WebProxy("127.0.0.1", Convert.ToInt32(proxy))).ToString();
            }
            else
                return request.Request("POST", url, null, Encoding.ASCII.GetBytes(data)).ToString();
        }

        public void AddCookie(string cookie)
        {
            var temp = cookie.Split(';');
            string cookie_temp = "";
            foreach (var item in temp)
            {
                var temp2 = item.Split('=');
                if (temp2.Count() > 1)
                {
                    cookie_temp += temp2[0] + "=" + temp2[1] + ";";
                }
            }
            request.SetDefaultHeaders(new string[]
            {
                "content-type: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8;charset=UTF-8",
                "user-agent: "+UserAgent,
                "cookie: "+cookie_temp
            });
        }

        public string GetCookie()
        {
            return request.GetCookiesString();
        }
    }
}
