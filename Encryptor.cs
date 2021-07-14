using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace License.RNCryptor
{
    public class Encryptor : Cryptor
    {
        //Write by: Le Bach Hiep

        private Schema defaultSchemaVersion = Schema.V2;

        public string Encrypt(string plaintext, string password)
        {
            try
            {
                return Base64Encode(this.Encrypt(plaintext, password, this.defaultSchemaVersion));
            }
            catch
            {
                return "null";
            }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Encrypt(string plaintext, string password, Schema schemaVersion)
        {

            this.configureSettings(schemaVersion);

            byte[] plaintextBytes = TextEncoding.GetBytes(plaintext);

            PayloadComponents components = new PayloadComponents();
            components.schema = new byte[] { (byte)schemaVersion };
            components.options = new byte[] { (byte)this.options };
            components.salt = this.generateRandomBytes(Cryptor.saltLength);
            components.hmacSalt = this.generateRandomBytes(Cryptor.saltLength);
            components.iv = this.generateRandomBytes(Cryptor.ivLength);

            byte[] key = this.generateKey(components.salt, password);

            switch (this.aesMode)
            {
                case AesMode.CTR:
                    components.ciphertext = this.encryptAesCtrLittleEndianNoPadding(plaintextBytes, key, components.iv);
                    break;

                case AesMode.CBC:
                    components.ciphertext = this.encryptAesCbcPkcs7(plaintextBytes, key, components.iv);
                    break;
            }

            components.hmac = this.generateHmac(components, password);

            List<byte> binaryBytes = new List<byte>();
            binaryBytes.AddRange(this.assembleHeader(components));
            binaryBytes.AddRange(components.ciphertext);
            binaryBytes.AddRange(components.hmac);

            return Convert.ToBase64String(binaryBytes.ToArray());
        }

        private byte[] encryptAesCbcPkcs7(byte[] plaintext, byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var encryptor = aes.CreateEncryptor(key, iv);

            byte[] encrypted;

            using (var ms = new MemoryStream())
            {
                using (var cs1 = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs1.Write(plaintext, 0, plaintext.Length);
                }

                encrypted = ms.ToArray();
            }

            return encrypted;
        }

        private byte[] generateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            return randomBytes;
        }
    }
}
