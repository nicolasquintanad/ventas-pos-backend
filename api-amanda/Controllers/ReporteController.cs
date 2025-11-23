using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using api_amanda.Models;
using api_amanda.Models.DTO;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace api_amanda.Controllers
{
    [RoutePrefix("reporte")]
    public class ReporteController : ApiController
    {
        [HttpGet]
        [Route("ventas")]
        public IHttpActionResult GetReporteVentas(
    DateTime? inicio = null,
    DateTime? fin = null,
    int? idUsuario = null,
    int? idCaja = null,
    int? idProveedor = null,
    int? idProducto = null)
        {
            using (var db = new AMANDAEntities())
            {
                // ============================================================
                // 0) Consulta base con JOIN dinámico sobre el PACK
                // ============================================================
                var baseQuery = db.DETALLE_VENTA
                    .Select(d => new
                    {
                        Det = d, // DETALLE_VENTA original
                IdPack = db.PACK
                            .Where(p => p.SKU_PACK == d.SKU)
                            .Select(p => p.ID_PACK)
                            .FirstOrDefault() // si no tiene PACK = null
            });

                // ============================================================
                // 1) FILTROS: fecha, usuario, caja
                // ============================================================
                if (inicio.HasValue)
                    baseQuery = baseQuery.Where(x => x.Det.FECHA_CREACION >= inicio);

                if (fin.HasValue)
                    baseQuery = baseQuery.Where(x => x.Det.FECHA_CREACION <= fin);

                if (idUsuario.HasValue)
                    baseQuery = baseQuery.Where(x => x.Det.USUARIO_VENTA == idUsuario);

                if (idCaja.HasValue)
                    baseQuery = baseQuery.Where(x => x.Det.VENTA.ID_CAJA == idCaja);

                // ============================================================
                // 2) FILTRO POR PROVEEDOR (Regla: si hay producto, ignorar)
                // ============================================================
                if (!idProducto.HasValue && idProveedor.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        // Venta normal de producto
                        (x.Det.ID_PRODUCTO != null &&
                         x.Det.PRODUCTO.ENTRADA_PRODUCTO.Any(e => e.ID_PROVEEDOR == idProveedor))

                        ||

                        // Producto de un PACK proviene de ese proveedor
                        (x.Det.ID_PRODUCTO == null &&
                          db.PACK_DETALLE.Any(pd =>
                              pd.ID_PACK == x.IdPack &&
                              pd.PRODUCTO.ENTRADA_PRODUCTO.Any(e => e.ID_PROVEEDOR == idProveedor)
                          )
                        )
                    );
                }

                // ============================================================
                // 3) Filtro por producto específico (incluyendo ventas desde PACK)
                // ============================================================
                if (idProducto.HasValue)
                {
                    baseQuery = baseQuery.Where(x =>
                        // producto vendido directo
                        x.Det.ID_PRODUCTO == idProducto.Value

                        ||

                        // producto desglosado desde un pack
                        (
                            x.Det.ID_PRODUCTO == null &&
                            db.PACK_DETALLE.Any(pd =>
                                pd.ID_PACK == x.IdPack &&
                                pd.ID_PRODUCTO == idProducto.Value
                            )
                        )
                    );
                }

                // ============================================================
                // 4) DETALLE DE PRODUCTOS (NO PACK)
                // ============================================================
                var detalleProductos =
                    baseQuery
                    .Where(x => x.Det.ID_PRODUCTO != null)
                    .Select(x => new ReporteVentaDTO
                    {
                        FECHA = (DateTime)x.Det.VENTA.FECHA,
                        Usuario = x.Det.VENTA.USUARIO.NOMBRE,
                        Caja = x.Det.VENTA.CAJA.NOMBRE,
                        Producto = x.Det.PRODUCTO.NOMBRE,
                        Proveedor = x.Det.PRODUCTO.ENTRADA_PRODUCTO
                            .Select(e => e.PROVEEDOR.NOMBRE)
                            .FirstOrDefault(),
                        Cantidad = (decimal)x.Det.CANTIDAD,
                        PrecioUnitario = (decimal)x.Det.PRECIO_UNITARIO,
                        SubTotal = (decimal)x.Det.SUBTOTAL
                    });

                // ============================================================
                // 5) DETALLE DESGLOSADO DE PACKS
                // ============================================================
                var detallePackProductos =
                    baseQuery
                    .Where(x => x.Det.ID_PRODUCTO == null)
                    .SelectMany(x =>
                        db.PACK_DETALLE
                        .Where(det =>
                            det.ID_PACK == x.IdPack &&
                            (!idProducto.HasValue || det.ID_PRODUCTO == idProducto.Value) &&
                            (!idProveedor.HasValue ||
                                det.PRODUCTO.ENTRADA_PRODUCTO.Any(p => p.ID_PROVEEDOR == idProveedor.Value))
                        )
                        .Select(det => new ReporteVentaDTO
                        {
                            FECHA = (DateTime)x.Det.VENTA.FECHA,
                            Usuario = x.Det.VENTA.USUARIO.NOMBRE,
                            Caja = x.Det.VENTA.CAJA.NOMBRE,
                            Producto = det.PRODUCTO.NOMBRE,
                            Proveedor = det.PRODUCTO.ENTRADA_PRODUCTO
                                .Select(e => e.PROVEEDOR.NOMBRE)
                                .FirstOrDefault(),
                            Cantidad = (decimal)det.CANTIDAD_PRODUCTO * (decimal)x.Det.CANTIDAD,
                            PrecioUnitario = (decimal)det.PRODUCTO.PRECIO,
                            SubTotal = (decimal)det.PRODUCTO.PRECIO *
                                       ((decimal)det.CANTIDAD_PRODUCTO * (decimal)x.Det.CANTIDAD)
                        })
                    );

                // ============================================================
                // 6) UNION FINAL
                // ============================================================
                var resultado = detalleProductos
                    .Union(detallePackProductos)
                    .OrderBy(r => r.FECHA)
                    .ToList();

                // ============================================================
                // 7) RESPUESTA
                // ============================================================
                return Ok(new
                {
                    total = resultado.Sum(x => x.SubTotal),
                    cantidadItems = resultado.Sum(x => x.Cantidad),
                    detalle = resultado
                });
            }
        }
         // =========================================================================
        // 2) EXPORTAR A EXCEL
        // =========================================================================
        [HttpGet]
        [Route("ventas/excel")]
        public HttpResponseMessage ExportarVentasExcel(
            DateTime? inicio = null,
            DateTime? fin = null,
            int? idUsuario = null,
            int? idCaja = null,
            int? idProveedor = null,
            int? idProducto = null)
        {
            using (var db = new AMANDAEntities())
            {
                var detalle = ObtenerDetalleVentas(db, inicio, fin, idUsuario, idCaja, idProveedor, idProducto);

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("ReporteVentas");

                    // Encabezados
                    ws.Cell(1, 1).Value = "Fecha";
                    ws.Cell(1, 2).Value = "Usuario";
                    ws.Cell(1, 3).Value = "Caja";
                    ws.Cell(1, 4).Value = "Producto";
                    ws.Cell(1, 5).Value = "Proveedor";
                    ws.Cell(1, 6).Value = "Cantidad";
                    ws.Cell(1, 7).Value = "Precio Unitario";
                    ws.Cell(1, 8).Value = "Subtotal";

                    int row = 2;
                    foreach (var item in detalle)
                    {
                        ws.Cell(row, 1).Value = item.FECHA;
                        ws.Cell(row, 1).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";

                        ws.Cell(row, 2).Value = item.Usuario;
                        ws.Cell(row, 3).Value = item.Caja;
                        ws.Cell(row, 4).Value = item.Producto;
                        ws.Cell(row, 5).Value = item.Proveedor;
                        ws.Cell(row, 6).Value = item.Cantidad;
                        ws.Cell(row, 7).Value = item.PrecioUnitario;
                        ws.Cell(row, 8).Value = item.SubTotal;
                        row++;
                    }

                    // Auto ajuste columnas
                    ws.Columns().AdjustToContents();

                    // Total al final
                    ws.Cell(row + 1, 7).Value = "TOTAL:";
                    ws.Cell(row + 1, 8).Value = detalle.Sum(x => x.SubTotal);

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        var result = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(stream.ToArray())
                        };

                        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "ReporteVentas.xlsx"
                        };
                        result.Content.Headers.ContentType =
                            new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                        return result;
                    }
                }
            }
        }

        // =========================================================================
        // 3) EXPORTAR A PDF
        // =========================================================================
        [HttpGet]
        [Route("ventas/pdf")]
        public HttpResponseMessage ExportarVentasPdf(
            DateTime? inicio = null,
            DateTime? fin = null,
            int? idUsuario = null,
            int? idCaja = null,
            int? idProveedor = null,
            int? idProducto = null)
        {
            using (var db = new AMANDAEntities())
            {
                var detalle = ObtenerDetalleVentas(db, inicio, fin, idUsuario, idCaja, idProveedor, idProducto);

                using (var stream = new MemoryStream())
                {
                    var doc = new Document(PageSize.A4, 40, 40, 40, 40);
                    var writer = PdfWriter.GetInstance(doc, stream);
                    doc.Open();

                    // Título
                    var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var fontTexto = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    doc.Add(new Paragraph("Reporte de Ventas", fontTitulo));
                    doc.Add(new Paragraph("Fecha de generación: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm"), fontTexto));
                    doc.Add(new Paragraph(" ", fontTexto));

                    // Filtros usados (simple)
                    if (inicio.HasValue || fin.HasValue)
                    {
                        doc.Add(new Paragraph(
                            $"Rango: {inicio?.ToString("dd-MM-yyyy HH:mm") ?? "-"} a {fin?.ToString("dd-MM-yyyy HH:mm") ?? "-"}",
                            fontTexto));
                        doc.Add(new Paragraph(" ", fontTexto));
                    }

                    // Tabla
                    PdfPTable table = new PdfPTable(8);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 15, 15, 10, 20, 15, 10, 10, 15 });

                    // Encabezados
                    void AddHeader(string text)
                    {
                        var cell = new PdfPCell(new Phrase(text, fontTexto));
                        cell.BackgroundColor = new BaseColor(230, 230, 230);
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }

                    AddHeader("Fecha");
                    AddHeader("Usuario");
                    AddHeader("Caja");
                    AddHeader("Producto");
                    AddHeader("Proveedor");
                    AddHeader("Cant.");
                    AddHeader("P. Unit");
                    AddHeader("Subtotal");

                    // Filas
                    foreach (var item in detalle)
                    {
                        table.AddCell(new Phrase(item.FECHA.ToString("dd-MM-yyyy HH:mm"), fontTexto));
                        table.AddCell(new Phrase(item.Usuario ?? "", fontTexto));
                        table.AddCell(new Phrase(item.Caja ?? "", fontTexto));
                        table.AddCell(new Phrase(item.Producto ?? "", fontTexto));
                        table.AddCell(new Phrase(item.Proveedor ?? "", fontTexto));
                        table.AddCell(new Phrase(item.Cantidad.ToString("N3"), fontTexto));
                        table.AddCell(new Phrase(item.PrecioUnitario.ToString("N0"), fontTexto));
                        table.AddCell(new Phrase(item.SubTotal.ToString("N0"), fontTexto));
                    }

                    doc.Add(table);

                    doc.Add(new Paragraph(" ", fontTexto));
                    doc.Add(new Paragraph("Total ventas: " + detalle.Sum(x => x.SubTotal).ToString("N0"), fontTexto));

                    doc.Close();
                    writer.Close();

                    var result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(stream.ToArray())
                    };

                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "ReporteVentas.pdf"
                    };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    return result;
                }
            }
        }

        // =========================================================================
        // MÉTODO PRIVADO: usa la misma lógica que ya tienes para armar el reporte
        // =========================================================================
        private List<ReporteVentaDTO> ObtenerDetalleVentas(
            AMANDAEntities db,
            DateTime? inicio,
            DateTime? fin,
            int? idUsuario,
            int? idCaja,
            int? idProveedor,
            int? idProducto)
        {
            // 0) Base con IdPack calculado por SKU
            var baseQuery = db.DETALLE_VENTA
                .Select(d => new
                {
                    Det = d,
                    IdPack = db.PACK
                        .Where(p => p.SKU_PACK == d.SKU)
                        .Select(p => p.ID_PACK)
                        .FirstOrDefault()
                });

            // Filtros base
            if (inicio.HasValue)
                baseQuery = baseQuery.Where(x => x.Det.FECHA_CREACION >= inicio);
            if (fin.HasValue)
                baseQuery = baseQuery.Where(x => x.Det.FECHA_CREACION <= fin);
            if (idUsuario.HasValue)
                baseQuery = baseQuery.Where(x => x.Det.USUARIO_VENTA == idUsuario);
            if (idCaja.HasValue)
                baseQuery = baseQuery.Where(x => x.Det.VENTA.ID_CAJA == idCaja);

            // Filtro proveedor (si NO hay producto)
            if (!idProducto.HasValue && idProveedor.HasValue)
            {
                baseQuery = baseQuery.Where(x =>
                    // Venta directa
                    (x.Det.ID_PRODUCTO != null &&
                     x.Det.PRODUCTO.ENTRADA_PRODUCTO.Any(e => e.ID_PROVEEDOR == idProveedor))

                    ||

                    // Venta pack → algún producto interno del pack tiene ese proveedor
                    (x.Det.ID_PRODUCTO == null &&
                        db.PACK_DETALLE.Any(pd =>
                            pd.ID_PACK == x.IdPack &&
                            pd.PRODUCTO.ENTRADA_PRODUCTO.Any(e => e.ID_PROVEEDOR == idProveedor)
                        )
                    )
                );
            }

            // Filtro producto (directo o dentro de pack)
            if (idProducto.HasValue)
            {
                baseQuery = baseQuery.Where(x =>
                    x.Det.ID_PRODUCTO == idProducto.Value
                    ||
                    (
                        x.Det.ID_PRODUCTO == null &&
                        db.PACK_DETALLE.Any(pd =>
                            pd.ID_PACK == x.IdPack &&
                            pd.ID_PRODUCTO == idProducto.Value
                        )
                    )
                );
            }

            // Detalle normal
            var detalleProductos =
                baseQuery
                .Where(x => x.Det.ID_PRODUCTO != null)
                .Select(x => new ReporteVentaDTO
                {
                    FECHA = (DateTime)x.Det.VENTA.FECHA,
                    Usuario = x.Det.VENTA.USUARIO.NOMBRE,
                    Caja = x.Det.VENTA.CAJA.NOMBRE,
                    Producto = x.Det.PRODUCTO.NOMBRE,
                    Proveedor = x.Det.PRODUCTO.ENTRADA_PRODUCTO
                        .Select(e => e.PROVEEDOR.NOMBRE)
                        .FirstOrDefault(),
                    Cantidad = (decimal)x.Det.CANTIDAD,
                    PrecioUnitario = (decimal)x.Det.PRECIO_UNITARIO,
                    SubTotal = (decimal)x.Det.SUBTOTAL
                });

            // Detalle desde packs
            var detallePackProductos =
                baseQuery
                .Where(x => x.Det.ID_PRODUCTO == null)
                .SelectMany(x =>
                    db.PACK_DETALLE
                    .Where(det =>
                        det.ID_PACK == x.IdPack &&
                        (!idProducto.HasValue || det.ID_PRODUCTO == idProducto.Value) &&
                        (!idProveedor.HasValue ||
                            det.PRODUCTO.ENTRADA_PRODUCTO.Any(p => p.ID_PROVEEDOR == idProveedor.Value)
                        )
                    )
                    .Select(det => new ReporteVentaDTO
                    {
                        FECHA = (DateTime)x.Det.VENTA.FECHA,
                        Usuario = x.Det.VENTA.USUARIO.NOMBRE,
                        Caja = x.Det.VENTA.CAJA.NOMBRE,
                        Producto = det.PRODUCTO.NOMBRE,
                        Proveedor = det.PRODUCTO.ENTRADA_PRODUCTO
                            .Select(e => e.PROVEEDOR.NOMBRE)
                            .FirstOrDefault(),
                        Cantidad = (decimal)det.CANTIDAD_PRODUCTO * (decimal)x.Det.CANTIDAD,
                        PrecioUnitario = (decimal)det.PRODUCTO.PRECIO,
                        SubTotal = (decimal)det.PRODUCTO.PRECIO *
                                   ((decimal)det.CANTIDAD_PRODUCTO * (decimal)x.Det.CANTIDAD)
                    })
                );

            var resultado = detalleProductos
                .Union(detallePackProductos)
                .OrderBy(r => r.FECHA)
                .ToList();

            return resultado;
        }
        [HttpGet]
        [Route("productos")]
        public IHttpActionResult GetReporteProductos(
    DateTime? inicio = null,
    DateTime? fin = null,
    int? idProveedor = null,
    int? idProducto = null)
        {
            using (var db = new AMANDAEntities())
            {
                var data = ObtenerDetalleVentas(db, inicio, fin, null, null, idProveedor, idProducto);

                var agrupado = data
                    .GroupBy(x => new { x.Producto, x.Proveedor })
                    .Select(g => new
                    {
                        Producto = g.Key.Producto,
                        Proveedor = g.Key.Proveedor,
                        CantidadTotal = g.Sum(x => x.Cantidad),
                        SubTotalTotal = g.Sum(x => x.SubTotal),
                        PrecioPromedio = g.Sum(x => x.SubTotal) / g.Sum(x => x.Cantidad)
                    })
                    .OrderByDescending(x => x.CantidadTotal)
                    .ToList();

                return Ok(new
                {
                    totalProductos = agrupado.Count,
                    totalCantidad = agrupado.Sum(x => x.CantidadTotal),
                    totalVentas = agrupado.Sum(x => x.SubTotalTotal),
                    detalle = agrupado
                });
            }
        }
        [HttpGet]
        [Route("productos-filtro")]
        public IHttpActionResult GetFiltroProductos()
        {
            using (var db = new AMANDAEntities())
            {
                var productos = db.PRODUCTO
                    .Select(p => new {
                        id = p.ID_PRODUCTO,
                        name = p.NOMBRE
                    }).ToList();

                var proveedores = db.PROVEEDOR
                    .Select(p => new {
                        id = p.ID_PROVEEDOR,
                        name = p.NOMBRE
                    }).ToList();

                return Ok(new
                {
                    productos,
                    proveedores
                });
            }
        }
    }
}