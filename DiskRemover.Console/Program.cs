

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Remover>((sp) =>
        {
            var logger = sp.GetRequiredService<ILogger<Remover>>();
            var remover = new Remover(logger);
            remover.SetDirectory("C:\\Users\\exper\\Desktop\\Test");
            remover.SetDays(10);
            return remover;
        });
    })
    .Build()
    .Run();


class Remover : BackgroundService
{
    public DirectoryInfo? SaveDirectory => saveDirectory;
    public int? Days => days;

    DirectoryInfo? saveDirectory;
    int? days;
    private readonly ILogger _logger;
    public Remover(ILogger<Remover> logger)
    {
        _logger = logger;
    }
    public int? SetDays(int days)
    {
        this.days = days;
        return Days;
    }

    public DirectoryInfo? SetDirectory(string path)
    {
        if(!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException();
        }
        if(Path.GetDirectoryName(path) == null)
        {
            throw new ArgumentException();
        }

        saveDirectory = new DirectoryInfo(path);

        return SaveDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DiskRemover running at: {time}", DateTimeOffset.Now);

        await BackgroundProcessing(stoppingToken);

    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (SaveDirectory == null)
            {
                _logger.LogWarning("Save directory is not set");
                continue;
            }
            if (Days == null)
            {
                _logger.LogWarning("Days is not set");
                continue;
            }

            var directories = SaveDirectory.GetDirectories();
            var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            foreach (var directory in directories)
            {
                try
                {
                    var year = int.Parse(directory.Name.Substring(0, 4));
                    var month = int.Parse(directory.Name.Substring(4, 2));
                    var day = int.Parse(directory.Name.Substring(6, 2));
                    var creationTime = new DateTime(year, month, day);
                    _logger.LogInformation($"{directory.Name} is created for {(today - creationTime).TotalDays} days");
                    if ((today - creationTime).TotalDays > Days)
                    {
                        directory.Delete(true);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    continue;
                }
            }
        }
    }
}