using Microsoft.AspNetCore.Hosting;
using NotificaitonSender;
using NotificaitonSender.NotificationService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<NotificationService>();
    })
    .Build();

await host.RunAsync();
