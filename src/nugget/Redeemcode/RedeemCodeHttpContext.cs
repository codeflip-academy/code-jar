using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CodeJar.Nugget
{
    public class RedeemCode
    {
        public HttpClient Client = new HttpClient();
        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        public async Task<HttpResponseMessage> RedeemCodeAsync(string codeStringValue)
        {
            var content = JsonSerializer.Serialize(codeStringValue, _jsonOptions);
            var response = await Client.PostAsync(
                requestUri: "http://localhost:5000/redeem-code",
                content:
                    new StringContent(
                        content: content,
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    )
            );
            return response;
        }
    }
}