using HttpRequest;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Max_Join.Properties;
using Common;
using License.RNCryptor;
using DeviceId;
using System.Net;
using System.Security.Principal;

namespace Template_MinSoftware
{
    public partial class fMain : Form
    {
        public fMain(string log)
        {
            //CheckKey.CheckVersion(nameSoft);
            InitializeComponent();

            //string[] dt = log.Split('|');
            //lblDateExpried.Text = Convert.ToDateTime(dt[2]).ToString("dd/MM/yyyy");
            //lblKey.Text = dt[3].Substring(0, 10) + "****";
            //lblUser.Text = dt[0];
        }

        public const int softIndex = 44;
        public const string nameSoft = "maxbackup";
        public string deviceId = "";

        #region Close, Min, Max Form
        private void btn1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            CloseApplication();
            try
            {
                Environment.Exit(0);
            }
            catch
            {
                Application.Exit();
            }
        }
        #endregion

        #region Variable
        Random rd = new Random();
        bool isStop = false;
        object _lock = new object();
        object _lock2 = new object();
        object _lock3 = new object();
        object _lock4 = new object();

        //Cấu hình chung
        int iSoLuongAnh = 0;
        bool isFakeUserAgent = false;
        List<string> lstUserAgent = new List<string>();
        bool isUseTokenTg = false;
        List<string> lstToken = new List<string>();

        //Cấu hình đổi IP
        int iTypeChangeIP = 0;//0-ko đổi, 1-dcom, 2-hma, 3-connect 911
        int iCountChangeIP = 0;
        string sProfileDcom = "";
        #endregion

        #region Method

        #region setting
        private void SaveSetting()
        {
            //Save configs
            Settings.Default.nudThread = Convert.ToInt32(nudThread.Value);
            Settings.Default.nudSoLuongAnh = Convert.ToInt32(nudSoLuongAnh.Value);
            Settings.Default.ckbFakeUa = ckbFakeUa.Checked;
            Settings.Default.ckbUseTokenTg = ckbUseTokenTg.Checked;

            Settings.Default.nudChangeIpCount = Convert.ToInt32(nudChangeIpCount.Value);
            int i = 0;
            if (rdChangeIPNone.Checked == true)
                i = 0;
            else if (rdChangeIPDcom.Checked == true)
                i = 1;
            else if (rdChangeIPHMA.Checked == true)
                i = 2;
            else if (rdConnect911.Checked == true)
                i = 3;
            Settings.Default.iTypeChangeIP = i;
            Settings.Default.txtProfileNameDcom = txtProfileNameDcom.Text;
            Settings.Default.txtPort911 = txtPort911.Text;
            Settings.Default.Save();
        }
        private void LoadSetting()
        {
            //Load configs
            nudThread.Value = Settings.Default.nudThread;
            nudSoLuongAnh.Value = Settings.Default.nudSoLuongAnh;
            ckbFakeUa.Checked = Settings.Default.ckbFakeUa;
            ckbUseTokenTg.Checked = Settings.Default.ckbUseTokenTg;

            nudChangeIpCount.Value = Settings.Default.nudChangeIpCount;
            int i = Settings.Default.iTypeChangeIP;
            if (i == 0)
                rdChangeIPNone.Checked = true;
            else if (i == 1)
                rdChangeIPDcom.Checked = true;
            else if (i == 2)
                rdChangeIPHMA.Checked = true;
            else if (i == 3)
                rdConnect911.Checked = true;
            txtProfileNameDcom.Text = Settings.Default.txtProfileNameDcom;
            txtPort911.Text = Settings.Default.txtPort911;
        }

        private void SaveDatagridview()
        {
            string namePath = @"input\data.txt";
            CommonCSharp.SaveDatagridview(dtgvAcc, namePath);
        }
        private void LoadDatagridview()
        {
            string namePath = @"input\data.txt";
            CommonCSharp.LoadDatagridview(dtgvAcc, namePath);
            UpdateSelectCount();
        }
        protected override void OnLoad(EventArgs args)
        {
            Application.Idle += this.OnLoaded;
        }
        private void OnLoaded(object sender, EventArgs e)
        {
            Application.Idle -= this.OnLoaded;
            StartApplication();
        }
        private void CloseApplication()
        {
            SaveDatagridview();
            SaveSetting();
            CommonCSharp.KillProcess("chromedriver");
        }
        private void StartApplication()
        {
            //LoadCheck();
            EnableSoftware();
            LoadSetting();
            LoadDatagridview();
            CheckedChangedFull();
        }

        private void EnableSoftware()
        {
            grAccount.Enabled = true;
            grConfig.Enabled = true;
        }

        private void LoadCheck()
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

