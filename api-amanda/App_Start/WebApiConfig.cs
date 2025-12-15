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
            // Habilitar CORS para ambos orígenes
            var cors = new EnableCorsAttribute(
                "http://localhost:5173,http://192.168.1.50:8081,https://192.168.1.50:8082,http://192.168.1.60:8081",
                "*",
                "*"
            );

            config.EnableCors(cors);

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
