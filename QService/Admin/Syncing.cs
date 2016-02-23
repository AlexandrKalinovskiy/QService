using QService.Concrete;
using QService.Entities;
using StockSharp.IQFeed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace QService.Admin
{
    /// <summary>
    /// Класс позволяет поддерживать базу данных в актульном состоянии. 
    /// Выполняется синхронизация списка ценных бумаг и исторических данных c таймфреймами - день, неделя, месяц, год.
    /// </summary>
    public class Syncing
    {
        private EFDbContext context;
        private IQFeedTrader connector;

        private int addedCount;

        public Syncing()
        {
            context = new EFDbContext();

            connector = new IQFeedTrader();
            connector.Connect();

            addedCount = 0;

            Thread.Sleep(1000);

            Console.WriteLine("Connection state {0}", connector.ConnectionState);
        }

        public int SyncSecurities()
        {
            connector.NewSecurities += Connector_NewSecurities;
            connector.LookupSecuritiesResult += Connector_LookupSecuritiesResult;

            var criteria = new StockSharp.BusinessEntities.Security
            {
                Code = ""
            };

            connector.LookupSecurities(criteria);

            return 0;
        }

        private void Connector_LookupSecuritiesResult(IEnumerable<StockSharp.BusinessEntities.Security> securities)
        {
            Console.WriteLine("Добавлено {0} инструмент(ов)", securities.Count());
        }

        private void Connector_NewSecurities(IEnumerable<StockSharp.BusinessEntities.Security> securities)
        {
            foreach (var security in securities)
            {
                if (security.Board.Code == "NYSE" || security.Board.Code == "NYSE_ARCA")
                {
                    var sec = context.Securities.Where(s => s.Ticker == security.Code).ToList();
                    if (sec.Count == 0)
                    {
                        string boardCode = "NYSE";
                        var exchangeBoard = context.ExchangeBoards.FirstOrDefault(e => e.Code == boardCode);

                        Console.WriteLine("{0} {1} {2}", security.Code, security.Board.Code, security.Name);
                        var secDb = new Entities.Security
                        {
                            Ticker = security.Code,
                            Code = security.Id,
                            Name = security.Name,
                            ExchangeBoard = exchangeBoard
                        };

                        context.Securities.Add(secDb);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
