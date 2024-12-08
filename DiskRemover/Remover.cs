using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskRemover
{
    public class Remover : BackgroundService
    {
        public DirectoryInfo? SaveDirectory => saveDirectory;
        public int? Days => days;

        DirectoryInfo? saveDirectory;
        int? days;
        private readonly ILogger<Remover> _logger;
        public Remover(ILogger<Remover> logger)
        {
            _logger = logger;
        }
        public int? SetDays(int? days)
        {
            this.days = days;
            return Days;
        }

        public DirectoryInfo? SetDirectory(string? path)
        {
            if (path == null)
                return null;

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
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
                await Task.Delay(1000, stoppingToken);
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

                _logger.LogInformation($"Try Remove At {SaveDirectory} For {Days} Days");
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
                        if ((today - creationTime).TotalDays > Days)
                        {
                            directory.Delete(true);
                            _logger.LogInformation($"{directory.Name} deleted");
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
}
