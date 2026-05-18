using System.Net;
using System.Net.Sockets;
namespace UdpSockets;
public class UDPClient
{
    public static async Task<string?> SearchForServer()
    {
        using var udp = new UdpClient();

        udp.EnableBroadcast = true;

        var discoverPacket =
            DiscoveryPacket.CreateDiscoverPacket();

        byte[] data = discoverPacket.ToBytes();

        await udp.SendAsync(
            data,
            data.Length,
            new IPEndPoint(IPAddress.Broadcast, 8888)
        );

        var result = await udp.ReceiveAsync();

        var responsePacket =
            DiscoveryPacket.FromBytes(result.Buffer);

        if (responsePacket == null)
            return null;

        if (responsePacket.Command != DiscoveryCommand.INFO)
            return null;

        string ip = responsePacket
            .GetIpAddress()
            .ToString();

        ushort port = responsePacket.Port;

        return $"https://{ip}:{port}";
    }
}