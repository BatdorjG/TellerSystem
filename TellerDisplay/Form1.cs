using System.Net.Sockets;

namespace TellerDisplay;

public partial class Form1 : Form
{
    private TcpClient? _client;
    private readonly byte _displayId = 1;

    private const byte Key1 = 0x12;
    private const byte Key2 = 0x34;
    private const byte Key3 = 0x56;

    public Form1()
    {
        InitializeComponent();
        this.Load += Form1_Load;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        try
        {
            await ConnectToSocketServer();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Socket connection failed: " + ex.Message);
        }
    }

    private async Task ConnectToSocketServer()
    {
        _client = new TcpClient();

        await _client.ConnectAsync("127.0.0.1", 9000);

        NetworkStream stream = _client.GetStream();

        var helloPacket = new SocketPacket
        {
            Command = SocketCommand.HELLO,
            DisplayId = _displayId,
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
            {
                Console.WriteLine("Bad packet checksum");
                continue;
            }

            SocketPacket packet = SocketPacket.FromBytes(data);

            if (!packet.IsCommandValid())
                continue;

            if (packet.DisplayId != _displayId)
                continue;

            if (packet.Command == SocketCommand.OK)
            {
                Console.WriteLine("Server accepted display");
            }
            else if (packet.Command == SocketCommand.CALL)
            {
                UpdateDisplay(packet.TellerId, packet.CustomerNumber);
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
}