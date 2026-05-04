using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace QueueService;

/// <summary>
/// Customer Queue class to make this class make only one object
/// used Singleton design pattern this class queues number representing customer
/// and when teller calls touch's a button represening a next customer
/// dequeue and send that to display using socket server 
/// </summary>
public sealed class CustomerQueue 
{
    private static CustomerQueue _instance;
    
    private static readonly object _lock = new object();

    private readonly Queue<byte> _queue = new Queue<byte>();

    private byte number;

    private CustomerQueue()
    {
        number = 1;
        Console.WriteLine("Customer queue created");
    }

    public static CustomerQueue GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new CustomerQueue();
                }
            }
        }
        return _instance;
    }
    //Enqueue's a number for customer and sends it 
    //to the printer back for it to print or something
    public byte EnqueueCustomer()
    {
        var value = number++; 
        _queue.Enqueue(value);
        return value;
    }
    


    public byte DequeueCustomer()
    {
        var value = _queue.Dequeue();
        return value;
    }
}