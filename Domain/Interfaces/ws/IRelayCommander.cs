using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Domain.Models;
using System.ComponentModel.Design;

namespace api_powergate.Domain.Interfaces.ws
{
    public interface IRelayCommander
    {
        Task SendToggleAsync( int dispositivoId, int? canalId, bool estado, string commandId);
    }
}
