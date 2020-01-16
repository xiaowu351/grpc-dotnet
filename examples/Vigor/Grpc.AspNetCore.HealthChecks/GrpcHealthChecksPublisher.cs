using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Grpc.AspNetCore.HealthChecks
{
    internal class GrpcHealthChecksPublisher : IHealthCheckPublisher
    {
        private readonly HealthServiceImpl _healthService;

        public GrpcHealthChecksPublisher(HealthServiceImpl healthService)
        {
            _healthService = healthService;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            foreach (var entry in report.Entries)
            {
                var status = entry.Value.Status;

                _healthService.SetStatus(entry.Key, ResolveStatus(status));
            }

            return Task.CompletedTask;
        }

        private static HealthCheckResponse.Types.ServingStatus ResolveStatus(HealthStatus status)
        {
            return status == HealthStatus.Unhealthy
                ? HealthCheckResponse.Types.ServingStatus.NotServing
                : HealthCheckResponse.Types.ServingStatus.Serving;
        }
    }

}