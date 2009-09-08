using System;
using Rhino.ServiceBus;
using Starbucks.Customer;
using Starbucks.Messages;

namespace Starbucks
{
    public class Program
    {
        public static void Main()
        {
            QueuePlatform platform = new RhinoQueuesPlatform();
//            QueuePlatform platform = new MsmqPlatform();
            
            platform.PrepareQueues();
            platform.ConfigureLoadBalancer();
            platform.ConfigureCashier();
            platform.ConfigureBarista();

            var customerHost = platform.ConfigureCustomer();
            var bus = customerHost.Container.Resolve<IServiceBus>();

            var customer = new CustomerController(bus)
            {
                Drink = "Hot Chocolate",
                Name = "Ayende",
                Size = DrinkSize.Venti
            };

            customer.BuyDrinkSync();

            Console.ReadLine();
        }
    }
}