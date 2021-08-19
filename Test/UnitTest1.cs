using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
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

        private static System.Text.Encoding enc = System.Text.Encoding.UTF8;

        [TestMethod]
        public void TestThreadSafe()
        {


            Console.WriteLine("Test thread-safe.");
            static void fn(int number)
            {
                var sw = new Stopwatch();
                KeyValue.CollectionIndexer.Define<testModel>()
                    .EnsureIndex(x => x.Id)
                    .EnsureIndex(x => x.IsAdult)
                    .EnsureIndex(x => x.BirtDate);

                using (var kc = new KeyValue.Collection())
                {
                    sw.Restart();
                    kc.Open("\\", "test");
                    sw.Stop();
                    Console.WriteLine("TH" + number + " :: File is opened. (" + sw.Elapsed.ToString() + ")");

                    sw.Restart();
                    for (int i = 0; i < 50000; i++)
                    {
                        var item = new testModel();
                        item.Id = "TH" + number + "-" + i;
                        item.Name = "Person " + i;
                        item.Age = ((i % 90) + 1) + 10;
                        item.BirtDate = new DateTime(DateTime.Now.Year - item.Age, (i % 12) + 1, 1);
                        item.IsAdult = item.Age > 18;

                        kc.Add(item);
                        //Task.Delay(100).GetAwaiter().GetResult();
                    }
                    sw.Stop();
                    Console.WriteLine("TH" + number + " :: " + kc.Count.ToString() + " items has been inserted. (" + sw.Elapsed.ToString() + ")");

                    sw.Restart();
                    var dt = DateTime.Now.AddYears(-30);
                    var adult_count = kc.FindByIndexValues(x => (bool)x[1] && (DateTime)x[2] > dt).Count();
                    sw.Stop();
                    Console.WriteLine("TH" + number + " :: Total " + adult_count + " adult(s) found. (" + sw.Elapsed.ToString() + ")");


                    sw.Restart();
                    for (int i = 0; i < 100; i++)
                    {
                        var item = kc.GetFirst();
                    }
                    sw.Stop();
                    Console.WriteLine("TH" + number + " :: Get First Record 100 times. (" + sw.Elapsed.ToString() + ")");

                    sw.Restart();
                    for (int i = 0; i < 100; i++)
                    {
                        var item = kc.GetLast();
                    }
                    sw.Stop();
                    Console.WriteLine("TH" + number + " :: Get Last Record 100 times. (" + sw.Elapsed.ToString() + ")");
                }
            }

            using (var kc = new KeyValue.Collection())
            {
                kc.Open("\\", "test");
                kc.Truncate();
            }

            var tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => fn(1)));
            tasks.Add(Task.Factory.StartNew(() => fn(2)));

            Task.WaitAll(tasks.ToArray());

        }


        private class testModel
        {
            public string Id;
            public string Name;
            public int Age;
            public DateTime BirtDate;
            public bool IsAdult;

            [JsonIgnore()]
            public bool IsAdult2;
        }
    }
}
