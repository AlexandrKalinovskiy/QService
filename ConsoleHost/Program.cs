using System;
using System.Linq;
using QService.Entities;
using QService.Concrete;
using System.ServiceModel;
using QService.Admin;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(QService.DataFeed)))
            {
                host.Open();
                Console.WriteLine("Server started...");

                Syncing syncing = new Syncing();
                //syncing.SyncSecurities();

                Console.ReadKey();
            }
            //using (var db = new EFDbContext())
            //{
            //    // Create and save a new Blog 
            //    Console.Write("Enter a name for a new Blog: ");
            //    var name = Console.ReadLine();

            //    var board = new ExchangeBoard { Name = name };
            //    db.ExchangeBoards.Add(board);
            //    db.SaveChanges();

            //    // Display all Blogs from the database 
            //    var query = from b in db.ExchangeBoards
            //                orderby b.Name
            //                select b;

            //    Console.WriteLine("All blogs in the database:");
            //    foreach (var item in query)
            //    {
            //        Console.WriteLine(item.Name);
            //    }

            //    Console.WriteLine("Press any key to exit...");
            //    Console.ReadKey();
            //}
        }
    }
}
