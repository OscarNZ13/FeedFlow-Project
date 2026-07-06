using FF_Business;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FF_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SourceItemApiController : ControllerBase
    {
        private readonly ISourceItemBusiness _sourceItemBusiness;

        public SourceItemApiController(ISourceItemBusiness sourceItemBusiness)
        {
            _sourceItemBusiness = sourceItemBusiness;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var items = await _sourceItemBusiness
             .GetAllAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> SaveJson(int sourceId, [FromBody] JsonElement json)
        {
            var result = await _sourceItemBusiness
                .SaveJsonAsync(
                    json.ToString(),
                    sourceId
                );


            if (!result)
            {
                return BadRequest(
                    "El JSON no tiene un formato válido"
                );
            }

            return Ok(
                "JSON guardado correctamente"
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _sourceItemBusiness
                .GetByIdAsync(id);


            if (item == null)
                return NotFound();


            return Ok(item);
        }
    }
}
