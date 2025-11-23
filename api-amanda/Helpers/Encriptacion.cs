using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Helpers
{
    public static class Encriptacion
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerificarPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}