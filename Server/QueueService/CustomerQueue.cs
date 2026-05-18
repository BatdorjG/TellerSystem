using IRepository;

namespace QueueService;

public sealed class CustomerQueue
{
    private readonly ICustomerQueueRepository _customerQueueRepository;
    private readonly object _lock = new();

    private int _nextNumber = 1;

    public CustomerQueue(ICustomerQueueRepository customerQueueRepository)
    {
        _customerQueueRepository = customerQueueRepository;
    }

    public async Task<int> EnqueueCustomer()
    {
        var activeNumbers = await _customerQueueRepository.GetActiveNumbers();

        int value;

        lock (_lock)
        {
            value = _nextNumber;

            while (activeNumbers.Contains(value))
            {
                value++;

                if (value > 255)
                    value = 1;

                if (value == _nextNumber)
                    throw new InvalidOperationException("Queue is full.");
            }

            _nextNumber = value + 1;

            if (_nextNumber > 255)
                _nextNumber = 1;
        }

        await _customerQueueRepository.Enqueue(value);

        return value;
    }

    public async Task<int?> DequeueCustomer()
    {
        return await _customerQueueRepository.Dequeue();
    }

    public async Task CleanQueue()
    {
        await _customerQueueRepository.CleanQueue();

        lock (_lock)
        {
            _nextNumber = 1;
        }
    }
}