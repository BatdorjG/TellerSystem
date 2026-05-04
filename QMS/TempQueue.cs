using System;
using System.Collections.Generic;
using System.Text;

namespace QMS;
public class TempQueue
{
    Queue<byte> queue = new Queue<byte>();

    public TempQueue() {
        for(int i = 0; i < 10; i++) {
            queue.Enqueue((byte)i);
        }
    }

    public byte Give()
    {
        byte value = queue.Dequeue();
        queue.Enqueue(value);

        return value;
    }


}
