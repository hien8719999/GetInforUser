using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace Common
{
    public class CommonCSharp
    {
        public static double ConvertDatetimeToTimestamp(DateTime value)
        {
            TimeSpan span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            return (double)span.TotalSeconds;
        }
        public static DateTime ConvertStringToDatetime(string datetime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            return DateTime.ParseExact(datetime, format, System.Globalization.CultureInfo.InvariantCulture);
        }
        public static void ShowMessageBox(object s)
        {
            MessageBox.Show(s.ToString());
        }

        /// <summary>
        /// Remove empty item of List
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static List<string> RemoveEmptyItems(List<string> lst)
        {
            List<string> lstOutput = new List<string>();
            string item = "";
            for (int i = 0; i < lst.Count; i++)
            {
                item = lst[i].Trim();
                if (item != "")
                    lstOutput.Add(item);
            }
            return lstOutput;
        }

        /// <summary>
        /// Auto fake ip Dcom
        /// </summary>
        /// <param name="profileDcom"></param>
        public static void resetDcom(string profileDcom)
        {
            Process process = new Process();
            process.StartInfo.FileName = "rasdial.exe";
            process.StartInfo.Arguments = "\"" + profileDcom + "\" /disconnect";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();

            Thread.Sleep(3000);
            process = new Process();
            process.StartInfo.FileName = "rasdial.exe";
            process.StartInfo.Arguments = "\"" + profileDcom + "\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();
            Thread.Sleep(1500);
        }
        public static string TrimEnd(string text, string value)
        {
            if (!text.EndsWith(value))
                return text;

            return text.Remove(text.LastIndexOf(value));
        }
        public static void SaveDatagridview(DataGridView dgv, string namePath)
        {
            List<string> list = new List<string>();
            string row = "";
            object r = null;
            for (int j = 0; j < dgv.RowCount; j++)
            {
                row = "";
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    r = dgv.Rows[j].Cells[i].Value;
                    row += r == null ? "" : r + "|";
                }
                row = row.TrimEnd('|');
                list.Add(row);
            }
            File.WriteAllLines(namePath, list);
        }
        public static void LoadDatagridview(DataGridView dgv, string namePath)
        {
            List<string> list = File.ReadAllLines(namePath).ToList();
            string row = "";
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    row = list[i];
                    dgv.Rows.Add(row.Split('|'));
                }
            }
        }
        public static string CheckIP(string port911 = "")
        {
            string ip = "";
            try
            {
                RequestXNet request = new RequestXNet("", "", port911);
                string rq = request.RequestGet("http://lumtest.com/myip.json");
                ip = JObject.Parse(rq)["ip"].ToString();
            }
            catch
            {
            }
            return ip;
        }
        public static string SelectFolder()
        {
            string path = "";
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        path = fbd.SelectedPath;
                    }
                }
            }
            catch { }
            return path;
        }

        public static void UpdateStatusDataGridView(DataGridView dgv, int row, string colName, object status)
        {
            try
            {
                dgv.Invoke(new MethodInvoker(delegate ()
                {
                    dgv.Rows[row].Cells[colName].Value = status;
                }));
            }
            catch { }
        }

        public static string GetStatusDataGridView(DataGridView dgv, int row, string colName)
        {
            string output = "";
            try
            {
                dgv.Invoke(new MethodInvoker(delegate ()
                {
                    output = dgv.Rows[row].Cells[colName].Value.ToString();
                }));
            }
            catch { }
            return output;
        }

        public static void KillProcess(string nameProcess)
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(nameProcess))
                {
                    proc.Kill();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Check string is contain latinh char?
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool CheckBasicString(string text)
        {
            bool result = true;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.'))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// Remove char is not latin in string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveCharNotLatin(string text)
        {
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    result += c;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert Text to UTF-8
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ConvertToUTF8(string text)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);
            text = Encoding.UTF8.GetString(bytes);
            return text;
        }
        public static bool IsContainNumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (Char.IsDigit(c))
                    return true;
            }
            return false;
        }

        public static void ReadHtmlText(string text)
        {
            string path = "zzz999.html";
            File.WriteAllText(path, text);
            Process.Start(path);
        }
        public static string ReadHTMLCode(string Url)
        {
            try
            {
                return new RequestHttp().RequestGet(Url);
                WebClient webClient = new WebClient();
                byte[] reqHTML = webClient.DownloadData(Url);
                UTF8Encoding objUTF8 = new UTF8Encoding();
                return objUTF8.GetString(reqHTML);
            }
            catch
            {
                return null;
            }
        }
        //kiem tra dinh dang mail
        public static bool IsValidMail(string emailaddress)
        {
            try
            {
                System.Net.Mail.MailAddress m = new System.Net.Mail.MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static string Md5Encode(string sChuoi)
        {
            MD5 obj = MD5.Create();
            byte[] data = obj.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sChuoi));
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                s.Append(data[i].ToString("X2"));
            }
            return s.ToString();
        }
        public static string Base64Encode(string text)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string CreateRandomString(int lengText, Random rd = null)
        {
            string outPut = "";
            if (rd == null)
                rd = new Random();
            string validChars = "abcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < lengText; i++)
            {
                outPut += validChars[rd.Next(0, validChars.Length)];
            }
            return outPut;
        }
        public static string CreateRandomNumber(int leng, Random rd = null)
        {
            string outPut = "";
            if (rd == null)
                rd = new Random();
            string validChars = "0123456789";
            for (int i = 0; i < leng; i++)
            {
                outPut += validChars[rd.Next(0, validChars.Length)];
            }
            return outPut;
        }

        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string RunCMD(string cmd)
        {
            Process cmdProcess;
            cmdProcess = new Process();
            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.Arguments = "/c " + cmd;
            cmdProcess.StartInfo.RedirectStandardOutput = true;
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.Start();
            string output = cmdProcess.StandardOutput.ReadToEnd();
            cmdProcess.WaitForExit();
            if (String.IsNullOrEmpty(output))
                return "";
            return output;
        }

        public static void DelayTime(double second)
        {
            Application.DoEvents();
            Thread.Sleep(Convert.ToInt32(second * 1000));
        }
        /// <summary>
        /// Dùng trong catch
        /// </summary>
        public static void ExportError(ChromeDriver chrome, string error)
        {
            try
            {
                Random rrrd = new Random();
                string fileName = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + rrrd.Next(1000, 9999);

                string html = "";
                if (chrome != null)
                {
                    html = chrome.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;").ToString();
                    Screenshot image = ((ITakesScreenshot)chrome).GetScreenshot();
                    image.SaveAsFile(@"log\images\" + fileName + ".png");
                    File.WriteAllText(@"log\html\" + fileName + ".html", html);
                }

                File.AppendAllText(@"log\log.txt", DateTime.Now + "|<" + fileName + ">|" + error + Environment.NewLine);
            }
            catch { }
        }
    }
}
