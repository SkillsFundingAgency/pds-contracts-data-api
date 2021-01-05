using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Implementations;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass]
    public class ContractServiceTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task GetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<Contract>();

            //Set up
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
    }
}