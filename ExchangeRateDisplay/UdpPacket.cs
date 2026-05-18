using System.Net;

public static class DiscoveryCommand
{
    public const byte HI = 0x10;
    public const byte INFO = 0x11;
}

public class DiscoveryPacket
{
    public const int Size = 8;

    public byte Command { get; set; }

    public byte Ip1 { get; set; }
    public byte Ip2 { get; set; }
    public byte Ip3 { get; set; }
    public byte Ip4 { get; set; }

    public ushort Port { get; set; }

    public byte[] ToBytes()
    {
        byte[] data = new byte[Size];

        data[0] = Command;
        data[1] = Ip1;
        data[2] = Ip2;
        data[3] = Ip3;
        data[4] = Ip4;

        data[5] = (byte)(Port >> 8);
        data[6] = (byte)(Port & 0xFF);

        data[7] = CalculateChecksum(data);

        return data;
    }

    public static DiscoveryPacket? FromBytes(byte[] data)
    {
        if (!IsChecksumValid(data))
            return null;

        ushort port = (ushort)((data[5] << 8) | data[6]);

        return new DiscoveryPacket
        {
            Command = data[0],
            Ip1 = data[1],
            Ip2 = data[2],
            Ip3 = data[3],
            Ip4 = data[4],
            Port = port
        };
    }

    public IPAddress GetIpAddress()
    {
        return new IPAddress(new byte[]
        {
            Ip1,
            Ip2,
            Ip3,
            Ip4
        });
    }

    public static DiscoveryPacket CreateDiscoverPacket()
    {
        return new DiscoveryPacket
        {
            Command = DiscoveryCommand.HI,
            Ip1 = 0,
            Ip2 = 0,
            Ip3 = 0,
            Ip4 = 0,
            Port = 0
        };
    }

    public static DiscoveryPacket CreateServerInfoPacket(
        IPAddress ip,
        ushort port)
    {
        var bytes = ip.GetAddressBytes();

        return new DiscoveryPacket
        {
            Command = DiscoveryCommand.INFO,
            Ip1 = bytes[0],
            Ip2 = bytes[1],
            Ip3 = bytes[2],
            Ip4 = bytes[3],
            Port = port
        };
    }

    private static byte CalculateChecksum(byte[] data)
    {
        byte checksum = 0;

        for (int i = 0; i < Size - 1; i++)
        {
            checksum ^= data[i];
        }

        return checksum;
    }

    private static bool IsChecksumValid(byte[] data)
    {
        if (data.Length != Size)
            return false;

        return data[7] == CalculateChecksum(data);
    }
}