using Ollim.Bot.Services;
using Ollim.Bot.Configurations;
using Discord.WebSocket;
using Discord.Interactions;
using Ollim.Infrastructure.Data;
using Ollim.Infrastructure.Interfaces;
using Ollim.Infrastructure.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHost(
    webhost => webhost
    .UseKestrel(kestrelOptions =>
    {
        kestrelOptions.ListenAnyIP(1111);
    })
    .Configure(app =>
    {
        app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

        app.Run(async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }))
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", false)
        .AddJsonFile("appsettings.Development.json", true)
        .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);



    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<DiscordSocketConfig>();
        services.AddSingleton<VoiceHandler>();
        services.AddSingleton<InteractionService>();
        services.AddSingleton<AppDbContext>();
        services.AddSingleton<ISendMessageService, SendMessageService>();
        services.AddHostedService<OllimBackgroundServices>();


        services.AddScoped<IImageProfileProcessingService, ImageProfileProcessingService>();

        services.AddHostedService<OllimBot>();


        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin() // Replace with specific allowed origins if needed
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    })
    .UseConsoleLifetime()
    .Build()
    .RunAsync();