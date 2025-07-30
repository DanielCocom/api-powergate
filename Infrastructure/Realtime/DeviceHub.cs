using api_powergate.Domain.Interfaces.ws;
using Microsoft.AspNetCore.SignalR;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace api_powergate.Infrastructure.Realtime
{

    public class DeviceHub : Hub
    {
        private readonly IDeviceConnectionTracker _tracker;
        public DeviceHub(IDeviceConnectionTracker tracker) => _tracker = tracker;

        public override async Task OnConnectedAsync()
        {
            var deviceId = Context.GetHttpContext().Request.Query["deviceId"];
            Console.WriteLine($"Conexión establecida: deviceId={deviceId}, connectionId={Context.ConnectionId}");

            if (int.TryParse(deviceId, out var id))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, DeviceGroup(id));
                await _tracker.MarkConnectedAsync(id, Context.ConnectionId);
            }
            else
            {
                throw new ArgumentException("deviceId no es válido.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Si guardas deviceId ↔ connectionId, puedes removerlo aquí
            await base.OnDisconnectedAsync(exception);
        }

        public static string DeviceGroup(int deviceId)
        {
            return $"device-{deviceId}";
        }

        // “From device” → el ESP32 puede reportar su estado/lectura
        public async Task DeviceAck(string commandId, bool ok)
        {
            // TODO: marcar comando como confirmado, generar Evento, etc.
            await Clients.Caller.SendAsync("AckReceived", commandId, ok);
        }

        public async Task Telemetry(object lecturaDto)
        {
            // TODO: persistir lectura y opcionalmente forward al frontend via Clients.All/Group
        }
    }
}
