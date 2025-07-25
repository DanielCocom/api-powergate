using api_powergate.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api_powergate.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DispositivoController : ControllerBase
    {
        private readonly IDispositivoService _service;
        public DispositivoController(IDispositivoService service)
        {
            _service = service;
        }
        [HttpGet("{dispositivoId:int}/estado")]
        public async Task<IActionResult> ObtenerEstado(int dispositivoId)
        {
            var response = await _service.ObtenerEstado(dispositivoId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(response);
            }
        }

    }
}
