using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ImageServer.Models;
using Microsoft.AspNetCore.Components.Forms;
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

        string[] legalExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };


        public ImagesController(IHostEnvironment environment)
        {
            this.environment = environment;
        }

        /// <summary>
        /// Saves images in directory corresponding to product ID
        /// </summary>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        /// <response code="200">Images are saved</response>
        /// <response code="400">Uploaded item contains non-images</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] SaveFile saveFile)
        {
            Console.WriteLine("Received image upload request");

            if (saveFile.Files == null || (saveFile.ProductID < 0))
            {
                return BadRequest("Upload an image");
            }

            foreach (var file in saveFile.Files)
            {
                string fileExtension = file.FileType.ToLower();

                if (!legalExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"Wrong fileextension: {fileExtension}");
                    return BadRequest("Image must be .jpg, .jpeg .png, .gif or .bmp");
                }

                if (!Directory.Exists($"{environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}"))
                {
                    Console.WriteLine($"Creating new directory at: {environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}");
                    Directory.CreateDirectory($"{environment.ContentRootPath}\\wwwroot\\images\\{saveFile.ProductID}");
                }

                string newFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(environment.ContentRootPath, "wwwroot", $"images/{saveFile.ProductID}", newFileName);

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(file.Data);
                }
            }
            Console.WriteLine($"Success! {saveFile.Files.Count} images uploaded");

            return Ok($"Image has been uploaded");
        }

        [HttpGet]
        public async Task<ActionResult<SaveFile>> GetImages([FromQuery] string quantity, [FromQuery] int productID)
        {
            Console.WriteLine("Getting images...");
            string filePath = $"{environment.ContentRootPath}\\wwwroot\\images\\{productID}";

            if (!Directory.Exists(filePath))
            {
                return BadRequest($"Directory does not exist for product {productID}");
            }

            List<FileData> fileData = new List<FileData>();
            string[] fileNames = Directory.GetFiles(filePath);

            if(fileNames.Length == 0)
            {
                return BadRequest($"No images found for product {productID}");
            }

            List<string> files = new List<string>();
            foreach (var file in fileNames)
            {
                files.Add(Path.GetFileName(file));
            }

            fileNames = files.ToArray();

            if (quantity.Equals("all"))
            {
                foreach (var file in fileNames)
                {
                    string fullPath = filePath + $"\\{file}";

                    var imageFileStream = System.IO.File.OpenRead(fullPath);
                    var buffer = new byte[imageFileStream.Length];
                    await imageFileStream.ReadAsync(buffer);

                    fileData.Add(new FileData
                    {
                        Data = buffer,
                        FileType = Path.GetExtension(fullPath),
                        Size = imageFileStream.Length
                    });
                }
            }
            else if (quantity.Equals("first"))
            {
                string file = fileNames[0];
                string fullPath = filePath + $"\\{file}";
                Console.WriteLine($"Fullpath: {fullPath}");

                var imageFileStream = System.IO.File.OpenRead(fullPath);
                var buffer = new byte[imageFileStream.Length];
                await imageFileStream.ReadAsync(buffer);

                fileData.Add(new FileData
                {
                    Data = buffer,
                    FileType = Path.GetExtension(fullPath),
                    Size = imageFileStream.Length
                });
            }

            SaveFile returnFile = new SaveFile
            {
                Files = fileData,
                ProductID = productID
            };

            return Ok(returnFile);
        }
    }
}
