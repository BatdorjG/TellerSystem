
namespace IRepository;

public interface ICustomerQueueRepository
{
    Task Enqueue(int value);

    Task<int?> Dequeue();

    Task<List<int>> GetActiveNumbers();

    Task CleanQueue();
}