using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public interface IQueueManager
    {
        void Publish(string queueName, byte[] message);
        void Publish(Uri queueUri, string queueName, byte[] message);
    }
    public class QueueManager: IQueueManager
    {
        public void Publish(string queueName, byte[] message)
        {
            throw new NotImplementedException();
        }

        public void Publish(Uri queueUri, string queueName, byte[] message)
        {
            throw new NotImplementedException();
        }
    }
}
