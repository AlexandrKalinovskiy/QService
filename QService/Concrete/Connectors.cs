using StockSharp.IQFeed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QService.Concrete
{
    public static class Connectors
    {
        private static List<Connector> connectors;
        private const int connectorsCount = 25;
        private static Connector connector;

        //Конструктор выполнится только один раз при первом созданиии класса
        static Connectors()
        {
            Console.WriteLine("Connectors created");
            connectors = new List<Connector>();
            for (int i = 0; i < connectorsCount; i++)
            {
                connector = new Connector
                {
                    IsAvialable = true
                };
                connector.Connect();
                Thread.Sleep(500);
                connectors.Add(connector);
            }

            new Task(AvialableCount).Start();
        }   

        public static Connector GetAvialableConnector()
        {
            while (true)
            {
                foreach (var connector in connectors)
                {
                    if (connector.IsAvialable)
                    {
                        connector.IsAvialable = false;
                        return connector;
                    }
                }
                Thread.Sleep(10);
            }
        }

        public static void FreeConnector(Connector connector)
        {
            connector.IsAvialable = true;
        }

        public class Connector: IQFeedTrader
        {
            public bool IsAvialable { get; set; }
        }

        private static void AvialableCount()    //Для мониторинга в процессе разработки
        {
            int free = 0;
            int connected = 0;
            int connecting = 0;
            int failed = 0;
            while(true)
            {
                foreach(var con in connectors)
                {
                    if(con.IsAvialable)
                        free++;

                    if (con.ConnectionState.ToString() == "Connected")
                        connected++;

                    if (con.ConnectionState.ToString() == "Connecting")
                        connecting++;

                    if (con.ConnectionState.ToString() == "Failed")
                        failed++;
                }
                Console.Clear();
                Console.WriteLine("Avialable connections {0}\nConnected {1}\nConnecting {2}\nFailed {3}", free, connected, connecting, failed);
                Thread.Sleep(500);
                free = 0;
                connected = 0;
                connecting = 0;
                failed = 0;
            }
        }
    }
}
