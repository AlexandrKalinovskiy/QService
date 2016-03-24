using System;
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

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class DataFeed : IDataFeed, IDisposable
    {
        private OperationContext operationContext;

        private Connector _connector;
        private EFDbContext context;
        private Listener listener;
        private static int stakeSize = 200;
        private static int conCount = 3; //Количество потоков для обработки исторических данных будет меняться в зависимости от тарифа. 3 - Basic
        private Info info;
        private UserAuthentication _userAuth;
        private string _userName;
        private User _user;
        private UManager _uManager;

        DataFeed()
        {
            context = new EFDbContext();

            operationContext = OperationContext.Current;
            operationContext.Channel.Opened += Channel_Opened;
            operationContext.Channel.Closed += Channel_Closed;
            operationContext.Channel.Faulted += Channel_Faulted;

            info = new Info();
            _uManager = new UManager(new UserStore<User>(new IdentityContext()));
 
            var test = new UserManager<User>(new UserStore<User>(new IdentityContext()));      

            _userAuth = new UserAuthentication();
            _userName = operationContext.ServiceSecurityContext.PrimaryIdentity.Name;

            _connector = GetAvialableConnector();
            _connector.ValuesChanged += _connector_Level1Changed;

            Console.WriteLine("SID: {0} ", operationContext.Channel.SessionId);

            listener = new Listener(operationContext);

            //Запускаем вторичные потоки для обработки исторических данных
            for (int i = 0; i <= conCount; i++)
            {
                new Task(listener.CandlesQueueStart).Start();
            }
        }

        //Срабатывает при обрыве канала связи с клиентом
        private void Channel_Faulted(object sender, EventArgs e)
        {
            var userAuth = new UserAuthentication();
            _uManager.SignOutAsync(_userName);

            FreeConnector(_connector);  //Освободить коннектор
            listener.IsRunned = false;  //Завершить работу вторичных потоков
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Client connected. {0}", _connector.Id);
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Client disconnected. {0}", _connector.Id);
            FreeConnector(_connector);  //Освободить коннектор
            listener.IsRunned = false;  //Завершить работу вторичных потоков

            _userAuth = new UserAuthentication();
            _uManager.SignOutAsync(_userName);
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
        private void _connector_Level1Changed(StockSharp.BusinessEntities.Security security, IEnumerable<KeyValuePair<StockSharp.Messages.Level1Fields, object>> changes, DateTimeOffset arg3, DateTime arg4)
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
                            if (_connector != null)
                                _connector.UnRegisterSecurity(security);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Могут вызывать пользователи только с ролями "Level1" и "Level2"
        /// </summary>
        /// <param name="security"></param>
        public void SubscribeLevel1(Security security)
        {
            string[] roles = { "Level1", "Level2", "Admin" };

            if (_userAuth.IsInRole(_userName, roles))
            {
                if (security != null)
                {
                    var criteria = new StockSharp.BusinessEntities.Security
                    {
                        Code = security.Ticker,
                        Id = security.Code,
                        Board = StockSharp.BusinessEntities.ExchangeBoard.Nyse
                    };
                    //Регистрируем инструмент для получения Level1
                    _connector.RegisterSecurity(criteria);
                    Console.WriteLine("Register SECURITY {0}, {1}", _connector.ConnectionState, _connector.Id);
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
            string[] roles = {"Basic", "Level1", "Level2", "Admin" };   //Доступно ролям

            if (_userAuth.IsInRole(_userName, roles))    //Доступно только ролям Basic и выше.
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

                Callback.NewSecurities(list);
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

            if (_userAuth.IsInRole(_userName, roles))    //Доступно только ролям Basic и выше.
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
        }

        public void GetExchangeBoards(string code)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            FreeConnector(_connector);  //Осовободить коннектор
            Console.WriteLine("Dispose instance {0}", _connector.Id);
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
