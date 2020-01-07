using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class TicketRepository
    {

        private readonly ILogger _logger;
        private int _availableTickets = 5;

        public TicketRepository(ILogger<TicketRepository> logger)
        {
            _logger = logger;
        }

        public int GetAvailableTickets()
        {
            return _availableTickets;
        }

        public bool BuyTickets(string user,int count)
        {
            var updatedCount = _availableTickets - count;
            if (updatedCount < 0)
            {
                _logger.LogError($"{user} failed to purchase tickets. Not enough available tickets.");
                return false;
            }

            _availableTickets = updatedCount;
            _logger.LogInformation($"{user} successfully purchase tickets.");
            return true;
        }


    }
}
