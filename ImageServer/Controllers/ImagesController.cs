using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ImageServer.Models;
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

        string[] legalExtensions = { ".jpg", ".png", ".gif", ".bmp" };


        public ImagesController(IHostEnvironment environment)
        {
            this.environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SaveFile saveFile)
        {
            Console.WriteLine("Received image upload request");

            if (saveFile.Files == null || (saveFile.ProductID < 0))
            {
                Console.WriteLine("Bad request");
                return BadRequest("Upload an image");
            }

            Console.WriteLine("Starting loop");
            
            foreach (var file in saveFile.Files)
            {
                Console.WriteLine("Loop begin");
                string fileExtension = file.FileType.ToLower();

                Console.WriteLine("Checking file extension");
                if (!legalExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"Wrong fileextension: {fileExtension}");
                    return BadRequest("Image must be .jpg, .png, .gif or .bmp");
                }

                Console.WriteLine("Checking directory");
                if (!Directory.Exists($"{environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}"))
                {
                    Console.WriteLine($"Creating new directory at: {environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}");
                    Directory.CreateDirectory($"{environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}");
                }
                else
                {
                    Console.WriteLine("Directory exists");
                }

                Console.WriteLine("Creating filename");
                string newFileName = $"{Guid.NewGuid()}{fileExtension}";
                Console.WriteLine("Creating path");
                string filePath = Path.Combine(environment.ContentRootPath, "wwwroot", $"images/{saveFile.ProductID}", newFileName);

                Console.WriteLine("Doing filestream stuff");
                using (var fileStream = System.IO.File.Create(filePath))
                {
                    Console.WriteLine("Writing filestream");
                    await fileStream.WriteAsync(file.Data);
                }
            }
            Console.WriteLine("Success");

            return Ok($"Image has been uploaded");
        }

        [HttpGet]
        public async Task<string> GetImage()
        {
            Console.WriteLine("returning");
            return "Yo dawg";
        }
    }
}
