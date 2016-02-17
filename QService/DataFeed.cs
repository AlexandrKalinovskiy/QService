//using StockSharp.BusinessEntities;
using StockSharp.IQFeed;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Ecng.Xaml;
using System.Threading;
using StockSharp.Algo.Storages;
using System.Linq;
using QService.Entities;
using QService.Concrete;
using StockSharp.Algo.Candles;
using Ecng.Common;


namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class DataFeed : IDataFeed
    {
        private IQFeedTrader connector;
        OperationContext operationContext;
        private EFDbContext context;
        private const int portionSize = 10;
        private int requestsCount;
        private List<Entities.Candle> candlesTest;
        private CandleManager candleManager;

        DataFeed()
        {
            requestsCount = 0;
            candlesTest = new List<Entities.Candle>();
            context = new EFDbContext();
            //context.Configuration.ProxyCreationEnabled = false;

            operationContext = OperationContext.Current;
            operationContext.Channel.Closed += Channel_Closed;

            connector = new IQFeedTrader();
            //connector.ValuesChanged += Connector_Level1Changed;
            candleManager = new CandleManager(connector);
            connector.Connect();

     
            candleManager.Processing += CandleManager_Processing;

            Thread.Sleep(500);

            Console.WriteLine("SID: {0}", operationContext.Channel.SessionId);
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Client disconnected. {0}", connector.Id);
        }

        ~DataFeed()
        {
            Console.WriteLine("Destroy {0}", connector.Id);
        }

        //private void Connector_Level1Changed(Security security, IEnumerable<KeyValuePair<StockSharp.Messages.Level1Fields, object>> changes, DateTimeOffset arg3, DateTime arg4)
        //{            
        //    foreach(var change in changes)
        //    {
        //        if (change.Key == StockSharp.Messages.Level1Fields.BestAskPrice || change.Key == StockSharp.Messages.Level1Fields.BestBidPrice)
        //        {
        //            //Console.WriteLine("{0} change {1} connector {2}", security.Code, change.Value, connector.Id);
        //            try
        //            {
        //                operationContext.GetCallbackChannel<IDataFeedCallback>().NewLevel1Values((decimal)change.Value, (decimal)change.Value);
        //            }
        //            catch
        //            {
        //                connector.UnRegisterSecurity(security);
        //                operationContext.Channel.Close();
        //            }                   
        //        }
        //    }
        //}

        public void SubscribeLevel1(Security security)
        {
            //operationContext = OperationContext.Current;

            //Console.WriteLine("IQFeed connection state: {0} ", connector.ConnectionState);

            //var criteria = new Security()
            //{
            //    Code = security,
            //    Board = ExchangeBoard.Nyse
            //};
            //connector.RegisterSecurity(criteria);
        }

        public void GetSecurities(string ticker, string exchangeBoardCode)
        {
            var securities = new List<Security>();

            if (ticker != null && ticker != string.Empty)   //Если указан тикер бумаги
            {
                securities = context.Securities.Where(s => s.Ticker == ticker).ToList();

                if (securities.Count == 0)
                    operationContext.GetCallbackChannel<IDataFeedCallback>().NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящей бумаги
            }
            else if (exchangeBoardCode != null && exchangeBoardCode != string.Empty)
            {
                var exchangeBoard = context.ExchangeBoards.Where(e => e.Code == exchangeBoardCode).FirstOrDefault();

                if (exchangeBoard != null)
                {
                    securities = context.Securities.Where(s => s.ExchangeBoard.Id == exchangeBoard.Id).ToList();

                    if (securities.Count == 0)
                        operationContext.GetCallbackChannel<IDataFeedCallback>().NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящих бумаг
                }
                else
                {
                    operationContext.GetCallbackChannel<IDataFeedCallback>().NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящих бумаг
                }
            }

            //Выполнить преобразование в "чистую" модель данных, в случае если найдены бумаги по указанным критериям
            var list = new List<Security>();
            int i = 0;

            foreach (var security in securities)
            {
                list.Add(new Security
                {
                    Id = security.Id,
                    Code = security.Code,
                    Name = security.Name,
                    StepPrice = security.StepPrice,
                    Ticker = security.Ticker,
                    ExchangeBoard = new ExchangeBoard
                    {
                        Id = security.ExchangeBoard.Id,
                        Code = security.ExchangeBoard.Code,
                        Name = security.ExchangeBoard.Name,
                        Description = security.ExchangeBoard.Description
                    }
                });

                try
                {
                    Callback.NewSecurities(list);
                    list.Clear();
                    i = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}", e);
                    operationContext.Channel.Close();
                    break;
                }
            };
        }

        public List<ExchangeBoard> GetExchangeBoards(string exchangeBoardCode)
        {
            var boards = context.ExchangeBoards.Where(b => b.Code == exchangeBoardCode).ToList();

            if (boards == null)
                return context.ExchangeBoards.ToList();

            return boards;
        }

        /// <summary>
        /// Метод возвращает исторические ссвечки с интервалом от секунды до месяца.
        /// </summary>
        /// <param name="security">Инструмент</param>
        /// <param name="from">С какой даты и времени начинать закачку</param>
        /// <param name="to">До какой даты и времени закачивать</param>
        /// <param name="timeFrame">Таймфрем свечки</param>
        public void GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame)
        {
            //Console.WriteLine("Requests count: {0} threadId: {1}", requestsCount++, Thread.CurrentThread.ManagedThreadId);
            //var formatFrom = new DateTime(from.Year, from.Month, from.Day + 1, 9, 30, 00);
            //var formatTo = new DateTime(to.Year, to.Month, to.Day, 16, 00, 0) - timeFrame;

            ////var board = context.ExchangeBoards.FirstOrDefault(b => b.Id == security.ExchangeBoard.Id);
            var board = new StockSharp.BusinessEntities.ExchangeBoard();

            var criteria = new StockSharp.BusinessEntities.Security
            {
                Code = security.Ticker,
                Id = security.Code,
                Board = board
            };

            var series = new CandleSeries(typeof(TimeFrameCandle), criteria, timeFrame);
            candleManager.Start(series);

            Console.WriteLine("Start {0}", security.Ticker);

            //series.GetCandles<TimeFrameCandle>(20);

            //Console.WriteLine("{0}", candleManager.Series.Count);      

            //bool isSuccess;
            //var candles = connector.GetHistoricalCandles(criteria, typeof(TimeFrameCandle), timeFrame, formatFrom, to, out isSuccess);

            //var list = new List<Entities.Candle>();

            //foreach (var candle in candles)
            //{
            //    var candleOpenTime = candle.OpenTime - TimeSpan.FromHours(5);   //Разница UTC и NY 5 часов
            //    var candleCloseTime = candle.CloseTime - TimeSpan.FromHours(5);   //Разница UTC и NY 5 часов

            //    var rcandle = new Entities.Candle
            //    {
            //        OpenPrice = candle.OpenPrice,
            //        OpenTime = candle.OpenTime,
            //        HighPrice = candle.HighPrice,
            //        LowPrice = candle.ClosePrice,
            //        ClosePrice = candle.ClosePrice,
            //        CloseTime = candle.CloseTime,
            //        Security = new Security
            //        {
            //            Ticker = candle.Security.Code,
            //            Code = candle.Security.Id,
            //            Name = candle.Security.Name,
            //            ExchangeBoard = new ExchangeBoard
            //            {
            //                Code = candle.Security.Board.Code
            //            }
            //        },
            //        TotalVolume = candle.TotalVolume
            //    };

            //    list.Add(rcandle);
            //};

            //if (list.Count > 0)
            //{
            //    try
            //    {
            //        Callback.NewCandles(list);
            //        ////SendResult(list);
            //        //asyncResult = func.BeginInvoke(list, null, null);
            //        list.Clear();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //        //operationContext.Channel.Close();
            //    }
            //}
            //connector.Disconnect();
        }

        int candlesCount = 0;
        DateTimeOffset sendCandle;

        private void CandleManager_Processing(CandleSeries series, StockSharp.Algo.Candles.Candle candle)
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

            if (rcandle.OpenTime.Ticks != sendCandle.Ticks)
            {
                Console.WriteLine("{0}", candlesCount++);
                Callback.NewCandles(rcandle);
            }
            sendCandle = rcandle.OpenTime;           
        }


        //public IEnumerable<Entities.Candle> GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame)
        //{
        //    var formatFrom = new DateTime(from.Year, from.Month, from.Day + 1, 9, 30, 00);
        //    var formatTo = new DateTime(to.Year, to.Month, to.Day, 16, 00, 0) - timeFrame;

        //    //var board = context.ExchangeBoards.FirstOrDefault(b => b.Id == security.ExchangeBoard.Id);
        //    var board = new StockSharp.BusinessEntities.ExchangeBoard();

        //    var criteria = new StockSharp.BusinessEntities.Security
        //    {
        //        Code = security.Ticker,
        //        Id = security.Code,
        //        Board = board
        //    };

        //    bool isSuccess;
        //    var candles = connector.GetHistoricalCandles(criteria, typeof(TimeFrameCandle), timeFrame, formatFrom, to, out isSuccess);

        //    var list = new List<Entities.Candle>();

        //    foreach (var candle in candles)
        //    {
        //        var candleOpenTime = candle.OpenTime - TimeSpan.FromHours(5);   //Разница UTC и NY 5 часов
        //        var candleCloseTime = candle.CloseTime - TimeSpan.FromHours(5);   //Разница UTC и NY 5 часов

        //        var rcandle = new Entities.Candle
        //        {
        //            OpenPrice = candle.OpenPrice,
        //            OpenTime = candle.OpenTime,
        //            HighPrice = candle.HighPrice,
        //            LowPrice = candle.ClosePrice,
        //            ClosePrice = candle.ClosePrice,
        //            CloseTime = candle.CloseTime,
        //            Security = new Security
        //            {
        //                Ticker = candle.Security.Code,
        //                Code = candle.Security.Id,
        //                Name = candle.Security.Name,
        //                ExchangeBoard = new ExchangeBoard
        //                {
        //                    Code = candle.Security.Board.Code
        //                }
        //            },
        //            TotalVolume = candle.TotalVolume
        //        };

        //        //if (candleOpenTime.Ticks >= formatFrom.Ticks &&  candleOpenTime.Ticks <= formatTo.Ticks)
        //        //{
        //        list.Add(rcandle);
        //        //operationContext.GetCallbackChannel<IDataFeedCallback>().NewCandles(list);
        //        //}
        //    };

        //    Console.WriteLine("Send candles from SID: {0}", operationContext.Channel.SessionId);
        //    return list;
        //}

        IDataFeedCallback Callback
        {
            get
            {
                return operationContext.GetCallbackChannel<IDataFeedCallback>();
            }
        }
    }
}
