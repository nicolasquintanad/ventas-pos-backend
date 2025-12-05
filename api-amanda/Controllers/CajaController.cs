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
    [RoutePrefix("caja")]
    public class CajaController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetCajas()
        {
            using (var db = new AMANDAEntities())
            {
                var list = db.CAJA
                    .Select(c => new
                    {
                        c.ID_CAJA,
                        c.NOMBRE
                    }).ToList();

                return Ok(list);
            }
        }
        [HttpGet]
        [Route("activa/{idUsuario}")]
        public IHttpActionResult GetCajaActiva(int idUsuario)
        {
            using (var db = new AMANDAEntities())
            {
                var activa = db.APERTURA_CIERRE
                    .Where(a => a.ID_USUARIO == idUsuario && a.ACTIVA == true)
                    .Select(a => new
                    {
                        a.ID_APERTURA_CIERRE,
                        a.MONTO_INICIAL,
                        a.FECHA_APERTURA,
                        a.ID_CAJA,
                        caja = a.CAJA.NOMBRE
                    })
                    .FirstOrDefault();

                return Ok(activa);
            }
        }
        [HttpPost]
        [Route("apertura")]
        public IHttpActionResult AbrirCaja(int idUsuario, int idCaja, int montoInicial)
        {
            using (var db = new AMANDAEntities())
            {
                // Validar usuario con permiso
                var usuario = db.USUARIO.Find(idUsuario);
                var rol = db.ROL_USUARIO.FirstOrDefault(r => r.ID_USUARIO == usuario.ID_USUARIO);

                if (rol == null || (rol.SLUG != "admin" && rol.SLUG != "cajero"))
                    return Content(HttpStatusCode.BadRequest, new Error("No tiene permisos para abrir caja."));

                // Validar si ya tiene caja activa
                var activa = db.APERTURA_CIERRE
                    .FirstOrDefault(a => a.ID_CAJA == idCaja && a.ACTIVA == true);

                if (activa != null)
                    return Content(HttpStatusCode.BadRequest, new Error("Este usuario ya tiene una caja activa."));

                // Crear apertura
                var apertura = new APERTURA_CIERRE
                {
                    ID_CAJA = idCaja,
                    ID_USUARIO = idUsuario,
                    FECHA_APERTURA = System.DateTime.Now,
                    MONTO_INICIAL = montoInicial,
                    ACTIVA = true
                };

                db.APERTURA_CIERRE.Add(apertura);
                db.SaveChanges();

                return Ok(new { message = "Caja abierta correctamente" });
            }
        }
        [HttpPost]
        [Route("cierre")]
        public IHttpActionResult CerrarCaja(int idUsuario)
        {
            using (var db = new AMANDAEntities())
            {
                // Buscar caja activa del usuario
                var apertura = db.APERTURA_CIERRE
                    .FirstOrDefault(a => a.ID_USUARIO == idUsuario && a.ACTIVA == true);

                if (apertura == null)
                    return Content(HttpStatusCode.BadRequest, new Error("Este usuario no tiene una caja activa."));

                DateTime fechaAhora = DateTime.Now;

                // Total de ventas SOLO durante la apertura
                var totalVentas = db.VENTA
                    .Where(v => v.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE)
                    .Sum(v => (int?)v.TOTAL) ?? 0;

                // Cálculo correcto del Monto Final
                var montoFinal = apertura.MONTO_INICIAL + totalVentas;

                // Cerrar caja
                apertura.ACTIVA = false;
                apertura.FECHA_CIERRE = fechaAhora;
                apertura.MONTO_FINAL = montoFinal;

                db.SaveChanges();

                var montoCigarros = db.DETALLE_VENTA
                    .Where(d => d.VENTA.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE
                     && d.ID_PRODUCTO != null
                     && d.PRODUCTO.TIPO_PRODUCTO.NOMBRE == "Cigarros")
                    .Sum(d => (decimal?)d.SUBTOTAL) ?? 0;

                var montoProductos = (totalVentas - (int)montoCigarros);

                // TOTAL PRODUCTOS (sin cigarrillos)
                //var montoProductos = db.DETALLE_VENTA
                //    .Where(d => d.VENTA.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE
                //             && d.ID_PRODUCTO != null
                //             && d.PRODUCTO.TIPO_PRODUCTO.NOMBRE.ToLower() != "cigarrillos")
                //    .Sum(d => (decimal?)(d.CANTIDAD * d.PRECIO_UNITARIO)) ?? 0;

                var totalGeneral = montoCigarros + montoProductos;

                // PACKS vendidos (líneas pack)
                var packsVendidos = db.DETALLE_VENTA
                    .Where(d => d.VENTA.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE
                             && d.ID_PRODUCTO == null)
                    .Sum(d => (decimal?)d.CANTIDAD) ?? 0;

                // Productos individuales vendidos
                var productosIndividualesVendidos = db.DETALLE_VENTA
                    .Where(d => d.VENTA.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE
                             && d.ID_PRODUCTO != null)
                    .Sum(d => (decimal?)d.CANTIDAD) ?? 0;

                // Productos contenidos dentro de packs
                var productosEnPacks = (
                    from dv in db.DETALLE_VENTA
                    join p in db.PACK on dv.SKU equals p.SKU_PACK
                    join pd in db.PACK_DETALLE on p.ID_PACK equals pd.ID_PACK
                    where dv.VENTA.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE
                          && dv.ID_PRODUCTO == null
                    select (pd.CANTIDAD_PRODUCTO * dv.CANTIDAD)
                ).Sum() ?? 0;

                // Total productos (individuales + internos de packs)
                var productosVendidosTotal = productosIndividualesVendidos + productosEnPacks;


                // RESPUESTA
                //return Ok(new
                //{
                //    message = "Caja cerrada correctamente",
                //    idAperturaCierre = apertura.ID_APERTURA_CIERRE,
                //    caja = apertura.CAJA.NOMBRE,
                //    fechaApertura = apertura.FECHA_APERTURA,
                //    fechaCierre = apertura.FECHA_CIERRE,
                //    montoInicial = apertura.MONTO_INICIAL,
                //    totalVentas = totalVentas,
                //    montoFinal = montoFinal,
                //    montoCigarros = montoCigarros,


                //    // Extras interesantes para mostrar en impresión o PDF
                //    usuario = db.USUARIO.Where(u => u.ID_USUARIO == apertura.ID_USUARIO).Select(u => u.NOMBRE).FirstOrDefault(),
                //    totalTransacciones = db.VENTA
                //        .Where(v => v.ID_CAJA == apertura.ID_CAJA
                //                 && v.FECHA >= apertura.FECHA_APERTURA
                //                 && v.FECHA <= fechaAhora).Count(),
                //    productosVendidos = db.DETALLE_VENTA
                //        .Where(d => d.VENTA.ID_CAJA == apertura.ID_CAJA
                //                 && d.VENTA.FECHA >= apertura.FECHA_APERTURA
                //                 && d.VENTA.FECHA <= fechaAhora
                //                 && d.ID_PRODUCTO != null)
                //        .Sum(d => (decimal?)d.CANTIDAD) ?? 0,
                //    packsVendidos = db.DETALLE_VENTA
                //        .Where(d => d.VENTA.ID_CAJA == apertura.ID_CAJA
                //                 && d.VENTA.FECHA >= apertura.FECHA_APERTURA
                //                 && d.VENTA.FECHA <= fechaAhora
                //                 && d.ID_PRODUCTO == null)
                //        .Sum(d => (decimal?)d.CANTIDAD) ?? 0,
                //    contieneCigarros = db.VENTA.Any(v => v.ID_CAJA == apertura.ID_CAJA
                //                 && v.FECHA >= apertura.FECHA_APERTURA
                //                 && v.FECHA <= fechaAhora
                //                 && v.CONTIENE_CIGARROS == true)
                //});
                return Ok(new
                {
                    message = "Caja cerrada correctamente",
                    idAperturaCierre = apertura.ID_APERTURA_CIERRE,
                    caja = apertura.CAJA.NOMBRE,
                    fechaApertura = apertura.FECHA_APERTURA,
                    fechaCierre = apertura.FECHA_CIERRE,
                    montoInicial = apertura.MONTO_INICIAL,

                    totalVentas = totalGeneral,
                    montoProductos = montoProductos,
                    montoCigarros = montoCigarros,
                    montoFinal = apertura.MONTO_INICIAL + totalGeneral,

                    usuario = db.USUARIO.Where(u => u.ID_USUARIO == apertura.ID_USUARIO).Select(u => u.NOMBRE).FirstOrDefault(),
                    totalTransacciones = db.VENTA
        .Where(v => v.ID_APERTURA_CIERRE == apertura.ID_APERTURA_CIERRE)
        .Count(),

                    
                    productosVendidos = productosVendidosTotal,
                    packsVendidos = packsVendidos,

                    contieneCigarros = montoCigarros > 0
                });
            }
        }
    }
}
