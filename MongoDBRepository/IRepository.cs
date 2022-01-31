using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBRepository
{
    public interface IRepository<TDocument> where TDocument : class
    {
        Task InsertAsync(TDocument document);

        Task InsertManyAsync(IEnumerable<TDocument> documents);

        Task<TDocument> GetByIdAsync(string id);

        Task<IEnumerable<TDocument>> GetAllAsync();

        Task<long> DeleteAsync(string id);

        Transaction StartTransaction();
    }

    public abstract class BaseMongoRepository<TDocument> : IRepository<TDocument> where TDocument : class
    {
        public abstract string CollectionName { get; }

        protected IMongoCollection<TDocument> Collection { get; }

        protected IMongoDbContext DbContext { get; }

        public BaseMongoRepository(string connectionString)
        {
            DbContext = new MongoDbContext(connectionString);

            Collection = DbContext.Database.GetCollection<TDocument>(CollectionName);
        }

        public async Task InsertAsync(TDocument document)
        {
            try
            {
                await (SessionContainer.AmbientSession != null
                    ? this.Collection.InsertOneAsync(SessionContainer.AmbientSession, document).ConfigureAwait(false)
                    : this.Collection.InsertOneAsync(document).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InsertManyAsync(IEnumerable<TDocument> documents)
        {
            try
            {
                await (SessionContainer.AmbientSession != null
                    ? this.Collection.InsertManyAsync(SessionContainer.AmbientSession, documents).ConfigureAwait(false)
                    : this.Collection.InsertManyAsync(documents).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TDocument> GetByIdAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var filter = new BsonDocument("_id", id);

            var cursor = await Collection.FindAsync(filter).ConfigureAwait(false);

            return await cursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<TDocument>> GetAllAsync()
        {
            var filter = new BsonDocument();

            var cursor = await Collection.FindAsync(filter).ConfigureAwait(false);

            return await cursor.ToListAsync().ConfigureAwait(false);
        }

        public async Task<TDocument> FirstOrDefaultAsync<TKey>(Expression<Func<TDocument, bool>> whereClause = null, Expression<Func<TDocument, TKey>> orderByClause = null)
        {
            if (whereClause == null) throw new ArgumentNullException(nameof(whereClause));

            var query = Collection.AsQueryable<TDocument>();

            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }

            if (orderByClause != null)
            {
                query = query.OrderBy(orderByClause);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<long> DeleteAsync(string id)
        {
            var filter = new BsonDocumentFilterDefinition<TDocument>(new BsonDocument("_id", id));

            var deleteResult = await (SessionContainer.AmbientSession != null
                ? Collection.DeleteOneAsync(SessionContainer.AmbientSession, filter).ConfigureAwait(false)
                : Collection.DeleteOneAsync(filter).ConfigureAwait(false));

            return deleteResult.DeletedCount;
        }

        public Transaction StartTransaction()
        {
            return DbContext.StartTransaction();
        }
    }

}
