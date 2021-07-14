﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet;

namespace Common
{
    public class RequestXNet
    {
        public RequestXNet(string cookie = "", string userAgent = "", string proxy = "")
        {
            //Set UserAgent
            if (userAgent == "")
                userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
            this.request = new xNet.HttpRequest
            {
                KeepAlive = true,
                AllowAutoRedirect=true,
                Cookies = new CookieDictionary(),
                UserAgent = userAgent
            };
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.AddHeader("Accept-Language", "en-US,en;q=0.9");

            //Add cookie
            if (cookie != "")
            {
                AddCookie(cookie);
            }
            
            if (proxy != "")
            {
                int dem = proxy.Split(':').Count();
                switch (dem)
                {
                    case 1:
                        request.Proxy = xNet.Socks5ProxyClient.Parse("127.0.0.1:" + proxy);
                        break;
                    case 2:
                        request.Proxy = xNet.Socks5ProxyClient.Parse(proxy);
                        break;
                    case 4:
                        request.Proxy = new xNet.HttpProxyClient(proxy.Split(':')[0], Convert.ToInt32(proxy.Split(':')[1]), proxy.Split(':')[2], proxy.Split(':')[3]);
                        break;
                    default:
                        break;
                }
            }
        }

        public xNet.HttpRequest request;

        public string RequestGet(string url)
        {
            return request.Get(url).ToString();
        }
        public string RequestPost(string url, string data = "", string contentType = "application/x-www-form-urlencoded")
        {
            try
            {
                if (data == "" || contentType == "")
                    return request.Post(url).ToString();
                else
                    return request.Post(url, data, contentType).ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void AddCookie(string cookie)
        {
            var temp = cookie.Split(';');
            foreach (var item in temp)
            {
                var temp2 = item.Split('=');
                if (temp2.Count() > 1 && !request.Cookies.ContainsKey(temp2[0]))
                {
                    request.Cookies.Add(temp2[0], temp2[1]);
                }
            }
        }

        public string GetCookie()
        {
            return request.Cookies.ToString();
        }
    }
}
