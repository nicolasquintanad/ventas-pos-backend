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
    [RoutePrefix("types")]
    public class TipoProductoController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTipos()
        {
            using (var db = new AMANDAEntities())
            {
                var tipos = db.TIPO_PRODUCTO.Select(t => new
                {
                    id = t.ID_TIPO_PRODUCTO,
                    name = t.NOMBRE,
                    cigarrillo = t.CIGARRILLO
                }).ToList();

                return Ok(tipos);
            }
        }
        // POST: products/types
        [HttpPost]
        [Route("")]
        public IHttpActionResult CrearTipo(TipoProductoDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos");

            using (var db = new AMANDAEntities())
            {
                var nuevo = new TIPO_PRODUCTO
                {
                    NOMBRE = dto.NOMBRE,
                    CIGARRILLO = dto.CIGARRILLO
                };

                db.TIPO_PRODUCTO.Add(nuevo);
                db.SaveChanges();

                return Ok(new { message = "Tipo de producto creado correctamente" });
            }
        }

        // PUT: products/types/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult EditarTipo(int id, TipoProductoDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos");

            using (var db = new AMANDAEntities())
            {
                var tipo = db.TIPO_PRODUCTO.FirstOrDefault(t => t.ID_TIPO_PRODUCTO == id);
                if (tipo == null) return NotFound();

                tipo.NOMBRE = dto.NOMBRE;
                tipo.CIGARRILLO = dto.CIGARRILLO;

                db.SaveChanges();

                return Ok(new { message = "Tipo de producto actualizado" });
            }
        }

        // DELETE: products/types/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult EliminarTipo(int id)
        {
            using (var db = new AMANDAEntities())
            {
                var tipo = db.TIPO_PRODUCTO.FirstOrDefault(t => t.ID_TIPO_PRODUCTO == id);
                if (tipo == null) return NotFound();

                db.TIPO_PRODUCTO.Remove(tipo);
                db.SaveChanges();

                return Ok(new { message = "Tipo de producto eliminado" });
            }
        }
    }
}
