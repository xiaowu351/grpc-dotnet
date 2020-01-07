using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace.Configuration;

namespace Server
{
    public class Startup
    {

        private readonly IConfiguration _configuration;
        private const string EnableOpenTelemetryKey = "EnableOpenTelemetry";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton<IncrementingCounter>();

            if (_configuration.GetValue<bool>(EnableOpenTelemetryKey))
            {
                services.AddOpenTelemetry(telemetry =>
                {
                    telemetry.UseZipkin(o =>
                    {
                        o.ServiceName = "aggregator";
                        o.Endpoint = new Uri("http://192.168.178.129:9411/api/v2/spans");
                    });
                    telemetry.AddDependencyCollector();
                    telemetry.AddRequestCollector();
                });
            }

            services.AddGrpcClient<Greet.Greeter.GreeterClient>((s, o) =>
            {
                o.Address = GetCurrentAddress(s);
            }).EnableCallContextPropagation();

            services.AddGrpcClient<Count.Counter.CounterClient>((s, o) =>
            {
                o.Address = GetCurrentAddress(s);
            }).EnableCallContextPropagation();

            static Uri GetCurrentAddress(IServiceProvider serviceProvider)
            {
                var context = serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
                if (context == null)
                {
                    throw new InvalidOperationException("Could not get HttpContext.");
                }
                return new Uri($"{context.Request.Scheme}://{context.Request.Host.Value}");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<CounterService>();
                endpoints.MapGrpcService<AggregatorService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
