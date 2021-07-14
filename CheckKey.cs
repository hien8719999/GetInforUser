using DeviceId;
using License.RNCryptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public class CheckKey
    {
        public static void CheckVersion(string softname = "test")
        {
            string hostname = "https://minsoftware.xyz/file/" + softname + "/";
            try
            {
                WebClient ud = new WebClient();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ud.DownloadFileCompleted += new AsyncCompletedEventHandler(udcom);
                Uri update = new Uri(hostname + "update.ini");
                ud.DownloadFile(update, "./update/update.ini");

                //read server update file
                CommonIniFile ini_server = new CommonIniFile("./update/update.ini");
                string new_version = ini_server.Read("Version", "Infor");
                double dNewVersion = Convert.ToDouble(new_version.Replace(".", "").Insert(1, "."));
                //read local update file
                CommonIniFile ini_local = new CommonIniFile("update.ini");
                string old_version = ini_local.Read("Version", "Infor");
                double dOldVersion = Convert.ToDouble(old_version.Replace(".", "").Insert(1, "."));

                if (dNewVersion > dOldVersion)
                {
                    string content = "\r\n"+"Version: " + new_version;
                    content += "\r\n" + "Nội dung update:";
                    content += "\r\n" + CommonCSharp.Base64Decode(ini_server.Read("Content", "Infor"));
                    content += "\r\n\r\n" + "Bạn có muốn cập nhật phần mềm?";
                    if (MessageBox.Show("Đã có bản cập nhật mới!" + "\r\n" + content, "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        MessageBox.Show("Vui lòng tắt phần mềm rồi chạy file AutoUpdate.exe để tiến hành cập nhật phiên bản mới nhất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                }
            }
            catch
            {
            }
        }
    }
}
