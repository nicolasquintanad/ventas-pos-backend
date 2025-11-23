using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class UsuarioDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Username { get; set; }
        public string Rut { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // admin, cajero, etc.
    }
}