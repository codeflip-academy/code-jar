using CodeJar.Domain;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using CodeJar.Infrastructure;
using System.Collections.Generic;
using System.IO;
using CodeFlip.CodeJar.Api;
using CodeJar.WebApp.Commands;
using CodeJar.Infrastructure.Guids;
using System.Diagnostics;

namespace CodeJar.ServiceBusAzure
{
    public class ReceiveServiceBus : IHostedService
    {
        const string QueueName = "codejar";
        private IQueueClient _queueClient;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly ISequentialGuidGenerator _idGenerator;

        public ReceiveServiceBus(ILoggerFactory loggerFactory, IConfiguration configuration, ISequentialGuidGenerator idGenerator)
        {
            _logger = loggerFactory.CreateLogger<ReceiveServiceBus>();
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _idGenerator = idGenerator;
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var watch = Stopwatch.StartNew();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("Storage")))
            {
                var batchRepository = new SqlBatchRepository(connection);
                var codeRepository = new SqlCodeRepository(connection);

                var command = JsonConvert.DeserializeObject<CreateBatchCommand>(Encoding.UTF8.GetString(message.Body));

                var reader = new CloudReader(_configuration.GetSection("File")["SeedBlobUrl"], connection);

                var batch = new Batch
                {
                    BatchName = command.BatchName,
                    BatchSize = command.BatchSize,
                    PromotionType = command.PromotionType,
                    DateActive = command.DateActive,
                    DateExpires = command.DateExpires,
                    Id = _idGenerator.NextId()
                };

                var codes = batch.GenerateCodesAsync(reader);

                await batchRepository.AddAsync(batch);
                await codeRepository.AddAsync(codes);

                _logger.LogInformation($"Batch {batch.Id} with {batch.BatchSize} generated in {watch.ElapsedMilliseconds}ms.");
            }
        }

        protected void ProcessError(Exception e)
        {
            _logger.LogError(e, "Error while processing queue item in BusListenerService.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _queueClient = new QueueClient(_configuration.GetConnectionString("AzureServiceBus"), QueueName);

            _logger.LogDebug($"BusListenerService starting; registering message handler.");

            var messageHandlerOptions = new MessageHandlerOptions(e =>
            {
                ProcessError(e.Exception);
                return Task.CompletedTask;
            })
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"BusListenerService stopping.");
            await _queueClient.CloseAsync();
        }
    }
}