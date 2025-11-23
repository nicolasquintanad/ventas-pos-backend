using api_amanda.Helpers;
using api_amanda.Models;
using api_amanda.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api_amanda.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("auth")]
    public class AuthController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public IHttpActionResult LoginUser(LoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Datos incompletos");

            using (var db = new AMANDAEntities())
            {
                // Buscar usuario
                var usuario = db.USUARIO
                    .FirstOrDefault(u => u.USERNAME == dto.Username && u.ACTIVO == true);

                if (usuario == null || !Encriptacion.VerificarPassword(dto.Password, usuario.PASSWORD))
                    return BadRequest("Usuario o contraseña inválidos");

                if (usuario == null)
                    return BadRequest("Usuario o contraseña inválidos");

                // Obtener rol
                var rol = db.ROL_USUARIO.FirstOrDefault(r => r.ID_USUARIO == usuario.ID_USUARIO);

                if (rol == null)
                    return BadRequest("El usuario no tiene rol asignado");

                // Simular token
                string token = Guid.NewGuid().ToString();

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = usuario.ID_USUARIO,
                        name = usuario.NOMBRE,
                        email = usuario.CORREO,
                        username = usuario.USERNAME,
                        role = rol.SLUG // admin, cajero, etc.
                    }
                });
            }
        }
        //[HttpGet]
        //[Route("encriptar")]
        //public IHttpActionResult passEncriptar()
        //{
        //    using (var db = new AMANDAEntities())
        //    {
        //        var usuarios = db.USUARIO.ToList();

        //        foreach (var u in usuarios)
        //        {
        //            u.PASSWORD = Encriptacion.HashPassword(u.PASSWORD);
        //        }
        //        db.SaveChanges();
        //        return Ok();
        //    }
        //}
    }
    public class Root
    {
        public string token { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string email { get; set; }
    }
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
