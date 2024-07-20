using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ollim.Bot.Services;
using Ollim.Bot.Configurations;
using Ollim.Infrastructure.Data;
using Ollim.Infrastructure.Interfaces;
using Ollim.Infrastructure.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<DiscordSocketConfig>();
        services.AddSingleton<VoiceHandler>();
        services.AddSingleton<InteractionService>();
        services.AddSingleton<AppDbContext>();
        services.AddSingleton<ISendMessageService, SendMessageService>();


        services.AddScoped<IImageProfileProcessingService, ImageProfileProcessingService>();

        services.AddHostedService<OllimBot>();
    })
    .UseConsoleLifetime()
    .Build()
    .RunAsync();