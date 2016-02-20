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
using System.ComponentModel;

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DataFeed : IDataFeed
    {
        private IQFeedTrader connector;
        public OperationContext operationContext;
        private EFDbContext context;
        private List<Entities.Candle> candlesTest;
        private Thread listenerThtread;
        private Listener listener;

        DataFeed()
        {
            context = new EFDbContext();
            //context.Configuration.ProxyCreationEnabled = false;

            operationContext = OperationContext.Current;
            operationContext.Channel.Closed += Channel_Closed;

            connector = new IQFeedTrader();
            //connector.ValuesChanged += Connector_Level1Changed;
 
            Thread.Sleep(500);

            Console.WriteLine("SID: {0}", operationContext.Channel.SessionId);

            listener = new Listener(connector, operationContext);

            listenerThtread = new Thread(listener.Start);
            listenerThtread.Start();

            connector.Connect();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Client disconnected. {0}", connector.Id);
        }

        ~DataFeed()
        {
            Console.WriteLine("Destroy {0}", connector.Id);
        }

        private void Connector_Level1Changed(StockSharp.BusinessEntities.Security security, IEnumerable<KeyValuePair<StockSharp.Messages.Level1Fields, object>> changes, DateTimeOffset arg3, DateTime arg4)
        {
            foreach (var change in changes)
            {
                if (change.Key == StockSharp.Messages.Level1Fields.BestAskPrice || change.Key == StockSharp.Messages.Level1Fields.BestBidPrice)
                {
                    //Console.WriteLine("{0} change {1} connector {2}", security.Code, change.Value, connector.Id);
                    try
                    {
                        operationContext.GetCallbackChannel<IDataFeedCallback>().NewLevel1Values((decimal)change.Value, (decimal)change.Value);
                    }
                    catch
                    {
                        connector.UnRegisterSecurity(security);
                        operationContext.Channel.Close();
                    }
                }
            }
        }

        public void SubscribeLevel1(Security security)
        {
            operationContext = OperationContext.Current;

            var criteria = new StockSharp.BusinessEntities.Security()
            {
                Code = security.Ticker,
                Board = StockSharp.BusinessEntities.ExchangeBoard.Nyse
            };

            connector.RegisterSecurity(criteria);
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

            var requestCandlies = new RequestCandles
            {
                Security = criteria,
                Type = typeof(TimeFrameCandle),
                From = from,
                To = to,
                TimeFrame = timeFrame
            };

            listener.requestCandlesQueue.Enqueue(requestCandlies);
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
