using QService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using static QService.Concrete.Connectors;

namespace QService
{
    public static class Support
    {
        public static void SubscribeNews(Connector connector, Security security)
        {
            connector.SubscribeMarketData((StockSharp.BusinessEntities.Security)security, StockSharp.Messages.MarketDataTypes.News);
        }

        /// <summary>
        /// Могут вызывать пользователи только с ролями "Level1", "Level2" и "Admin"
        /// </summary>
        /// <param name="security"></param>
        public static FaultException SubscribeLevel1(Connector connector, Security security, List<string> roles)
        {
            string[] rolesPermission = { "Level1", "Level2", "Admin" };   //Доступно ролям.

            if (rolesPermission.Intersect(roles).Any())
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
                    connector.RegisterSecurity(criteria);
                    Console.WriteLine("Register SECURITY {0}, {1}", connector.ConnectionState, connector.Id);
                }
                else
                {
                    return new FaultException("Значение инструмента не может быть неопределенным.");
                }
            }
            else
            {
                return new FaultException("Level1 недоступен для этого аккаунта.");
            }

            return null;
        }

        public static bool UnSubscribeLevel1(Connector connector, Security security)
        {
            if (security != null)
            {
                var criteria = new StockSharp.BusinessEntities.Security
                {
                    Code = security.Ticker,
                    Id = security.Code,
                    Board = StockSharp.BusinessEntities.ExchangeBoard.Nyse
                };
                //Отписываемся от получения новой информации по Level1
                connector.UnRegisterSecurity(criteria);
                return true;
            }

            return false;
        }
    }
}
