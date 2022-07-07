using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace NotificaitonSender
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing((builder) => builder
             .AddAspNetCoreInstrumentation()
             .AddHttpClientInstrumentation()
             .AddAWSInstrumentation()
             .AddXRayTraceId()
             .AddOtlpExporter(o =>
             {
                 o.Protocol = OtlpExportProtocol.HttpProtobuf;
                 o.Endpoint = !string.IsNullOrEmpty(Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"]) ? new Uri(Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"]) : DecodeAndExtractServerUrl(Configuration["RM_DEV_SL_TOKEN"], Configuration["OTEL_AGENT_COLLECTOR_PORT"]);
                 o.Headers = AddHeaders(Configuration["RM_DEV_SL_TOKEN"], Configuration["OTEL_AGENT_COLLECTOR_PROTOCOL"]);

                 Console.WriteLine($"OTLP endpoint: {o.Endpoint}");
                 Console.WriteLine($"OTLP headers {o.Headers}");
                 Console.WriteLine($"OTLP protocol {o.Protocol}");
             }));
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
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Notification Sender");
                });
            });
        }

        private static Uri DecodeAndExtractServerUrl(string token, string port)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("token is empty");
            }

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            JwtSecurityToken tokenS = jsonToken as JwtSecurityToken;

            string apiLabAddress = tokenS.Claims.First(claim => claim.Type == "x-sl-server").Value;

            if (string.IsNullOrEmpty(apiLabAddress))
            {
                throw new Exception("x-sl-server url value is empty");
            }

            string host = apiLabAddress.Replace("https://", "");
            host = host.Replace("/api", "");

            return new Uri($@"https://ingest.{host}:{port}/v1/traces");
        }

        private static string AddHeaders(string token, string protocol)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("token is empty");
            }

            if (string.IsNullOrEmpty(protocol))
            {
                throw new Exception("protocol is empty");
            }

            return $"Authorization=Bearer {token}, x-otlp-protocol={protocol}";
        }
    }
}
