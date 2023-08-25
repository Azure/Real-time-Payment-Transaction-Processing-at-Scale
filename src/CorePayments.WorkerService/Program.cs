using CorePayments.WorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ChangeFeedWorker>();
    })
    .Build();

host.Run();
