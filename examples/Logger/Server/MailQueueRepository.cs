using System.Collections.Concurrent;

namespace Server
{
    public class MailQueueRepository
    {
        ConcurrentDictionary<string, MailQueue> _mailQueues = new ConcurrentDictionary<string, MailQueue>();

        public MailQueue GetMailQueue(string name){
            return _mailQueues.GetOrAdd(name,(n)=> new MailQueue(n));
        }
    }

}