using System;
using System.Linq;
using System.Threading.Tasks;
using CodeJar.Domain;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace Domain.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task GivenANewBatchWithSize_WhenGeneratingCodes_ThenGenerateCodesWithSize()
        {
            //verify number of codes
            var batch = new Batch();
            batch.BatchSize = 10;
            var codes = await batch.GenerateCodesAsync(new SeedValueReader()).ToListAsync();
            codes.Count.Should().Be(batch.BatchSize);
        }
        
        [Fact]
        public async Task GivenTheBatchId_WhenGeneratingCodesWithABatchId_ThenGenerateCodesWithSameBatchId()
        {
            //verify number of codes
            var batch = new Batch();
            batch.BatchSize = 1;
            batch.Id = Guid.Parse("76b1c51b-c700-4b01-bb61-89ea004e64d8");
            var codes = await batch.GenerateCodesAsync(new SeedValueReader()).ToListAsync();
            codes.Select(x => x.BatchId).Should().Equal(batch.Id);
        }

        [Fact]
        public async Task GivenTheCodeIsCreated_WhenStateSetToGenerated_ThenStateEqualsGenerated()
        {
            //verify number of codes
            var seedValueReader = new SeedValueReader();
            var batch = new Batch();
            batch.BatchSize = 1;
            batch.Id = Guid.Parse("76b1c51b-c700-4b01-bb61-89ea004e64d8");
            var codes = await batch.GenerateCodesAsync(seedValueReader).ToListAsync();
            codes.First().State.Should().Be("Generated");
        }

        [Fact]
        public void GivenTheCodeIsGenerated_WhenStateIsStateIsSetToActivated_ThenStateEqualsActivated()
        {
            var code = new ActivatingCode();
            code.Activate();
            code.State.Should().Be("Active");            
        }

        [Fact]
        public void GivenTheCodeIsGenerated_WhenStateIsSetToExpired_ThenStateEqualsExpired()
        {
            var code = new ExpiringCode();
            code.Expire();
            code.State.Should().Be("Expired");
        }

        [Fact]
        public void GivenTheCodeIsActivated_WhenStateIsSetToRedeemed_ThenStateEquaIsRedeemed()
        {
            var code = new RedeemingCode();
            var date = new DateTime();
            code.Redeem("user", date);
            code.State.Should().Be("Redeemed");
        }

        [Fact]
        public void GivenTheCodeIsActive_WhenStateIsSetToDeactivate_ThenStateEqualsIDeactivated()
        {
            var code = new DeactivatingCode();
            var date = new DateTime();
            code.Deactivate("user", date);
            code.State.Should().Be("Inactive");
        }

        [Fact]
        public void GivenSeedValue_WhenConvertToCodeAlgorithmRun_ThenSeedValueShouldBeCode()
        {
            var result = CodeConverter.ConvertToCode(1, "AB", 0);

            result.Should().Be("B");
        }

        [Fact]
        public void GivenCode_WheConvertFromCodeAlgorithmRun_ThenCodeShouldBeSeedValue()
        {
            var result = CodeConverter.ConvertFromCode(code: "B", alphabet: "AB");

            result.Should().Be(1);
        }
    }

    public class SeedValueReader : ISeedValueReader
    {
        public Task<List<int>> ReadSeedValuesAsync(int count)
        {
            return Task.FromResult(Enumerable.Range(0, count).ToList());
        }
    }
}
