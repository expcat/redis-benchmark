using System;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;
using StackExchange.Redis;

namespace RedisBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "localhost:30379";

            var secon = ConnectionMultiplexer.Connect(server + ",allowAdmin=true");
            var db = secon.GetDatabase(1);
            var cscon = new CSRedis.CSRedisClient(server);
            RedisHelper.Initialization(cscon);

            ThreadPool.SetMinThreads(1000, 1000);

            Console.WriteLine("---热身---");
            secon.GetServer(server).FlushDatabase(1);
            SERedisTest(db, 1);
            cscon.NodesServerManager.FlushDb();
            CSRedisTest(cscon, 1);

            // int[] param = { 1 };
            int[] param = { 1, 10, 100, 1000, 10000 };

            foreach (var loop in param)
            {
                Console.WriteLine($"---{loop}---");
                secon.GetServer(server).FlushDatabase(1);
                SERedisTest(db, loop);
                cscon.NodesServerManager.FlushDb();
                CSRedisTest(cscon, loop);
            }
            Console.WriteLine("---结束---");
        }

        static void SERedisTest(IDatabase db, int count)
        {
            Task[] tasks = new Task[count];
            string[] keys = new string[count];
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                keys[i] = Guid.NewGuid().ToString();
                tasks[i] = db.StringSetAsync(keys[i], Guid.NewGuid().ToString());
            }
            Task.WaitAll(tasks);
            sw.Stop();
            var set = sw.ElapsedMilliseconds;
            sw.Restart();
            for (int i = 0; i < count; i++)
            {
                tasks[i] = db.StringGetAsync(keys[i]);
            }
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine($"SERedisTest: [{count}] SetAsync: [{set}ms] GetAsync: [{sw.ElapsedMilliseconds}ms]");

            // Test Result
            // foreach (var task in tasks)
            // {
            //     Console.WriteLine(((Task<RedisValue>) task).Result);
            // }
        }

        static void CSRedisTest(CSRedisClient con, int count)
        {
            Task[] tasks = new Task[count];
            string[] keys = new string[count];
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                keys[i] = Guid.NewGuid().ToString();
                tasks[i] = con.SetAsync(keys[i], Guid.NewGuid().ToString());
            }
            Task.WaitAll(tasks);
            sw.Stop();
            var set = sw.ElapsedMilliseconds;
            sw.Restart();
            for (int i = 0; i < count; i++)
            {
                tasks[i] = con.GetAsync(keys[i]);
            }
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine($"CSRedisTest: [{count}] SetAsync: [{set}ms] GetAsync: [{sw.ElapsedMilliseconds}ms]");

            // Test Result
            // foreach (var task in tasks)
            // {
            //     Console.WriteLine(((Task<string>) task).Result);
            // }
        }
    }
}
