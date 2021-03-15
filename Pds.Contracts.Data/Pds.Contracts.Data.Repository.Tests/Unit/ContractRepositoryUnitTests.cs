﻿using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractRepositoryUnitTests
    {
        private readonly Mock<ILoggerAdapter<ContractRepository>> _mockLogger = new Mock<ILoggerAdapter<ContractRepository>>();

        [TestMethod]
        public async Task GetAsyncTestAsync()
        {
            //Arrange
            var expected = new Contract { Id = 1 };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetAsync(expected.Id);

            //Assert
            actual.Should().Be(expected);
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersionAsyncTestAsync()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1 };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByPredicateAsync(It.IsAny<Expression<Func<Contract, bool>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetByContractNumberAndVersionAsync(expected.ContractNumber, expected.ContractVersion);

            //Assert
            actual.Should().Be(expected);
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetByContractNumberAsyncTestAsync()
        {
            //Arrange
            const string ContractNumber = "expected-contract-number";
            var expected = new List<Contract>
            {
                new Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1 },
                new Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2 },
                new Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3 },
                new Contract { Id = 4, ContractNumber = ContractNumber, ContractVersion = 4 },
            };
            var mockDbSet = new DbSetMock<Contract>(expected, (c, _) => c.Id, true);
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetByContractNumberAsync(ContractNumber);

            //Assert
            actual.Should().BeEquivalentTo(expected);
            Mock.Get(mockRepo)
                .Verify(r => r.GetMany(c => c.ContractNumber == ContractNumber), Times.Once);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedResultTest()
        {
            //Arrange
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            DateTime currentDateTimeMinusNumberOfDays = DateTime.UtcNow.Date.AddDays(-reminderInterval).AddHours(23).AddMinutes(59);
            const string ContractNumber = "expected-contract-number";
            var createdAt = DateTime.UtcNow.AddDays(-30);
            var lastEmailReminderSent = DateTime.UtcNow.AddDays(-15);

            var contracts = new List<Contract>()
            {
                new Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 2, ContractNumber = ContractNumber, ContractVersion = 2, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 3, ContractNumber = ContractNumber, ContractVersion = 3, LastEmailReminderSent = lastEmailReminderSent, Status = 0, FundingType = 1, CreatedAt = createdAt },
                new Contract { Id = 4, ContractNumber = ContractNumber, ContractVersion = 4, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
            };

            var contractsExpected = new List<Contract>()
            {
                new Contract { Id = 1, ContractNumber = ContractNumber, ContractVersion = 1, LastEmailReminderSent = null, Status = 0, FundingType = 1, CreatedAt = createdAt },
            };
            var pagedListExpected = new PagedList<Contract>(contractsExpected, 4, pageNumber, pageSize);

            var mockDbSet = new DbSetMock<Contract>(contracts, (c, _) => c.Id, true);

            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(mockDbSet.Object);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetContractRemindersAsync(currentDateTimeMinusNumberOfDays, pageNumber, pageSize, sort, order);

            //Assert
            actual.Should().BeEquivalentTo(pagedListExpected);
            Mock.Get(mockRepo)
                .Verify(r => r.GetMany(q => ((q.LastEmailReminderSent == null && currentDateTimeMinusNumberOfDays >= q.CreatedAt) || (q.LastEmailReminderSent != null && currentDateTimeMinusNumberOfDays >= q.LastEmailReminderSent)) && q.Status == (int)ContractStatus.PublishedToProvider), Times.Exactly(1));
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAt_ReturnsExpectedResult_TestAsync()
        {
            //Arrange
            int searchContractId = 1;
            int contractId = 1;
            DateTime updatedDate = DateTime.UtcNow;
            var dummyContract = new Contract { Id = contractId, ContractNumber = "expected-contract-number", ContractVersion = 1, LastEmailReminderSent = null, LastUpdatedAt = updatedDate };

            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            Mock.Get(mockUnitOfWork)
                .Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByIdAsync(searchContractId))
                .ReturnsAsync(dummyContract);

            SetMockLogger();

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var result = await contractRepo.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(searchContractId);

            //Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(searchContractId), Times.Once);
            dummyContract.LastEmailReminderSent.Should().NotBeNull();
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Once);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAt_ReturnsNullResult_TestAsync()
        {
            //Arrange
            int searchContractId = 99;
            DateTime updatedDate = DateTime.UtcNow;
            Contract dummyContract = null;

            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            Mock.Get(mockUnitOfWork)
                .Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByIdAsync(searchContractId))
                .ReturnsAsync(dummyContract);

            SetMockErrorLogger();

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var result = await contractRepo.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(searchContractId);

            //Assert
            result.Should().BeNull();
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(searchContractId), Times.Once);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Never);
            _mockLogger.VerifyAll();
        }

        private void SetMockLogger()
        {
            _mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetMockErrorLogger()
        {
            _mockLogger
                .Setup(logger => logger.LogError(It.IsAny<string>()))
                .Verifiable();
        }
    }
}