﻿using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var expected = new Contract
            {
                Id = 1,
                ContractContent = new ContractContent
                {
                    FileName = "testfilename"
                }
            };

            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
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
        public async Task GetByContractNumberVersionAndUkprnAsyncTestAsync()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, Ukprn = 12345678 };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByPredicateAsync(It.IsAny<Expression<Func<Contract, bool>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetContractAsync(expected.ContractNumber, expected.ContractVersion, expected.Ukprn);

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

        [TestMethod]
        public async Task UpdateContractStatusAsync_ReturnsExpectedResult_TestAsync()
        {
            //Arrange
            ContractStatus requiredContractStatus = ContractStatus.ApprovedWaitingConfirmation;
            ContractStatus newContractStatus = ContractStatus.Approved;
            int searchContractId = 1;
            int contractId = 1;
            DateTime expectedUpdatedDate = DateTime.UtcNow;
            DateTime lastUpdatedDate = expectedUpdatedDate.AddDays(40);
            var dummyContract = new Contract { Id = contractId, ContractNumber = "expected-contract-number", ContractVersion = 1, Status = (int)requiredContractStatus, LastUpdatedAt = lastUpdatedDate };

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
            var result = await contractRepo.UpdateContractStatusAsync(searchContractId, requiredContractStatus, newContractStatus);

            //Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(searchContractId), Times.Once);
            dummyContract.Status.Should().Be((int)newContractStatus);
            dummyContract.LastUpdatedAt.Date.Should().BeSameDateAs(expectedUpdatedDate.Date);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Once);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void UpdateContractConfirmApprovalAsync_ReturnsContractStatusExceptionResult_Test()
        {
            //Arrange
            ContractStatus requiredContractStatus = ContractStatus.ApprovedWaitingConfirmation;
            ContractStatus newContractStatus = ContractStatus.Approved;
            int searchContractId = 1;
            int contractId = 1;
            DateTime lastUpdatedDate = DateTime.UtcNow;
            var dummyContract = new Contract { Id = contractId, ContractNumber = "expected-contract-number", ContractVersion = 1, Status = (int)ContractStatus.WithdrawnByAgency, LastUpdatedAt = lastUpdatedDate };

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
            Func<Task> act = async () => await contractRepo.UpdateContractStatusAsync(searchContractId, requiredContractStatus, newContractStatus);

            //Assert
            act.Should().ThrowAsync<ContractStatusException>();
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(searchContractId), Times.Once);
            dummyContract.Status.Should().Be((int)ContractStatus.WithdrawnByAgency);
            dummyContract.LastUpdatedAt.Should().BeSameDateAs(lastUpdatedDate);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Never);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void UpdateContractConfirmApprovalAsync_ReturnsContractNotFoundExceptionResult_Test()
        {
            //Arrange
            ContractStatus requiredContractStatus = ContractStatus.ApprovedWaitingConfirmation;
            ContractStatus newContractStatus = ContractStatus.Approved;
            int searchContractId = 1;
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
            Func<Task> act = async () => await contractRepo.UpdateContractStatusAsync(searchContractId, requiredContractStatus, newContractStatus);

            //Assert
            act.Should().ThrowAsync<ContractNotFoundException>();
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(searchContractId), Times.Once);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Never);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void UpdateContractAsync_IsTrackedExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 } };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            Mock.Get(mockUnitOfWork)
               .Setup(u => u.IsTracked(expected))
               .Returns(true)
               .Verifiable();
            Mock.Get(mockUnitOfWork)
                .Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()))
                .Returns(Task.CompletedTask);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            Func<Task> act = async () => await contractRepo.UpdateContractAsync(expected);

            //Assert
            act.Should().NotThrowAsync();
            Mock.Get(mockUnitOfWork).Verify(u => u.IsTracked(expected), Times.Once);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Once);
            Mock.Get(mockRepo).Verify(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()), Times.Never);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void UpdateContractAsync_ContractUpdateConcurrencyExceptionResultTest()
        {
            //Arrange
            var updatedAt = DateTime.UtcNow.AddSeconds(-1);
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, LastUpdatedAt = updatedAt, ContractContent = new ContractContent() { Id = 1 } };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            Mock.Get(mockUnitOfWork)
               .Setup(u => u.IsTracked(expected))
               .Returns(true)
               .Verifiable();
            Mock.Get(mockUnitOfWork)
                .Setup(u => u.CommitAsync())
                .Throws(new DbUpdateConcurrencyException())
                .Verifiable();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);
            Mock.Get(mockRepo)
                .Setup(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()))
                .Returns(Task.CompletedTask);
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            expected.LastUpdatedAt = DateTime.UtcNow;

            //Act
            Func<Task> act = async () => await contractRepo.UpdateContractAsync(expected);

            //Assert
            var result = act.Should().ThrowAsync<ContractUpdateConcurrencyException>();
            Mock.Get(mockUnitOfWork).Verify(u => u.IsTracked(expected), Times.Once);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Once);
            Mock.Get(mockRepo).Verify(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()), Times.Never);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void UpdateContractAsync_NotTrackedExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 } };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            Mock.Get(mockUnitOfWork)
               .Setup(u => u.IsTracked(expected))
               .Returns(false)
               .Verifiable();
            Mock.Get(mockUnitOfWork)
                .Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()))
                .Returns(Task.CompletedTask);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            Func<Task> act = async () => await contractRepo.UpdateContractAsync(expected);

            //Assert
            act.Should().NotThrowAsync();
            Mock.Get(mockUnitOfWork).Verify(u => u.IsTracked(expected), Times.Once);
            Mock.Get(mockUnitOfWork).Verify(u => u.CommitAsync(), Times.Never);
            Mock.Get(mockRepo).Verify(r => r.PatchAsync(It.IsAny<int>(), It.IsAny<Contract>()), Times.Once);
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetContractWithContentAndDatasAsync_ReturnsExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 } };
            expected.ContractContent = new ContractContent();
            expected.ContractData = new ContractData();
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetContractWithContentAndDatasAsync(expected.Id);

            //Assert
            actual.Should().Be(expected);
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetContractWithDatasAsync_ReturnsExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 } };
            expected.ContractData = new ContractData();
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetContractWithDatasAsync(expected.Id);

            //Assert
            actual.Should().Be(expected);
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersionWithIncludesAsync_Datas_ReturnsExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1 };
            expected.ContractData = new ContractData();
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(expected.ContractNumber, expected.ContractVersion, ContractDataEntityInclude.Datas);

            //Assert
            actual.Should().Be(expected);
            actual.ContractContent.Should().BeNull();
            actual.ContractData.Should().NotBeNull();
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersionWithIncludesAsync_Content_ReturnsExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 } };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(expected.ContractNumber, expected.ContractVersion, ContractDataEntityInclude.Content);

            //Assert
            actual.Should().Be(expected);
            actual.ContractContent.Should().NotBeNull();
            actual.ContractData.Should().BeNull();
            Mock.Get(mockRepo).VerifyAll();
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersionWithIncludesAsync_DataAndContent_ReturnsExpectedResultTest()
        {
            //Arrange
            var expected = new Contract { Id = 1, ContractNumber = "expected-contract-number", ContractVersion = 1, ContractContent = new ContractContent() { Id = 1 }, ContractData = new ContractData() { Id = 1 } };
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(MockBehavior.Strict);
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetFirstOrDefault(It.IsAny<Expression<Func<Contract, bool>>>(), It.IsAny<Func<IQueryable<Contract>, IIncludableQueryable<Contract, object>>>()))
                .ReturnsAsync(expected);

            //Act
            var contractRepo = new ContractRepository(mockRepo, mockUnitOfWork, _mockLogger.Object);
            var actual = await contractRepo.GetByContractNumberAndVersionWithIncludesAsync(expected.ContractNumber, expected.ContractVersion, ContractDataEntityInclude.Content | ContractDataEntityInclude.Content);

            //Assert
            actual.Should().Be(expected);
            actual.ContractContent.Should().NotBeNull();
            actual.ContractData.Should().NotBeNull();
            Mock.Get(mockRepo).VerifyAll();
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