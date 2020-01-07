using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticket;


namespace Server
{
    public class TicketerService:Ticketer.TicketerBase
    {
        private readonly TicketRepository _ticketRepository;

        public TicketerService(TicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public override Task<AvailableTicketsResponse> GetAvailableTickets(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new AvailableTicketsResponse { 
                Count = _ticketRepository.GetAvailableTickets()
            }); 
        }

        [Authorize]
        public override Task<BuyTicketsResponse> BuyTickets(BuyTicketsRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            return Task.FromResult(new BuyTicketsResponse
            {
                Success = _ticketRepository.BuyTickets(user.Identity.Name, request.Count)
            }); 
        }
    }
}
