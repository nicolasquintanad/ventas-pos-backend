using System;
using System.Linq;
using System.Web.Http;
using api_amanda.Models;
using api_amanda.Models.DTO;

namespace api_amanda.Controllers
{
    [RoutePrefix("packs")]
    public class PackController : ApiController
    {
        // 🚀 Generar SKU automático
        private string GenerarSkuPack(AMANDAEntities db)
        {
            int count = db.PACK.Count() + 1;
            return "PK" + count.ToString("D4");
        }

        // 📌 POST /packs (crear pack con detalles)
        [HttpPost]
        [Route("")]
        public IHttpActionResult CrearPack([FromBody] PackDto dto)
        {
            if (dto == null || dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest("Datos incompletos.");

            using (var db = new AMANDAEntities())
            {
                var skuGenerado = "";
                if (string.IsNullOrWhiteSpace(dto.SKU_PACK))
                {
                    skuGenerado = GenerarSkuPack(db);
                }
                else
                {
                    skuGenerado = dto.SKU_PACK;
                }

                //var skuGenerado = GenerarSkuPack(db);

                var nuevoPack = new PACK
                {
                    SKU_PACK = skuGenerado,
                    NOMBRE_PACK = dto.NOMBRE_PACK,
                    PRECIO_PACK = dto.PRECIO_PACK,
                    EXCENTO_IVA = dto.EXCENTO_IVA,
                    FECHA_CRECION = DateTime.Now
                };

                db.PACK.Add(nuevoPack);
                db.SaveChanges(); // Obtener ID_PACK

                // Guardar productos del pack
                foreach (var item in dto.Detalles)
                {
                    db.PACK_DETALLE.Add(new PACK_DETALLE
                    {
                        ID_PACK = nuevoPack.ID_PACK,
                        ID_PRODUCTO = item.ID_PRODUCTO,
                        CANTIDAD_PRODUCTO = item.CANTIDAD_PRODUCTO,
                        FECHA_CRECION = DateTime.Now
                    });
                }

                db.SaveChanges();

                return Ok("Pack creado correctamente");
            }
        }

        //  GET /packs (listar packs)
        [HttpGet]
        [Route("")]
        public IHttpActionResult ListarPacks()
        {
            using (var db = new AMANDAEntities())
            {
                var list = db.PACK
                    .Select(p => new
                    {
                        p.ID_PACK,
                        p.SKU_PACK,
                        p.NOMBRE_PACK,
                        p.PRECIO_PACK,
                        p.EXCENTO_IVA,
                        typeName = "pack",

                // 🔥 STOCK DEL PACK (mínimo de los productos que lo componen)
                STOCK_PACK = db.PACK_DETALLE
                            .Where(d => d.ID_PACK == p.ID_PACK)
                            .Select(d => (int?)(
                                db.PRODUCTO
                                    .Where(pr => pr.ID_PRODUCTO == d.ID_PRODUCTO)
                                    .Select(pr => pr.STOCK)
                                    .FirstOrDefault() / d.CANTIDAD_PRODUCTO
                            ))
                            .Min() ?? 0
                    })
                    .OrderBy(p => p.NOMBRE_PACK)
                    .ToList();

                return Ok(list);
            }
        }

        //  GET /packs/{id}/detalles (listar productos dentro del pack)
        [HttpGet]
        [Route("{id}/detalles")]
        public IHttpActionResult DetallesPack(int id)
        {
            using (var db = new AMANDAEntities())
            {
                var detalles = db.PACK_DETALLE
                    .Where(d => d.ID_PACK == id)
                    .Select(d => new
                    {
                        producto = d.PRODUCTO.NOMBRE,
                        cantidad = d.CANTIDAD_PRODUCTO,
                        sku = d.PRODUCTO.SKU
                    })
                    .ToList();

                return Ok(detalles);
            }
        }
    }
}
