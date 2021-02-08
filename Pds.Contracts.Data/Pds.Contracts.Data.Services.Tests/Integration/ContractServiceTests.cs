using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.AutoMapperProfiles;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Contracts.Data.Services.Tests.SetUp;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using DataModels = Pds.Contracts.Data.Repository.DataModels;
using ServiceModels = Pds.Contracts.Data.Services.Models;

namespace Pds.Contracts.Data.Services.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ContractServiceTests
    {
        private IMapper _mapper = null;

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedTest()
        {
            //Arrange
            SetMapperHelper();
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            string baseUrl = $"https://localhost:5001";
            string routeTemplateUrl = $"/api/contractReminders?reminderInterval={reminderInterval}&pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}&order={order}";
            const string contractNumber = "Test-Contract-Number";
            const string title = "Test Title";
            DateTime lastEmailReminderSent = DateTime.UtcNow;

            var working = new List<DataModels.Contract>
            {
                new DataModels.Contract { Id = 1, Title = title, ContractNumber = contractNumber, ContractVersion = 1, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent, Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent.AddDays(-14), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
                new DataModels.Contract { Id = 3, Title = title, ContractNumber = contractNumber, ContractVersion = 3, Ukprn = 12345678, LastEmailReminderSent = lastEmailReminderSent.AddDays(-15), Status = (int)ContractStatus.PublishedToProvider, FundingType = (int)ContractFundingType.AdvancedLearnerLoans },
            };

            var expectedList = new List<ContractReminderItem>
            {
                new ContractReminderItem { Id = 2, Title = title, ContractNumber = contractNumber, ContractVersion = 2, Ukprn = 12345678, Status = ContractStatus.PublishedToProvider, FundingType = ContractFundingType.AdvancedLearnerLoans }
            };

            var expected = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(expectedList)
            {
                Paging = new Metadata()
                {
                    CurrentPage = pageNumber,
                    HasNextPage = true,
                    HasPreviousPage = false,
                    NextPageUrl = baseUrl + $"/api/contractReminders?reminderInterval={reminderInterval}&pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}&order={order}",
                    PageSize = pageSize,
                    PreviousPageUrl = string.Empty,
                    TotalCount = 2,
                    TotalPages = 2
                }
            };

            ILoggerAdapter<ContractService> logger = new LoggerAdapter<ContractService>(new Logger<ContractService>(new LoggerFactory()));

            var inMemPdsDbContext = HelperExtensions.GetInMemoryPdsDbContext();
            var repo = new Repository<DataModels.Contract>(inMemPdsDbContext);
            var work = new SingleUnitOfWorkForRepositories(inMemPdsDbContext);
            var contractRepo = new ContractRepository(repo, work);
            var uriService = new UriService(baseUrl);
            var service = new ContractService(contractRepo, _mapper, uriService, logger);

            //Act
            foreach (var item in working)
            {
                await repo.AddAsync(item);
            }

            await work.CommitAsync();

            var result = await service.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, routeTemplateUrl);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Set the mapper config.
        /// </summary>
        private void SetMapperHelper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ContractMapperProfile());
            });

            _mapper = mapperConfig.CreateMapper();
        }
    }
}