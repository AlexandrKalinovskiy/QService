using QService.Entities;
using System.Collections.Generic;
using System.ServiceModel;

namespace QService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IDataFeed" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IDataFeedCallback))]
    public interface IDataFeed
    {
        [OperationContract(IsOneWay = true)]
        void GetSecurities(string id, string boardCode);

        [OperationContract]
        List<ExchangeBoard> GetExchangeBoards(string code);

        [OperationContract(IsOneWay = true)]
        void SubscribeLevel1(string security);
    }

    public interface IDataFeedCallback
    {
        [OperationContract(IsOneWay = true)]
        void NewSecurities(IEnumerable<Security> securities);

        [OperationContract]
        void NewCandles(string candles);

        [OperationContract(IsOneWay = true)]
        void NewLevel1Values(decimal BestBidPrice, decimal BestAskPrice);
    }
}
