using System.Web.Http;
using System.Web.Http.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace api_amanda
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilitar CORS para tu frontend
            var cors = new EnableCorsAttribute(
                "http://localhost:5173", // tu frontend
                "*",                     // headers permitidos
                "*"                      // métodos permitidos
            );
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            // Rutas
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
