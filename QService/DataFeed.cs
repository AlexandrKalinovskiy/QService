﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using QService.Entities;
using QService.Concrete;
using StockSharp.Algo.Candles;
using QService.Admin;
using static QService.Concrete.Connectors;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StockSharp.Messages;
using static QService.Support;

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class DataFeed : IDataFeed, IDisposable
    {
        private OperationContext operationContext;
        private Connector _connector;
        private EFDbContext _context;
        private Listener _listener;
        private static int _stakeSize = 200;
        private int _conCount = 1; //Количество потоков для обработки исторических данных будет меняться в зависимости от тарифа. 1 - Default
        private Info info;
        private UManager _uManager;
        private User _user;
        private List<string> _roles;   //Коллекция ролей пользователя. Служит для снижения нагрузки на базу данных при частых вызовах методов

        DataFeed()
        {
            _context = new EFDbContext();

            operationContext = OperationContext.Current;
            operationContext.Channel.Opened += Channel_Opened;
            operationContext.Channel.Closed += Channel_Closed;

            info = new Info();
            _uManager = new UManager(new UserStore<User>(new IdentityContext()));
            _user = _uManager.FindByName(operationContext.ServiceSecurityContext.PrimaryIdentity.Name); //Получаем текущего Identity пользователя 

            var roles = _uManager.GetUserRoles(_user.Id);  //Создадим список ролей пользователя к которым будем обращаться в методах для проверки, чтобы не загружать БД лишними запросами.

            _roles = roles.Select(r => r.Name).ToList();

            _conCount = roles.Max(r => r.NumberOfThreads);  //Установить максимальное количество потоков доступное из ролей данному пользователю

            _connector = GetAvialableConnector();
            _connector.ValuesChanged += Level1Changed;
            _connector.MarketDepthsChanged += Level2Changed;
            _connector.NewNews += NewNews;
            _connector.Error += Error;

            Console.WriteLine("SID: {0} ", operationContext.Channel.SessionId);

            _listener = new Listener(operationContext);

            //Запускаем вторичные потоки для обработки исторических данных
            for (int i = 0; i < _conCount; i++)
            {
                new Task(_listener.CandlesQueueStart).Start();
            }
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            FreeConnector(_connector);  //Осовободить коннектор
            _listener.IsRunned = false;  //Завершить работу вторичных потоков
            _uManager.SignOut(_user.UserName);
            Console.WriteLine("Closed channel {0}", _connector.Id);
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Client connected. {0}", _connector.Id);
        }

        ~DataFeed()
        {
            Console.WriteLine("Destroy instance {0}", _connector.Id);
        }

        public void Connect()
        {

        }

        /// <summary>
        /// Метод срабатывает при поступлении новых значений Level1
        /// </summary>
        /// <param name="security"></param>
        /// <param name="changes"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        private void Level1Changed(StockSharp.BusinessEntities.Security security, IEnumerable<KeyValuePair<Level1Fields, object>> changes, DateTimeOffset arg3, DateTime arg4)
        {
            if (info.IsChannelOpened(operationContext))
            {
                List<KeyValuePair<Level1, object>> listChanges = new List<KeyValuePair<Level1, object>>();
                foreach (var change in changes)
                {
                    if (change.Key != (Level1Fields)Level1.BestBidTime && change.Key != (Level1Fields)Level1.BestAskTime)
                    {
                        var kV = new KeyValuePair<Level1, object>((Level1)change.Key, change.Value);
                        listChanges.Add(kV);
                    }
                }

                if (operationContext.Channel.State == CommunicationState.Opened)
                {
                    Callback.NewLevel1Values((Security)security, listChanges);
                }
            }
        }


        /// <summary>
        /// Могут вызывать пользователи с ролями "Basic", "Level1" и "Level2"
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="exchangeBoardCode"></param> 
        public void GetSecurities(string ticker, string exchangeBoardCode)
        {
            string[] roles = { "Basic", "Level1", "Level2", "Admin" };   //Доступно ролям

            if (roles.Intersect(_roles).Any())  //Доступно только ролям Basic и выше.
            {
                try
                {
                    var securities = new List<Security>();

                    if (ticker != null && ticker != string.Empty)   //Если указан тикер бумаги
                    {
                        securities = _context.Securities.Where(s => s.Ticker == ticker).ToList();

                        if (securities.Count == 0)
                            Callback.NewSecurities(new List<Security>());    //Возвратить пустой список в случае отсутствия подходящей бумаги
                    }
                    else if (exchangeBoardCode != null && exchangeBoardCode != string.Empty)
                    {
                        var exchangeBoard = _context.ExchangeBoards.Where(e => e.Code == exchangeBoardCode).FirstOrDefault();

                        if (exchangeBoard != null)
                        {
                            securities = _context.Securities.Where(s => s.ExchangeBoard.Id == exchangeBoard.Id).ToList();

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

                        if (list.Count >= _stakeSize)
                        {
                            try
                            {
                                Callback.NewSecurities(list);
                                list.Clear();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("{0}", e);
                                break;
                            }
                        }
                    };

                    Callback.NewSecurities(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Метод возвращает исторические свечки с интервалом от секунды до месяца. Для ролей "Basic", "Level1" и "Level2"
        /// </summary>
        /// <param name="security">Инструмент</param>
        /// <param name="from">С какой даты и времени начинать закачку</param>
        /// <param name="to">До какой даты и времени закачивать</param>
        /// <param name="timeFrame">Таймфрем свечки</param>
        public void GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame)
        {
            string[] roles = { "Basic", "Level1", "Level2", "Admin" };   //Доступно ролям

            if (roles.Intersect(_roles).Any())  //Доступно только ролям Basic и выше.
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

                _listener.requestCandlesQueue.Enqueue(requestCandlies);
            }
        }

        public void GetExchangeBoards(string code)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Метод подписывается на получение рыночной информации
        /// </summary>
        /// <param name="security"></param>
        /// <param name="marketDataTypes"></param>
        public void SubscribeMarketData(Security security, Entities.MarketDataTypes marketDataTypes)
        {
            if (security == null)
            {
                Callback.OnError(new FaultException("Значение инструмента не может быть неопределенным.")); //Если объект инструмента не создан
            }
            else {
                FaultException result = null;

                var criteria = _context.Securities.Where(s => s.Ticker == security.Ticker).FirstOrDefault();

                if (criteria != null)
                {
                    switch (marketDataTypes)
                    {
                        case Entities.MarketDataTypes.Level1:
                            result = SubscribeLevel1(_connector, criteria, _roles);     //Подписываемся на получение Level1.
                            if (result != null)
                                Callback.OnError(result);
                            break;
                        case Entities.MarketDataTypes.News:
                            SubscribeNews(_connector, criteria);                            //Подписываемся на новости.
                            break;
                        case Entities.MarketDataTypes.Level2:
                            result = SubscribeLevel2(_connector, criteria, _roles);             //Подписываемся на стакан.
                            if (result != null)
                                Callback.OnError(result);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Callback.OnError(new FaultException("Инструмент не найден."));  //Если объект инструмента создан, но такого инструмента нет в базе.
                }
            }    
        }

        public void UnSubscribeMarketData(Security security, Entities.MarketDataTypes marketDataTypes)
        {
            switch(marketDataTypes)
            {
                case Entities.MarketDataTypes.Level1:
                    UnSubscribeLevel1(_connector, security);    //Отписываемся от получения Level1 по указанному инструменту
                    break;
                case Entities.MarketDataTypes.News:
                    UnSubscribeNews(_connector, security);
                    break;
                case Entities.MarketDataTypes.Level2:
                    UnSubscribeLevel2(_connector, security);
                    break;
            }
            _connector.UnSubscribeMarketData((StockSharp.BusinessEntities.Security)security, (StockSharp.Messages.MarketDataTypes)marketDataTypes);
        }

        //Callback for SubscribeMarketData -> Level2
        private void Level2Changed(IEnumerable<StockSharp.BusinessEntities.MarketDepth> marketDepths)
        {
            Console.WriteLine("MarketDepth {0}", marketDepths.FirstOrDefault().BestAsk);
            var listMarketDepths = new List<Level2>();
            foreach (var marketDepth in marketDepths)
            {
                listMarketDepths.Add((Level2)marketDepth);             
            }
            Callback.NewLevel2Values((Security)marketDepths.FirstOrDefault().Security, listMarketDepths);
        }

        //Callback for SubscribeMarketData -> NewNews
        private void NewNews(StockSharp.BusinessEntities.News news)
        {
            Console.WriteLine("NewNews {0}", news.Headline);
            Callback.NewNews((News)news);
        }

        public void Dispose()
        {
            FreeConnector(_connector);  //Осовободить коннектор
            _listener.IsRunned = false;  //Завершить работу вторичных потоков
            _uManager.SignOut(_user.UserName);  //Завершить сеанс пользователя
            Console.WriteLine("Dispose instance {0}", _connector.Id);
        }
       
        /// <summary>
        /// Метод для обработки исключений во время выполнения
        /// </summary>
        /// <param name="obj"></param>
        private void Error(Exception exception)
        {
            Console.WriteLine("Error {0}", exception);
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
