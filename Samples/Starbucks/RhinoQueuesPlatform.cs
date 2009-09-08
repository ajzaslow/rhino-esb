using Rhino.Queues.Storage;

namespace Starbucks
{
    public class RhinoQueuesPlatform : QueuePlatform
    {
        const int BaristaPort = 4545;
        const int CashierPort = 4546;
        const int CustomerPort = 4547;
        const string StarbucksDatabase = "starbucks.esent";

        public override void PrepareQueues()
        {
            Prepare(BaristaQueueName);
            Prepare(CashierQueueName);
            Prepare(CustomerQueueName);
        }

        protected override string BaristaConfig
        {
            get { return GetRqConfig(base.BaristaConfig); }
        }

        protected override string CashierConfig
        {
            get { return GetRqConfig(base.CashierConfig); }
        }

        protected override string BaristaUri
        {
            get { return GetQueueUri("starbucks.barista", BaristaPort); }
        }

        protected override string CashierUri
        {
            get { return GetQueueUri("starbucks.cashier", CashierPort); }
        }

        protected override string CustomerUri
        {
            get { return GetQueueUri("starbucks.customer", CustomerPort); }
        }

        static string GetEsentSafeQueueName(string queueName)
        {
            return queueName.Replace('.', '_');
        }

        static string GetQueueUri(string queueName, int port)
        {
            return string.Format("rhino.queues://localhost:{0}/{1}", port, GetEsentSafeQueueName(queueName));
        }

        static string GetRqConfig(string config)
        {
            return config.Replace(".config", ".Rq.config");
        }

        static void Prepare(string queueName)
        {
            var safeQueueName = GetEsentSafeQueueName(queueName);

            using (var queueStorage = new QueueStorage(StarbucksDatabase))
            {
                queueStorage.Initialize();

                queueStorage.Global(actions =>
                {
                    actions.CreateQueueIfDoesNotExists(safeQueueName);

                    PurgeQueue(actions, actions.GetQueue(safeQueueName));
                });
            }
        }

        static void PurgeQueue(AbstractActions actions, QueueActions queue)
        {
            while (queue.Dequeue(null) != null) { }

            foreach (var subqueue in queue.Subqueues)
            {
                PurgeQueue(actions, actions.GetQueue(subqueue));
            }
        }
    }
}