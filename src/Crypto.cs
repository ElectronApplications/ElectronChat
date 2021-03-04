using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace ElectronChat
{
    class Crypto
    {

        public static byte[] sha256(byte[] bytes)
        {
            return SHA256.Create().ComputeHash(bytes);
        }

        public static byte[] AES256Encrypt(byte[] key, string msg)
        {
            var aes = new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };
            var encryptor = aes.CreateEncryptor();
            
            var msgList = Encoding.UTF8.GetBytes(msg).ToList();
            while (msgList.Count % 16 != 0)
                msgList.Add(0x00);
            var msgArr = msgList.ToArray();

            return encryptor.TransformFinalBlock(msgArr, 0, msgArr.Length);
        }

        public static string AES256Decrypt(byte[] key, byte[] msg)
        {
            var aes = new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };
            var decryptor = aes.CreateDecryptor();
            return Encoding.UTF8.GetString(Program.TrimBytes(decryptor.TransformFinalBlock(msg, 0, msg.Length)));
        }

        public static byte[] RSAEncrypt(string publicKeyStr, byte[] msg)
        {
            var csp = new RSACryptoServiceProvider();

            var sr = new StringReader(publicKeyStr);
            var xs = new XmlSerializer(typeof(RSAParameters));
            var publicKey = (RSAParameters) xs.Deserialize(sr);
            csp.ImportParameters(publicKey);

            return csp.Encrypt(msg, true);
        }

        public static byte[] RSADecrypt(RSAParameters privateKey, byte[] msg)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);
            return csp.Decrypt(msg, true);
        }

        public static (RSAParameters privateKey, string publicKey) RSACreate()
        {
            var csp = new RSACryptoServiceProvider(2048);
            var privateKey = csp.ExportParameters(true);
            var publicKey = csp.ExportParameters(false);

            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));

            xs.Serialize(sw, publicKey);
            string publicKeyStr = sw.ToString();

            return (privateKey:privateKey, publicKey:publicKeyStr);
        }

    }
}