using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
        [TestMethod]
        public async Task GetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<Contract>();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            var contractService = new ContractService(mockRepo, mockMapper);

            // Act
            var actual = await contractService.GetAsync(1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<Contract>()), Times.Once);
        }

        [TestMethod]
        public void GetByContractNumber_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModelCollection = Mock.Of<IList<Models.Contract>>();
            var expectedDataModelCollection = Mock.Of<IQueryable<Contract>>();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(expectedDataModelCollection)
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<Contract>>()))
                .Returns(expectedServiceModelCollection)
                .Verifiable();

            var contractService = new ContractService(mockRepo, mockMapper);

            // Act
            var actual = contractService.GetByContractNumber("some-contract-number");

            // Assert
            actual.Should().BeEquivalentTo(expectedServiceModelCollection);
            Mock.Get(mockRepo).Verify(r => r.GetMany(It.IsAny<Expression<Func<Contract, bool>>>()), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<Contract>>()), Times.Once);
        }

        [TestMethod]
        public void GetByContractNumberAndVersion_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<Contract>();
            var mockRepo = Mock.Of<IRepository<Contract>>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByPredicate(It.IsAny<Expression<Func<Contract, bool>>>()))
                .Returns(expectedDataModel)
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            var contractService = new ContractService(mockRepo, mockMapper);

            // Act
            var actual = contractService.GetByContractNumberAndVersion("some-contract-number", 1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetByPredicate(It.IsAny<Expression<Func<Contract, bool>>>()), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<Contract>()), Times.Once);
        }
    }
}