using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spectacle.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spectacle.Controllers
{
    [Route("upload")]
    public class UploadController : Controller
    {
        private ComputerVisionService _computerVisionService;

        public UploadController(ComputerVisionService computerVisionService)
        {
            _computerVisionService = computerVisionService;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            using (Stream readStream = file.OpenReadStream())
            {
                return Ok(await _computerVisionService.FindTextOnImageAsync(readStream));
            }
            
        }
    }
}
