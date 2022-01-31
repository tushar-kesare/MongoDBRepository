using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDBRepository.IntegrationTests
{
    public static class Configuration
    {
        private static IConfiguration configuration;

        public static IConfiguration Settings {
            get
            {
                if (configuration == null)
                {
                    configuration = new ConfigurationBuilder().AddJsonFile("appsettings.test.json")
                                                              .Build();
                }

                return configuration;
            }
        }



    }
}
