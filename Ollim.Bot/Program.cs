using Ollim.Bot.Services;
using Ollim.Bot.Configurations;
using Discord.WebSocket;
using Discord.Interactions;
using Ollim.Infrastructure.Data;
using Ollim.Infrastructure.Interfaces;
using Ollim.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Ollim.Domain.Repositories;
using Ollim.Infrastructure.Repositories;
using Ollim.Bot.Configurations.Handlers;

await Host.CreateDefaultBuilder()
    .ConfigureWebHost(
    webhost => webhost
    .UseKestrel(kestrelOptions =>
    {
        kestrelOptions.ListenAnyIP(1111);
    })
    .Configure((app) =>
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
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        var env = hostContext.HostingEnvironment;
        Console.WriteLine($"Current environment: {env.EnvironmentName}");

        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>(provider => OllimBot.CreateDiscordClient());

        services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(client.Rest);
        });

        services.AddSingleton<DiscordSocketConfig>();
        services.AddSingleton<VoiceHandler>();
        services.AddSingleton<ISendMessageService, SendMessageService>();
        services.AddSingleton<DiscordExceptionHandler>();
        services.AddHostedService<OllimBackgroundServices>();


        services.AddSingleton<IImageProfileProcessingService, ImageProfileProcessingService>();
        services.AddScoped<GeminiService>();
        services.AddScoped<IChannelRepository, ChannelRepository>();

        services.AddHostedService<OllimBot>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection"));
        });


        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    })
    .UseConsoleLifetime()
    .Build()
    .RunAsync();