using QService.Entities;
using System.Linq;
using StockSharp.IQFeed;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using QService.Admin;
using System;
using System.Data;

namespace QService
{
    /// <summary>
    /// Класс выполняется в отдельном потоке и постоянно следит за очередью сообщений. 
    /// Служит для того, чтобы распределять нагрузку на сервер при большом количестве асинхронных запросов от клиентов и большом количестве ответов от сервера.
    /// </summary>
    public class Listener
    {
        public Queue<RequestCandles> requestCandlesQueue;   //Очередь запросов на получение свечек по указанным инструментам
        public bool IsRunned;

        private const int _stakeSize = 200;
        private static object locker = new object();
        private OperationContext operationContext;
        private static Info info;
        private static List<Connector> connectorsList;  //Список коннекторов между которыми будут распределяться задачи загрузки свечек
        private static int conCount = 10; //Количество коннекторов, которые будут обслуживать загрузку свечек
        private static int count;

        public Listener(OperationContext operationContext)
        {
            this.operationContext = operationContext;
            requestCandlesQueue = new Queue<RequestCandles>();
            info = new Info();
            
            if (connectorsList == null)
            {
                connectorsList = new List<Connector>();
                for (int i = 0; i < conCount; i++)
                {
                    var connector = new Connector
                    {
                        Trader = new IQFeedTrader(),
                        IsAvialable = true
                    };

                    connector.Trader.Connect();
                    connectorsList.Add(connector);
                }
            }

            IsRunned = true;

            Console.WriteLine("Count {0}", count++);
            foreach(var connector in connectorsList)
            {
                Console.WriteLine("Connector {0} {1}", connector.Trader.Id, connector.IsAvialable);
            }
        }

        /// <summary>
        /// Метод обрабатывает очередь запросов на получение свечек
        /// </summary>
        public void CandlesQueueStart()
        {
            bool isSuccess;
            List<Candle> candlesStake = new List<Candle>();
            RequestCandles request;
            IEnumerable<StockSharp.Algo.Candles.Candle> candles;

            while (IsRunned)    //Постоянно следим за очередью запросов
            {              
                if (info.IsChannelOpened(operationContext) && requestCandlesQueue.Count > 0)   //Выполнять код будем если только очередь не пуста и канал связи с клиентом в порядке
                {
                    var connector = GetAvialableConnector();    //Получить свободный коннектор для загрузки свечек
                    try
                    {                        
                        lock (locker)   //Синхронизация потоков
                        {
                            request = requestCandlesQueue.Dequeue();    //Запросы выполняются в порядке очереди
                        }

                        candles = connector.Trader.GetHistoricalCandles(request.Security, request.Type, request.TimeFrame, request.From, request.To, out isSuccess);
                        connector.IsAvialable = true;

                        if (candles != null && candles.Count() > 0)
                        {
                            foreach (var candle in candles)
                            {
                                var rcandle = new Entities.Candle
                                {
                                    OpenPrice = candle.OpenPrice,
                                    OpenTime = candle.OpenTime,
                                    HighPrice = candle.HighPrice,
                                    LowPrice = candle.ClosePrice,
                                    ClosePrice = candle.ClosePrice,
                                    CloseTime = candle.CloseTime,
                                    Security = new Security
                                    {
                                        Ticker = candle.Security.Code,
                                        Code = candle.Security.Id,
                                        Name = candle.Security.Name,
                                        ExchangeBoard = new ExchangeBoard
                                        {
                                            Code = candle.Security.Board.Code
                                        }
                                    },
                                    TotalVolume = candle.TotalVolume
                                };

                                candlesStake.Add(rcandle);

                                if(candlesStake.Count >= _stakeSize)    //Отправляем свечки порциями чтобы предотвратить "падение" канала связи
                                {
                                    //Console.WriteLine("Queue size: {0}, candles count: {1} from thread {2} {3} {4}", requestCandlesQueue.Count, candles.Count(), Thread.CurrentThread.ManagedThreadId, request.Security.Code, connector.Trader.ConnectionState);
                                    Callback.NewCandles(candlesStake);
                                    candlesStake.Clear();
                                    //Console.WriteLine("connectors count {0}", connectorsList.Count);
                                }
                            };

                            Callback.NewCandles(candlesStake);  //Последняя порция свечек
                            candlesStake.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        connector.IsAvialable = true;
                        Console.Write(e);
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }

            Console.WriteLine("Завершение потока {0}", Thread.CurrentThread.ManagedThreadId);
            //connector.Disconnect();
            //connector = null;          
        }

        IDataFeedCallback Callback
        {
            get
            {
                return operationContext.GetCallbackChannel<IDataFeedCallback>();
            }
        }


        /// <summary>
        /// Метод возвращает первый найденный свободный от работы коннектор
        /// </summary>
        /// <returns></returns>
        private Connector GetAvialableConnector()
        {
            while(true)
            {
                foreach(var connector in connectorsList)
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

        //Вспомогательный класс.
        private class Connector
        {
            public IQFeedTrader Trader { get; set; }
            public bool IsAvialable { get; set; }   //Доступен или занят в данный момент текущий коннектор
        }
    }
}
