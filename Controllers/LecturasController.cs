using api_powergate.Aplication.Dtos;
using api_powergate.Aplication.Services;

using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LecturasController : ControllerBase
{
    private readonly LecturaService _lecturaService;

    public LecturasController(LecturaService lecturaService)
    {
        _lecturaService = lecturaService;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarLecturaDto dto)
    {
        var id = await _lecturaService.RegistrarLecturaAsync(dto);
        return Ok(new { id });
    }
}