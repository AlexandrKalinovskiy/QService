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
        /// <summary>
        /// Метод подписывает пользователя на получение новостей по указанному инструменту.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="security"></param>
        public static void SubscribeNews(Connector connector, Security security)
        {
            connector.SubscribeMarketData((StockSharp.BusinessEntities.Security)security, StockSharp.Messages.MarketDataTypes.News);
        }

        /// <summary>
        /// Метод отписывает пользователя от получения новостей по указанному инструменту.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="security"></param>
        public static void UnSubscribeNews(Connector connector, Security security)
        {
            connector.UnSubscribeMarketData((StockSharp.BusinessEntities.Security)security, StockSharp.Messages.MarketDataTypes.News);
        }

        /// <summary>
        /// Метод подписывает пользователя на получение новых значений Level1.
        /// Могут использовать только пользователи с ролями: "Level1", "Level2" и "Admin".
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

        /// <summary>
        /// Метод отписывает пользователя от получения новых значений Level1.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="security"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод подписывает пользователя на получения новых значений по стакану.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="security"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static FaultException SubscribeLevel2(Connector connector, Security security, List<string> roles)
        {
            string[] rolesPermission = { "Level2", "Admin" };   //Доступно ролям.
            if (rolesPermission.Intersect(roles).Any())
            {
                if (security != null)
                {
                    connector.SubscribeMarketData((StockSharp.BusinessEntities.Security)security, StockSharp.Messages.MarketDataTypes.MarketDepth);
                    Console.WriteLine("Level2 subscribed");
                }
                else
                {
                    return new FaultException("Значение инструмента не может быть неопределенным.");
                }
            }
            else
            {
                return new FaultException("Level2 недоступен для этого аккаунта.");
            }

            return null;
        }

        /// <summary>
        /// Метод отписывает пользователя от получения новых значений по стакану.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="security"></param>
        /// <returns></returns>
        public static FaultException UnSubscribeLevel2(Connector connector, Security security)
        {
            if (security != null)
            {
                connector.UnSubscribeMarketData((StockSharp.BusinessEntities.Security)security, StockSharp.Messages.MarketDataTypes.MarketDepth);
            }
            else
            {
                return new FaultException("Значение инструмента не может быть неопределенным.");
            }

            return null;
        }
    }
}
