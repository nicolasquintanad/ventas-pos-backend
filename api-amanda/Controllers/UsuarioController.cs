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
    [RoutePrefix("usuarios")]
    public class UsuarioController : ApiController
    {
        // LISTAR USUARIOS CON ROL
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUsuarios(int idAdmin)
        {
            if (!Permisos.EsAdmin(idAdmin))
                return BadRequest("No autorizado.");

            using (var db = new AMANDAEntities())
            {
                var lista = (from u in db.USUARIO
                             join r in db.ROL_USUARIO on u.ID_USUARIO equals r.ID_USUARIO
                             select new
                             {
                                 u.ID_USUARIO,
                                 u.NOMBRE,
                                 u.CORREO,
                                 u.USERNAME,
                                 u.RUT,
                                 u.ACTIVO,
                                 rol = r.SLUG
                             }).ToList();

                return Ok(lista);
            }
        }
        [HttpPost]
        [Route("crear")]
        public IHttpActionResult CrearUsuario(UsuarioDto dto, int idAdmin)
        {
            if (!Permisos.EsAdmin(idAdmin))
                return BadRequest("No autorizado.");

            if (dto == null || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Datos incompletos");

            using (var db = new AMANDAEntities())
            {
                if (db.USUARIO.Any(u => u.USERNAME == dto.Username))
                    return BadRequest("Ya existe un usuario con ese nombre");

                var nuevo = new USUARIO
                {
                    NOMBRE = dto.Nombre,
                    CORREO = dto.Correo,
                    USERNAME = dto.Username,
                    PASSWORD = Encriptacion.HashPassword(dto.Password),
                    RUT = dto.Rut,
                    ACTIVO = true,
                    FECHA_CREACION = System.DateTime.Now
                };

                db.USUARIO.Add(nuevo);
                db.SaveChanges();

                var rol = new ROL_USUARIO
                {
                    NOMBRE = dto.Rol.ToUpper(),
                    SLUG = dto.Rol.ToLower(),
                    FECHA_CREACION = System.DateTime.Now,
                    ID_USUARIO = nuevo.ID_USUARIO
                };

                db.ROL_USUARIO.Add(rol);
                db.SaveChanges();

                return Ok("Usuario creado correctamente");
            }
        }
        [HttpPut]
        [Route("editar/{id}")]
        public IHttpActionResult EditarUsuario(int id, UsuarioDto dto, int idAdmin)
        {
            if (!Permisos.EsAdmin(idAdmin))
                return BadRequest("No autorizado.");

            using (var db = new AMANDAEntities())
            {
                var u = db.USUARIO.Find(id);
                if (u == null) return NotFound();

                if (db.USUARIO.Any(x => x.USERNAME == dto.Username && x.ID_USUARIO != id))
                    return BadRequest("Ya existe otro usuario con ese nombre de usuario");

                u.NOMBRE = dto.Nombre;
                u.CORREO = dto.Correo;
                u.USERNAME = dto.Username;
                u.RUT = dto.Rut;

                if (!string.IsNullOrEmpty(dto.Password))
                    u.PASSWORD = Encriptacion.HashPassword(dto.Password);

                // Cambia rol
                var rol = db.ROL_USUARIO.FirstOrDefault(r => r.ID_USUARIO == id);
                if (rol != null) rol.SLUG = dto.Rol.ToLower();

                db.SaveChanges();
                return Ok("Usuario actualizado");
            }
        }
        [HttpDelete]
        [Route("estado/{id}")]
        public IHttpActionResult CambiarEstado(int id, int idAdmin)
        {
            if (!Permisos.EsAdmin(idAdmin))
                return BadRequest("No autorizado.");
            using (var db = new AMANDAEntities())
            {
                var u = db.USUARIO.Find(id);
                if (u == null) return NotFound();

                u.ACTIVO = !u.ACTIVO;
                db.SaveChanges();
                return Ok("Estado actualizado");
            }
        }
    }
}
