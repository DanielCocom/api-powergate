using api_powergate.Aplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_powergate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanalController : ControllerBase
    {
        private readonly CanalCargaService _canalService;
        public CanalController(CanalCargaService canalService) => _canalService = canalService;

        [HttpPost("rele")]
        public async Task<IActionResult> Toggle([FromBody] ToggleRelayRequest dto)
        {
            var result = await _canalService.CambiarEstadoRele(dto.CanalId, dto.ReleActivo);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("{dispositivoId:int}/estado")]
        public async Task<IActionResult> GetEstadoRele(int dispositivoId)
        {
            var result = await _canalService.GetEstadoRele(dispositivoId);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }
    }

    public class ToggleRelayRequest
    {
        public int CanalId { get; set; }
        public bool ReleActivo { get; set; }
    }
}
