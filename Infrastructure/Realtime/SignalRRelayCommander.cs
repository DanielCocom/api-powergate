using api_powergate.Domain.Interfaces.ws;
using Microsoft.AspNetCore.SignalR;

namespace api_powergate.Infrastructure.Realtime
{
    public class SignalRRelayCommander : IRelayCommander
    {
        private readonly IHubContext<DeviceHub> _hub;
        public SignalRRelayCommander(IHubContext<DeviceHub> hub) => _hub = hub;


      public  Task SendToggleAsync(int dispositivoId, int ?canalId, bool estado, string commandId)
        {
            var payload = new
            {
                type = "relay.toggle",
                commandId,
                canalId,
                state = estado
            };
            return _hub.Clients.Group(DeviceHub.DeviceGroup(dispositivoId))
                      .SendAsync("RelayCommand", payload);
        }

       
    }
}
