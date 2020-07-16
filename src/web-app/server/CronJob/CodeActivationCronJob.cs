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
    public class CodeActivationCronJob : CronJobService
    {
        private readonly IConfiguration _configuration;
        private readonly IScheduleConfig<CodeActivationCronJob> config;
        private readonly ILogger<CodeActivationCronJob> _logger;
        public CodeActivationCronJob(
            IConfiguration configuration,
            IScheduleConfig<CodeActivationCronJob> config,
            ILogger<CodeActivationCronJob> logger)
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
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Activation job is working.");

            var now = DateTime.Now;

            using (var connection = new SqlConnection(_configuration.GetConnectionString("Storage")))
            {
                var codeRepository = new SqlCodeRepository(connection); 
                var generatedCodes = codeRepository.GetActivatingAsync(now);
                var codes = new List<Code>();

                await foreach(var code in generatedCodes)
                {
                    code.Activate();
                    codes.Add(code);
                }

                await codeRepository.UpdateAsync(codes);
            }

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Activation job completed.");
        }
    }
}
