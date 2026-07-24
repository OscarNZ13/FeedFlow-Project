using FF.Architecture.Dtos;
using FF_Business;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FF_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImportExportApiController : ControllerBase
    {

        private readonly IImportExportBusiness _business;

        public ImportExportApiController(
            IImportExportBusiness business)
        {
            _business = business;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(
            [FromBody] SourcePackageDto package)
        {
            var result =
                await _business.ImportAsync(package);

            if (!result)
            {
                return BadRequest(
                    "No se pudo importar el JSON"
                );
            }

            return Ok(
                "JSON importado correctamente"
            );
        }

        [HttpPost("import/item")]
        public async Task<IActionResult> ImportItem(
        [FromBody] ExportSourceItemDto item)
        {
            try
            {
                var result = await _business.ImportItemAsync(item);

                if (!result)
                {
                    return BadRequest("No se pudo importar la noticia.");
                }

                return Ok("Noticia importada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("export/{sourceId}")]
        public async Task<IActionResult> Export(int sourceId)
        {
            var result = await _business
                .ExportAsync(sourceId);

            if (result == null)
            {
                return NotFound();
            }

            var json = JsonSerializer.Serialize(
                result,


                new JsonSerializerOptions()
                {
                    WriteIndented = true
                }
            );

            var bytes = System.Text.Encoding.UTF8
                .GetBytes(json);


            return File(
                bytes,
                "application/json",
                $"source-{sourceId}.json"
            );
        }

        [HttpGet("export/item/{sourceItemId}")]
        public async Task<IActionResult> ExportItem(int sourceItemId)
        {
            var result = await _business
                .ExportItemAsync(sourceItemId);

            if (result == null)
            {
                return NotFound();
            }

            var json = JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                }
            );

            var bytes = System.Text.Encoding.UTF8
                .GetBytes(json);

            string fileName = result.Title;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '-');
            }

            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 50);
            }

            return File(
                bytes,
                "application/json",
                $"{fileName}.json"
            );
        }
    }
}