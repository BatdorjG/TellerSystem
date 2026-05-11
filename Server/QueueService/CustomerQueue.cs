using Repository;

namespace QueueService;

public sealed class CustomerQueue
{
    private readonly CustomerQueueRepos _customerQueue = new();
    private readonly object _lock = new();

    private int _nextNumber = 1;

    public async Task<int> EnqueueCustomer()
    {
        var activeNumbers = await _customerQueue.GetActiveNumbers();
        int value;
        lock (_lock)
        {
            value = _nextNumber;

            while (activeNumbers.Contains(value))
            {
                value++;
            }

            _nextNumber = value + 1;
        }
        await _customerQueue.Enqueue(value);
        return value;
    }

    public async Task<int?> DequeueCustomer()
    {
        return await _customerQueue.Dequeue();
    }

    public async Task CleanQueue()
    {
        await _customerQueue.CleanQueue();
    }
}