using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Security.Cryptography;
using System.Configuration;

namespace License
{
    public class Hardware
    {
        private string maHoa(string sChuoi)
        {
            MD5 obj = MD5.Create();
            byte[] data = obj.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sChuoi));
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                s.Append(String.Format("{0:x2}", data[i]));
            }
            return s.ToString();
        }
        public string getHDD() //Hardware hardware = new Hardware(); hardware.getHDD(); <--- la okay nha
        {
            string temp = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                temp = maHoa(EncryptHDD("" + queryObj["SerialNumber"], true)).ToUpper();
                break;
            }
            return temp;
        }
        private static string EncryptHDD(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();

            string key = "#HiepdepTrai#";
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
    }
}
