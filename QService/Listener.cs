using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QService
{
    /// <summary>
    /// Класс выполняется в отдельном потоке и постоянно следит за очередью сообщений. 
    /// Служит для того, чтобы распределять нагрузку на сервер при большом количестве асинхронных запросов от клиентов.
    /// </summary>
    public class Listener
    {
        public Queue<CandleSeries> getCandlesQueue;

        public Listener()
        {
            getCandlesQueue = new Queue<CandleSeries>();
        }

        public void Start()
        {
            while (true)    //Постоянно следим за очередью запросов
            {
                Thread.Sleep(200);
                try
                {
                    var series = getCandlesQueue.Dequeue();
                    if (series != null)
                    {
                        Console.WriteLine("Следим за очередью");
                    }
                }
                catch
                {

                }
            }
        }
    }
}
