using System.Threading.Tasks;
using Xunit;
using MongoDB.Driver;
using MongoDB.Bson;
using FluentAssertions;
using AutoFixture;
using System;
using System.Linq;

namespace MongoDBRepository.IntegrationTests
{
    [Collection("RepositoryTests")]
    public class BaseRepositoryTests
    {
        private TestProductsRepository repository;

        private IMongoCollection<TestProduct> mongoCollection;

        public BaseRepositoryTests()
        {
            var connectionString = Configuration.Settings["MongoSettings:TestDbConnectionString"];

            var mongoUrl = new MongoUrl(connectionString);

            var db = new MongoClient(connectionString).GetDatabase(mongoUrl.DatabaseName);

            repository = new TestProductsRepository(connectionString);

            mongoCollection = db.GetCollection<TestProduct>(repository.CollectionName);

            mongoCollection.DeleteMany(FilterDefinition<TestProduct>.Empty);
        }

        [Fact]
        public async Task InsertAsync_Shoud_Insert_A_Single_Document()
        {
            // Arrange
            var testProduct = new Fixture().Create<TestProduct>();

            // Act
            await repository.InsertAsync(testProduct);

            // Assert
            var filter = new BsonDocument("_id", testProduct.Id);

            var cursor = await mongoCollection.FindAsync(filter);

            var document = await cursor.FirstOrDefaultAsync();

            testProduct.Should().BeEquivalentTo(document);
        }

        [Fact]
        public async Task InsertManyAsync_Shoud_Insert_Multiple_Documents()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4);

            // Act
            await repository.InsertManyAsync(testProducts);

            // Assert
            var cursor = await mongoCollection.FindAsync(FilterDefinition<TestProduct>.Empty);

            var documents = await cursor.ToListAsync();

            testProducts.Should().BeEquivalentTo(documents);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Document_If_Exists()
        {
            // Arrange
            var testProduct = new Fixture().Create<TestProduct>();
            await repository.InsertAsync(testProduct);

            // Act
            var document = await repository.GetByIdAsync(testProduct.Id);

            // Assert
            testProduct.Should().BeEquivalentTo(document);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_Not_Exists()
        {
            // Act
            var document = await repository.GetByIdAsync("dummy");

            // Assert
            document.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Document_If_Exists()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4);
            await repository.InsertManyAsync(testProducts);

            // Act
            var documents = await repository.GetAllAsync();

            // Assert
            testProducts.Should().BeEquivalentTo(documents);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Document_If_Exists()
        {
            // Arrange
            var testProduct = new Fixture().Create<TestProduct>();
            await repository.InsertAsync(testProduct);

            // Act
            var deleteCount = await repository.DeleteAsync(testProduct.Id);

            // Assert
            deleteCount.Should().Be(1);

            var filter = new BsonDocument("_id", testProduct.Id);

            var cursor = await mongoCollection.FindAsync(filter);

            var document = await cursor.FirstOrDefaultAsync();

            document.Should().BeNull();
        }
    }
}
