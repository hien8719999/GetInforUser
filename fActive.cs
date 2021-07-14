using Common;
using DeviceId;
using HttpRequest;
using License.RNCryptor;
using Max_Join.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Template_MinSoftware
{
    public partial class fActive : Form
    {
        int typeError = 0;
        public fActive(int typeError, string idKey)
        {
            InitializeComponent();
            this.typeError = typeError;
            lblStatus.Text = GetStatusFromCode(typeError);
            lblKey.Text = idKey;
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private string GetStatusFromCode(int typeError = 0)
        {
            switch (typeError)
            {
                case 0:
                    return "";
                case 1:
                    return "Vui lòng đăng nhập để sử dụng phần mềm!!!";
                case 2:
                    return "Thiết bị của bạn chưa được kích hoạt!!!";
                case 3:
                    return "Thiết bị của bạn đã hết hạn sử dụng!!!";
                case 4:
                    return "Tài khoản hoặc mật khẩu bạn nhập không đúng!!!";
                default:
                    return "Lỗi không xác định!!!";
            }
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
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

            string userName = txbUserName.Text.Trim();
            string pass = txbPassword.Text.Trim();
            if (userName == "" || pass == "")
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (CommonCSharp.IsValidMail(txbUserName.Text) == false)
            {
                lblStatus.Invoke((MethodInvoker)delegate ()
                {
                    lblStatus.Text = "Email bạn nhập không đúng định dạng!";
                });
                return;
            }

            new Thread(new ThreadStart(() =>
            {
                btnLogin.Invoke((MethodInvoker)delegate ()
                {
                    btnLogin.Enabled = false;
                });
                lblStatus.Invoke((MethodInvoker)delegate ()
                {
                    lblStatus.Text = "Đang kiểm tra đăng nhập...";
                });
                try
                {
                    Encryptor encrypt = new Encryptor();
                    string deviceId = CommonCSharp.Md5Encode(new DeviceIdBuilder().AddMachineName().AddProcessorId().AddMotherboardSerialNumber().AddSystemDriveSerialNumber().ToString());
                    RequestHttp request = new RequestHttp();
                    Decryptor decrypt = new Decryptor();
                    Random rd = new Random();
                    string api_token = CommonCSharp.ReadHTMLCode("http://app.minsoftware.vn/api/auth?datavery=" + CommonCSharp.Base64Encode(userName + "|" + pass)).Replace("\"", "");
                    if (api_token.Trim() == "")
                    {
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = GetStatusFromCode(4);
                        });
                        btnLogin.Invoke((MethodInvoker)delegate ()
                        {
                            btnLogin.Enabled = true;
                        });
                        return;
                    }
                    int codekeyrandom = rd.Next(0, 10000) + rd.Next(100, 1000);
                    string url = "http://app.minsoftware.vn/minapi/minapi/api.php/checkkeynew?data=";
                    string strRequest = deviceId + "|" + api_token + "|" + fMain.softIndex + "|" + codekeyrandom + "|" + "tungxuan94";
                    string strEncryptRequest = encrypt.Encrypt(strRequest, "minsoftwarenew94");
                    string checkLisence = request.RequestGet(url + strEncryptRequest).Replace("\"", "");
                    checkLisence = CommonCSharp.Base64Decode(checkLisence);
                    checkLisence = decrypt.Decrypt(checkLisence, "minsoftwarenew94"+codekeyrandom);
                    if (checkLisence.Contains("chuakichhoat"))
                    {
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = GetStatusFromCode(2);
                        });
                        btnLogin.Invoke((MethodInvoker)delegate ()
                        {
                            btnLogin.Enabled = true;
                        });
                        return;
                    }
                    if (checkLisence.Contains("error"))
                    {
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = GetStatusFromCode(5);
                        });
                        btnLogin.Invoke((MethodInvoker)delegate ()
                        {
                            btnLogin.Enabled = true;
                        });
                        return;
                    }

                    if (checkLisence.Contains("hethan"))
                    {
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = GetStatusFromCode(3);
                        });
                        btnLogin.Invoke((MethodInvoker)delegate ()
                        {
                            btnLogin.Enabled = true;
                        });
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
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = "Không thể kích hoạt tài khoản của bạn, vui lòng thử lại!!!";
                        });
                        btnLogin.Invoke((MethodInvoker)delegate ()
                        {
                            btnLogin.Enabled = true;
                        });
                        return;
                    }
                    else
                    {
                        lblStatus.Invoke((MethodInvoker)delegate ()
                        {
                            lblStatus.Text = "Đăng nhập thành công!";
                        });
                        MessageBox.Show("Thiết bị của bạn đã được kích hoạt, cảm ơn đã sử dụng phần mềm của Min Software." + Environment.NewLine + Environment.NewLine + "Vui lòng mở lại phần mềm để sử dụng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Settings.Default.UserName = txbUserName.Text;
                        Settings.Default.PassWord = txbPassword.Text;
                        Settings.Default.Save();
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi không xác định!!!");
                }

                btnLogin.Invoke((MethodInvoker)delegate ()
                {
                    btnLogin.Enabled = true;
                });
            })).Start();
        }

        private void FActive_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("chrome.exe", "http://app.minsoftware.vn/signup");
            }
            catch
            {
                Process.Start("http://app.minsoftware.vn/signup");
            }
        }

        private void LblKey_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblKey.Text);
            MessageBox.Show("Đã copy mã thiết bị!");
        }

        private void fActive_Load(object sender, EventArgs e)
        {

        }

        private void fActive_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txbUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(null, null);
            }
        }
    }
}
