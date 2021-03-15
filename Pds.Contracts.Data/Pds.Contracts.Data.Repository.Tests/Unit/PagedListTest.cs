using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
    public class PagedListTest
    {
        [TestMethod]
        public async Task PagedList_FirstPageTest()
        {
            //Arrange
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = await mockDbSet.Object.ToPagedList(1, 5);

            //Assert
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeTrue();
            result.PageSize.Should().Be(5);
            result.PageSize.Should().Be(result.Items.Count());
            result.NextPageNumber.Should().Be(2);
            result.PreviousPageNumber.Should().BeNull();
            result.TotalCount.Should().Be(20);
        }

        [TestMethod]
        public async Task PagedList_SecondPageTest()
        {
            //Arrange
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = await mockDbSet.Object.ToPagedList(2, 5);

            //Assert
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
            result.PageSize.Should().Be(5);
            result.PageSize.Should().Be(result.Items.Count());
            result.NextPageNumber.Should().Be(3);
            result.PreviousPageNumber.Should().Be(1);
            result.TotalCount.Should().Be(20);
        }

        [TestMethod]
        public async Task PagedList_LastPageTest()
        {
            //Arrange
            var contracts = GetContracts().AsQueryable();

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var result = await mockDbSet.Object.ToPagedList(4, 5);

            //Assert
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeFalse();
            result.PageSize.Should().Be(5);
            result.PageSize.Should().Be(result.Items.Count());
            result.NextPageNumber.Should().BeNull();
            result.PreviousPageNumber.Should().Be(3);
            result.TotalCount.Should().Be(20);
        }

        private List<Contract> GetContracts()
        {
            const string ContractNumber = "expected-contract-number";
            var createdAt = DateTime.UtcNow.AddDays(-30);
            var lastEmailReminderSent = DateTime.UtcNow.AddDays(-15);
            return new List<Contract>()
            {
                new Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 4, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 5, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 6, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 7, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 8, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 9, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 10, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 11, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 12, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 13, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 14, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 15, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 16, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 17, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 18, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 19, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 20, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
            };
        }
    }
}