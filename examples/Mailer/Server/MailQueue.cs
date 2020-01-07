using System;
using Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Server
{
    public class Mail
    {
        public Mail(int id, string content)
        {
            Id = id;
            Content = content;
        }

        public int Id { get; }
        public string Content { get; }
    }
    public class MailQueue
    {
        private readonly Channel<Mail> _incomingMail;
        private int _totalMailCount;
        private int _forwardedMailCount;

        public string Name {get;}

        public event Func<(int totalCount,int newCount, MailboxMessage.Types.Reason reason),Task> ? Changed;

        public MailQueue(string name){
            Name = name;
            _incomingMail = Channel.CreateUnbounded<Mail>();

            Task.Run(async ()=>{
                var random = new Random();
                while(true){
                   _totalMailCount ++;
                    var mail = new Mail(_totalMailCount,$"Message #{_totalMailCount}");
                    await _incomingMail.Writer.WriteAsync(mail);
                     OnChange(MailboxMessage.Types.Reason.Received);

                    await Task.Delay(TimeSpan.FromSeconds(random.Next(5,15)));
                }
            });
            
        }

        public bool TryForwardMail(out Mail message){
            if(_incomingMail.Reader.TryRead(out message)){
                Interlocked.Increment(ref _forwardedMailCount);
                OnChange(MailboxMessage.Types.Reason.Forwarded);
                return true;
            }

            return false;
        }

        private void OnChange(MailboxMessage.Types.Reason reason){
            Changed?.Invoke((_totalMailCount,_forwardedMailCount,reason));
        }


    }
}