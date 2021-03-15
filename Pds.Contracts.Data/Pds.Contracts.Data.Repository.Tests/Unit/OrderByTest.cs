using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Extensions;
using Pds.Contracts.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class OrderByTest
    {
        [TestMethod]
        public void OrderByDynamic_LastUpdatedAt_Asc_Test()
        {
            //Arrange
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = mockDbSet.Object.OrderByDynamic(sort.ToString(), order).ToList();

            //Assert
            result.FirstOrDefault().Id.Should().Be(1);
        }

        [TestMethod]
        public void OrderByDynamic_LastUpdatedAt_Desc_Test()
        {
            //Arrange
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Desc;
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = mockDbSet.Object.OrderByDynamic(sort.ToString(), order).ToList();

            //Assert
            result.FirstOrDefault().Id.Should().Be(20);
        }

        [TestMethod]
        public void OrderByDynamic_UKPRN_Asc_Test()
        {
            //Arrange
            ContractSortOptions sort = ContractSortOptions.UKPRN;
            SortDirection order = SortDirection.Asc;
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = mockDbSet.Object.OrderByDynamic(sort.ToString(), order).ToList();

            //Assert
            result.FirstOrDefault().Id.Should().Be(11);
        }

        [TestMethod]
        public void OrderByDynamic_UKPRN_Desc_Test()
        {
            //Arrange
            ContractSortOptions sort = ContractSortOptions.UKPRN;
            SortDirection order = SortDirection.Desc;
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = mockDbSet.Object.OrderByDynamic(sort.ToString(), order).ToList();

            //Assert
            result.FirstOrDefault().Id.Should().Be(10);
        }

        private List<Contract> GetContracts()
        {
            const string ContractNumber = "expected-contract-number";
            var createdAt = DateTime.UtcNow.AddDays(-30);
            var lastEmailReminderSent = DateTime.UtcNow.AddDays(-15);
            var lastUpdatedAt = DateTime.UtcNow;

            return new List<Contract>()
            {
                new Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt, Ukprn = 10000010 },
                new Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(1), Ukprn = 10000011 },
                new Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(2), Ukprn = 10000012 },
                new Contract { Id = 4, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(3), Ukprn = 10000013 },
                new Contract { Id = 5, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(4), Ukprn = 10000014 },
                new Contract { Id = 6, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(5), Ukprn = 10000015 },
                new Contract { Id = 7, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(6), Ukprn = 10000016 },
                new Contract { Id = 8, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(7), Ukprn = 10000017 },
                new Contract { Id = 9, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(8), Ukprn = 10000018 },
                new Contract { Id = 10, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(9), Ukprn = 10000019 },
                new Contract { Id = 11, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(10), Ukprn = 10000000 },
                new Contract { Id = 12, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(11), Ukprn = 10000001 },
                new Contract { Id = 13, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(12), Ukprn = 10000002 },
                new Contract { Id = 14, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(13), Ukprn = 10000003 },
                new Contract { Id = 15, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(14), Ukprn = 10000004 },
                new Contract { Id = 16, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(15), Ukprn = 10000005 },
                new Contract { Id = 17, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(16), Ukprn = 10000006 },
                new Contract { Id = 18, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(17), Ukprn = 10000007 },
                new Contract { Id = 19, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(18), Ukprn = 10000008 },
                new Contract { Id = 20, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt, LastUpdatedAt = lastUpdatedAt.AddDays(19), Ukprn = 10000009 },
            };
        }
    }
}