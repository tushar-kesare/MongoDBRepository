using AutoFixture;
using FluentAssertions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MongoDBRepository.IntegrationTests
{
    [Collection("RepositoryTests")]
    public class TransactionsTests
    {
        private TestProductsRepository repository;

        private IMongoCollection<TestProduct> mongoCollection;

        public TransactionsTests()
        {
            var connectionString = Configuration.Settings["MongoSettings:TestDbConnectionString"];

            var mongoUrl = new MongoUrl(connectionString);

            var db = new MongoClient(connectionString).GetDatabase(mongoUrl.DatabaseName);

            repository = new TestProductsRepository(connectionString);

            mongoCollection = db.GetCollection<TestProduct>(repository.CollectionName);

            mongoCollection.DeleteMany(FilterDefinition<TestProduct>.Empty);
        }

        [Fact]
        public async Task Transaction_If_Committed_DataIsSaved()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4).ToList();

            // Act
            using (var transaction = repository.StartTransaction())
            {
                await repository.InsertAsync(testProducts[0]);

                await repository.InsertAsync(testProducts[1]);

                await repository.InsertAsync(testProducts[2]);

                await repository.InsertAsync(testProducts[3]);

                await transaction.CommitAsync();
            }

            // Assert
            var cursor = await mongoCollection.FindAsync(FilterDefinition<TestProduct>.Empty);

            var documents = await cursor.ToListAsync();

            testProducts.Should().BeEquivalentTo(documents);
        }

        [Fact]
        public async Task Transaction_If_Not_Committed_DataIsNotSaved()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4).ToList();

            // Act
            using (var transaction = repository.StartTransaction())
            {
                await repository.InsertAsync(testProducts[0]);

                await repository.InsertAsync(testProducts[1]);

                await repository.InsertAsync(testProducts[2]);

                await repository.InsertAsync(testProducts[3]);
            }

            // Assert
            var cursor = await mongoCollection.FindAsync(FilterDefinition<TestProduct>.Empty);

            var documents = await cursor.ToListAsync();

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task Transaction_If_Aborted_DataIsNotSaved()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4).ToList();

            // Act
            using (var transaction = repository.StartTransaction())
            {
                await repository.InsertAsync(testProducts[0]);

                await repository.InsertAsync(testProducts[1]);

                await repository.InsertAsync(testProducts[2]);

                await repository.InsertAsync(testProducts[3]);

                await transaction.AbortAsync();
            }

            // Assert
            var cursor = await mongoCollection.FindAsync(FilterDefinition<TestProduct>.Empty);

            var documents = await cursor.ToListAsync();

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task Transaction_If_Exception_Before_Commit_DataIsNotSaved()
        {
            // Arrange
            var testProducts = new Fixture().CreateMany<TestProduct>(4).ToList();

            // Act
            try
            {
                using (var transaction = repository.StartTransaction())
                {
                    await repository.InsertAsync(testProducts[0]);

                    await repository.InsertAsync(testProducts[1]);

                    await repository.InsertAsync(testProducts[2]);

                    await repository.InsertAsync(testProducts[3]);

                    throw new InvalidOperationException();
                }

            }
            catch (Exception)
            {
            }

            // Assert
            var cursor = await mongoCollection.FindAsync(FilterDefinition<TestProduct>.Empty);

            var documents = await cursor.ToListAsync();

            documents.Should().BeEmpty();
        }
    }
}
