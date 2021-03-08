using Microservices;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class RedisDistributedLockExecutor : IExecutor
    {
        private const string REDIS_HOST = "localhost";
        private const int REDIS_PORT = 6379;

        private readonly int lockExpirationMs;
        private readonly int maxRetries, retryWaitMs;

        public RedisDistributedLockExecutor(int lockExpirationMs = 5000, int maxRetries = 8, int retryWaitMs = 250)
        {
            this.lockExpirationMs = lockExpirationMs;
            this.retryWaitMs = retryWaitMs;
            this.maxRetries = maxRetries;
            Connection.GetEndPoints(); // Warmup 
        }


        public async Task Execute(int requestId, string name, string key, params ExecutionTask[] tasks)
        {
            var lockKey = $"lock:{key}";
            var lockValue = "1";
            var expiration = TimeSpan.FromMilliseconds(lockExpirationMs);
            var val = 0;
            var isLocked = AcquireLock(lockKey, lockValue, expiration);
            while (!isLocked && val <= maxRetries)
            {
                ++val;
                await Task.Delay(retryWaitMs);
                isLocked = AcquireLock(lockKey, lockValue, expiration);
            }
            if (isLocked)
            {
                var invocations = tasks.Select(t => t.ActionTask());
                await Task.WhenAll(invocations);
                ReleaseLock(lockKey, lockValue);
            }
            else
            {
                throw new FailToAcquireLogException();
            }
        }

        /// <summary>  
        /// https://www.c-sharpcorner.com/article/creating-distributed-lock-with-redis-in-net-core/
        /// https://github.com/catcherwong/Demos/blob/master/src/RedisLockDemo/RedisLockDemo/Program.cs
        /// </summary>  
        private static ConnectionMultiplexer CreateConnection()
        {
            ConfigurationOptions configuration = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
            };

            configuration.EndPoints.Add(REDIS_HOST, REDIS_PORT);
            return ConnectionMultiplexer.Connect(configuration.ToString());
        }

        /// <summary>  
        /// Gets the connection.  
        /// </summary>  
        /// <value>The connection.</value>  
        public static readonly ConnectionMultiplexer Connection = CreateConnection();

        /// <summary>  
        /// Acquires the lock.  
        /// </summary>  
        /// <returns><c>true</c>, if lock was acquired, <c>false</c> otherwise.</returns>  
        /// <param name="key">Key.</param>  
        /// <param name="value">Value.</param>  
        /// <param name="expiration">Expiration.</param>  
        static bool AcquireLock(string key, string value, TimeSpan expiration)
        {
            bool flag = false;
            try
            {
                flag = Connection.GetDatabase().StringSet(key, value, expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Acquire lock fail...{ex.Message}");
            }
            return flag;
        }

        /// <summary>  
        /// Releases the lock.  
        /// </summary>  
        /// <returns><c>true</c>, if lock was released, <c>false</c> otherwise.</returns>  
        /// <param name="key">Key.</param>  
        /// <param name="value">Value.</param>  
        static bool ReleaseLock(string key, string value)
        {
            string lua_script = @"  
    if (redis.call('GET', KEYS[1]) == ARGV[1]) then  
        redis.call('DEL', KEYS[1])  
        return true  
    else  
        return false  
    end  
    ";

            try
            {
                var res = Connection.GetDatabase().ScriptEvaluate(lua_script,
                                                           new RedisKey[] { key },
                                                           new RedisValue[] { value });
                return (bool)res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReleaseLock lock fail...{ex.Message}");
                return false;
            }
        }

    }


    [Serializable]
    public class FailToAcquireLogException : Exception
    {
        public FailToAcquireLogException() { }
        public FailToAcquireLogException(string message) : base(message) { }
        public FailToAcquireLogException(string message, Exception inner) : base(message, inner) { }
        protected FailToAcquireLogException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
