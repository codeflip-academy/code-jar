using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeJar.Domain;
using CodeJar.Infrastructure;
using CodeJar.WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeJar.WebApp.Controllers
{
    [ApiController]
    [Route("codes")]
    public class CodesController : ControllerBase
    {
        private readonly ILogger<CodesController> _logger;
        private readonly IConfiguration _config;
        private readonly ICodeRepository _codeRepository;
        private readonly SqlConnection _connection;

        public CodesController(ILogger<CodesController> logger, IConfiguration config, ICodeRepository codeRepository, SqlConnection connection)
        {
            _logger = logger;
            _config = config;
            _codeRepository = codeRepository;
            _connection = connection;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string stringValue)
        {
            var alphabet = _config.GetSection("Base26")["alphabet"];
            var seedValue = CodeConverter.ConvertFromCode(stringValue, alphabet);

            try
            {
                await _connection.OpenAsync();

                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.State FROM Codes
                                            WHERE Codes.SeedValue = @seedValue";

                    command.Parameters.AddWithValue("@seedValue", seedValue);

                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            var code = new CodeViewModel();
                            code.Id = (int) reader["ID"];
                            code.State = CodeStateSerializer.DeserializeState((byte) reader["State"]);
                            code.StringValue = CodeConverter.ConvertToCode(seedValue, alphabet);
                            
                            return Ok(code);
                        }

                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            
            finally
            {
                await _connection.CloseAsync();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Deactivate([FromBody]string[] codes)
        {
            var alphabet = _config.GetSection("Base26")["alphabet"];
            var now = DateTime.Now;

            foreach(var code in codes)
            {
                var seedValue = CodeConverter.ConvertFromCode(code, alphabet);
                var codeToDeactivate = await _codeRepository.GetDeactivatingAsync(seedValue);
                codeToDeactivate.Deactivate("user", now);
                await _codeRepository.UpdateAsync(codeToDeactivate);
            }

            return Ok();
        }

        [HttpPost("/redeem-code")]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            var alphabet = _config.GetSection("Base26")["alphabet"];
            var seedValue = CodeConverter.ConvertFromCode(value, alphabet);
            var code = await _codeRepository.GetRedeemingAsync(seedValue);

            if(code.Id != 0)
            {
                code.Redeem("user", DateTime.Now);

                await _codeRepository.UpdateAsync(code);

                return Ok();
            }
            return BadRequest();               
        }
    }
}
