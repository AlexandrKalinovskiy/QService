using QService.Entities;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IDataFeed" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IDataFeedCallback), SessionMode = SessionMode.Required)]
    public interface IDataFeed
    {
        [OperationContract(IsInitiating = true)]
        void Connect();

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void GetSecurities(string id, string boardCode);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void GetExchangeBoards(string code);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void SubscribeMarketData(Security security, MarketDataTypes marketDataTypes);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void UnSubscribeMarketData(Security security, MarketDataTypes marketDataTypes);
    }

    public interface IDataFeedCallback
    {
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewSecurities(IEnumerable<Security> securities);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewLevel1Values(Security security, IEnumerable<KeyValuePair<Level1, object>> changes);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewLevel2Values(Security security, IEnumerable<Level2> changes);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewCandles(IEnumerable<Candle> candles);
       
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void OnError(FaultException exception);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewNews(News news);
    }
}
