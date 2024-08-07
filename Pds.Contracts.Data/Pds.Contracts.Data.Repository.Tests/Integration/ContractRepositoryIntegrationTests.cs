using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Tests.SetUp;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ContractRepositoryIntegrationTests
    {
        private readonly ILoggerAdapter<ContractRepository> _logger = new LoggerAdapter<ContractRepository>(new Logger<ContractRepository>(new LoggerFactory()));

        [TestMethod]
        public async Task ExampleCreateContractAsync_TestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, _logger);
            var contract = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = "Test",
                ContractVersion = 1,
                Year = "2021",
                Title = "Mock"
            };

            //Act
            var before = await repo.GetByIdAsync(1);
            await contractRepo.CreateAsync(contract);
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
            var contractRepo = new ContractRepository(repo, work, _logger);
            var expected = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = "Test",
                ContractVersion = 1,
                Year = "2021",
                Title = "Mock"
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
            var contractRepo = new ContractRepository(repo, work, _logger);
            const string ContractNumber = "Test";
            var expected = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = ContractNumber,
                ContractVersion = 1,
                Year = "2021",
                Title = "Mock"
            };

            var initialLoad = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, Title = "Mock", Year = "2021" },
                expected,
                new DataModels.Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, Title = "Mock", Year = "2021" },
            };

            foreach (var item in initialLoad)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
            var actual = await contractRepo.GetByContractNumberAndVersionAsync(expected.ContractNumber, expected.ContractVersion);

            //Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public async Task GetByContractNumberVersionAndUkprnAsync_TestAsync()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, _logger);
            const string ContractNumber = "Test";
            var expected = new DataModels.Contract
            {
                Id = 1,
                ContractNumber = ContractNumber,
                ContractVersion = 1,
                Year = "2021",
                Ukprn = 12345678,
                Title = "Mock",
            };

            var initialLoad = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, Title = "Mock", Year = "2021" },
                expected,
                new DataModels.Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, Title = "Mock", Year = "2021" },
            };

            foreach (var item in initialLoad)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
            var actual = await contractRepo.GetContractAsync(expected.ContractNumber, expected.ContractVersion, expected.Ukprn);

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
            var contractRepo = new ContractRepository(repo, work, _logger);
            const string ContractNumber = "Test";
            var expected = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1, Title = "Mock", Year = "2021" },
                new DataModels.Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, Title = "Mock", Year = "2021" },
                new DataModels.Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, Title = "Mock", Year = "2021" },
            };

            foreach (var item in expected)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
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
            var contractRepo = new ContractRepository(repo, work, _logger);

            const string contractNumber = "Test-Contract-Number";
            const string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, LastEmailReminderSent = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
                new DataModels.Contract { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent.AddDays(-14), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
                new DataModels.Contract { Id = 3, Title = title, ContractNumber = contractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent.AddDays(-15), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans, Year = "2021" },
            };
            var expected = working.Skip(1);

            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            //Act
            var actual = await contractRepo.GetContractRemindersAsync(DateTime.UtcNow.AddDays(-14), 1, 10, ContractSortOptions.LastUpdatedAt, SortDirection.Asc);

            //Assert
            actual.Items.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAtAsync_Test()
        {
            //Arrange
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, _logger);

            int contractId = 1;
            const string contractNumber = "main-0001";
            const string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = contractId, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, LastEmailReminderSent = null, LastUpdatedAt = lastEmailReminderSent, Year = "2021" }
            };

            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            await contractRepo.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(contractId);

            //Act
            var actual = await contractRepo.GetAsync(contractId);

            //Assert
            actual.LastEmailReminderSent.Should().NotBeNull();
        }

        [TestMethod]
        public async Task UpdateContractConfirmApprovalAsync_Test()
        {
            //Arrange
            ContractStatus requiredContractStatus = ContractStatus.ApprovedWaitingConfirmation;
            ContractStatus newContractStatus = ContractStatus.Approved;
            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work, _logger);

            int contractId = 1;
            const string contractNumber = "main-0001";
            const string title = "Test Title";
            DateTime expectedUpdatedDate = DateTime.UtcNow;
            DateTime lastUpdatedDate = expectedUpdatedDate.AddDays(40);

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = contractId, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, Status = (int)requiredContractStatus, LastUpdatedAt = lastUpdatedDate, Year = "2021" }
            };

            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            await contractRepo.UpdateContractStatusAsync(contractId, requiredContractStatus, newContractStatus);

            //Act
            var actual = await contractRepo.GetAsync(contractId);

            //Assert
            actual.Should().NotBeNull();
            actual.Status.Should().Be((int)newContractStatus);
            actual.LastUpdatedAt.Should().BeSameDateAs(expectedUpdatedDate);
        }
    }
}