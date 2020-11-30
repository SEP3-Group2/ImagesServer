using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ImageServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IHostEnvironment environment;

        public ImagesController(IHostEnvironment environment)
        {
            this.environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] IFormFile image, [FromForm] int id)
        {
            Console.WriteLine("Received image upload request");
            if(image == null || image.Length == 0 || (id < 0))
            {
                return BadRequest("Upload an image");
            }

            string fileName = image.FileName;
            string extension = Path.GetExtension(fileName);

            string[] legalExtensions = { ".jpg", ".png", ".gif", ".bmp" };


            if (!legalExtensions.Contains(extension))
            {
                return BadRequest("Image must be .jpg, .png, .gif or .bmp");
            }

            Console.WriteLine($"Checking path: {environment.ContentRootPath}\\wwwroot\\images\\{id}");
            if (!Directory.Exists($"{environment.ContentRootPath}\\wwwroot\\images\\{id}"))
            {
                Console.WriteLine($"Creating new directory at: {environment.ContentRootPath}\\wwwroot\\images\\{id}");
                Directory.CreateDirectory($"{environment.ContentRootPath}\\wwwroot\\images\\{id}");
            }

            string newFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(environment.ContentRootPath, "wwwroot", $"images/{id}", newFileName);

            using(var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await image.CopyToAsync(fileStream);
            }

            return Ok($"Image has been uploaded");
        }
    }
}
