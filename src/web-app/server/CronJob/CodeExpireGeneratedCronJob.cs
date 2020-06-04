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
    public class CodeExpirationGeneratedCronJob : CronJobService
    {
        private readonly IConfiguration _configuration;
        private readonly IScheduleConfig<CodeExpirationGeneratedCronJob> config;
        private readonly ILogger<CodeExpirationGeneratedCronJob> _logger;
        public CodeExpirationGeneratedCronJob(
            IConfiguration configuration,
            IScheduleConfig<CodeExpirationGeneratedCronJob> config,
            ILogger<CodeExpirationGeneratedCronJob> logger)
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

            var connection = new SqlConnection(_configuration.GetConnectionString("Storage"));

            var codeRepository = new AdoCodeRepository(connection); 
            var activeCodes = await codeRepository.GetCodesForExpirationAsync(DateTime.Now.Date, _configuration.GetSection("Base26")["alphabet"]);

            foreach(var code in activeCodes)
                code.ExpireGenerated(DateTime.Now.Date);

            await codeRepository.UpdateCodesAsync(activeCodes);

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Expiration job completed.");
        }
    }
}
