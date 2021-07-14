using License.RNCryptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace License
{
    public class License
    {
        private string Usr = "";
        private string Pwd = "";
        private string Hardware = "";
        private string SoftIndex = "";
        private string RandomKey = "";

        private string ServerURL = "http://minmaxsoft.com/chapall/api.php/CheckAllToken/?api=";

        public License(string usr, string pwd, string hardware,string softIndex,string randomKey)
        {
            Usr = usr;
            Pwd = pwd;
            Hardware = hardware;
            SoftIndex = softIndex;
            RandomKey = randomKey;

        }
        public string CheckLic()
        {
            string result = "";
            try
            {

                string privateKey = "MINSOFTWARE_KEY_PRO" + RandomKey; // privateKey: khởi tạo key Random để khi Server nhận được Packer giải mã ở chuỗi này KEY_MD5 sau đó Encrypt lại với privateKey
                                                                   //Nếu decrypt thành công result trả từ Server xuống thì kiểm tra Validate
                                                                   //Để mình Setup trước cho cho dễ hiểu nguyên lý hoạt động chỉ có thế.
                                                                   //Nếu bị replace Server cũng không sao ,vì không có Key để Decrypt và chương trình không hiểu result Server là gì 
                                                                   //Kết hợp với Pack NETProtect.IO
                                                                   //Xong toàn bộ tôi hổ trợ Pack Dll để ngon lành tại NETProtect
                                                                   //co so crack xoa ham la xong khong a' khong xoa dc dau Thang nha boi vi Pack an toàn nhất thế giới rồi.
                                                                   //H setup chac tam 7h xong di an sang la vua :v

                Encryptor encrypt = new Encryptor();
                Decryptor decrypt = new Decryptor();

                string strRequest = SoftIndex + "|" + Hardware + "|" + Usr + "|" + Pwd + "|" + privateKey;

                //	$SoftIndex = $data[0];
                //  $Hardware = $data[1];
                //	$UserName = $data[2];
                //	$PassWord = $data[3];
                //	$CheckKey = $data[4];

                string strEncryptRequest = encrypt.Encrypt(strRequest, "KEY_8f558b28346e6cf3c_HASH_989ae0d760020f2");
                //Key này như kiểu để 2 chương trình hiểu nhau
                //Còn privateKey nó sinh ra mỗi khi Request ,nhận Response nó decrypt bằng Key đó.
                //thôi bank luôn có gì hỏi sau nhé
                //Okay Thăng nha ,yên tâm mình support ma xD 
                //Có dự án nào thì nói mình 
                //dự án?
                //ông code thuê
                //hay muốn hợp tác % thế
                //code thuê giá rẻ thôi ông 
                //Còn hợp tác thì thiệt ông quá doanh thu đang ổn mà xD
                //k thiệt đâu, t đang cần 1 số thứ hay ho hơn
                //Thì thăng cứ lên project đi mình code thuê cũng dc vì mình làm nhiều dự án lắm
                //Bên mình dev App,Webiste,Mobile,Android,IOS,Dich nguoc ma nguon,Crack,...Nhan tat ca du an lien quan toi IT
                //giai quyet nhanh gon le
                //ngân sách ntn nhỉ  - tùy vào mức độ của project, project material isocal? cai do thi tam khoang 50m full server quan ly ,admin ,....code bang nodejs, oke >< cai do code gap tam 3-7 ngay 
                //Dua toan bo len website ,quet sdt,khoi tao project,...admin quan ly + trinh quan ly da cap goi la refferal + auto payment <-- co nghia la tu dong thanh toan ko phai mua qua support nua, để nghĩ ra project gì t báo nhé, ông code nodejs thôi hở
                //Nodejs,php,C#,C++,C,AutoIT, phP frameword ? php codeigniter framework ,wordpress 

                string readHtmlServer = ReadHTMLCode(ServerURL + strEncryptRequest).Replace("\"","");

                if (readHtmlServer != null)
                {
                    result = readHtmlServer;
                }

            }
            catch { result = null; }
            return result;

        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string ReadHTMLCode(string URL)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] reqHTML = webClient.DownloadData(URL);
                UTF8Encoding objUTF8 = new UTF8Encoding();
                return objUTF8.GetString(reqHTML);
            }
            catch
            {
                return null;
            }
        }

    }
}
