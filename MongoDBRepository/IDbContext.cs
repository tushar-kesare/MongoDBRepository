using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace MongoDBRepository
{
    public interface IMongoDbContext
    {
        IMongoClient Client { get; }

        IMongoDatabase Database { get; }

        Transaction StartTransaction(ClientSessionOptions sessionOptions = null, TransactionOptions transactionOptions = null);
    }

    public class MongoDbContext : IMongoDbContext
    {
        public IMongoClient Client { get; }

        public IMongoDatabase Database { get; }

        public MongoDbContext(string connectionString)
        {
            var mongoUrl = new MongoUrl(connectionString);

            Client = new MongoClient(mongoUrl);

            Database = Client.GetDatabase(mongoUrl.DatabaseName);
        }

        public Transaction StartTransaction(ClientSessionOptions sessionOptions = null, TransactionOptions transactionOptions = null)
        {
            if (SessionContainer.AmbientSession != null)
            {
                throw new InvalidOperationException("A transaction is already in progress. Nested transactions are not supported.");
            }

            var session = Client.StartSession(sessionOptions);

            session.StartTransaction(transactionOptions);

            SessionContainer.SetSession(session);

            return new Transaction(session, committed => SessionContainer.SetSession(null));
        }
    }

}
