using CodeJar.Domain;
using CodeJar.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TodoWebAPI.CronJob
{
    public class CodeExpirationCronJob : CronJobService
    {
        private readonly IConfiguration _configuration;
        private readonly IScheduleConfig<CodeExpirationCronJob> config;
        private readonly ILogger<CodeExpirationCronJob> _logger;
        public CodeExpirationCronJob(
            IConfiguration configuration,
            IScheduleConfig<CodeExpirationCronJob> config,
            ILogger<CodeExpirationCronJob> logger)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _configuration = configuration;
            this.config = config;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Due Date Starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Due Date Job is Stopping");
            return base.StopAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Expiration job is working.");

            using(var connection = new SqlConnection(_configuration.GetConnectionString("Storage")))
            {
                var codeRepository = new AdoCodeRepository(connection);

                var codes = await codeRepository.GetCodesForExpirationAsync(DateTime.Now.Date, _configuration.GetSection("Base26")["alphabet"]);

                foreach(var code in codes)
                    code.Expire();

                await codeRepository.UpdateCodesAsync(codes);
            }

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Expiration job completed.");
        }
    }
}
