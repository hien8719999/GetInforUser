using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keys = OpenQA.Selenium.Keys;

namespace Common
{
    public class CommonChrome
    {
        public static ChromeDriver OpenChrome(ChromeDriver chrome, bool isHideChrome, bool isHideImage, bool isDisableSound, string UserAgent, string LinkProfile,
            Point Size, Point Position, string Proxy, string LinkToOtherBrowser = "", int TimeWaitForSearchingElement = 0, int TimeWaitForLoadingPage = 60)
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            ChromeOptions options = new ChromeOptions();
            options.AddArguments(new string[] {
                "--disable-notifications",
                "--window-size="+Size.X+","+Size.Y,
                "--window-position="+Position.X+","+Position.Y,
                "--no-sandbox",
                "--disable-gpu",// applicable to windows os only
                "--disable-dev-shm-usage",//overcome limited resource problems       
                "--disable-web-security",
                "--disable-rtc-smoothness-algorithm",
                "--disable-webrtc-hw-decoding",
                "--disable-webrtc-hw-encoding",
                "--disable-webrtc-multiple-routes",
                "--disable-webrtc-hw-vp8-encoding",
                "--enforce-webrtc-ip-permission-check",
                "--force-webrtc-ip-handling-policy",
                "--ignore-certificate-errors",
                "--disable-infobars",
                "--disable-popup-blocking"
            });
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.plugins", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.popups", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.auto_select_certificate", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.mixed_script", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protocol_handlers", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.midi_sysex", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.push_messaging", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.ssl_cert_decisions", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.metro_switch_to_desktop", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protected_media_identifier", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.site_engagement", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.durable_storage", 1);
            //options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);//hide image
            options.AddUserProfilePreference("useAutomationExtension", true);

            if (isDisableSound)
            {
                options.AddArgument("--mute-audio");
            }

            if (!isHideChrome)
            {
                if (isHideImage)
                    options.AddArgument("--blink-settings=imagesEnabled=false");

                if (!string.IsNullOrEmpty(LinkProfile.Trim()))
                    options.AddArgument("--user-data-dir=" + LinkProfile);
            }
            else
            {
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddArgument("--headless");
            }

            if (!string.IsNullOrEmpty(Proxy.Trim()))
            {
                if (Proxy.Contains(":"))
                    options.AddArgument("--proxy-server= " + Proxy);
                else
                    options.AddArgument("--proxy-server= socks5://127.0.0.1:" + Proxy);
            }
            if (LinkToOtherBrowser != "")
            {
                options.AddArgument("disable-infobars");
                options.BinaryLocation = LinkToOtherBrowser;
            }

            if (!string.IsNullOrEmpty(UserAgent.Trim()))
                options.AddArgument("--user-agent=" + UserAgent);

            chrome = new ChromeDriver(service, options);

            chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(TimeWaitForSearchingElement);
            chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(TimeWaitForLoadingPage);
            return chrome;
        }
        public static void QuitChrome(ChromeDriver chrome)
        {
            try
            {
                chrome.Quit();
            }
            catch
            { }
        }
        public string GetURL(ChromeDriver chrome)
        {
            try
            {
                return chrome.Url;
            }
            catch (Exception ex)
            {
            }
            return "";
        }
        public static bool CheckChromeClosed(ChromeDriver chrome)
        {
            bool isClosed = true;
            try
            {
                var x = chrome.Title;
                isClosed = false;
            }
            catch
            { }
            return isClosed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeAttribute">1-Id, 2-Name, 3-Xpath</param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public static bool CheckExistElement(ChromeDriver chrome, int typeAttribute, string attributeValue, int timeOut = 0)
        {
            bool isExist = false;
            TimeSpan timeImplicitWait = chrome.Manage().Timeouts().ImplicitWait;
            chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);

            int timeStart = Environment.TickCount;
            while (true)
            {
                switch (typeAttribute)
                {
                    case 1:
                        isExist = chrome.FindElementsById(attributeValue).Count > 0;
                        break;
                    case 2:
                        isExist = chrome.FindElementsByName(attributeValue).Count > 0;
                        break;
                    case 3:
                        isExist = chrome.FindElementsByXPath(attributeValue).Count > 0;
                        break;
                    default:
                        break;
                }

                if (isExist)
                {
                    break;
                }
                else
                {
                    if (Environment.TickCount - timeStart > timeOut * 1000)
                    {
                        break;
                    }
                }
            }

            chrome.Manage().Timeouts().ImplicitWait = timeImplicitWait;
            return isExist;
        }

        public static bool NavigateChrome(ChromeDriver chrome, string url)
        {
            bool isSuccess = false;
            try
            {
                chrome.Navigate().GoToUrl(url);
                isSuccess = true;
            }
            catch
            { }
            return isSuccess;
        }

        public static bool SendEnterChrome(ChromeDriver chrome, int typeAttribute, string attributeValue)
        {
            bool isSuccess = false;
            try
            {
                switch (typeAttribute)
                {
                    case 1:
                        chrome.FindElementById(attributeValue).SendKeys(Keys.Enter);
                        break;
                    case 2:
                        chrome.FindElementByName(attributeValue).SendKeys(Keys.Enter);
                        break;
                    case 3:
                        chrome.FindElementByXPath(attributeValue).SendKeys(Keys.Enter);
                        break;
                    default:
                        break;
                }
                isSuccess = true;
            }
            catch
            {
            }
            return isSuccess;
        }
        public static bool ScrollChrome(ChromeDriver chrome, int x, int y)
        {
            bool isSuccess = false;
            try
            {
                var js1 = String.Format("window.scrollTo({0}, {1})", x, y);
                chrome.ExecuteScript(js1);
                isSuccess = true;
            }
            catch
            {
            }
            return isSuccess;
        }

        public static bool SendKeysChrome(ChromeDriver chrome, int typeAttribute, string attributeValue, string content, double timeDelay)
        {
            bool isSuccess = false;
            try
            {
                for (int i = 0; i < content.Length; i++)
                {
                    switch (typeAttribute)
                    {
                        case 1:
                            chrome.FindElementById(attributeValue).SendKeys(content[i].ToString());
                            break;
                        case 2:
                            chrome.FindElementByName(attributeValue).SendKeys(content[i].ToString());
                            break;
                        case 3:
                            chrome.FindElementByXPath(attributeValue).SendKeys(content[i].ToString());
                            break;
                        default:
                            break;
                    }

                    if (i < content.Length - 1)
                    {
                        Thread.Sleep(Convert.ToInt32(timeDelay * 1000));
                    }
                }
                isSuccess = true;
            }
            catch
            { }
            return isSuccess;
        }
        public static bool SendKeysChrome(ChromeDriver chrome, int typeAttribute, string attributeValue, string content)
        {
            bool isSuccess = false;
            try
            {
                switch (typeAttribute)
                {
                    case 1:
                        chrome.FindElementById(attributeValue).SendKeys(content);
                        break;
                    case 2:
                        chrome.FindElementByName(attributeValue).SendKeys(content);
                        break;
                    case 3:
                        chrome.FindElementByXPath(attributeValue).SendKeys(content);
                        break;
                    default:
                        break;
                }
                isSuccess = true;
            }
            catch
            {
            }
            return isSuccess;
        }
        public static bool ClickChrome(ChromeDriver chrome, int typeAttribute, string attributeValue)
        {
            bool isSuccess = false;
            try
            {
                switch (typeAttribute)
                {
                    case 1:
                        chrome.FindElementById(attributeValue).Click();
                        break;
                    case 2:
                        chrome.FindElementByName(attributeValue).Click();
                        break;
                    case 3:
                        chrome.FindElementByXPath(attributeValue).Click();
                        break;
                    default:
                        break;
                }
                isSuccess = true;
            }
            catch
            {
            }
            return isSuccess;
        }
        public static string GetXPath(ChromeDriver chrome, string tagName, string attribute, string attributeValue)
        {
            try
            {
                string xpath = (string)chrome.ExecuteScript("function getXPathForElement(element) { const idx = (sib, name) => sib ? idx(sib.previousElementSibling, name||sib.localName) + (sib.localName == name) : 1; const segs = elm => !elm || elm.nodeType !== 1 ? [''] : elm.id && document.getElementById(elm.id) === elm ? [`//*[@id=\"${elm.id}\"]`] : [...segs(elm.parentNode), `${elm.localName.toLowerCase()}[${idx(elm)}]`]; return segs(element).join('/'); } function getElementByXPath(path) { return (new XPathEvaluator()) .evaluate(path, document.documentElement, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null) .singleNodeValue; } ; var x=document.getElementsByTagName('" + tagName + "');for(i=0;i<x.length;i++){if(x[i].getAttribute('" + attribute + "')=='" + attributeValue + "') {y=x[i];break;}}; var output=getXPathForElement(y).split('[1]').join('');return output;");
                return xpath;
            }
            catch
            {
                return "";
            }
        }
        public static void ClickChrome(ChromeDriver chrome, string tagName, string attribute, string attributeValue)
        {
            try
            {
                chrome.ExecuteScript("var x=document.getElementsByTagName('" + tagName + "');for(i=0;i<x.length;i++){if(x[i].getAttribute('" + attribute + "')=='" + attributeValue + "') {x[i].click();break;}}");
            }
            catch
            {
            }
        }
        public static bool CheckExistElement(ChromeDriver chrome, string tagName, string attribute, string attributeValue, int timeOut = 0)
        {
            bool isExist = false;
            int timeStart = Environment.TickCount;
            while (true)
            {
                isExist = Convert.ToBoolean(chrome.ExecuteScript("var check=false;var x = document.getElementsByTagName('" + tagName + "'); for (i = 0; i < x.length; i++) { if (x[i].getAttribute('" + attribute + "') == '" + attributeValue + "') { check=true; break; } };return check+'';"));
                if (isExist)
                {
                    break;
                }
                else
                {
                    if (Environment.TickCount - timeStart > timeOut * 1000)
                    {
                        break;
                    }
                }
            }
            return isExist;
        }
        public static bool ExecuteScriptChrome(ChromeDriver chrome, string script)
        {
            bool isSuccess = false;
            try
            {
                chrome.ExecuteScript(script);
                isSuccess = true;
            }
            catch
            {
            }
            return isSuccess;
        }

        public static void AddCookieIntoChrome(ChromeDriver chrome, string cookie, string domain = ".facebook.com")
        {
            string[] arrData = cookie.Split(';');
            foreach (string item in arrData)
            {
                if (item.Trim() != "")
                {
                    string[] pars = item.Split('=');
                    if (pars.Count() > 1)
                    {
                        OpenQA.Selenium.Cookie cok = new OpenQA.Selenium.Cookie(pars[0].Trim(), pars[1].Trim(), domain, "/", DateTime.Now.AddDays(10));
                        chrome.Manage().Cookies.AddCookie(cok);
                    }
                }
            }
        }
        public static string GetCookieFromChrome(ChromeDriver chrome, string domain = "facebook")
        {
            string cookie = "";
            var sess = chrome.Manage().Cookies.AllCookies.ToArray();
            foreach (var item in sess)
            {
                if (item.Domain.Contains(domain))
                    cookie += item.Name + "=" + item.Value + ";";
            }
            return cookie;
        }

        #region Chia Màn hình 3x2
        public static int getWidthScreen = Screen.PrimaryScreen.Bounds.Width;
        public static int getHeightScreen = Screen.PrimaryScreen.Bounds.Height;
        public static Point GetSizeChrome()
        {
            int getWidthChrome = (2 * getWidthScreen) / 6;
            int getHeightChrome = getHeightScreen / 2;
            return new Point(getWidthChrome, getHeightChrome);
        }
        public static Point GetPointFromIndexPosition(int indexPos, int maxApp = 6)
        {
            Point location = new Point();
            int widthWindowChrome = (2 * getWidthScreen) / maxApp;
            int totalAppPerLine = maxApp / 2;
            while (indexPos > 5)
            {
                indexPos -= 6;
            }
            if (indexPos <= totalAppPerLine - 1)
            {
                location.Y = 0;
            }
            else if (indexPos < maxApp)
            {
                location.Y = getHeightScreen / 2;
                indexPos -= totalAppPerLine;
            }
            location.X = (indexPos) * (widthWindowChrome);
            return location;
        }
        public static int GetIndexOfPossitionApp(ref List<int> lstPossition)
        {
            int indexPos = 0;
            lock (lstPossition)
            {
                for (int i = 0; i < lstPossition.Count; i++)
                {
                    if (lstPossition[i] == 0)
                    {
                        indexPos = i;
                        lstPossition[i] = 1;
                        break;
                    }
                }
            }
            return indexPos;
        }
        public static void FillIndexPossition(ref List<int> lstPossition, int indexPos)
        {
            lock (lstPossition)
            {
                lstPossition[indexPos] = 0;
            }
        }
        #endregion
    }
}
