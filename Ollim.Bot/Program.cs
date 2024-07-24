using Ollim.Bot.Services;
using Ollim.Bot.Configurations;

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
        .AddJsonFile("appsettings.Production.json", false);



    })
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