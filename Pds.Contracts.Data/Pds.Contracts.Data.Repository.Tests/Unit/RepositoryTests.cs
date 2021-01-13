using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Tests.SetUp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class RepositoryTests
    {
        [TestMethod]
        public async Task AddAsyncTestAsync()
        {
            //Arrange
            var dummyModel = new DummyDataModel { Id = 1, ATableColumn = "expected" };
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.AddAsync(dummyModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(EntityEntry<DummyDataModel>));

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            await repo.AddAsync(dummyModel);

            //Assert
            Mock.Get(mockDbContext)
                .Verify(db => db.AddAsync(dummyModel, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public void GetAllTest()
        {
            //Arrange
            var dummyCollection = new List<DummyDataModel>
            {
                 new DummyDataModel { Id = 1, ATableColumn = "expected 1" },
                 new DummyDataModel { Id = 2, ATableColumn = "expected 2" }
            };

            var mockDbSet = dummyCollection.GetMockDbSet();
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(mockDbSet.Object);

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            var result = repo.GetAll();

            //Assert
            result.Should().BeEquivalentTo(dummyCollection);
            Mock.Get(mockDbContext)
                .Verify(db => db.Set<DummyDataModel>(), Times.Once);
        }

        [TestMethod]
        public async Task GetByIdAsyncTestAsync()
        {
            //Arrange
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.FindAsync<DummyDataModel>(1))
                .ReturnsAsync(default(DummyDataModel));

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            _ = await repo.GetByIdAsync(1);

            //Assert
            Mock.Get(mockDbContext)
                .Verify(db => db.FindAsync<DummyDataModel>(1), Times.Once);
        }

        [TestMethod]
        public void GetByPredicateTest()
        {
            //Arrange
            var expectation = new DummyDataModel { Id = 1, ATableColumn = "expected" };
            var dummyCollection = new List<DummyDataModel>
            {
                 expectation,
                 new DummyDataModel { Id = 2, ATableColumn = "not expected" }
            };

            var mockDbSet = dummyCollection.GetMockDbSet();
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(mockDbSet.Object);

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            var actual = repo.GetByPredicate(dummy => dummy.Id == 1);

            //Assert
            actual.Should().Be(expectation);
            Mock.Get(mockDbContext).Verify();
        }

        [TestMethod]
        public void GetByPredicateReturnsNullTest()
        {
            //Arrange
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(default(DbSet<DummyDataModel>));

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            var actual = repo.GetByPredicate(dummy => dummy.Id == 1);

            //Assert
            actual.Should().BeNull();
            Mock.Get(mockDbContext)
                .Verify(db => db.Set<DummyDataModel>(), Times.Once);
        }

        [TestMethod]
        public void GetManyTest()
        {
            //Arrange
            var expectation = new List<DummyDataModel>
            {
                new DummyDataModel { Id = 1, ATableColumn = "exactly-expected 1" },
                new DummyDataModel { Id = 2, ATableColumn = "exactly-expected 2" }
            };

            var dummyCollection = new List<DummyDataModel>
            {
                new DummyDataModel { Id = 1, ATableColumn = "exactly-expected 1" },
                new DummyDataModel { Id = 2, ATableColumn = "exactly-expected 2" },
                new DummyDataModel { Id = 3, ATableColumn = "un-expected 3" }
            };

            var mockDbSet = dummyCollection.GetMockDbSet();
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(mockDbSet.Object);

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            var actual = repo.GetMany(dummy => dummy.ATableColumn.Contains("exactly-expected"));

            //Assert
            actual.Should().BeEquivalentTo(expectation);
            Mock.Get(mockDbContext).Verify();
        }

        [TestMethod]
        public void GetManyReturnsNullTest()
        {
            //Arrange
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(default(DbSet<DummyDataModel>));

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            var actual = repo.GetMany(dummy => dummy.ATableColumn.Contains("exactly-expected"));

            //Assert
            actual.Should().BeNull();
            Mock.Get(mockDbContext)
                .Verify(db => db.Set<DummyDataModel>(), Times.Once);
        }

        [TestMethod]
        public void UpdateTest()
        {
            //Arrange
            var dummyModel = new DummyDataModel();
            var mockDbSet = Mock.Of<DbSet<DummyDataModel>>();
            var mockDbContext = Mock.Of<DbContext>(MockBehavior.Strict);
            Mock.Get(mockDbContext)
                .Setup(db => db.Set<DummyDataModel>())
                .Returns(mockDbSet);

            //Act
            var repo = new Repository<DummyDataModel>(mockDbContext);
            repo.Update(dummyModel);

            //Assert
            Mock.Get(mockDbContext).Verify();
            Mock.Get(mockDbSet)
                .Verify(e => e.Update(dummyModel), Times.Once);
        }
    }
}