using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Tests.SetUp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ContractRepositoryTests
    {
        [TestMethod]
        public async Task ExampleCreateContractTestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);
            var contract = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = "Test",
                ContractVersion = 1,
                Year = "2021"
            };

            //Act
            var before = await repo.GetByIdAsync(1);
            await contractRepo.ExampleCreate(contract);
            var after = await repo.GetByIdAsync(1);

            //Assert
            before.Should().BeNull();
            after.Should().BeEquivalentTo(contract);
        }
    }
}