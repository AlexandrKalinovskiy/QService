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
using QService.Admin;

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class DataFeed : IDataFeed
    {
        private IQFeedTrader connector;
        public OperationContext operationContext;
        private EFDbContext context;
        private Listener listener;
        private const int stakeSize = 200;
        private const int conCount = 5; //Количество потоков для обработки исторических данных будет меняться в зависимости от тарифа.
        private Info info; 

        DataFeed()
        {
            context = new EFDbContext();

            operationContext = OperationContext.Current;
            operationContext.Channel.Opened += Channel_Opened;
            operationContext.Channel.Closed += Channel_Closed;

            info = new Info();

            var secContext = ServiceSecurityContext.Current;

            connector = new IQFeedTrader();
            connector.ValuesChanged += Connector_Level1Changed;

            Console.WriteLine("SID: {0}", operationContext.Channel.SessionId);

            listener = new Listener(connector, operationContext);

            string name = ServiceSecurityContext.Current.PrimaryIdentity.Name;

            //Запускаем вторичные потоки для обработки исторических данных
            for (int i = 0; i <= conCount; i++)
            {
                new Thread(listener.CandlesQueueStart).Start();
            }

            connector.Connect();

            Thread.Sleep(1000);
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Client connected. {0}", connector.Id);
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Client disconnected. {0}", connector.Id);
            connector.Disconnect();
            connector = null;
            listener.IsRunned = false;
        }

        ~DataFeed()
        {
            Console.WriteLine("Destroy {0}", connector.Id);
        }

        private void Connector_Level1Changed(StockSharp.BusinessEntities.Security security, IEnumerable<KeyValuePair<StockSharp.Messages.Level1Fields, object>> changes, DateTimeOffset arg3, DateTime arg4)
        {
            if (info.IsChannelOpened(operationContext))
            {
                foreach (var change in changes)
                {
                    if (change.Key == StockSharp.Messages.Level1Fields.BestAskPrice || change.Key == StockSharp.Messages.Level1Fields.BestBidPrice)
                    {
                        var level1 = new Level1
                        {
                            BestAskPrice = (decimal)change.Value,
                            BestBidPrice = (decimal)change.Value,
                            Security = security
                        };

                        try
                        {
                            Callback.NewLevel1Values(level1);
                        }
                        catch (Exception e)
                        {
                            if (connector != null)
                                connector.UnRegisterSecurity(security);
                        }
                    }
                }
            }
        }

        public void SubscribeLevel1(Security security)
        {
            if (security != null)
            {
                var criteria = new StockSharp.BusinessEntities.Security
                {
                    Code = security.Ticker,
                    Id = security.Code,
                    Board = StockSharp.BusinessEntities.ExchangeBoard.Nyse
                };

                connector.RegisterSecurity(criteria);
                Console.WriteLine("Register SECURITY {0}, {1}", connector.ConnectionState, connector.Id);
            }
        }

        public void GetSecurities(string ticker, string exchangeBoardCode)
        {
            var securities = new List<Security>();

            if (ticker != null && ticker != string.Empty)   //Если указан тикер бумаги
            {
                securities = context.Securities.Where(s => s.Ticker == ticker).ToList();

                if (securities.Count == 0)
                    Callback.NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящей бумаги
            }
            else if (exchangeBoardCode != null && exchangeBoardCode != string.Empty)
            {
                var exchangeBoard = context.ExchangeBoards.Where(e => e.Code == exchangeBoardCode).FirstOrDefault();

                if (exchangeBoard != null)
                {
                    securities = context.Securities.Where(s => s.ExchangeBoard.Id == exchangeBoard.Id).ToList();

                    if (securities.Count == 0)
                        Callback.NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящих бумаг
                }
                else
                {
                    Callback.NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящих бумаг
                }
            }

            //Выполнить преобразование в "чистую" модель данных, в случае если найдены бумаги по указанным критериям
            var list = new List<Security>();

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

                if (list.Count >= stakeSize)
                {
                    try
                    {
                        Callback.NewSecurities(list);
                        list.Clear();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0}", e);
                        //operationContext.Channel.Close();
                        break;
                    }
                }
            };

            Console.WriteLine(operationContext.Channel.State);
            //Callback.NewSecurities(list);
        }

        public List<ExchangeBoard> GetExchangeBoards(string exchangeBoardCode)
        {
            var boards = context.ExchangeBoards.Where(b => b.Code == exchangeBoardCode).ToList();

            if (boards == null)
                return context.ExchangeBoards.ToList();

            return boards;
        }

        /// <summary>
        /// Метод возвращает исторические свечки с интервалом от секунды до месяца.
        /// </summary>
        /// <param name="security">Инструмент</param>
        /// <param name="from">С какой даты и времени начинать закачку</param>
        /// <param name="to">До какой даты и времени закачивать</param>
        /// <param name="timeFrame">Таймфрем свечки</param>
        public void GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame)
        {
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
