using QService.Entities;
using System.Linq;
using StockSharp.IQFeed;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using QService.Admin;
using System;

namespace QService
{
    /// <summary>
    /// Класс выполняется в отдельном потоке и постоянно следит за очередью сообщений. 
    /// Служит для того, чтобы распределять нагрузку на сервер при большом количестве асинхронных запросов от клиентов и большом количестве ответов от сервера.
    /// </summary>
    public class Listener
    {
        public Queue<RequestCandles> requestCandlesQueue;   //Очередь запросов на получение свечек по указанным инструментам
        public Queue<Level1> responseLevel1Queue;   //Очередь ответов новых значений Level1
        public IQFeedTrader connector;
        static object locker = new object();
        public int Counter = 0;

        private OperationContext operationContext;
        private Info info;

        public Listener(IQFeedTrader connector, OperationContext operationContext)
        {
            this.connector = connector;
            this.operationContext = operationContext;
            requestCandlesQueue = new Queue<RequestCandles>();
            responseLevel1Queue = new Queue<Level1>();
            info = new Info();
        }

        public void CandlesQueueStart()
        {
            bool isSuccess;
            List<Candle> candlesStake = new List<Candle>();

            while (true)    //Постоянно следим за очередью запросов
            {
                Thread.Sleep(10);
                if (info.IsChannelOpened(operationContext) && requestCandlesQueue.Count > 0)   //Выполнять код будем если только очередь не пуста и канал связи с клиентом в порядке
                {
                    try
                    {
                        var request = requestCandlesQueue.Dequeue();    //Запросы выполняются в порядке очереди
                        var candles = connector.GetHistoricalCandles(request.Security, request.Type, request.TimeFrame, request.From, request.To, out isSuccess);

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
                            };
                            Console.WriteLine("request {0} {1}", requestCandlesQueue.Count, candles.Count());
                            Callback.NewCandles(candlesStake);
                            candlesStake.Clear();
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        public void Level1QueueStart()
        {
            int i = 0;
            Console.WriteLine("i = {0} {1}", i, operationContext.Channel.SessionId);
            while (true)
            {
                Thread.Sleep(1);
                //Callback.Test(i++);
                
                //lock (locker)
                //{            
                //    if (info.IsChannelOpened(operationContext) && responseLevel1Queue.Count > 0)   //Выполнять код будем если только очередь не пуста и канал связи с клиентом в порядке
                //    {
                //        var level1 = responseLevel1Queue.Dequeue();    //Запросы выполняются в порядке очереди
                //                                                       //var level1 = new Level1
                //                                                       //{
                //                                                       //    BestAskPrice = 0,
                //                                                       //    BestAskVolume = 0,
                //                                                       //    BestBidPrice = 1,
                //                                                       //    BestBidVolume = 1,
                //                                                       //    Security = new StockSharp.BusinessEntities.Security
                //                                                       //    {
                //                                                       //        Code = "A",
                //                                                       //        Board = new StockSharp.BusinessEntities.ExchangeBoard
                //                                                       //        {
                //                                                       //            Code = "NYSE"
                //                                                       //        }
                //                                                       //    }
                //                                                       //};
                //        if (level1 != null)
                //        {
                //            try
                //            {
                //                //Callback.NewLevel1Values(level1);
                //                Callback.Test(i);
                //                Console.WriteLine("Send level1 {0} {1} {2} : ", level1.Security.Code, i++, responseLevel1Queue.Count);
                //                //Console.WriteLine("Queue size {0}", responseLevel1Queue.Count);
                //            }
                //            catch (Exception e)
                //            {
                //                Console.WriteLine("Error {0}", e);
                //                //connector.UnRegisterSecurity(level1.Security);
                //                //Console.WriteLine("UnRegistered {0}", level1.Security.Code);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Thread.Sleep(1);
                //    }
                //}
            }
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
