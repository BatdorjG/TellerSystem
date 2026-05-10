using System.Net.Sockets;
using System.Text.Json;
namespace TellerDisplay;

public record DisplayConfig
{
    public string ServerIp { get; init; } = "127.0.0.1";
    public int SocketPort { get; init; } = 9000;
    public byte DisplayId { get; init; } = 1;
}

public partial class Form1 : Form
{
    private TcpClient? _client;
    private DisplayConfig _displayConfig = new();

    private const byte Key1 = 0x12;
    private const byte Key2 = 0x34;
    private const byte Key3 = 0x56;

    public Form1()
    {
        InitializeComponent();
        _displayConfig = LoadConfig();
        this.Load += Form1_Load;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        _ = ConnectToSocketServer();
    }

    private async Task ConnectToSocketServer()
    {
        while (true)
        {
            try
            {
                using var client = new TcpClient();

                await client.ConnectAsync(_displayConfig.ServerIp, _displayConfig.SocketPort);

                _client = client;

                NetworkStream stream = client.GetStream();

                var helloPacket = new SocketPacket
                {
                    Command = SocketCommand.HELLO,
                    DisplayId = _displayConfig.DisplayId,
                    CustomerNumber = 0,
                    TellerId = 0,
                    Key1 = Key1,
                    Key2 = Key2,
                    Key3 = Key3
                };

                await stream.WriteAsync(helloPacket.ToBytes());
                await stream.FlushAsync();

                while (true)
                {
                    byte[] data = await ReadExactlyAsync(stream, SocketPacket.Size);

                    if (!SocketPacket.IsChecksumValid(data))
                        continue;

                    SocketPacket packet = SocketPacket.FromBytes(data);

                    if (!packet.IsCommandValid())
                        continue;

                    if (packet.DisplayId != _displayConfig.DisplayId)
                        continue;

                    if (packet.Command == SocketCommand.CALL)
                    {
                        UpdateDisplay(packet.TellerId, packet.CustomerNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket disconnected: " + ex.Message);
                await Task.Delay(3000);
            }
        }
    }

    private static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int size)
    {
        byte[] buffer = new byte[size];
        int offset = 0;

        while (offset < size)
        {
            int read = await stream.ReadAsync(buffer, offset, size - offset);

            if (read == 0)
                throw new Exception("Disconnected from server");

            offset += read;
        }

        return buffer;
    }

    private void UpdateDisplay(byte tellerId, byte customerNumber)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateDisplay(tellerId, customerNumber));
            return;
        }

        labelCustomerNumber.Text = customerNumber.ToString();
    }

    private static DisplayConfig LoadConfig()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "display-config.json");

        if (!File.Exists(path))
        {
            var defaultConfig = new DisplayConfig();

            string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(path, json);

            return defaultConfig;
        }

        string fileJson = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DisplayConfig>(fileJson) ?? new DisplayConfig();
    }
}