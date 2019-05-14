using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class Key
    {
        
        public string GetAssemblyPath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }


        public byte[] GetAssemblyHash()
        {
            System.Security.Cryptography.HashAlgorithm HA = System.Security.Cryptography.SHA256.Create();

            try
            {
                byte[] b;
                using (System.IO.FileStream fs = new System.IO.FileStream(GetAssemblyPath(), System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    b = HA.ComputeHash(fs);
                return b;
            }
            catch
            {
                return null;
                //Random r = new Random();
                //int keyLen = 128;
                //byte[] b = new byte[keyLen];
                //r.NextBytes(b);
                //return b;
            }
            finally
            {
                HA.Dispose();
            }
        }

        private byte[] key = null;
        int bufferSize = 16384;


        public bool Encrypt(byte[] src, out byte[] dst)
        {
            dst = new byte[src.Length];

            if (key == null)
                key = GetAssemblyHash();

            System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create();
            System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;

            System.Security.Cryptography.ICryptoTransform ict =  aes.CreateEncryptor();
            
            for(int l = 0; l < src.Length;)
            {

                l += ict.TransformBlock(src, l, l + bufferSize< src.Length? bufferSize: src.Length - l, dst, l);
            }

            return true;


        }


    }
}
