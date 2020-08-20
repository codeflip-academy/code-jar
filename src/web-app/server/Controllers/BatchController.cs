using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeJar.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using System.Text;
using Newtonsoft.Json;
using CodeJar.Infrastructure;
using CodeJar.WebApp.ViewModels;
using System.Data.SqlClient;
using CodeJar.WebApp.Commands;

namespace CodeJar.WebApp.Controllers
{
    [ApiController]
    public class BatchController : ControllerBase
    {
        private readonly IQueueClient _queueClient;
        private readonly PaginationCount _pagination;
        private readonly SqlConnection _connection;
        private readonly ILogger<CodesController> _logger;
        private readonly IConfiguration _config;
        private readonly IBatchRepository _batchRepository;
        private readonly ICodeRepository _codeRepository;

        public BatchController(
            ILogger<CodesController> logger,
            IConfiguration config,
            IBatchRepository batchRepository,
            ICodeRepository codeRepository,
            IQueueClient queueClient,
            PaginationCount paginationCount,
            SqlConnection connection)
        {
            _logger = logger;
            _config = config;
            _batchRepository = batchRepository;
            _codeRepository = codeRepository;
            _queueClient = queueClient;
            _pagination = paginationCount;
            _connection = connection;
        }

        [HttpGet("batch")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _batchRepository.GetBatchesAsync());
        }

        [HttpGet("batch/{id}")]
        public async Task<IActionResult> GetBatch(Guid id, [FromQuery] int page)
        {
            var alphabet = _config.GetSection("Base26")["alphabet"];
            var pageSize = Convert.ToInt32(_config.GetSection("Pagination")["PageNumber"]);

            var codes = new List<CodeViewModel>();

            try
            {
                await _connection.OpenAsync();

                var p = PageHelper.PaginationPageNumber(page, pageSize);

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.SeedValue, Codes.State FROM Codes
                                            WHERE Codes.BatchId = @batchID
                                            ORDER BY ID OFFSET @page ROWS FETCH NEXT @pageSize ROWS ONLY";

                    command.Parameters.AddWithValue("@page", p);
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@batchID", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var seedValue = (int)reader["SeedValue"];

                            var code = new CodeViewModel();
                            code.Id = (int)reader["ID"];
                            code.StringValue = CodeConverter.ConvertToCode(seedValue, alphabet, _config.GetValue<int>("CodeLength"));
                            code.State = CodeStateSerializer.DeserializeState((byte)reader["State"]);

                            codes.Add(code);
                        }
                    }
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }

            var pages = await _pagination.PageCount(id);

            return Ok(new CodesViewModel(codes, pages));
        }

        [HttpDelete("batch")]
        public async Task<IActionResult> DeactivateBatch([FromBody] Batch batch)
        {
            await _batchRepository.DeactivateBatchAsync(batch.Id);
            return Ok();
        }

        [HttpPost("batch")]
        public async Task<IActionResult> Post([FromBody] CreateBatchCommand request)
        {
            var today = DateTime.Now;

            if (request.DateActive.Date < request.DateExpires.Date && request.DateActive.Date >= today.Date)
            {
                string messageBody = JsonConvert.SerializeObject(request);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await _queueClient.SendAsync(message);

                return Ok(request);
            }

            return BadRequest();
        }
    }
}
