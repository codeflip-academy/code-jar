﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;

namespace api_tester
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Create new codeJarClient
            var codeJarClient = new CodeJarClient();
            var test = new CodeJarTests();

            var today = DateTime.Now;
            var batch = new Batch()
            {
                BatchName = "Batch",
                BatchSize = 10,
                DateActive = today,
                DateExpires = today.AddDays(30)
            };

            var batchResponse = codeJarClient.CreateBatchAsync(batch).Result;
            var batchContent = batchResponse.Content.ReadAsStringAsync().Result;
            var createdBatch = JsonSerializer.Deserialize<Batch>(batchContent, options);

            // Checking if Codes Generated State is correct.
            if (test.IsCodeStateCorrect(createdBatch).Result)
            {
                Console.WriteLine("State when generated is correct.");
            }

            // // //Checking if pagination works
            if (test.PageComparison(createdBatch).Result)
            {
                Console.WriteLine("Pagination works.");
            }

            //Testing if there are dupicate batches
            if (test.TestingForDuplicateBatch(createdBatch).Result)
            {
                Console.WriteLine("No duplicate batches.");
            }

            //Testing if the offset updates correctly.

            if (test.TestingForOffset(createdBatch).Result)
            {
                Console.WriteLine("Offset updates correctly");
            }
            //testing if you can deactivate a code
            if (test.DeactivateCode(createdBatch).Result)
            {
                Console.WriteLine("Code deactive works");
            }
            //testing if you can deactivate a batch
            if (test.DeactivateBatch(createdBatch).Result)
            {
                Console.WriteLine("Batch deactive works");
            }
            //testing if you can search
            if(test.TestForSearch(createdBatch).Result)
            {
                Console.WriteLine("search works.");
            }
        }
    }
}