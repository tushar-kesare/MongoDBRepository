using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDBRepository.IntegrationTests
{
    public class TestProduct
    {
        public string Id { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public decimal Value { get; set; }
    }

    public class TestProductsRepository : BaseMongoRepository<TestProduct>
    {
        public TestProductsRepository(string connectionString) : base(connectionString)
        {
        }

        public override string CollectionName => "testProducts";
    }
}
