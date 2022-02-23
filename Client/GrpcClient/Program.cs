// See https://aka.ms/new-console-template for more information





using OpenTelemetry;
using OpenTelemetry.Trace;

using GrpcGreeter;
using Grpc.Net.Client;
using OpenTelemetry.Resources;
using System.Diagnostics;

ActivitySource activitySource = new ActivitySource("ActivitySourceName");
ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService("grpc-client");
    

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource(activitySource.Name)
    .AddGrpcClientInstrumentation()
    .AddHttpClientInstrumentation()
    .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4317");
                o.ExportProcessorType = ExportProcessorType.Batch;
            })
    .Build();

using var activity= activitySource.StartActivity("MainClientActivity");
using var channel = GrpcChannel.ForAddress("http://localhost:4300");
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(
                  new HelloRequest { Name = "GreeterClient" });


Console.WriteLine("Hello, World!");
