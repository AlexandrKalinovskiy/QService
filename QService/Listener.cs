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
        public IQFeedTrader connector;
        public bool IsRunned;

        private List<IQFeedTrader> connectors;
        private const int conCount = 3;   //Количество коннекторов для обработки запросов на получение свечек
        private OperationContext operationContext;
        private Info info;

        public Listener(IQFeedTrader connector, OperationContext operationContext)
        {
            this.connector = connector;
            this.operationContext = operationContext;
            requestCandlesQueue = new Queue<RequestCandles>();
            info = new Info();
            IsRunned = true;

            connectors = new List<IQFeedTrader>();

            for (int i = 0; i < conCount; i++)
            {
                var con = new IQFeedTrader();
                con.Connect();
                connectors.Add(con);
            }
        }

        public void CandlesQueueStart()
        {
            bool isSuccess;
            List<Candle> candlesStake = new List<Candle>();

            while (IsRunned)    //Постоянно следим за очередью запросов
            {
                Thread.Sleep(10);
                Console.WriteLine("Connection state: {0} {1} {2}", connectors[0].ConnectionState, connectors[1].ConnectionState, connectors[2].ConnectionState);
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

        IDataFeedCallback Callback
        {
            get
            {
                return operationContext.GetCallbackChannel<IDataFeedCallback>();
            }
        }
    }
}
