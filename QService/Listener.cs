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
        //public IQFeedTrader connector;
        public bool IsRunned;
        private static object locker = new object();

        private OperationContext operationContext;
        private Info info;

        public Listener(IQFeedTrader connector, OperationContext operationContext)
        {
            //this.connector = connector;
            this.operationContext = operationContext;
            requestCandlesQueue = new Queue<RequestCandles>();
            info = new Info();
            IsRunned = true;
        }

        public void CandlesQueueStart()
        {
            var connector = new IQFeedTrader();
            connector.Connect();

            bool isSuccess;
            List<Candle> candlesStake = new List<Candle>();

            while (IsRunned)    //Постоянно следим за очередью запросов
            {
                if (info.IsChannelOpened(operationContext) && requestCandlesQueue.Count > 0 && connector.ConnectionState == StockSharp.Messages.ConnectionStates.Connected)   //Выполнять код будем если только очередь не пуста и канал связи с клиентом в порядке
                {
                    try
                    {
                        RequestCandles request;

                        lock (locker)
                        {
                            request = requestCandlesQueue.Dequeue();    //Запросы выполняются в порядке очереди
                        }

                        var candles = connector.GetHistoricalCandles(request.Security, request.Type, request.TimeFrame, request.From, request.To, out isSuccess);

                        if (candles != null && candles.Count() > 0)
                        {
                            int i = 0;
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
                                if(i<10)
                                    candlesStake.Add(rcandle);
                                i++;
                            };
                            Console.WriteLine("Queue size: {0}, candles count: {1} from thread {2} {3} {4}", requestCandlesQueue.Count, candles.Count(), Thread.CurrentThread.ManagedThreadId, request.Security.Code, connector.ConnectionState);
                            Callback.NewCandles(candlesStake);                           
                            candlesStake.Clear();
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }

            Console.WriteLine("Завершение потока {0}", Thread.CurrentThread.ManagedThreadId);
            connector.Disconnect();
            connector = null;          
        }

        IDataFeedCallback Callback
        {
            get
            {
                return operationContext.GetCallbackChannel<IDataFeedCallback>();
            }
        }
    }
}
