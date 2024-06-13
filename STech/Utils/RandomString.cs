using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.Utils
{
    public class RandomString
    {
        public static string random(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] randomChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                randomChars[i] = chars[random.Next(chars.Length)];
            }

            string str = new string(randomChars);

            return str;
        }
    }
}