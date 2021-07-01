﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GrpcGreeter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddOpenTelemetryTracing(ConfigureOpenTelemetryTracing);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        private  void ConfigureOpenTelemetryTracing(TracerProviderBuilder builder)
        {
            Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
                new TextMapPropagator[]{
                                    new B3Propagator(),
                                    new TraceContextPropagator()
                }
            ));
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService("ovidiu-hello");
            builder.SetResourceBuilder(resourceBuilder)
                .AddSource("activitySourceName")
                .SetSampler(new AlwaysOnSampler())
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddAspNetCoreInstrumentation(config => {
                    config.RecordException = true;
                    config.EnableGrpcAspNetCoreSupport = true;
                });

            builder.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4317");
                o.ExportProcessorType = ExportProcessorType.Batch;
            });
        }

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

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
