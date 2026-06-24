using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace CloudClinic.Shared.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet] // changed, this was not called properly
        [Route("OtherError")]
        public ActionResult OtherError()
        {
            object response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel { Type = ErrorTypes.ServerError, Message = "Error occurred on the server." },
                null);

            return Ok(response);
        }

        //[HttpGet("StatusCodeError/{code}", Name = "StatusCodeError")] // changed, this was not called properly
        [HttpGet]
        [Route("StatusCodeError/{code}", Name = "StatusCodeError")]
        public IActionResult StatusCodeError(int code)
        {
            string error = "";

            switch (code)
            {
                case 401:
                    error = "You are not authorized to access this part of the system.";
                    break;
                case 403:
                    error = "You are not allowed to access this part of the system";
                    break;
                case 405:
                    error = "Method not allowed";
                    break;
                default:
                    error = "Unknow error caused by the server";
                    break;
            }

            object response = HelperMethods.CreateResponse(
                code.ToString(),
                new ErrorModel { Type = ErrorTypes.ServerError, Message = error },
                null);

            return Ok(response);
        }
    }
}