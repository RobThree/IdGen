using IdGen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdGenConsole
{
    class Program
    {
        private static int N = 2000000;
        private static HashSet<long> set = new HashSet<long>();
        private static IdGenerator generator = new IdGenerator(0);
        private static int taskCount = 0;

        static void Main(string[] args)
        {
            Task.Run(() => GetID());
            //Task.Run(() => GetID());
            //Task.Run(() => GetID());

            Task.Run(() => Printf());
            Console.ReadKey();
        }

        private static void Printf()
        {
            while (taskCount != 3)
            {
                Console.WriteLine("...");
                Thread.Sleep(1000);
            }
            Console.WriteLine(set.Count == N * taskCount);
        }

        private static object o = new object();
        private static void GetID()
        {
            for (var i = 0; i < N; i++)
            {

                var id = generator.CreateId();
                if (i % 10000 == 0)
                {
                    Console.WriteLine("开始计算 : {0} --{1}", Thread.CurrentThread.ManagedThreadId, i);
                }
                lock (o)
                {
                    if (set.Contains(id))
                    {
                        Console.WriteLine("发现重复项 : {0}", id);
                    }
                    else
                    {
                        set.Add(id);
                    }
                }

            }
            Interlocked.Increment(ref taskCount);
            Console.WriteLine($"任务{taskCount}完成");
        }
    }
}
