using System;
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
            JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Create new codeJarClient
            var codeJarClient = new CodeJarClient();
            var test = new CodeJarTests();
            var batch = new Batch();


            var generatedBatch = test.CreateGeneratedBatch(batch).Result;
            var activeBatchOne = test.CreateActiveBatch(batch).Result;
            var activeBatchTwo = test.CreateActiveBatch(batch).Result;
            var activeBatchThree = test.CreateActiveBatch(batch).Result;



            //Checking if pagination works
            if (test.PaginationTest(activeBatchThree).Result)
            {
                Console.WriteLine("\nPagination test SUCCESSFUL.");
            }

            //Testing if there are dupicate batches
            if (test.DuplicateBatchesTest(activeBatchThree).Result)
            {
                Console.WriteLine("DuplicateBatches test SUCCESSFUL");
            }

            //Testing if the offset updates correctly.
            if (test.OffsetTest(activeBatchThree).Result)
            {
                Console.WriteLine("\nOffset test SUCCESSFUL");
            }

            //testing if you can search
            if (test.SearchForCodeTest(activeBatchThree).Result)
            {
                Console.WriteLine("\nSearchForCode test SUCCESSFUL");
            }

            //Checking if Codes Generated State is correct.
            if (test.CodeStateTest(activeBatchThree).Result)
            {
                Console.WriteLine("CodeState test SUCCESSFUL");
            }

            //testing if you can deactivate a code
            if (test.DeactivateCodeTest(activeBatchThree).Result)
            {
                Console.WriteLine("DeactivateCode test SUCCESSFUL");
            }

            //testing if you can deactivate a batch
            if (test.DeactivateBatchTest(activeBatchTwo).Result)
            {
                Console.WriteLine("DeactiveBatch test SUCCESSFUL");
            }

            if (test.CodeStateChangeTest(generatedBatch, activeBatchOne).Result)
            {
                Console.WriteLine("Code State Change test SUCCESSFUL.");
            }
        }
    }
}