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
    [RoutePrefix("api/impresoras")]
    public class ImpresoraController : ApiController
    {
        private static readonly List<ImpresoraDto> _data = new List<ImpresoraDto>();

        // GET: api/impresoras
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            using (var db = new AMANDAEntities())
            {
                var data = (
                    from i in db.IMPRESORA
                    join ci in db.CAJA_IMPRESORA on i.ID_IMPRESORA equals ci.ID_IMPRESORA into gj
                    from x in gj.DefaultIfEmpty()
                    join c in db.CAJA on x.ID_CAJA equals c.ID_CAJA into cj
                    from caja in cj.DefaultIfEmpty()
                    select new ImpresoraDto
                    {
                        IdImpresora = i.ID_IMPRESORA,
                        NombreWindows = i.NOMBRE_WINDOWS,
                        Tipo = i.TIPO,
                        Activa = i.ACTIVA,
                        IdCaja = caja != null ? (int?)caja.ID_CAJA : null,
                        NombreCaja = caja != null ? caja.NOMBRE : null
                    }
                ).ToList();

                return Ok(data);
            }
        }

        // POST: api/impresoras
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(ImpresoraDto dto)
        {
            using (var db = new AMANDAEntities())
            {
                var imp = new IMPRESORA
                {
                    NOMBRE_WINDOWS = dto.NombreWindows,
                    TIPO = dto.Tipo,
                    ACTIVA = dto.Activa
                };

                db.IMPRESORA.Add(imp);
                db.SaveChanges();

                if (dto.IdCaja.HasValue)
                {
                    db.CAJA_IMPRESORA.Add(new CAJA_IMPRESORA
                    {
                        ID_CAJA = dto.IdCaja.Value,
                        ID_IMPRESORA = imp.ID_IMPRESORA
                    });
                    db.SaveChanges();
                }

                return Ok();
            }
        }

        // PUT: api/impresoras/{id}
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, ImpresoraDto dto)
        {
            using (var db = new AMANDAEntities())
            {
                var imp = db.IMPRESORA.Find(id);
                if (imp == null) return NotFound();

                imp.NOMBRE_WINDOWS = dto.NombreWindows;
                imp.TIPO = dto.Tipo;
                imp.ACTIVA = dto.Activa;

                var rel = db.CAJA_IMPRESORA.FirstOrDefault(x => x.ID_IMPRESORA == id);
                if (rel != null)
                    db.CAJA_IMPRESORA.Remove(rel);

                if (dto.IdCaja.HasValue)
                {
                    db.CAJA_IMPRESORA.Add(new CAJA_IMPRESORA
                    {
                        ID_CAJA = dto.IdCaja.Value,
                        ID_IMPRESORA = id
                    });
                }

                db.SaveChanges();
                return Ok();
            }
        }

        // DELETE: api/impresoras/{id}
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            using (var db = new AMANDAEntities())
            {
                var rel = db.CAJA_IMPRESORA.FirstOrDefault(x => x.ID_IMPRESORA == id);
                if (rel != null)
                    db.CAJA_IMPRESORA.Remove(rel);

                var imp = db.IMPRESORA.Find(id);
                if (imp == null) return NotFound();

                db.IMPRESORA.Remove(imp);
                db.SaveChanges();

                return Ok();
            }
        }
    }
}
