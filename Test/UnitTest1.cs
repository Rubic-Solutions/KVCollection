using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [ClassInitialize]
        public static void BeforeClass(TestContext tc)
        {
            Console.WriteLine("this is before class.");
        }

        [ClassCleanup]
        public static void AfterClass()
        {
            Console.WriteLine("this is after class.");
        }

        [TestInitialize]
        public void BeforeTest()
        {
            Console.WriteLine("this is before test.");
        }

        [TestCleanup]
        public void AfterTest()
        {
            Console.WriteLine("this is after test.");
        }

        [TestMethod]
        public void TestThreadSafe()
        {
            Console.WriteLine("Test thread-safe.");
            void fn(int number)
            {
                using (var kc = new KeyValue.Collection())
                {
                    kc.Open("test");
                    Console.WriteLine("File is opened.");

                    for (int i = 0; i < 50; i++)
                        kc.Add("task-" + number + "-key-" + i, "task-" + number + "-value-" + i);

                    Console.WriteLine("50 items has been inserted.");
                }
            }

            var tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => fn(1)));
            tasks.Add(Task.Factory.StartNew(() => fn(2)));

            Task.WaitAll(tasks.ToArray());

        }
    }
}
