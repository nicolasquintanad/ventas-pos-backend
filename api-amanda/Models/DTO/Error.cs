using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class Error
    {
        public string Message { get; set; }
        public string Detail { get; set; }

        public Error(string message, string detail = null)
        {
            Message = message;
            Detail = detail;
        }
    }
}