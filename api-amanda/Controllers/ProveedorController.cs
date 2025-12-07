using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        // ✅ 1) Obtener sugerencias de pedido para todos los proveedores que pasan hoy
        [HttpGet]
        [Route("sugerencias-hoy")]
        public IHttpActionResult GetSugerenciasHoy()
        {
            using (var db = new AMANDAEntities())
            {
                var hoy = DateTime.Today;
                var dia = hoy.DayOfWeek; // Sunday=0, Monday=1, ...

                var proveedoresHoy = db.PROVEEDOR.Where(p =>
                    (dia == DayOfWeek.Monday    && p.VISITA_LUNES == true) ||
                    (dia == DayOfWeek.Tuesday   && p.VISITA_MARTES == true) ||
                    (dia == DayOfWeek.Wednesday && p.VISITA_MIERCOLES == true) ||
                    (dia == DayOfWeek.Thursday  && p.VISITA_JUEVES == true) ||
                    (dia == DayOfWeek.Friday    && p.VISITA_VIERNES == true) ||
                    (dia == DayOfWeek.Saturday  && p.VISITA_SABADO == true) ||
                    (dia == DayOfWeek.Sunday    && p.VISITA_DOMINGO == true)
                ).ToList();

                var resultado = new List<object>();

                foreach (var prov in proveedoresHoy)
                {
                    var sugerencias = db.Database.SqlQuery<SugerenciaProductoDto>(
                        "EXEC sp_SugerenciaPedidoProveedor @IdProveedor, @FechaCorte, @DiasAnalisis, @DiasHorizonte",
                        new SqlParameter("@IdProveedor", prov.ID_PROVEEDOR),
                        new SqlParameter("@FechaCorte", hoy),
                        new SqlParameter("@DiasAnalisis", 28),
                        new SqlParameter("@DiasHorizonte", 7)
                    ).ToList();

                    // Opcional: filtrar sólo productos con sugerencia > 0
                    var conPedido = sugerencias.Where(s => s.CantidadSugerida > 0).ToList();
                    if (!conPedido.Any()) continue;

                    resultado.Add(new
                    {
                        idProveedor = prov.ID_PROVEEDOR,
                        proveedor = prov.NOMBRE,
                        sugerencias = conPedido
                    });
                }

                return Ok(resultado);
            }
        }
        // ✅ 2) Enviar por correo las sugerencias de hoy
        [HttpPost]
        [Route("sugerencias-hoy/email")]
        public IHttpActionResult EnviarSugerenciasHoyEmail()
        {
            using (var db = new AMANDAEntities())
            {
                // Reusar la lógica del GET
                var hoy = DateTime.Today;
                var dia = hoy.DayOfWeek;

                var proveedoresHoy = db.PROVEEDOR.Where(p =>
                    (dia == DayOfWeek.Monday && p.VISITA_LUNES == true) ||
                    (dia == DayOfWeek.Tuesday && p.VISITA_MARTES == true) ||
                    (dia == DayOfWeek.Wednesday && p.VISITA_MIERCOLES == true) ||
                    (dia == DayOfWeek.Thursday && p.VISITA_JUEVES == true) ||
                    (dia == DayOfWeek.Friday && p.VISITA_VIERNES == true) ||
                    (dia == DayOfWeek.Saturday && p.VISITA_SABADO == true) ||
                    (dia == DayOfWeek.Sunday && p.VISITA_DOMINGO == true)
                ).ToList();

                var bloquesHtml = new List<string>();

                foreach (var prov in proveedoresHoy)
                {
                    var sugerencias = db.Database.SqlQuery<SugerenciaProductoDto>(
                        "EXEC sp_SugerenciaPedidoProveedor @IdProveedor, @FechaCorte, @DiasAnalisis, @DiasHorizonte",
                        new SqlParameter("@IdProveedor", prov.ID_PROVEEDOR),
                        new SqlParameter("@FechaCorte", hoy),
                        new SqlParameter("@DiasAnalisis", 28),
                        new SqlParameter("@DiasHorizonte", 7)
                    ).ToList();

                    var conPedido = sugerencias.Where(s => s.CantidadSugerida > 0).ToList();
                    if (!conPedido.Any()) continue;

                    // Armar una mini tabla HTML por proveedor
                    var rows = string.Join("", conPedido.Select(s => $@"
                        <tr>
                            <td>{s.SKU}</td>
                            <td>{s.NOMBRE}</td>
                            <td style='text-align:right'>{s.STOCK}</td>
                            <td style='text-align:right'>{s.VendidoPeriodo}</td>
                            <td style='text-align:right'>{s.CantidadSugerida}</td>
                        </tr>
                    "));

                    var bloque = $@"
                        <h3>Proveedor: {prov.NOMBRE}</h3>
                        <table border='1' cellpadding='4' cellspacing='0' style='border-collapse:collapse;font-size:12px;'>
                            <thead>
                                <tr>
                                    <th>SKU</th>
                                    <th>Producto</th>
                                    <th>Stock actual</th>
                                    <th>Vendido periodo</th>
                                    <th>Sugerido comprar</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                        <br/>";
                    bloquesHtml.Add(bloque);
                }

                if (!bloquesHtml.Any())
                {
                    return Ok("No hay sugerencias de compra para hoy.");
                }

                var cuerpo = $@"
                    <h2>Pedidos sugeridos para hoy ({hoy:dd-MM-yyyy})</h2>
                    {string.Join("<hr/>", bloquesHtml)}
                ";

                // ⚠️ CONFIGURA ESTO CON TU SMTP REAL
                var mensaje = new MailMessage();
                mensaje.From = new MailAddress("tucorreo@tudominio.cl", "Sistema Amanda");
                mensaje.To.Add("destino@tudominio.cl"); // puedes parametrizar esto
                mensaje.Subject = $"Pedidos sugeridos - {hoy:dd-MM-yyyy}";
                mensaje.Body = cuerpo;
                mensaje.IsBodyHtml = true;

                var smtp = new SmtpClient("smtp.tudominio.cl", 587)
                {
                    Credentials = new System.Net.NetworkCredential("usuario_smtp", "password_smtp"),
                    EnableSsl = true
                };

                try
                {
                    smtp.Send(mensaje);
                    return Ok("Correo enviado con éxito.");
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, "Error al enviar correo: " + ex.Message);
                }
            }
        }
    }
}
