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
        List<ExchangeBoard> GetExchangeBoards(string code);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void SubscribeLevel1(Security security);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void GetHistoricalCandles(Security security, DateTime from, DateTime to, TimeSpan timeFrame);
    }

    public interface IDataFeedCallback
    {
        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewSecurities(IEnumerable<Security> securities);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewLevel1Values(Level1 level1);

        [OperationContract(IsOneWay = true, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void NewCandles(IEnumerable<Candle> candles);
    }
}
