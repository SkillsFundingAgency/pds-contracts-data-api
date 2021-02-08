using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Tests.SetUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ContractRepositoryIntegrationTests
    {
        [TestMethod]
        public async Task ExampleCreateContractAsync_TestAsync()
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

        [TestMethod]
        public async Task GetAsync_TestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);
            var expected = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = "Test",
                ContractVersion = 1,
                Year = "2021"
            };

            //Act
            await repo.AddAsync(expected);
            await work.CommitAsync();
            var actual = await contractRepo.GetAsync(expected.Id);

            //Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersionAsync_TestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);
            const string ContractNumber = "Test";
            var expected = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = ContractNumber,
                ContractVersion = 1,
                Year = "2021"
            };

            var initialLoad = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2 },
                expected,
                new DataModels.Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3 },
            };

            //Act
            foreach (var item in initialLoad)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();
            var actual = await contractRepo.GetByContractNumberAndVersionAsync(expected.ContractNumber, expected.ContractVersion);

            //Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public async Task GetByContractNumberAsync_TestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);
            const string ContractNumber = "Test";
            var expected = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1 },
                new DataModels.Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2 },
                new DataModels.Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3 },
            };

            //Act
            foreach (var item in expected)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            var actual = await contractRepo.GetByContractNumberAsync(ContractNumber);

            //Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedResultTest()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);

            const string contractNumber = "Test-Contract-Number";
            const string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, LastEmailReminderSent = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent.AddDays(-14), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 3, Title = title, ContractNumber = contractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent.AddDays(-15), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
            };
            var expected = working.Skip(1);

            //Act
            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            var actual = await contractRepo.GetContractRemindersAsync(DateTime.UtcNow.AddDays(-14), 1, 10, ContractSortOptions.LastUpdatedAt, SortDirection.Asc);

            //Assert
            actual.Items.Should().BeEquivalentTo(expected);
        }
    }
}