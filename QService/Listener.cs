using QService.Entities;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using QService.Admin;
using System;
using static QService.Concrete.Connectors;

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

        public Listener(OperationContext operationContext)
        {
            this.operationContext = operationContext;
            requestCandlesQueue = new Queue<RequestCandles>();
            info = new Info();

            IsRunned = true;
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
                    var connector = GetAvialableConnector();  //Получить свободный коннектор для загрузки свечек
                    try
                    {                        
                        lock (locker)   //Синхронизация потоков
                        {
                            request = requestCandlesQueue.Dequeue();    //Запросы выполняются в порядке очереди
                        }

                        candles = connector.GetHistoricalCandles(request.Security, request.Type, request.TimeFrame, request.From, request.To, out isSuccess);
                        FreeConnector(connector);   //Освободить коннектор

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
                                    Callback.NewCandles(candlesStake);
                                    candlesStake.Clear();
                                }
                            };

                            Callback.NewCandles(candlesStake);  //Последняя порция свечек
                            candlesStake.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        FreeConnector(connector);
                        //Console.Write(e);
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
    }
}
