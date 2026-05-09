namespace QueueService;

public sealed class CustomerQueue
{
    private readonly Queue<byte> _queue = new();
    private readonly object _lock = new();
    //used byte for the queue to save little bit less memory usage;
    private byte number = 1;

    public byte EnqueueCustomer()
    {
        lock (_lock)
        {
            //Isnt possible unless 255 customer is in the building
            if (_queue.Count >= 255)
                throw new InvalidOperationException("Queue is full.");

            byte value = number;

            //If number is in queue we skip that number and 
            //search for a number that is not in the queue
            while (_queue.Contains(value))
            {
                value++;

                if (value == 0)
                    value = 1;
            }

            number = value;
            _queue.Enqueue(value);

            number++;
            // if byte overflows from 255 its going to become 0 so check if overflowed 
            // and if its overflowed change the number to 1
            if (number == 0)
                number = 1;

            return value;
        }
    }
    public bool TryDequeueCustomer(out byte value)
    {
        lock (_lock)
        {
            if (_queue.Count == 0)
            {
                value = 0;
                return false;
            }

            value = _queue.Dequeue();
            return true;
        }
    }
}