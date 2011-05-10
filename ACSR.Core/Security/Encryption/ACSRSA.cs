using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ACSR.Core.Security.Encryption
{
    public class ACSRSA
    {
        private RSACryptoServiceProvider _rsa;
        private string _privateKey;
        private string _publicKey;

        public ACSRSA ()
        {
            _rsa = new RSACryptoServiceProvider();
            _privateKey = _rsa.ToXmlString(true);
            
        }
        public string PublicKey
        { 
            get
            {
                return _rsa.ToXmlString(false);
            }           
            set
            {
                _publicKey = value;
            }
        }

        public string PrivateKey
        {
            get
            {
                return _rsa.ToXmlString(true);
            }
            set
            {
                _rsa.FromXmlString(value);
            }
        }

       
        public string EncryptString(string ClearText)
        {                            
            _rsa.FromXmlString(_publicKey);
            return Convert.ToBase64String(_rsa.Encrypt(Encoding.Unicode.GetBytes(ClearText), false));
        }
        public string DecryptString(string EncryptedString)
        {
            _rsa.FromXmlString(_privateKey);
            var dec = _rsa.Decrypt(Convert.FromBase64String(EncryptedString), false);
            return Encoding.Unicode.GetString(dec);
        }
    }
}
