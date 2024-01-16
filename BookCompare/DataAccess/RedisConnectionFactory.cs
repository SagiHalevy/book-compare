using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookCompare.DataAccess
{
    public class RedisConnectionFactory
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            return ConnectionMultiplexer.Connect(connectionString);
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;
    }
}