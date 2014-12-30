using System;
using System.IO;
using System.Text;

namespace RestSharp.Portable.Socks.Socks5.Messages.Authentication
{
    public class PlainAuthenticationRequest : Request
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false);

        public string UserName { get; private set; }
        public string Password { get; private set; }

        public PlainAuthenticationRequest(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Parameter must not be null or empty", "userName");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Parameter must not be null or empty", "password");
            UserName = userName;
            Password = password;
        }

        protected override void WritePayloadTo(BinaryWriter writer)
        {
            var userNameData = _encoding.GetBytes(UserName);
            var passwordData = _encoding.GetBytes(Password);
            writer.Write((byte)userNameData.Length);
            writer.Write(userNameData);
            writer.Write((byte)passwordData.Length);
            writer.Write(passwordData);
        }
    }
}