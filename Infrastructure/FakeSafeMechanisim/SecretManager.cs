﻿using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FakeSafeMechanisim
{
    public class SecretManager
    {
        public static (bool, NetworkCredential) GetUserCredentials(TokenSafeCode tokenSafeCode)
        {
            (string Username, string Password, bool isExist) = UserCredentials.GetUsernamePassword(tokenSafeCode.UsernameCode, tokenSafeCode.PasswordCode);
            if (isExist)
            {
                return (true, new NetworkCredential(Username,Password));
            }
            return (false, new NetworkCredential());
        }
        public static SecureString ToSecureString(string text)
        {
            if (text is null)
                return null;
            var secureText = new SecureString();
            foreach (var c in text)
                secureText.AppendChar(c);
            secureText.MakeReadOnly();
            return secureText;
        }
    }
    public class UserCredentials
    {
        public string UsernameCode { get; set; }
        public string PasswordCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static (string Username, string Password, bool isExist) GetUsernamePassword(string usernameCode, string passwordCode)
        {
            var credentialsList = new List<UserCredentials>
            {
                new UserCredentials
                {
                    UsernameCode = "TERMINAL_USERNAME",
                    PasswordCode = "TERMINAL_PASSWORD",
                    Username = "terminal_user",
                    Password = "123456"
                },
                new UserCredentials
                {
                    UsernameCode = "SERVER_USERNAME",
                    PasswordCode = "SERVER_PASSWORD",
                    Username = "ServiceAppUser",
                    Password = "1q2w3e4r5t*"
                }
            };

            UserCredentials credentials = credentialsList.FirstOrDefault(c =>
                c.UsernameCode == usernameCode && c.PasswordCode == passwordCode);

            if (credentials != null)
            {
                return (credentials.Username, credentials.Password, true);
            }
            else
            {
                return (null, null, false);
            }
        }
    }
}
