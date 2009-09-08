using System;
using System.Messaging;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Msmq;

namespace Starbucks
{
    public class MsmqPlatform : QueuePlatform
    {
        public override void PrepareQueues()
        {
            Prepare(LoadBalancerQueueName, QueueType.LoadBalancer);
            Prepare(BaristaQueueName, QueueType.Standard);
            Prepare(CashierQueueName, QueueType.Standard);
            Prepare(CustomerQueueName, QueueType.Standard);
        }

        public void Prepare(string queueName, QueueType queueType)
        {
            var queueUri = new Uri(GetQueueUri(queueName));
            var queuePath = MsmqUtil.GetQueuePath(new Endpoint
            {
                Uri = queueUri
            });
            CreateQueueIfNotExists(queuePath.QueuePath);
            PurgeQueue(queuePath.QueuePath);
            PurgeSubqueues(queuePath.QueuePath, queueType);
        }

        public override void ConfigureLoadBalancer()
        {
            new RemoteAppDomainLoadBalancerHost(typeof(RemoteAppDomainHost).Assembly, LoadBalancerConfig).Start();

            Console.WriteLine("Barista load balancer has started");
        }

        protected override string BaristaUri
        {
            get { return GetQueueUri(LoadBalancerQueueName); }
        }

        protected override string CashierUri
        {
            get { return GetQueueUri(CashierQueueName); }
        }

        protected override string CustomerUri
        {
            get { return GetQueueUri(CustomerQueueName); }
        }

        public string LoadBalancerConfig
        {
            get { return "LoadBalancer.config"; }
        }

        public virtual string LoadBalancerQueueName
        {
            get { return "starbucks.barista.balancer"; }
        }

        static void CreateQueueIfNotExists(string queuePath)
        {
            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }
        }

        static string GetQueueUri(string queueName)
        {
            return string.Format("msmq://localhost/{0}", queueName);
        }

        static void PurgeQueue(string queuePath)
        {
            using (var queue = new MessageQueue(queuePath))
            {
                queue.Purge();
            }
        }

        static void PurgeSubqueues(string queuePath, QueueType queueType)
        {
            switch (queueType)
            {
                case QueueType.Standard:
                    PurgeSubqueue(queuePath, "errors");
                    PurgeSubqueue(queuePath, "discarded");
                    PurgeSubqueue(queuePath, "timeout");
                    PurgeSubqueue(queuePath, "subscriptions");
                    break;
                case QueueType.LoadBalancer:
                    PurgeSubqueue(queuePath, "endpoints");
                    PurgeSubqueue(queuePath, "workers");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("queueType", "Can't handle queue type: " + queueType);
            }
        }

        static void PurgeSubqueue(string queuePath, string subqueueName)
        {
            using (var queue = new MessageQueue(queuePath + ";" + subqueueName))
            {
                queue.Purge();
            }
        }
    }
}
