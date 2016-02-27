using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;

namespace QService.Admin
{
    public sealed class Info
    {
        //Возвращает true - канал связи с клиентом открыт и готов к приему сообщений, false - канал связи не готов к приему сообщений
        public bool IsChannelOpened(OperationContext operationContext)
        {
            if (operationContext.Channel.State == CommunicationState.Opened)
            {
                return true;
            }

            return false;
        }
    }
}
