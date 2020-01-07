using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Mail;
namespace Server
{
    public class MailerService : Mailer.MailerBase
    {
        private readonly ILogger<MailerService> _logger;
        private readonly MailQueueRepository _messageQueueRepository;
        public MailerService(ILogger<MailerService> logger, MailQueueRepository messageQueueRepository)
        {
            _logger = logger;
            _messageQueueRepository = messageQueueRepository;
        }

        public override async Task Mailbox(Grpc.Core.IAsyncStreamReader<ForwardMailMessage> requestStream, Grpc.Core.IServerStreamWriter<MailboxMessage> responseStream, Grpc.Core.ServerCallContext context)
        {
            var mailboxName = context.RequestHeaders.Single(e => e.Key == "mailbox-name").Value;
            var mailQueue = _messageQueueRepository.GetMailQueue(mailboxName);
            _logger.LogInformation($"Connected to {mailboxName}");

            mailQueue.Changed += ReportChanges;

            try
            {
                while (await requestStream.MoveNext())
                {
                    if (mailQueue.TryForwardMail(out var message))
                    {
                        _logger.LogInformation($"Forwarded mail:{message.Content}");
                    }
                    else
                    {
                        _logger.LogWarning("No mail to forward.");
                    }
                }
            }
            finally
            {
                mailQueue.Changed -= ReportChanges;
            }

            _logger.LogInformation($"{mailboxName} disconnected.");

            async Task ReportChanges((int totalCount, int forwardCount, MailboxMessage.Types.Reason reason) state)
            {

                await responseStream.WriteAsync(new MailboxMessage
                {
                    Forwarded = state.forwardCount,
                    New = state.totalCount - state.forwardCount,
                    Reason = state.reason
                });
            }
        }


    }
}
