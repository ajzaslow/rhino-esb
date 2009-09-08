using System;
using Rhino.ServiceBus.Hosting;
using Starbucks.Barista;
using Starbucks.Cashier;
using Starbucks.Customer;

namespace Starbucks
{
    public abstract class QueuePlatform
    {
        public abstract void PrepareQueues();

        protected abstract string BaristaUri { get; }
        protected abstract string CashierUri { get; }
        protected abstract string CustomerUri { get; }
        
        public void ConfigureBarista()
        {
            new RemoteAppDomainHost(typeof(BaristaBootStrapper)).Configuration(BaristaConfig).Start();
            
            Console.WriteLine("Barista has started");
        }

        public void ConfigureCashier()
        {
            new RemoteAppDomainHost(typeof(CashierBootStrapper)).Configuration(CashierConfig).Start();

            Console.WriteLine("Cashier has started");
        }

        public DefaultHost ConfigureCustomer()
        {
            var customerHost = new DefaultHost();

            customerHost.BusConfiguration(c => c
                .Bus(CustomerUri, "customer")
                .Receive("Starbucks.Messages.Cashier", CashierUri)
                .Receive("Starbucks.Messages.Barista", BaristaUri));
            
            customerHost.Start<CustomerBootStrapper>();

            return customerHost;
        }

        public virtual void ConfigureLoadBalancer()
        {
        }

        protected virtual string BaristaQueueName
        {
            get { return "starbucks.barista"; }
        }

        protected virtual string CashierQueueName
        {
            get { return "starbucks.cashier"; }
        }

        protected virtual string CustomerQueueName
        {
            get { return "starbucks.customer"; }
        }

        protected virtual string BaristaConfig
        {
            get { return "Barista.config"; }
        }

        protected virtual string CashierConfig
        {
            get { return "Cashier.config"; }
        }
    }
}