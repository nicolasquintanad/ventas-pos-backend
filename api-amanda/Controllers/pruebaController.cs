using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api_amanda.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("prueba")]
    public class pruebaController : ApiController
    {
    
        [HttpGet]
        [Route("uno")]

        public IHttpActionResult pruebauno()
        {
            return Ok("buena");
        }
    }
}
