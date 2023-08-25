namespace CorePayments.WorkerService
{
    public class ChangeFeedWorker : BackgroundService
    {
        private readonly ILogger<ChangeFeedWorker> _logger;

        public ChangeFeedWorker(ILogger<ChangeFeedWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}