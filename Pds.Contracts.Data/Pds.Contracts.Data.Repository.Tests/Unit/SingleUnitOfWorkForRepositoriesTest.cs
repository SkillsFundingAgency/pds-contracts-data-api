using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Repository.Implementations;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class SingleUnitOfWorkForRepositoriesTest
    {
        [TestMethod]
        public async Task CommitAsyncTestAsync()
        {
            //Arrange
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            //Act
            var repo = new SingleUnitOfWorkForRepositories(mockDbContext);
            await repo.CommitAsync();

            //Assert
            Mock.Get(mockDbContext)
                .Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}