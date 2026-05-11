using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

public class SocketServer : BackgroundService
{
    private readonly ConcurrentDictionary<int, TcpClient> _displays = new();

    private const int Port = 9000;

    private const byte Key1 = 0x12;
    private const byte Key2 = 0x34;
    private const byte Key3 = 0x56;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();

        Console.WriteLine($"Display socket server started on port {Port}");

        while (!stoppingToken.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
            _ = HandleClientAsync(client, stoppingToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        byte displayId = 0;

        try
        {
            NetworkStream stream = client.GetStream();

            byte[] data = await ReadExactlyAsync(stream, SocketPacket.Size, token);

            if (!SocketPacket.IsChecksumValid(data))
            {
                Console.WriteLine("Rejected display: bad checksum");
                client.Close();
                return;
            }

            SocketPacket packet = SocketPacket.FromBytes(data);

            if (packet.Command != SocketCommand.HELLO)
            {
                Console.WriteLine("Rejected display: expected HELLO");
                client.Close();
                return;
            }

            if (packet.Key1 != Key1 || packet.Key2 != Key2 || packet.Key3 != Key3)
            {
                Console.WriteLine("Rejected display: wrong key");
                client.Close();
                return;
            }

            displayId = packet.DisplayId;

            _displays[displayId] = client;

            Console.WriteLine($"Display {displayId} connected");

            var okPacket = new SocketPacket
            {
                Command = SocketCommand.OK,
                DisplayId = displayId,
                CustomerNumber = 0,
                TellerId = 0,
                Key1 = Key1,
                Key2 = Key2,
                Key3 = Key3
            };

            await stream.WriteAsync(okPacket.ToBytes(), token);
            await stream.FlushAsync(token);

            while (!token.IsCancellationRequested && client.Connected)
            {
                await Task.Delay(1000, token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Display error: {ex.Message}");
        }
        finally
        {
            if (displayId != 0)
            {
                _displays.TryRemove(displayId, out _);
                Console.WriteLine($"Display {displayId} disconnected");
            }

            client.Close();
        }
    }

    public async Task SendCallAsync(int displayId, int tellerId, int customerNumber)
    {
        if (!_displays.TryGetValue(displayId, out TcpClient? client))
        {
            Console.WriteLine($"Display {displayId} is not connected");
            return;
        }

        try
        {
            NetworkStream stream = client.GetStream();

            var packet = new SocketPacket
            {
                Command = SocketCommand.CALL,
                DisplayId = (byte)displayId,
                CustomerNumber = (byte)customerNumber,
                TellerId = (byte)tellerId,
                Key1 = 0,
                Key2 = 0,
                Key3 = 0
            };

            await stream.WriteAsync(packet.ToBytes());
            await stream.FlushAsync();

            Console.WriteLine($"Sent CALL to Display {displayId}, Customer {customerNumber}, Teller {tellerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send failed: {ex.Message}");

            _displays.TryRemove(displayId, out _);
            client.Close();
        }
    }

    private static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int size, CancellationToken token)
    {
        byte[] buffer = new byte[size];
        int offset = 0;

        while (offset < size)
        {
            int read = await stream.ReadAsync(buffer, offset, size - offset, token);

            if (read == 0)
                throw new Exception("Client disconnected");

            offset += read;
        }

        return buffer;
    }
}