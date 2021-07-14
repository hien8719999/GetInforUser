using Common;
using DeviceId;
using License.RNCryptor;
using Max_Join.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Template_MinSoftware
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Khi nào xuất tool thì phải comment 2 dòng này lại
            Application.Run(new fMain(""));
            return;

            string[] nameProcess = new string[] { "fiddler", "charles", "wireshark", "burp", "dnspy", "megadumper" };
            Process.GetProcesses().Where(p => nameProcess.Any(p.ProcessName.ToLower().Contains)).ToList().ForEach(y => y.Kill());
            Process.GetProcesses().Where(p => nameProcess.Any(p.MainWindowTitle.ToLower().Contains)).ToList().ForEach(y => y.Kill());
            if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator) == false)
            {
                MessageBox.Show("Vui lòng chạy bằng quyền Admin!" + "\r\n" + "Please Run Aplication As Administrator!", "Warning!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            string hostFileLocation = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
            if (File.Exists(hostFileLocation))
            {
                List<string> hostEntries = new List<string>() { "app.minsoftware.vn", "minsoftware.vn" };
                string hostFileTmpLocation = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts_tmp";
                using (StreamReader sr = new StreamReader(hostFileLocation))
                {
                    string currentLine = string.Empty;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (hostEntries.Contains(currentLine.ToLower()))
                        {
                            MessageBox.Show("Vui lòng cấu hình lại file hosts nếu muốn mở phần mềm!");
                            Environment.Exit(0);
                        }
                    }
                }
            }

            string userName = ""; string pass = "";

            userName = Settings.Default.UserName;
            pass = Settings.Default.PassWord;
            string deviceId = Common.CommonCSharp.Md5Encode(new DeviceIdBuilder().AddMachineName().AddProcessorId().AddMotherboardSerialNumber().AddSystemDriveSerialNumber().ToString());
            if (userName == "" || pass == "")
            {
                Application.Run(new fActive(1, deviceId));
                return;
            }
            if (Common.CommonCSharp.IsValidMail(userName) == false)
            {
                Application.Run(new fActive(1, deviceId));
                return;
            }

            RequestHttp request = new RequestHttp();
            Decryptor decrypt = new Decryptor();
            Random rd = new Random();
            int iRandom = rd.Next(0, 999999);

            string api_token = Common.CommonCSharp.ReadHTMLCode("http://app.minsoftware.vn/api/auth?datavery=" + Common.CommonCSharp.Base64Encode(userName + "|" + pass)).Replace("\"", "");
            if (api_token.Trim() == "")
            {
                Application.Run(new fActive(1, deviceId));
                return;
            }
            int codekeyrandom = rd.Next(0, 10000) + rd.Next(100, 1000);
            string url = "http://app.minsoftware.vn/minapi/minapi/api.php/checkkeynew?data=";
            string strRequest = deviceId + "|" + api_token + "|" + fMain.softIndex + "|" + codekeyrandom + "|" + "tungxuan94";
            Encryptor encrypt = new Encryptor();
            string strEncryptRequest = encrypt.Encrypt(strRequest, "minsoftwarenew94");
            string checkLisence = request.RequestGet(url + strEncryptRequest).Replace("\"", "");
            checkLisence = Common.CommonCSharp.Base64Decode(checkLisence);
            checkLisence = decrypt.Decrypt(checkLisence, "minsoftwarenew94"+codekeyrandom);
            if (checkLisence == null || checkLisence == "null")
            {
                MessageBox.Show("Lỗi hệ thống!!!");
                return;
            }
            if (checkLisence.Contains("chuakichhoat"))
            {
                Application.Run(new fActive(3, deviceId));
                return;
            }
            if (checkLisence.Contains("error"))
            {
                Application.Run(new fActive(1, deviceId));
                return;
            }

            if (checkLisence.Contains("hethan"))
            {
                Application.Run(new fActive(3, deviceId));
                return;
            }

            string full_name = checkLisence.Split('|')[0];
            string api_token_sv = checkLisence.Split('|')[1];
            string date_exp = checkLisence.Split('|')[2];
            string device_server = checkLisence.Split('|')[3];
            string codekeyrandom_sv = checkLisence.Split('|')[4];
            string keystatic_sv = checkLisence.Split('|')[5];

            if (deviceId != device_server || api_token_sv != api_token || codekeyrandom_sv != codekeyrandom.ToString() || keystatic_sv != "tungxuan94")
            {
                Application.Run(new fActive(1, deviceId));
                return;
            }
            else
            {
                Application.Run(new fMain(checkLisence));
            }
        }

    }
}