            deviceId = CommonCSharp.Md5Encode(new DeviceIdBuilder().AddMachineName().AddProcessorId().AddMotherboardSerialNumber().AddSystemDriveSerialNumber().ToString());
            try
            {
                string userName = ""; string pass = "";
                userName = Settings.Default.UserName;
                pass = Settings.Default.PassWord;

                if (userName == "" || pass == "" || CommonCSharp.IsValidMail(userName) == false)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.Hide();
                    });
                    fActive fa = new fActive(0, deviceId);
                    fa.ShowInTaskbar = true;
                    fa.ShowDialog();
                    return;
                }

                RequestHttp request = new RequestHttp();
                string api_token = CommonCSharp.ReadHTMLCode("http://app.minsoftware.vn/api/auth?datavery=" + CommonCSharp.Base64Encode(userName + "|" + pass)).Replace("\"", "");
                if (api_token.Trim() == "")
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.Hide();
                    });
                    fActive fa = new fActive(0, deviceId);
                    fa.ShowInTaskbar = true;
                    fa.ShowDialog();
                    return;
                }

                Random rd = new Random();
                int codekeyrandom = rd.Next(100000, 999999);
                string url = "http://app.minsoftware.vn/minapi/minapi/api.php/checkkeynew?data=";
                string strRequest = deviceId + "|" + api_token + "|" + fMain.softIndex + "|" + codekeyrandom + "|" + "tungxuan94";

                Encryptor encrypt = new Encryptor();
                string strEncryptRequest = encrypt.Encrypt(strRequest, "minsoftwarenew94");

                string checkLisence = request.RequestGet(url + strEncryptRequest).Replace("\"", "");
                checkLisence = CommonCSharp.Base64Decode(checkLisence);

                Decryptor decrypt = new Decryptor();
                checkLisence = decrypt.Decrypt(checkLisence, "minsoftwarenew94" + codekeyrandom);
                if (checkLisence.Contains("chuakichhoat") || checkLisence.Contains("error") || checkLisence.Contains("hethan"))
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.Hide();
                    });
                    fActive fa = new fActive(0, deviceId);
                    fa.ShowInTaskbar = true;
                    fa.ShowDialog();
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
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.Hide();
                    });
                    fActive fa = new fActive(0, deviceId);
                    fa.ShowInTaskbar = true;
                    fa.ShowDialog();
                    return;
                }
                else
                {
                    lblStatus.Text = "Đã kích hoạt";
                }
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Hide();
                });
                fActive fa = new fActive(0, deviceId);
                fa.ShowInTaskbar = true;
                fa.ShowDialog();
                return;
            }
        }
        private void rControl(string dt)
        {
            if (dt == "start")
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    btnPause.Enabled = true;
                    btnJoin.Enabled = false;
                });
            }
            else if (dt == "stop")
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    btnPause.Text = "Tạm dừng";
                    btnPause.Enabled = false;
                    btnJoin.Enabled = true;
                });
            }
        }
        private void UpdateSelectCount()
        {
            int count = 0;
            for (int i = 0; i < dtgvAcc.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                {
                    count++;
                }
            }
            try
            {
                lblChoosedCookie.Invoke((MethodInvoker)delegate ()
                {
                    lblChoosedCookie.Text = count.ToString();
                });
            }
            catch { }
        }
        private void CountAccount()
        {
            try
            {
                lblCountAccount.Text = dtgvAcc.Rows.Count.ToString();
            }
            catch { }
        }
        #endregion
        #endregion
        private void CheckCookie(int row, string port911)
        {
            try
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Đang check...");
                string cookie = CommonCSharp.GetStatusDataGridView(dtgvAcc, row, "cCookie");
                string check = CommonFacebook.CheckLiveCookie(cookie, "", port911).Split('|')[0];
                if (check == "1")
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Cookie Live!");
                }
                else
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Cookie Die!");
                }
            }
            catch
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Lỗi!");
            }
        }
        private void CheckToken(int row, string port911)
        {
            try
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Đang check...");
                string token = CommonCSharp.GetStatusDataGridView(dtgvAcc, row, "cToken");
                bool check = CommonFacebook.CheckLiveToken("", "", token, port911);
                if (check)
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Token Live!");
                }
                else
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Token Die!");
                }
            }
            catch
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Lỗi!");
            }
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                isStop = true;
                btnPause.Enabled = false;
                btnPause.Text = "Đang dừng...";
            }
            catch { }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            Excute();
        }

        private void Excute()
        {
            try
            {
                //Cấu hình chung
                linkBrowser = txtLinkToOtherBrowser.Text.Trim();
                int iThread = 0;
                int maxThread = Convert.ToInt32(nudThread.Value);
                if (maxThread == 0)
                {
                    MessageBox.Show("Số luồng>0!", "Thông báo");
                    return;
                }

                iSoLuongAnh = Convert.ToInt32(nudSoLuongAnh.Value);
                if (iSoLuongAnh == 0)
                {
                    MessageBox.Show("Số lượng ảnh/bạn>0!", "Thông báo");
                    return;
                }

                isFakeUserAgent = ckbFakeUa.Checked;
                if (isFakeUserAgent)
                {
                    lstUserAgent = File.ReadAllLines("input\\ua.txt").ToList();
                    lstUserAgent = CommonCSharp.RemoveEmptyItems(lstUserAgent);
                    if (lstUserAgent.Count == 0)
                    {
                        MessageBox.Show("Vui lòng nhập UserAgent cần dùng!", "Thông báo");
                        return;
                    }
                }

                isUseTokenTg = ckbUseTokenTg.Checked;
                if (isUseTokenTg)
                {
                    lstToken = File.ReadAllLines(@"input\token.txt").ToList();
                    lstToken = Common.CommonCSharp.RemoveEmptyItems(lstToken);
                    if (lstToken.Count == 0)
                    {
                        MessageBox.Show("Vui lòng nhập thêm token trung gian!", "Thông báo");
                        return;
                    }
                }

                //Cấu hình fake ip
                string port911 = "";
                iCountChangeIP = Convert.ToInt32(nudChangeIpCount.Value);
                if (rdChangeIPNone.Checked)
                    iTypeChangeIP = 0;
                if (rdChangeIPDcom.Checked)
                    iTypeChangeIP = 1;
                else if (rdChangeIPHMA.Checked)
                    iTypeChangeIP = 2;
                else if (rdConnect911.Checked)
                    iTypeChangeIP = 3;

                if (iTypeChangeIP == 1)
                {
                    sProfileDcom = txtProfileNameDcom.Text.Trim();
                    if (sProfileDcom == "")
                    {
                        MessageBox.Show("Vui lòng nhập tên cấu hình Dcom!", "Thông báo");
                        return;
                    }
                }
                else if (iTypeChangeIP == 3)
                {
                    port911 = txtPort911.Text.Trim();
                    if (port911 == "")
                    {
                        MessageBox.Show("Vui lòng nhập port 911!", "Thông báo");
                        return;
                    }
                    else if (CommonCSharp.CheckIP(port911) == "")
                    {
                        MessageBox.Show("Không thể connect 911!", "Thông báo");
                        return;
                    }
                }

                rControl("start");
                isStop = false;
                int curChangeIp = 0;
                bool isChangeIPSuccess = false;

                new Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < dtgvAcc.Rows.Count;)
                        {
                            if (isStop)
                                break;

                            if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                            {
                                if (iThread < maxThread)
                                {
                                    if (isStop)
                                        break;

                                    Interlocked.Increment(ref iThread);
                                    int row = i++;
                                    new Thread(() =>
                                    {
                                        try
                                        {
                                            ExcuteOneThread(row, port911);
                                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cChoose", false);

                                            Interlocked.Decrement(ref iThread);
                                        }
                                        catch (Exception ex)
                                        {
                                            CommonCSharp.ExportError(null, ex.ToString());
                                        }

                                    }).Start();
                                }
                                else
                                {
                                    if (iTypeChangeIP != 0 && iTypeChangeIP != 3)
                                    {
                                        while (iThread > 0)
                                        {
                                            CommonCSharp.DelayTime(1);
                                        }

                                        if (isStop)
                                            break;

                                        Interlocked.Increment(ref curChangeIp);
                                        if (curChangeIp >= iCountChangeIP)
                                        {
                                            for (int j = 0; j < 3; j++)
                                            {
                                                isChangeIPSuccess = CommonOther.ChangeIP(iTypeChangeIP, sProfileDcom);
                                                if (isChangeIPSuccess)
                                                    break;
                                            }
                                            if (!isChangeIPSuccess)
                                            {
                                                MessageBox.Show("Không thể đổi ip!");
                                                goto Xong;
                                            }
                                            curChangeIp = 0;
                                        }
                                    }
                                    else
                                    {
                                        CommonCSharp.DelayTime(1);
                                    }
                                }
                            }
                            else
                            {
                                i++;
                            }

                            if (isStop)
                                break;
                        }

                        while (iThread > 0)
                        {
                            CommonCSharp.DelayTime(1);
                        }

                    }
                    catch (Exception ex)
                    {
                        CommonCSharp.ExportError(null, ex.ToString());
                    }
                    Xong:
                    rControl("stop");
                    if (isUseTokenTg)
                    {
                        File.WriteAllLines(@"input\token.txt", lstToken);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                CommonCSharp.ExportError(null, ex.ToString());
                rControl("stop");
            }

        }
        public string RequestGet(ChromeDriver chrome, string url, string website)
        {
            try
            {
                if (!chrome.Url.StartsWith(website))
                    chrome.Navigate().GoToUrl(website);
                string rq = (string)chrome.ExecuteScript("async function RequestGet() { var output = ''; try { var response = await fetch('" + url + "'); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await RequestGet(); return c;");
                return rq;
            }
            catch { }

            return "";
        }
        public string RequestGet(ChromeDriver chrome, string url, string website, string token)
        {
            try
            {
                if (!chrome.Url.StartsWith(website))
                    chrome.Navigate().GoToUrl(website);
                string rq = (string)chrome.ExecuteScript("async function RequestGet() { var output = ''; try { var response = await fetch('" + url + "',{method: 'GET', headers: {'Authorization': 'OAuth " + token + "'}}); if (response.ok) { var body = await response.text(); return body; } } catch {} return output; }; var c = await RequestGet(); return c;");
                return rq;
            }
            catch { }

            return "";
        }
        private void BackupImageNew(ChromeDriver chrome, int indexRow, string uid, string token, int soLuongAnh)
        {
            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh...");
            //Backup ảnh
            List<string> listImageBackup = new List<string>();
            int iThread = 0;
            int countSuccess = 0;

            string url = "https://graph.facebook.com/?ids=" + uid + "&pretty=1&fields=friends.limit(20){id,name,photos.limit(" + soLuongAnh + "){images}}&after=\u00257D&limit=20&after=QVFIUlRTNjdEY2tpdlZAKbW03eGt5YXpVaWplbFFfMkhNc1FOQ2ZARbTBtdjh3Mmh2Wm9tWnMyOEdYOHIyM3U1ZAE1tN3hONjRXT0Jra3d5bkh2eHAtNFBKd213";
            //string url = "https://graph.facebook.com/" + uid + "/friends?pretty=0&fields=id,name,photos.limit(" + soLuongAnh + "){images}&limit=100&access_token=" + token;
            List<string> lstBody = new List<string>();
            string text = "";

            JObject json = null;
            int dem = 0;
            do
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Chuẩn bị dữ liệu backup: " + (++dem) + "...");
                text = RequestGet(chrome, url, "https://graph.facebook.com", token);
                lstBody.Add(text);
                json = JObject.Parse(text);
                url = (JToken)json[uid]["friends"]["paging"]["next"] != null ? json[uid]["friends"]["paging"]["next"].ToString() : "";
            } while (url != "");

            int maxThread = lstBody.Count;
            new Thread(() =>
            {
                while (iThread > 0)
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh: " + countSuccess + "...");
                    Thread.Sleep(100);
                }
            }).Start();

            for (int i = 0; i < lstBody.Count;)
            {
                if (iThread < maxThread)
                {
                    Interlocked.Increment(ref iThread);
                    int stt = i++;
                    new Thread(() =>
                    {
                        string text1 = lstBody[stt];
                        List<string> listAdd = ParseTextToList(text1, ref countSuccess);

                        if (listAdd.Count > 0)
                        {
                            lock (listImageBackup)
                            {
                                listImageBackup.AddRange(listAdd);
                            }
                        }
                        Interlocked.Decrement(ref iThread);
                    }).Start();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            while (iThread > 0)
            {
                Thread.Sleep(100);
            }
            if (listImageBackup.Count > 0)
            {
                lock (_lock2)
                {
                    Directory.CreateDirectory(@"output\" + uid);
                    File.WriteAllLines(@"output\" + uid + "\\" + uid + ".txt", listImageBackup);
                }
            }
            else
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
            }
        }
        private static List<string> ParseTextToList(string text, ref int countSuccess)
        {
            List<string> listImageBackup = new List<string>();
            try
            {
                JObject json = JObject.Parse(text);

                int stt = 0;
                string uidFr = "", nameFr = "";
                if (json != null && text.Contains("images"))
                {
                    foreach (var item in json["data"])
                    {
                        uidFr = item["id"].ToString();
                        nameFr = item["name"].ToString();
                        if ((JToken)item["photos"] != null)
                        {
                            foreach (var item1 in item["photos"]["data"])
                            {
                                stt = item1["images"].ToList().Count - 1;
                                listImageBackup.Add(uidFr + "*" + nameFr + "*" + item1["images"][stt]["source"] + "|" + item1["images"][stt]["width"] + "|" + item1["images"][stt]["height"]);
                            }
                            listImageBackup.Add("");
                        }
                        countSuccess++;
                    }
                }
            }
            catch { }

            return listImageBackup;
        }

        string linkBrowser = "";
        private void ExcuteOneThread(object data, string port911)
        {
            int indexRow = (int)data;

            ChromeDriver chrome = null;
            string cookie = CommonCSharp.GetStatusDataGridView(dtgvAcc, indexRow, "cCookie");
            string token = CommonCSharp.GetStatusDataGridView(dtgvAcc, indexRow, "cToken");
            string uid = CommonCSharp.GetStatusDataGridView(dtgvAcc, indexRow, "cUid");
            if (uid == "")
                uid = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;

            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang check thông tin...");
            bool isHaveToken = token != "";
            bool isHaveCookie = cookie != "";
            if (isUseTokenTg)
            {
                try
                {
                    if (uid.Trim() == "")
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Uid trống!");
                        goto Xong;
                    }

                    gettoken:
                    lock (lstToken)
                    {
                        if (lstToken.Count == 0)
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Token trung gian die hết!");
                            isStop = true;
                            goto Xong;
                        }
                        token = lstToken[rd.Next(0, lstToken.Count)];
                        if (!CommonFacebook.CheckLiveToken("", "", token, port911))
                        {
                            lstToken.Remove(token);
                            goto gettoken;
                        }
                    }

                    //Get uid + birthday
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ngày sinh...");
                    string rq = CommonFacebook.GetBirthdayOfUid(token, uid);
                    if (rq == "")
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Token die!");
                        goto gettoken;
                    }
                    else
                    {
                        lock (_lock)
                        {
                            /*Directory.CreateDirectory(@"output\" + uid);*/
                            File.AppendAllText(@"output\" + /*uid +*/ "\\" + "ngaysinh.txt", rq + Environment.NewLine);
                        }
                    }

                    //Backup image
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh...");
                    List<string> listImageBackup = new List<string>();

                    //Get list friend
                    List<string> lstFriend = CommonFacebook.GetListUidNameFriendOfUid(token, uid, port911);
                    if (lstFriend.Count > 0)
                    {
                        object _lock_countSuccess = new object();
                        //Get list friend
                        List<string> lstId = lstFriend;
                        int totalFriend = lstId.Count;
                        int iThread = 0;
                        List<string> lstQuery = GhepFileList(lstId);
                        int total = lstQuery.Count;
                        int countSuccess = 0;
                        
                        new Thread(() =>
                        {
                            while (iThread > 0)
                            {
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", string.Format(("Đang backup ảnh: {0}/{1}..."), countSuccess, totalFriend));
                                Thread.Sleep(100);
                            }
                        }).Start();

                        if (lstQuery.Count > 0)
                        {
                            int maxThread = lstQuery.Count > 10 ? 10 : lstQuery.Count;

                            for (int i = 0; i < lstQuery.Count;)
                            {
                                if (iThread < maxThread)
                                {
                                    Interlocked.Increment(ref iThread);
                                    int stt = i++;
                                    new Thread(() =>
                                    {
                                        string uids = lstQuery[stt];
                                        //string nameFr = lstFriend[stt].Split('|')[1];
                                        List<string> listAdd = BackupImageOne(uids, cookie, token, "", "", 0, 20, true);
                                        if (listAdd.Count > 0)
                                        {
                                            lock (listImageBackup)
                                                listImageBackup.AddRange(listAdd);
                                        }

                                        lock (_lock_countSuccess)
                                            countSuccess += uids.Split(',').Length;
                                        Interlocked.Decrement(ref iThread);
                                    }).Start();
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
                            }

                            while (iThread > 0)
                                Thread.Sleep(100);

                            if (listImageBackup.Count > 0)
                            {
                                lock (_lock2)
                                {
                                    /*Directory.CreateDirectory(@"output\" + uid);*/
                                    File.AppendAllLines(@"output\" + /*uid + "\\" + uid +*/ "total.txt", listImageBackup);
                                }
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Backup xong!");
                            }
                            else
                            {
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
                            }
                        }
                        else
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không lấy được ds bạn bè!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Lỗi không xác định!");
                    CommonCSharp.ExportError(chrome, ex.ToString());
                }
            }
            else if (isHaveToken && !isHaveCookie)
            {
                try
                {
                    //Get uid + birthday
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ngày sinh...");
                    string rq = CommonFacebook.GetMyBirthday(token);
                    if (rq == "")
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Token die!");
                        goto Xong;
                    }
                    else
                    {
                        uid = rq.Split('|')[0];
                        lock (_lock)
                        {
                            /*Directory.CreateDirectory(@"output\" + uid);*/
                            /*File.AppendAllText(@"output\" + uid + "\\ngaysinh.txt", rq + Environment.NewLine);*/
                            File.AppendAllText(@"output\" + /*uid +*/ "\\" + "ngaysinh.txt", rq + Environment.NewLine);
                        }
                    }

                    //Backup image
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh...");
                    List<string> listImageBackup = new List<string>();
                    int iThread = 0;
                    int countSuccess = 0;

                    //Get list friend
                    List<string> lstFriend = CommonFacebook.GetMyListUidNameFriend(token, "", "", port911);
                    if (lstFriend.Count > 0)
                    {
                        //Backup ảnh
                        int totalFriend = lstFriend.Count;
                        int maxThread = 50;

                        new Thread(() =>
                        {
                            while (iThread > 0)
                            {
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh: " + countSuccess + "/" + totalFriend);
                                Thread.Sleep(100);
                            }
                        }).Start();

                        for (int i = 0; i < lstFriend.Count;)
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int stt = i++;
                                new Thread(() =>
                                {
                                    string uidFr = lstFriend[stt].Split('|')[0];
                                    string nameFr = lstFriend[stt].Split('|')[1];
                                    List<string> listAdd = CommonFacebook.BackupImageOne(cookie, "", uidFr, nameFr, token, port911, iSoLuongAnh);
                                    if (listAdd.Count > 0)
                                    {
                                        lock (listImageBackup)
                                        {
                                            listImageBackup.AddRange(listAdd);
                                            listImageBackup.Add("");
                                        }
                                    }
                                    Interlocked.Increment(ref countSuccess);
                                    Interlocked.Decrement(ref iThread);
                                }).Start();
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }

                        while (iThread > 0)
                        {
                            Thread.Sleep(100);
                        }

                        if (listImageBackup.Count > 0)
                        {
                            lock (_lock2)
                            {
                                Directory.CreateDirectory(@"output\" + uid);
                                File.WriteAllLines(@"output\" + uid + "\\" + uid + ".txt", listImageBackup);
                            }

                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đã backup xong: " + countSuccess + "!");
                        }
                        else
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
                        }
                    }
                    else
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
                    }
                }
                catch (Exception ex)
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Lỗi không xác định!");
                    CommonCSharp.ExportError(chrome, ex.ToString());
                }
            }
            else if (isHaveCookie)
            {
                //Login using cookie
                try
                {
                    #region Đăng nhập
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang mở trình duyệt ẩn...");

                    string ua = "";
                    bool isSuccess = false;
                    try
                    {
                        ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";
                        if (lstUserAgent.Count > 0)
                        {
                            ua = lstUserAgent[rd.Next(0, lstUserAgent.Count)];
                        }

                        try
                        {
                            chrome = CommonChrome.OpenChrome(chrome, true, true, false, ua, "", new Point(300, 300), new Point(0, 0), port911, linkBrowser, 3, 60);
                        }
                        catch (Exception ex)
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Lỗi mở trình duyệt!");
                            CommonCSharp.ExportError(chrome, ex.ToString());
                            goto Xong;
                        }

                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đăng nhập bằng cookie...");
                        isSuccess = CommonFacebook.LoginFacebookUsingCookie(chrome, cookie, ua);
                        CommonCSharp.DelayTime(0.1);

                        if (isSuccess)
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đăng nhập thành công!");
                        }
                        else
                        {
                            if (CommonChrome.CheckExistElement(chrome, 1, "checkpointSubmitButton"))
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Checkpoint!");
                            else
                                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Cookie Die!");
                            goto Xong;
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Lỗi đăng nhập!");
                        CommonCSharp.ExportError(chrome, ex.ToString());
                        goto Xong;
                    }
                    #endregion

                    cookie = GetCookieFromChrome(chrome);
                    ua = GetUseragent(chrome);

                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Chuẩn bị backup...");
                    //check if token die => get token using js
                    if (token == "" || !CommonFacebook.CheckLiveToken(cookie, ua, token, port911))
                    {
                        token = CommonJSChrome.GetTokenEAAG(chrome);
                        if (token == "")
                        {
                            chrome.Navigate().GoToUrl("https://business.facebook.com/business_locations/");
                            string html = (string)chrome.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                            token = Regex.Match(html, "EAAG(.*?)\"").Value.Replace("\"", "");
                        }

                        if (token == "")
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không lấy được token!");
                            goto Xong;
                        }
                    }

                    //Backup ngày sinh
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Backup ngày sinh...");
                    string rq = CommonJSChrome.GetBirthday(chrome, token, uid);
                    if (rq == "")
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Token die!");
                        goto Xong;
                    }
                    else
                    {
                        uid = rq.Split('|')[0];
                        lock (_lock)
                        {
                            Directory.CreateDirectory(@"output\" + uid);
                            File.AppendAllText(@"output\" + uid + "\\ngaysinh.txt", rq + Environment.NewLine);
                        }
                    }


                    //Backup ảnh
                    //BackupImageNew(chrome, indexRow, uid, token, iSoLuongAnh);
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Đang backup ảnh...");
                    List<string> listImageBackup = new List<string>();
                    int iThread = 0;
                    object _lock_countSuccess = new object();
                    int countSuccess = 0;

                    //Get list friend
                    List<string> lstId = GetMyListUidNameFriend(cookie, token);
                    int totalFriend = lstId.Count;

                    List<string> lstQuery = GhepFileList(lstId);
                    int total = lstQuery.Count;

                    new Thread(() =>
                    {
                        while (iThread > 0)
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", string.Format(("Đang backup ảnh: {0}/{1}..."), countSuccess, totalFriend));
                            Thread.Sleep(100);
                        }
                    }).Start();

                    if (lstQuery.Count > 0)
                    {
                        int maxThread = lstQuery.Count > 10 ? 10 : lstQuery.Count;

                        for (int i = 0; i < lstQuery.Count;)
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int stt = i++;
                                new Thread(() =>
                                {
                                    string uids = lstQuery[stt];
                                    //string nameFr = lstFriend[stt].Split('|')[1];
                                    List<string> listAdd = BackupImageOne(uids, cookie, token, "", "", 0, 20, true);
                                    if (listAdd.Count > 0)
                                    {
                                        lock (listImageBackup)
                                            listImageBackup.AddRange(listAdd);
                                    }

                                    lock (_lock_countSuccess)
                                        countSuccess += uids.Split(',').Length;
                                    Interlocked.Decrement(ref iThread);
                                }).Start();
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }

                        while (iThread > 0)
                            Thread.Sleep(100);

                        if (listImageBackup.Count > 0)
                        {
                            lock (_lock2)
                            {
                                Directory.CreateDirectory(@"output\" + uid);
                                File.WriteAllLines(@"output\" + uid + "\\" + uid + ".txt", listImageBackup);
                            }
                        }
                        else
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
                        }
                    }
                    else
                    {
                        CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
                    }

                    //backup comment
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Backup bình luận...");
                    List<string> listCmt = CommonJSChrome.GetMyListComments(chrome, 5);
                    lock (_lock3)
                    {
                        Directory.CreateDirectory(@"output\" + uid);
                        File.WriteAllLines(@"output\" + uid + "\\" + "lscomment.txt", listCmt);
                    }

                    //backup tin nhan
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Backup tin nhắn...");
                    List<string> listMessager = CommonJSChrome.GetMyListUidMessage(chrome);
                    lock (_lock4)
                    {
                        Directory.CreateDirectory(@"output\" + uid);
                        File.WriteAllLines(@"output\" + uid + "\\" + "banbeinbox.txt", listMessager);
                    }
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Backup xong!");
                }
                catch (Exception ex)
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Lỗi không xác định!");
                    CommonCSharp.ExportError(chrome, ex.ToString());
                }
            }
            else if (!isHaveToken && !isHaveCookie)
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cStatus", "Không backup được!");
            }

            Xong:
            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, indexRow, "cUid", uid);
            CommonChrome.QuitChrome(chrome);
            return;
        }

        public static List<string> BackupImageOne(string uids, string cookie, string token, string userAgent, string proxy, int typeProxy, int countImage = 20, bool isBackupNangCao = false)
        {
            List<string> listImageBackup = new List<string>();
            try
            {
                Dictionary<string, List<string>> dicImage = new Dictionary<string, List<string>>();
                {
                    var lstUid = uids.Split(',');
                    for (int i = 0; i < lstUid.Length; i++)
                        dicImage.Add(lstUid[i], new List<string>());
                }//khai báo dictionary

                try
                {
                    RequestXNet request = new RequestXNet(cookie, userAgent, proxy);
                    request.request.AddHeader("Authorization", "OAuth " + token);
                    string url = "https://graph.facebook.com/?ids=" + uids + "&pretty=0&fields=id,name,photos.limit(" + countImage + "){images}";
                    string htmlImage = request.RequestGet(url);
                    JObject objImg = JObject.Parse(htmlImage);

                    if (objImg != null && htmlImage.Contains("images"))
                    {
                        var lstUid = uids.Split(',');
                        for (int i = 0; i < lstUid.Length; i++)
                        {
                            string uidFr = lstUid[i];
                            string nameFr = objImg[uidFr]["name"].ToString();

                            try
                            {
                                foreach (var photos in objImg[uidFr]["photos"]["data"])
                                {
                                    try
                                    {
                                        int stt = photos["images"].ToList().Count - 1;
                                        dicImage[uidFr].Add(uidFr + "*" + nameFr + "*" + photos["images"][stt]["source"] + "|" + photos["images"][stt]["width"] + "|" + photos["images"][stt]["height"]);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch
                {
                }//Backup ảnh bình thường

                if (isBackupNangCao)
                {
                    try
                    {
                        RequestXNet request = new RequestXNet(cookie, userAgent, proxy);
                        request.request.AddHeader("Authorization", "OAuth " + token);
                        string url = "https://graph.facebook.com/?ids=" + uids + "&pretty=0&fields=name,albums.limit(3){photos.limit(10){width,height,images}}";
                        string htmlImage = request.RequestGet(url);
                        JObject objImg = JObject.Parse(htmlImage);

                        if (objImg != null && htmlImage.Contains("images"))
                        {
                            var lstUid = uids.Split(',');
                            for (int i = 0; i < lstUid.Length; i++)
                            {
                                string uidFr = lstUid[i];
                                string nameFr = objImg[uidFr]["name"].ToString();

                                foreach (var albums in objImg[uidFr]["albums"]["data"])
                                {
                                    try
                                    {
                                        foreach (var photos in albums["photos"]["data"])
                                        {
                                            try
                                            {
                                                int stt = photos["images"].ToList().Count - 1;
                                                if (dicImage[uidFr].Count >= countImage)
                                                    goto Continue;
                                                dicImage[uidFr].Add(uidFr + "*" + nameFr + "*" + photos["images"][stt]["source"] + "|" + photos["images"][stt]["width"] + "|" + photos["images"][stt]["height"]);
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                Continue:
                                continue;
                            }
                        }
                    }
                    catch
                    {
                    }
                }//Backup ảnh từ album

                {
                    foreach (var item in dicImage)
                    {
                        if (item.Value.Count > 0)
                        {
                            listImageBackup.AddRange(item.Value);
                            listImageBackup.Add("");
                        }
                    }
                }//Nhập danh sách link ảnh từ dicImage vào list
            }
            catch
            {
            }

            return listImageBackup;
        }

        List<string> GhepFileList(List<string> lstId, int soLuongAccMoiLan = 50, string separator = ",")
        {
            int soLan = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((lstId.Count) * 1.0 / soLuongAccMoiLan)));
            List<string> lstQuery = new List<string>();
            for (int i = 0; i < soLan; i++)
                lstQuery.Add(string.Join(separator, lstId.GetRange(soLuongAccMoiLan * i, soLuongAccMoiLan * i + soLuongAccMoiLan <= lstId.Count ? soLuongAccMoiLan : lstId.Count % soLuongAccMoiLan)));
            return lstQuery;
        }

        public static List<string> GetMyListUidNameFriend(string cookie, string token, string userAgent="", string proxy="")
        {
            List<string> listFriend = new List<string>();
            try
            {
                string uid = Regex.Match(cookie + ";", "c_user=(.*?);").Groups[1].Value;
                RequestXNet request = new RequestXNet(cookie, userAgent, proxy);
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
                        //string nameFr = temp["data"][i]["name"].ToString();
                        //listFriend.Add(uidFr + "|" + nameFr);
                        listFriend.Add(uidFr);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return listFriend;
        }

        public string GetCookieFromChrome(ChromeDriver chrome, string domain = "facebook")
        {
            string cookie = "";
            try
            {
                var sess = chrome.Manage().Cookies.AllCookies.ToArray();
                foreach (var item in sess)
                {
                    if (item.Domain.Contains(domain))
                        cookie += item.Name + "=" + item.Value + ";";
                }
            }
            catch (Exception ex)
            {
            }
            return cookie;
        }
        public string GetUseragent(ChromeDriver chrome)
        {
            string ua = "";
            try
            {
                ua = chrome.ExecuteScript("return navigator.userAgent").ToString();
            }
            catch
            {
            }
            return ua;
        }
        private void loadNhómToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tươngTácLạiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dtgvAcc.Rows.Count; i++)
                {
                    if (dtgvAcc.Rows[i].Cells["cStatus"].Value.ToString() == "Die")
                        dtgvAcc.Rows[i].Cells["cChoose"].Value = true;
                    else
                        dtgvAcc.Rows[i].Cells["cChoose"].Value = false;
                }
                UpdateSelectCount();
            }
            catch (Exception)
            {
            }
        }

        private void chọnTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dtgvAcc.Rows.Count; i++)
                {
                    dtgvAcc.Rows[i].Cells["cChoose"].Value = true;
                }
                UpdateSelectCount();
            }
            catch
            {
            }
        }

        private void bỏChọnTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dtgvAcc.Rows.Count; i++)
                {
                    dtgvAcc.Rows[i].Cells["cChoose"].Value = false;
                }
                UpdateSelectCount();
            }
            catch
            { }
        }
        /// <summary>
        /// 1-token, 2-cookie, 3-token|cookie, 4-uid, 5-uid|token|cookie
        /// </summary>
        /// <param name="type"></param>
        private void PasteIntoDataGridView(int type)
        {
            switch (type)
            {
                case 1:
                    try
                    {
                        DataObject o = (DataObject)Clipboard.GetDataObject();
                        List<string> pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n").ToList();
                        pastedRows = CommonCSharp.RemoveEmptyItems(pastedRows);
                        foreach (string pastedRow in pastedRows)
                        {
                            dtgvAcc.Rows.Add(false, "", pastedRow, "", "");
                        }
                    }
                    catch { }
                    break;
                case 2:
                    try
                    {
                        DataObject o = (DataObject)Clipboard.GetDataObject();
                        List<string> pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n").ToList();
                        pastedRows = CommonCSharp.RemoveEmptyItems(pastedRows);
                        foreach (string pastedRow in pastedRows)
                        {
                            dtgvAcc.Rows.Add(false, "", "", pastedRow, "");
                        }
                    }
                    catch { }
                    break;
                case 3:
                    try
                    {
                        DataObject o = (DataObject)Clipboard.GetDataObject();
                        List<string> pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n").ToList();
                        pastedRows = CommonCSharp.RemoveEmptyItems(pastedRows);
                        foreach (string pastedRow in pastedRows)
                        {
                            dtgvAcc.Rows.Add(false, "", pastedRow.Split('|')[0], pastedRow.Split('|')[1], "");
                        }
                    }
                    catch { }
                    break;
                case 4:
                    try
                    {
                        DataObject o = (DataObject)Clipboard.GetDataObject();
                        List<string> pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n").ToList();
                        pastedRows = CommonCSharp.RemoveEmptyItems(pastedRows);
                        foreach (string pastedRow in pastedRows)
                        {
                            dtgvAcc.Rows.Add(false, pastedRow, "", "", "");
                        }
                    }
                    catch { }
                    break;
                case 5:
                    try
                    {
                        DataObject o = (DataObject)Clipboard.GetDataObject();
                        List<string> pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n").ToList();
                        pastedRows = CommonCSharp.RemoveEmptyItems(pastedRows);
                        foreach (string pastedRow in pastedRows)
                        {
                            dtgvAcc.Rows.Add(false, pastedRow.Split('|')[0], pastedRow.Split('|')[1], pastedRow.Split('|')[2], "");
                        }
                    }
                    catch { }
                    break;
                default:
                    break;
            }

        }

        private void pasteCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoDataGridView(2);
        }

        private void xóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dtgvAcc.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                    {
                        dtgvAcc.Rows.RemoveAt(i);
                        i--;
                    }
                }
                UpdateSelectCount();
            }
            catch
            {
            }
        }

        private void dtgvAcc_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue == (int)System.Windows.Forms.Keys.Space)
                {
                    for (int i = 0; i < dtgvAcc.SelectedRows.Count; i++)
                    {
                        int a = dtgvAcc.SelectedRows[i].Index;
                        if (Convert.ToBoolean(dtgvAcc.Rows[a].Cells["cChoose"].Value))
                        {
                            dtgvAcc.Rows[a].Cells["cChoose"].Value = false;
                        }
                        else
                        {
                            dtgvAcc.Rows[a].Cells["cChoose"].Value = true;
                        }
                    }
                    UpdateSelectCount();
                }
            }
            catch { }
        }

        private void DtgvAcc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    if (Convert.ToBoolean(dtgvAcc.CurrentRow.Cells["cChoose"].Value) == true)
                        dtgvAcc.CurrentRow.Cells["cChoose"].Value = false;
                    else
                        dtgvAcc.CurrentRow.Cells["cChoose"].Value = true;
                }
                UpdateSelectCount();
            }
            catch
            {
            }
        }

        private void btnUseragentOpen_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"input\ua.txt");
            }
            catch { }
        }

        private void dtgvAcc_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            CountAccount();
        }

        private void dtgvAcc_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            CountAccount();
        }

        private void copyCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string textCopy = "";
                for (int i = 0; i < dtgvAcc.RowCount; i++)
                {
                    if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                    {
                        try
                        {
                            textCopy += (dtgvAcc.Rows[i].Cells["cCookie"].Value == null ? "" : dtgvAcc.Rows[i].Cells["cCookie"].Value.ToString()) + "\r\n";
                        }
                        catch { }
                    }
                }
                Clipboard.SetText(textCopy);
            }
            catch { }
        }

        private void CheckedChangedFull()
        {
            ckbFakeUa_CheckedChanged(null, null);
            rdDcom_CheckedChanged(null, null);
            rdConnect911_CheckedChanged(null, null);
            rdConnect911_CheckedChanged_1(null, null);
            ckbUseTokenTg_CheckedChanged(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!rdChangeIPNone.Checked)
                {
                    int i = 0;
                    if (rdChangeIPNone.Checked == true)
                        i = 0;
                    else if (rdChangeIPDcom.Checked == true)
                        i = 1;
                    else if (rdChangeIPHMA.Checked == true)
                        i = 2;
                    string sProfileDcom = txtProfileNameDcom.Text.Trim();
                    bool isSuccess = CommonOther.ChangeIP(i, sProfileDcom);
                    if (isSuccess)
                        MessageBox.Show("Đổi IP thành công!");
                    else
                        MessageBox.Show("Đổi IP thất bại!");
                }
                else
                    MessageBox.Show("Chưa chọn kiểu đổi ip!");
            }
            catch
            {
                MessageBox.Show("Đổi IP thất bại!");
            }
        }

        private void ctmsAcc_Opening(object sender, CancelEventArgs e)
        {
            mnuItemChonTrangThai.DropDownItems.Clear();
            List<string> lv = new List<string>();
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                if (dtgvAcc.Rows[i].Cells["cStatus"].Value != null && dtgvAcc.Rows[i].Cells["cStatus"].Value.ToString().Equals("") == false && lv.Contains(dtgvAcc.Rows[i].Cells["cStatus"].Value.ToString()) == false)
                {
                    lv.Add(dtgvAcc.Rows[i].Cells["cStatus"].Value.ToString());
                }
            }
            for (int i = 0; i < lv.Count; i++)
            {
                mnuItemChonTrangThai.DropDownItems.Add(lv[i]);
                mnuItemChonTrangThai.DropDownItems[i].Click += SelectGridByStatus;
            }
        }
        private void SelectGridByStatus(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                ToolStripMenuItem it = (ToolStripMenuItem)sender;
                string name = it.Text;

                try
                {
                    if (dtgvAcc.Rows[i].Cells["cStatus"].Value.ToString().Equals(name))
                    {
                        dtgvAcc.Rows[i].Cells["cChoose"].Value = true;
                    }
                    else
                        dtgvAcc.Rows[i].Cells["cChoose"].Value = false;
                }
                catch
                {
                }
            }

            UpdateSelectCount();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CommonCSharp.KillProcess("chromedriver");
        }

        private void ckbFakeUa_CheckedChanged(object sender, EventArgs e)
        {
            btnUseragent.Enabled = ckbFakeUa.Checked;
        }
        private void rdDcom_CheckedChanged(object sender, EventArgs e)
        {
            plDcom.Enabled = rdChangeIPDcom.Checked;
        }

        private void btnUseragent_Click(object sender, EventArgs e)
        {

        }

        private void mnuItemChonTrangThai_Click(object sender, EventArgs e)
        {

        }

        private void loadTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string port911 = "";
            if (rdConnect911.Checked)
                port911 = txtPort911.Text.Trim();

            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < dtgvAcc.Rows.Count;)
                    {
                        if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int row = i++;
                                new Thread(() =>
                                {
                                    try
                                    {
                                        CheckCookie(row, port911);
                                        Interlocked.Decrement(ref iThread);
                                    }
                                    catch (Exception ex)
                                    {
                                        CommonCSharp.ExportError(null, ex.ToString());
                                    }
                                }).Start();
                            }
                            else
                            {
                                CommonCSharp.DelayTime(1);
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    while (iThread > 0)
                    {
                        CommonCSharp.DelayTime(1);
                    }
                }
                catch (Exception ex)
                {
                    CommonCSharp.ExportError(null, ex.ToString());
                }
            }).Start();
        }

        private void copyCookieToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

        }


        private void toolStripStatusLabel5_Click(object sender, EventArgs e)
        {
            Settings.Default.UserName = "";
            Settings.Default.PassWord = "";
            Settings.Default.Save();

            this.Hide();
            fActive fa = new fActive(0, deviceId);
            fa.ShowInTaskbar = true;
            fa.ShowDialog();
            return;
        }

        private void dtgvAcc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void rdConnect911_CheckedChanged(object sender, EventArgs e)
        {
            plConnect911.Enabled = rdConnect911.Checked;
        }


        private void tokenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PasteIntoDataGridView(1);
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            panel2.Visible = true;
        }

        private void tokenCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoDataGridView(3);
        }

        private void kiêmTraTokenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string port911 = "";
            if (rdConnect911.Checked)
                port911 = txtPort911.Text.Trim();

            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < dtgvAcc.Rows.Count;)
                    {
                        if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int row = i++;
                                new Thread(() =>
                                {
                                    try
                                    {
                                        CheckToken(row, port911);
                                        Interlocked.Decrement(ref iThread);
                                    }
                                    catch (Exception ex)
                                    {
                                        CommonCSharp.ExportError(null, ex.ToString());
                                    }
                                }).Start();
                            }
                            else
                            {
                                CommonCSharp.DelayTime(1);
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    while (iThread > 0)
                    {
                        CommonCSharp.DelayTime(1);
                    }
                }
                catch (Exception ex)
                {
                    CommonCSharp.ExportError(null, ex.ToString());
                }
            }).Start();
        }

        private void rdConnect911_CheckedChanged_1(object sender, EventArgs e)
        {
            plConnect911.Enabled = rdConnect911.Checked;
        }

        string lstNgaySinh = "";
        public string template_backup = CommonCSharp.Base64Decode("PGh0bWw+CjxoZWFkPgogICAgPHRpdGxlPnt7dWlkfX0gLSBNaW5Tb2Z0d2FyZTwvdGl0bGU+CiAgICA8bGluayByZWw9InN0eWxlc2hlZXQiIGhyZWY9Imh0dHBzOi8vY2RuanMuY2xvdWRmbGFyZS5jb20vYWpheC9saWJzL2J1bG1hLzAuNy41L2Nzcy9idWxtYS5taW4uY3NzIi8+CiAgICA8bGluayByZWw9InN0eWxlc2hlZXQiIGhyZWY9Imh0dHBzOi8vdW5wa2cuY29tL2J1ZWZ5QDAuNy43L2Rpc3QvYnVlZnkubWluLmNzcyI+CiAgICA8c3R5bGU+CiAgICAgICAgLm1lZGlhLWNvbnRlbnQgaW1nLmxhenkgewogICAgICAgICAgICBtYXJnaW4tcmlnaHQ6IDVweDsKICAgICAgICAgICAgbWFyZ2luLWJvdHRvbTogNXB4OwogICAgICAgIH0KICAgIDwvc3R5bGU+CjwvaGVhZD4KPGJvZHkgY2xhc3M9Imhhcy1iYWNrZ3JvdW5kLWxpZ2h0Ij4KPGRpdiBjbGFzcz0iY29udGFpbmVyIj4KICAgIDxkaXYgaWQ9ImFwcCIgc3R5bGU9InBhZGRpbmctdG9wOiAxNXB4OyI+CiAgICAgICAgPGgxIGNsYXNzPSJ0aXRsZSBpcy0xIGhhcy10ZXh0LWNlbnRlcmVkIj4KICAgICAgICAgICAgPHAgY2xhc3M9Imhhcy10ZXh0LWNlbnRlcmVkIj4KICAgICAgICAgICAgICAgIDxpbWcgc3JjPSJodHRwczovL2dyYXBoLmZhY2Vib29rLmNvbS97e3VpZH19L3BpY3R1cmU/aGVpZ2h0PTE1MCI+CiAgICAgICAgICAgIDwvcD4KICAgICAgICAgICAgPGEgY2xhc3M9Imhhcy10ZXh0LWluZm8iIGhyZWY9Imh0dHBzOi8vd3d3LmZhY2Vib29rLmNvbS97e3VpZH19IiB0YXJnZXQ9Il9ibGFuayI+e3t1aWR9fSAtIHt7bmFtZX19PC9hPgogICAgICAgIDwvaDE+CiAgICAgICAgPGgzIGNsYXNzPSJzdWJ0aXRsZSBpcy0zIGhhcy10ZXh0LWNlbnRlcmVkIj5OZ8OgeSBzaW5oOiB7e2JpcnRoZGF5fX0KICAgICAgICA8L2gzPgoKICAgICAgICA8Yi10YWJzIHBvc2l0aW9uPSJpcy1jZW50ZXJlZCI+CiAgICAgICAgICAgIDxiLXRhYi1pdGVtPgogICAgICAgICAgICAgICAgPHRlbXBsYXRlIHNsb3Q9ImhlYWRlciI+CiAgICAgICAgICAgICAgICAgICAgPHNwYW4gY2xhc3M9Imhhcy10ZXh0LXdpZWdodC1ib2xkIj4gSMOsbmgg4bqibmggPGItdGFnIHR5cGU9ImlzLXdhcm5pbmciCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJvdW5kZWQ+IHt7IHBob3Rvcy5sZW5ndGggfX0gPC9iLXRhZz4gPC9zcGFuPgogICAgICAgICAgICAgICAgPC90ZW1wbGF0ZT4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9ImZpZWxkIj4KICAgICAgICAgICAgICAgIDx0ZXh0YXJlYSBjbGFzcz0idGV4dGFyZWEiIHYtbW9kZWw9InBob3Rvc0tleXdvcmQiIHJvd3M9IjEwIgogICAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyPSJOaOG6rXAgbeG7l2kgdMOqbiAxIGTDsm5nIMSR4buDIHTDrG0ga2nhur9tIj48L3RleHRhcmVhPgogICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJmaWVsZCBib3giIHYtZm9yPSJwIGluIHBob3RvcyIgOmtleT0icC51aWQiIHYtc2hvdz0icC5zaG93Ij4KICAgICAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJtZWRpYSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9Im1lZGlhLWxlZnQgYm94Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxpbWcgY2xhc3M9ImxhenkiIDpkYXRhLXNyYz0iYGh0dHBzOi8vZ3JhcGguZmFjZWJvb2suY29tLyR7cC51aWR9L3BpY3R1cmU/aGVpZ2h0PTE1MGAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPHAgY2xhc3M9Imhhcy10ZXh0LWNlbnRlcmVkIGhhcy10ZXh0LXdpZ2h0LWJvbGQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxhIDpocmVmPSJgaHR0cHM6Ly93d3cuZmFjZWJvb2suY29tLyR7cC51aWR9YCI+e3sgcC5uYW1lIH19PC9hPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9wPgogICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0ibWVkaWEtY29udGVudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJjb250ZW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8aW1nIGNsYXNzPSJsYXp5IiB2LWZvcj0iKHNyYywgaSkgaW4gcC5waG90b3MiIDpkYXRhLXNyYz0ic3JjIiA6a2V5PSJgJHtwLnVpZH1fJHtpfWAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgIDwvYi10YWItaXRlbT4KICAgICAgICAgICAgPGItdGFiLWl0ZW0+CiAgICAgICAgICAgICAgICA8dGVtcGxhdGUgc2xvdD0iaGVhZGVyIj4KICAgICAgICAgICAgICAgICAgICA8c3BhbiBjbGFzcz0iaGFzLXRleHQtd2llZ2h0LWJvbGQiPiBCw6xuaCBMdeG6rW4gPGItdGFnIHR5cGU9ImlzLXdhcm5pbmciCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByb3VuZGVkPiB7eyBjb21tZW50cy5sZW5ndGggfX0gPC9iLXRhZz4gPC9zcGFuPgogICAgICAgICAgICAgICAgPC90ZW1wbGF0ZT4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9ImZpZWxkIj4KICAgICAgICAgICAgICAgIDx0ZXh0YXJlYSBjbGFzcz0idGV4dGFyZWEiIHYtbW9kZWw9ImNvbW1lbnRzS2V5d29yZCIgcm93cz0iMTAiCiAgICAgICAgICAgICAgICAgICAgICAgICAgcGxhY2Vob2xkZXI9Ik5o4bqtcCBt4buXaSBiw6xuaCBsdeG6rW4gMSBkw7JuZyDEkeG7gyB0w6xtIGtp4bq/bSI+PC90ZXh0YXJlYT4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0iZmllbGQiPgogICAgICAgICAgICAgICAgICAgIDx0YWJsZSBjbGFzcz0idGFibGUgaXMtZnVsbHdpZHRoIGlzLWJvcmRlcmVkIGlzLXN0cmlwZWQgaXMtaG92ZXJhYmxlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRib2R5PgogICAgICAgICAgICAgICAgICAgICAgICA8dHIgdi1mb3I9IihjLCBpKSBpbiBmaWx0ZXJlZENvbW1lbnRzIiA6a2V5PSJpIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0iaXMtbmFycm93Ij57eyBpKzEgfX08L3RkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPnt7IGMgfX08L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgICAgICAgICA8L3Rib2R5PgogICAgICAgICAgICAgICAgICAgIDwvdGFibGU+CiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgPC9iLXRhYi1pdGVtPgogICAgICAgICAgICA8Yi10YWItaXRlbT4KICAgICAgICAgICAgICAgIDx0ZW1wbGF0ZSBzbG90PSJoZWFkZXIiPgogICAgICAgICAgICAgICAgICAgIDxzcGFuIGNsYXNzPSJoYXMtdGV4dC13aWVnaHQtYm9sZCI+IFRpbiBOaOG6r24gPGItdGFnIHR5cGU9ImlzLXdhcm5pbmciCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJvdW5kZWQ+IHt7IG1lc3NhZ2VzLmxlbmd0aCB9fSA8L2ItdGFnPiA8L3NwYW4+CiAgICAgICAgICAgICAgICA8L3RlbXBsYXRlPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0iZmllbGQiPgogICAgICAgICAgICAgICAgPHRleHRhcmVhIGNsYXNzPSJ0ZXh0YXJlYSIgdi1tb2RlbD0ibWVzc2FnZXNLZXl3b3JkIiByb3dzPSIxMCIKICAgICAgICAgICAgICAgICAgICAgICAgICBwbGFjZWhvbGRlcj0iTmjhuq1wIG3hu5dpIHTDqm4gMSBkw7JuZyDEkeG7gyB0w6xtIGtp4bq/bSI+PC90ZXh0YXJlYT4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0iZmllbGQiPgogICAgICAgICAgICAgICAgICAgIDx0YWJsZSBjbGFzcz0idGFibGUgaXMtZnVsbHdpZHRoIGlzLWJvcmRlcmVkIGlzLXN0cmlwZWQgaXMtaG92ZXJhYmxlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRib2R5PgogICAgICAgICAgICAgICAgICAgICAgICA8dHIgdi1mb3I9IihtLCBpKSBpbiBmaWx0ZXJlZE1lc3NhZ2VzIiA6a2V5PSJpIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0iaXMtbmFycm93Ij57eyBpKzEgfX08L3RkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPnt7IG0gfX08L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgICAgICAgICA8L3Rib2R5PgogICAgICAgICAgICAgICAgICAgIDwvdGFibGU+CiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgPC9iLXRhYi1pdGVtPgogICAgICAgIDwvYi10YWJzPgogICAgPC9kaXY+CjwvZGl2Pgo8c2NyaXB0IHNyYz0iaHR0cHM6Ly9jZG4uanNkZWxpdnIubmV0L25wbS92dWVAMi42LjEwL2Rpc3QvdnVlLmpzIj48L3NjcmlwdD4KPHNjcmlwdCBzcmM9Imh0dHBzOi8vdW5wa2cuY29tL2J1ZWZ5QDAuNy43L2Rpc3QvYnVlZnkubWluLmpzIj48L3NjcmlwdD4KPHNjcmlwdCBzcmM9Imh0dHBzOi8vY2RuLmpzZGVsaXZyLm5ldC9ucG0vdmFuaWxsYS1sYXp5bG9hZEAxMi4wLjAvZGlzdC9sYXp5bG9hZC5taW4uanMiPjwvc2NyaXB0Pgo8c2NyaXB0PgogICAgbGV0IGxhenlMb2FkSW5zdGFuY2UgPSBuZXcgTGF6eUxvYWQoewogICAgICAgIGVsZW1lbnRzX3NlbGVjdG9yOiAiLmxhenkiCiAgICB9KTsKCiAgICBuZXcgVnVlKHsKICAgICAgICBlbDogIiNhcHAiLAogICAgICAgIGRhdGEoKSB7CiAgICAgICAgICAgIHJldHVybiB7CiAgICAgICAgICAgICAgICBjb21tZW50czogW3tjb21tZW50c31dLAogICAgICAgICAgICAgICAgbWVzc2FnZXM6IFt7bWVzc2FnZXN9XSwKICAgICAgICAgICAgICAgIHBob3RvczogW3twaG90b3N9XSwKICAgICAgICAgICAgICAgIGNvbW1lbnRzS2V5d29yZDogIiIsCiAgICAgICAgICAgICAgICBtZXNzYWdlc0tleXdvcmQ6ICIiLAogICAgICAgICAgICAgICAgcGhvdG9zS2V5d29yZDogIiIsCiAgICAgICAgICAgIH0KICAgICAgICB9LAogICAgICAgIG1vdW50ZWQoKSB7CiAgICAgICAgICAgIGxhenlMb2FkSW5zdGFuY2UudXBkYXRlKCk7CiAgICAgICAgICAgIGNvbnNvbGUubG9nKCJ1cGRhdGUiKTsKICAgICAgICB9LAogICAgICAgIHdhdGNoOiB7CiAgICAgICAgICAgIHBob3Rvc0tleXdvcmQobmV3VmFsKXsKICAgICAgICAgICAgICAgIGxldCBsaW5lcyA9IG5ld1ZhbC5zcGxpdCgiXG4iKS5maWx0ZXIoeCA9PiB4LnRyaW0oKSAhPT0gIiIpLm1hcCh4ID0+IHgudHJpbSgpKTsKICAgICAgICAgICAgICAgIGlmIChsaW5lcy5sZW5ndGggPT09IDApIHsKICAgICAgICAgICAgICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHRoaXMucGhvdG9zLmxlbmd0aDsgaSsrKSB7CiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMucGhvdG9zW2ldLnNob3cgPSB0cnVlOwogICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgIH0gZWxzZSB7CiAgICAgICAgICAgICAgICAgICAgZm9yIChsZXQgaSA9IDA7IGkgPCB0aGlzLnBob3Rvcy5sZW5ndGg7IGkrKykgewogICAgICAgICAgICAgICAgICAgICAgICBmb3IgKGxldCBqID0gMDsgaiA8IGxpbmVzLmxlbmd0aDsgaisrKSB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5waG90b3NbaV0ubmFtZS5pbmRleE9mKGxpbmVzW2pdKSA9PT0gLTEpIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLnBob3Rvc1tpXS5zaG93ID0gZmFsc2U7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2UgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMucGhvdG9zW2ldLnNob3cgPSB0cnVlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgIH0KICAgICAgICB9LAogICAgICAgIGNvbXB1dGVkOiB7CiAgICAgICAgICAgIGZpbHRlcmVkUGhvdG9zKCkgewogICAgICAgICAgICAgICAgbGV0IGxpbmVzID0gdGhpcy5waG90b3NLZXl3b3JkLnNwbGl0KCJcbiIpLmZpbHRlcih4ID0+IHgudHJpbSgpICE9PSAiIikubWFwKHggPT4geC50cmltKCkpOwogICAgICAgICAgICAgICAgaWYgKGxpbmVzLmxlbmd0aCA9PT0gMCkgewogICAgICAgICAgICAgICAgICAgIGxhenlMb2FkSW5zdGFuY2UudXBkYXRlKCk7CiAgICAgICAgICAgICAgICAgICAgY29uc29sZS5sb2coInVwZGF0ZSIpOwogICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLnBob3RvczsKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgIGxldCByID0gW107CiAgICAgICAgICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHRoaXMucGhvdG9zLmxlbmd0aDsgaSsrKSB7CiAgICAgICAgICAgICAgICAgICAgZm9yIChsZXQgaiA9IDA7IGogPCBsaW5lcy5sZW5ndGg7IGorKykgewogICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5waG90b3NbaV0ubmFtZS5pbmRleE9mKGxpbmVzW2pdKSAhPT0gLTEpIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgIHIucHVzaCh0aGlzLnBob3Rvc1tpXSk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhawogICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgIGxhenlMb2FkSW5zdGFuY2UudXBkYXRlKCk7CiAgICAgICAgICAgICAgICBjb25zb2xlLmxvZygidXBkYXRlIik7CiAgICAgICAgICAgICAgICByZXR1cm4gcjsKICAgICAgICAgICAgfSwKICAgICAgICAgICAgZmlsdGVyZWRDb21tZW50cygpIHsKICAgICAgICAgICAgICAgIGxldCBsaW5lcyA9IHRoaXMuY29tbWVudHNLZXl3b3JkLnNwbGl0KCJcbiIpLmZpbHRlcih4ID0+IHgudHJpbSgpICE9PSAiIikubWFwKHggPT4geC50cmltKCkpOwogICAgICAgICAgICAgICAgaWYgKGxpbmVzLmxlbmd0aCA9PT0gMCkgewogICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLmNvbW1lbnRzOwogICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgbGV0IHIgPSBbXTsKICAgICAgICAgICAgICAgIGZvciAobGV0IGkgPSAwOyBpIDwgdGhpcy5jb21tZW50cy5sZW5ndGg7IGkrKykgewogICAgICAgICAgICAgICAgICAgIGZvciAobGV0IGogPSAwOyBqIDwgbGluZXMubGVuZ3RoOyBqKyspIHsKICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuY29tbWVudHNbaV0uaW5kZXhPZihsaW5lc1tqXSkgIT09IC0xKSB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLnB1c2godGhpcy5jb21tZW50c1tpXSk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhawogICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgcmV0dXJuIHI7CiAgICAgICAgICAgIH0sCiAgICAgICAgICAgIGZpbHRlcmVkTWVzc2FnZXMoKSB7CiAgICAgICAgICAgICAgICBsZXQgbGluZXMgPSB0aGlzLm1lc3NhZ2VzS2V5d29yZC5zcGxpdCgiXG4iKS5maWx0ZXIoeCA9PiB4LnRyaW0oKSAhPT0gIiIpLm1hcCh4ID0+IHgudHJpbSgpKTsKICAgICAgICAgICAgICAgIGlmIChsaW5lcy5sZW5ndGggPT09IDApIHsKICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5tZXNzYWdlczsKICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgIGxldCByID0gW107CiAgICAgICAgICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHRoaXMubWVzc2FnZXMubGVuZ3RoOyBpKyspIHsKICAgICAgICAgICAgICAgICAgICBmb3IgKGxldCBqID0gMDsgaiA8IGxpbmVzLmxlbmd0aDsgaisrKSB7CiAgICAgICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLm1lc3NhZ2VzW2ldLmluZGV4T2YobGluZXNbal0pICE9PSAtMSkgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgci5wdXNoKHRoaXMubWVzc2FnZXNbaV0pOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWsKICAgICAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgIHJldHVybiByOwogICAgICAgICAgICB9CiAgICAgICAgfQogICAgfSkKPC9zY3JpcHQ+CjwvYm9keT4KPC9odG1sPg==");
        private void taoFileHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < dtgvAcc.Rows.Count;)
                    {
                        if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int row = i++;
                                new Thread(() =>
                                {
                                    try
                                    {
                                        CreateAndCopyHtmlFromBackupTxt(row, template_backup, false, "");
                                        Interlocked.Decrement(ref iThread);
                                    }
                                    catch (Exception ex)
                                    {
                                        CommonCSharp.ExportError(null, ex.ToString());
                                    }
                                }).Start();
                            }
                            else
                            {
                                CommonCSharp.DelayTime(1);
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    while (iThread > 0)
                    {
                        CommonCSharp.DelayTime(1);
                    }
                }
                catch (Exception ex)
                {
                    CommonCSharp.ExportError(null, ex.ToString());
                }
            }).Start();
        }
        private void CreateAndCopyHtmlFromBackupTxt(int row, string template, bool isOpen = false, string pathDestination = "")
        {
            try
            {
                string cookie = CommonCSharp.GetStatusDataGridView(dtgvAcc, row, "cCookie");
                string uid = dtgvAcc.Rows[row].Cells["cUid"].Value.ToString();
                if (uid == "")
                    uid = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;
                lstNgaySinh = File.ReadAllText("output\\" /*+ uid + "\\"*/ +"ngaysinh.txt");
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Đang tạo file Html...");
                if (uid != "")
                {
                    string name = lstNgaySinh.Split('|')[1].Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'");
                    string birthday = lstNgaySinh.Split('|')[2];

                    //htmlEx += "\r\n $scope.user_obj = {\"name\": \"" + name + "\",\"type\": 0,\"email\": \"" + email + "\",\"link\": \"https://www.facebook.com/" + uid + "\",\"mobile_phone\": \"\",\"fb_id\": \"" + uid + "\",\"fb_cover_id\": \"\",\"fb_all_virtual_ids\": {\".\": \".\"},\"fb_avatar\": \"\",\"number_page\": 0,\"number_group\": \"" + group + "\",\"number_friend\": \"" + friend + "\",\"account_id\": \"" + uid + "\",\"data_obj\": {\"gender\": 2,\"birthday\": \"" + birthday + "\"},\"status\": 1,\"updated\": \"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\",\"customer_id\": \"xxx\",\"store_id\": \"xxx\",\"id\": \".\"}\r\n";
                    template = template.Replace("{{uid}}", uid).Replace("{{birthday}}", birthday).Replace("{{name}}", name);
                    string dirPath = @"output\" /*+ uid*/;

                    if (Directory.Exists(dirPath))
                    {
                        //Comment
                        if (File.Exists(dirPath + "\\" + "lscomment.txt"))
                        {
                            string comments = "";
                            List<string> lstDataCmt = File.ReadAllLines(dirPath + "\\" + "lscomment.txt").ToList();
                            int dem = lstDataCmt.Count;
                            for (int i = 0; i < dem; i++)
                            {
                                comments += "\"" + lstDataCmt[i].Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'") + "\",";
                            }
                            if (comments != "")
                            {
                                comments = comments.Remove(comments.Length - 1, 1);
                                template = template.Replace("{comments}", comments);
                            }
                            else
                                template = template.Replace("{comments}", "");
                        }
                        else
                            template = template.Replace("{comments}", "");
                        //Inbox
                        if (File.Exists(dirPath + "\\" + "banbeinbox.txt"))
                        {
                            string messages = "";
                            List<string> lstDataInbox = File.ReadAllLines(dirPath + "\\" + "banbeinbox.txt").ToList();
                            int dem = lstDataInbox.Count;
                            for (int i = 0; i < dem; i++)
                            {
                                messages += "\"" + lstDataInbox[i].Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'") + "\",";
                            }
                            if (messages != "")
                            {
                                messages = messages.Remove(messages.Length - 1, 1);
                                template = template.Replace("{messages}", messages);
                            }
                            else
                                template = template.Replace("{messages}", "");
                        }
                        else
                            template = template.Replace("{messages}", "");

                        //Ảnh
                        if (File.Exists(dirPath + "\\" + /*uid +*/ "total.txt"))
                        {
                            string photos = "";
                            string temp = "";
                            string[] splitInfor;
                            string data = File.ReadAllText(dirPath + /*"\\" + uid +*/ "total.txt");
                            string[] dataSplit = data.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < dataSplit.Length; i++)
                            {
                                string[] datals = dataSplit[i].Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                                if (datals.Length == 0)
                                    continue;
                                temp = "";
                                //string dataPt = "$scope.raw_photo_objs.push(";
                                //dataPt += "{\"id\": \"" + splitInfor[0] + "\",\"name\": \"" + splitInfor[1].Replace("\"", "\\\"") + "\", \"data\": [";
                                for (int j = 0; j < datals.Length; j++)
                                {
                                    splitInfor = datals[j].Split('*');
                                    //string source = datals[j].Split('*')[2].Split('|')[0];
                                    //dataPt += "{\"source\": \"" + source + "\",\"created_time\": \"" + DateTime.Now.ToString() + "\",\"id\": \"" + "__" + "\"},\r\n";
                                    temp += "\"" + splitInfor[2].Split('|')[0] + "\",";
                                }
                                //dataPt += "]});\r\n";
                                //htmlEx += dataPt;
                                temp = temp.Remove(temp.Length - 1, 1);
                                photos += "{\"uid\":\"" + datals[0].Split('*')[0] + "\",\"name\":\"" + datals[0].Split('*')[1] + "\",\"photos\":[" + temp + "],\"show\":true},";
                            }
                            photos = photos.Remove(photos.Length - 1, 1);
                            template = template.Replace("{photos}", photos);
                            File.AppendAllText(dirPath + "\\" + /*uid +*/ "total.html", template);
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Tạo thành công!");
                        }
                        else
                        {
                            CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Chưa backup!");
                        }
                    }
                }
                else
                {
                    CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Không có uid!");
                }
            }
            catch
            {
                CommonCSharp.UpdateStatusDataGridView(dtgvAcc, row, "cStatus", "Lỗi tạo file!");
            }
        }

        private void uidToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PasteIntoDataGridView(4);
        }

        private void uidTokenCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoDataGridView(5);
        }

        private void ckbUseTokenTg_CheckedChanged(object sender, EventArgs e)
        {
            plUseTokenTg.Enabled = ckbUseTokenTg.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ckbUseTokenTg_Click(object sender, EventArgs e)
        {
            if (ckbUseTokenTg.Checked)
            {
                MessageBox.Show("Chú ý nếu sử dụng token trung gian để backup theo Uid thì không backup tin nhắn, bình luận được!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void pasteCookieToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"input\token.txt");
            }
            catch
            {
            }
        }

        private void copyFileBackuptxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathDesti = "";
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    pathDesti = fbd.SelectedPath;
                }
                else
                {
                    return;
                }
            }
            int count = 0;
            for (int i = 0; i < dtgvAcc.Rows.Count; i++)
            {
                try
                {
                    if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChoose"].Value))
                    {
                        string uid = dtgvAcc.Rows[i].Cells["cUid"].Value.ToString();
                        try
                        {
                            if (File.Exists(@"output\" + uid + "\\" + uid + ".txt"))
                            {
                                File.Copy(@"output\" + uid + "\\" + uid + ".txt", pathDesti + "\\" + uid + ".txt");
                                count++;
                            }
                        }
                        catch { }
                    }
                }
                catch
                {

                }
            }
            MessageBox.Show("Copy thành công " + count + " tệp backup html!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("output");
            }
            catch
            {
            }
        }
    }
}