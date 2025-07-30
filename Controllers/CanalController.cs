using api_powergate.Aplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_powergate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanalesController : ControllerBase
    {
        private readonly CanalCargaService _canalService;
        public CanalesController(CanalCargaService canalService) => _canalService = canalService;

        [HttpPost("{canalId}/rele")]
        public async Task<IActionResult> Toggle(int canalId, [FromBody] ToggleRelayRequest dto)
        {
            var result = await _canalService.CambiarEstadoRele(canalId, dto.ReleActivo);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }

    public class ToggleRelayRequest
    {
        public bool ReleActivo { get; set; }
    }
}
