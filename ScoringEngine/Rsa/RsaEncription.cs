using System;
using System.Security.Cryptography;
using System.Text;

namespace ScoringEngine.Rsa
{
    public class RsaEncription : IDisposable
    {
        private RSACryptoServiceProvider _rsaCryptoService;

        public RsaEncription()
        {
            _rsaCryptoService = new RSACryptoServiceProvider();
        }

        public void RefreshKeys()
        {
            _rsaCryptoService = new RSACryptoServiceProvider();
        }

        public string GetKeyForEncrypt()
        {
            return _rsaCryptoService.ToXmlString(false);
        }

        public string EncryptData(string data, string publicKey = null)
        {
            if(!string.IsNullOrEmpty(publicKey))
            _rsaCryptoService.FromXmlString(publicKey);

            var dataToEncrypt = Encoding.UTF8.GetBytes(data);
            return Encoding.UTF8.GetString(_rsaCryptoService.Encrypt(dataToEncrypt, false));
        }

        public string DecryptData(string data)
        {
            var dataToEncrypt = Encoding.UTF8.GetBytes(data);
            return Encoding.UTF8.GetString(_rsaCryptoService.Decrypt(dataToEncrypt, false));
        }

        public void Dispose()
        {
            _rsaCryptoService.Dispose();
        }
    }
}