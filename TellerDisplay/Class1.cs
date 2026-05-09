public static class SocketCommand
{
    public const byte HELLO = 0x01;
    public const byte HI = 0x02;
    public const byte OK = 0x03;
    public const byte CALL = 0x04;
}

public class SocketPacket
{
    public const int Size = 8;

    public byte Command { get; set; }
    public byte DisplayId { get; set; }
    public byte CustomerNumber { get; set; }
    public byte TellerId { get; set; }
    public byte Key1 { get; set; }
    public byte Key2 { get; set; }
    public byte Key3 { get; set; }

    public byte[] ToBytes()
    {
        byte[] data = new byte[Size];

        data[0] = Command;
        data[1] = DisplayId;
        data[2] = CustomerNumber;
        data[3] = TellerId;
        data[4] = Key1;
        data[5] = Key2;
        data[6] = Key3;
        data[7] = CalculateChecksum(data);

        return data;
    }

    public static SocketPacket FromBytes(byte[] data)
    {
        return new SocketPacket
        {
            Command = data[0],
            DisplayId = data[1],
            CustomerNumber = data[2],
            TellerId = data[3],
            Key1 = data[4],
            Key2 = data[5],
            Key3 = data[6]
        };
    }

    public static byte CalculateChecksum(byte[] data)
    {
        byte checksum = 0;

        for (int i = 0; i < Size - 1; i++)
            checksum ^= data[i];

        return checksum;
    }

    public static bool IsChecksumValid(byte[] data)
    {
        return data.Length == Size && data[7] == CalculateChecksum(data);
    }

    public bool IsCommandValid()
    {
        return Command == SocketCommand.HELLO ||
               Command == SocketCommand.HI ||
               Command == SocketCommand.OK ||
               Command == SocketCommand.CALL;
    }
}