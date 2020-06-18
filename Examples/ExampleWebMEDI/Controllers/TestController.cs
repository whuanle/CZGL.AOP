using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebMEDI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private ITest _test;
        public TestController(ITest test)
        {
            _test = test;
        }

        [HttpGet("/GetStr")]
        public string GetStr()
        {
            _test.MyMethod("","");
            return "666";
        }
    }
}
