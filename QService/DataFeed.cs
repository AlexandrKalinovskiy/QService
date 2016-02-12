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

namespace QService
{
    //    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "DataFeed" в коде и файле конфигурации.
    public class DataFeed : IDataFeed
    {
        private IQFeedTrader connector;
        List<string> list = new List<string>();
        OperationContext operationContext;
        private EFDbContext context;
        
        DataFeed()
        {
            context = new EFDbContext();
            //context.Configuration.ProxyCreationEnabled = false;

            operationContext = OperationContext.Current;
            operationContext.Channel.Closed += Channel_Closed;

            connector = new IQFeedTrader();
            //connector.ValuesChanged += Connector_Level1Changed;

            connector.Connect();

            Thread.Sleep(500);

            Console.WriteLine("IQFeed connection state: {0} {1} ", connector.ConnectionState, connector.Id);

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

        public void SubscribeLevel1(string security)
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

        //public List<Security> GetSecurities(string ticker, string exchangeBoardCode)
        //{
        //    var securities = new List<Security>();

        //    if (ticker != null && ticker != string.Empty)   //Если указан тикер бумаги
        //    {
        //        securities = context.Securities.Where(s => s.Ticker == ticker).ToList();

        //        if (securities.Count == 0)
        //            return new List<Security>();    //Возвратить пустой список в случае отсутствия подходящей бумаги
        //    }
        //    else if (exchangeBoardCode != null && exchangeBoardCode != string.Empty)
        //    {
        //        var exchangeBoard = context.ExchangeBoards.Where(e => e.Code == exchangeBoardCode).FirstOrDefault();

        //        if (exchangeBoard != null)
        //        {
        //            securities = context.Securities.Where(s => s.ExchangeBoard.Id == exchangeBoard.Id).ToList();

        //            if (securities.Count == 0)
        //                return new List<Security>();    //Возвратить пустой список в случае отсутствия подходящих бумаг
        //        }
        //    }

        //    //Выполнить преобразование в "чистую" модель данных, в случае если найдены бумаги по указанным критериям
        //    var list = new List<Security>();

        //    foreach (var security in securities)
        //    {
        //        list.Add(new Security
        //        {
        //            Id = security.Id,
        //            Code = security.Code,
        //            Name = security.Name,
        //            StepPrice = security.StepPrice,
        //            Ticker = security.Ticker,
        //            ExchangeBoard = new ExchangeBoard
        //            {
        //                Id = security.ExchangeBoard.Id,
        //                Code = security.ExchangeBoard.Code,
        //                Name = security.ExchangeBoard.Name,
        //                Description = security.ExchangeBoard.Description
        //            }
        //        });

        //        operationContext.GetCallbackChannel<IDataFeedCallback>().NewSecurities(list);
        //        list.Clear();
        //    }         

        //    return list;
        //}

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

                operationContext.GetCallbackChannel<IDataFeedCallback>().NewSecurities(list);
                list.Clear();
            }
        }

        public List<ExchangeBoard> GetExchangeBoards(string exchangeBoardCode)
        {
            var boards = context.ExchangeBoards.Where(b => b.Code == exchangeBoardCode).ToList();

            if (boards == null)
                return context.ExchangeBoards.ToList();

            return boards;
        }
    }
}
