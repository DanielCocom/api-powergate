using api_powergate.Aplication.Dtos;
using api_powergate.Aplication.Services;
using api_powergate.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LecturasController : ControllerBase
{
    private readonly ILecturaService _lecturaService;

    public LecturasController(ILecturaService lecturaService)
    {
        _lecturaService = lecturaService;
    }

    [HttpPost("EnviarLectura")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarLecturaDto dto)
    {
        var response = await _lecturaService.RegistrarLectura(dto);

        if (response.IsSuccess)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
    [HttpGet]
    public async Task<IActionResult> ObtenerLecturasPorDispositivoAsync(int dispositivoId)
    {
        var response = await _lecturaService.ObtenerLecturasPorDispositivoAsync(dispositivoId);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest(response);
        }
    }
}