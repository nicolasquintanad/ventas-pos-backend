using System;
using System.Linq;
using System.Web.Http;
using api_amanda.Models;
using api_amanda.Models.DTO;

namespace api_amanda.Controllers
{
    [RoutePrefix("proveedores")]
    public class ProveedorController : ApiController
    {
        // GET /proveedores
        private readonly AMANDAEntities db = new AMANDAEntities();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProveedores()
        {
            using (var db = new AMANDAEntities())
            {
                var proveedores = db.PROVEEDOR
                    .OrderBy(p => p.NOMBRE)
                    .Select(p => new
                    {
                        id = p.ID_PROVEEDOR,
                        nombre = p.NOMBRE,
                        telefono = p.TELEFONO,
                        correo = p.CORREO,
                        direccion = p.DIRECCION,
                        ciudad = p.CIUDAD,
                        VISITA_LUNES = p.VISITA_LUNES,
                        VISITA_MARTES = p.VISITA_MARTES,
                        VISITA_MIERCOLES = p.VISITA_MIERCOLES,
                        VISITA_JUEVES = p.VISITA_JUEVES,
                        VISITA_VIERNES = p.VISITA_VIERNES,
                        VISITA_SABADO = p.VISITA_SABADO,
                        VISITA_DOMINGO = p.VISITA_DOMINGO
                    })
                    .ToList();

                return Ok(proveedores);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateProvider(ProveedorDto dto)
        {
            var p = new PROVEEDOR
            {
                NOMBRE = dto.NOMBRE,
                TELEFONO = dto.TELEFONO,
                CORREO = dto.CORREO,
                DIRECCION = dto.DIRECCION,
                CIUDAD = dto.CIUDAD,
                FECHA_CREACION = DateTime.Now,
                VISITA_LUNES = dto.VISITA_LUNES,
                VISITA_MARTES = dto.VISITA_MARTES,
                VISITA_MIERCOLES = dto.VISITA_MIERCOLES,
                VISITA_JUEVES = dto.VISITA_JUEVES,
                VISITA_VIERNES = dto.VISITA_VIERNES,
                VISITA_SABADO = dto.VISITA_SABADO,
                VISITA_DOMINGO = dto.VISITA_DOMINGO
            };

            db.PROVEEDOR.Add(p);
            db.SaveChanges();
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateProvider(int id, ProveedorDto dto)
        {
            var p = db.PROVEEDOR.Find(id);
            if (p == null) return NotFound();

            p.NOMBRE = dto.NOMBRE;
            p.TELEFONO = dto.TELEFONO;
            p.CORREO = dto.CORREO;
            p.DIRECCION = dto.DIRECCION;
            p.CIUDAD = dto.CIUDAD;
            p.VISITA_LUNES = dto.VISITA_LUNES;
            p.VISITA_MARTES = dto.VISITA_MARTES;
            p.VISITA_MIERCOLES = dto.VISITA_MIERCOLES;
            p.VISITA_JUEVES = dto.VISITA_JUEVES;
            p.VISITA_VIERNES = dto.VISITA_VIERNES;
            p.VISITA_SABADO = dto.VISITA_SABADO;
            p.VISITA_DOMINGO = dto.VISITA_DOMINGO;

            db.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteProvider(int id)
        {
            var p = db.PROVEEDOR.Find(id);
            if (p == null) return NotFound();

            db.PROVEEDOR.Remove(p);
            db.SaveChanges();
            return Ok();
        }
    }
}
