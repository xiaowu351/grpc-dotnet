using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Certify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Certificate;
using Google.Protobuf.WellKnownTypes;

namespace Server
{
    public class CertifierService : Certifier.CertifierBase
    {
        private readonly ILogger<CertifierService> _logger;
        public CertifierService(ILogger<CertifierService> logger)
        {
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
        public override Task<CertificateInfoResponse> GetCertificateInfo(Empty request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var clientCertificate = httpContext.Connection.ClientCertificate;
          

            var name = string.Join(',',context.AuthContext.PeerIdentity.Select(i=>i.Value));

            var certificateInfo = new CertificateInfoResponse {
                HasCertificate = context.AuthContext.IsPeerAuthenticated,
                Name = name
            };
            return Task.FromResult(certificateInfo);
        }
    }
}
