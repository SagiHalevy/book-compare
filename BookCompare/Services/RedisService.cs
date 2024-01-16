using BookCompare.DataAccess;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookCompare.Services
{
    public class RedisService
    {
        private readonly IDatabase _redisDatabase;

        public RedisService()
        {
            var connectionMultiplexer = RedisConnectionFactory.Connection;
            _redisDatabase = connectionMultiplexer.GetDatabase();
        }

        public void SetValue(string key, string value)
        {
            _redisDatabase.StringSet(key, value);
        }

        public string GetValue(string key)
        {
            return _redisDatabase.StringGet(key);
        }
    }
}