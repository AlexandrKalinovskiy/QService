using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QService.Admin;
using StockSharp.IQFeed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QService.Concrete
{
    public static class Connectors
    {
        private static IdentityContext _identityContext;
        private static UserManager<User> _userManager;
        private static List<Connector> _connectors;
        private const int _connectorsCount = 10;
        private static Connector _connector;


        //Конструктор выполнится только один раз при первом созданиии класса
        static Connectors()
        {
            _identityContext = new IdentityContext();
            _userManager = new UserManager<User>(new UserStore<User>(_identityContext));

            Console.WriteLine("Connectors creating");
            _connectors = new List<Connector>();
            for (int i = 0; i < _connectorsCount; i++)
            {
                _connector = new Connector
                {
                    IsAvialable = true
                };
                _connector.Connect();
                Thread.Sleep(500);
                _connectors.Add(_connector);
            }

            Console.WriteLine("Connectors created");          

            //new Task(AvialableCount).Start();   //Запуск задачи мониторинга пула коннекторов
        }

        /// <summary>
        /// Метод возвращает свободный от работы коннектор.
        /// Выполняется до тех пор, пока не будет найдет свободный коннектор.
        /// </summary>
        /// <returns></returns>
        public static Connector GetAvialableConnector()
        {
            while (true)
            {
                foreach (var connector in _connectors)
                {
                    if (connector.IsAvialable)
                    {
                        connector.IsAvialable = false;
                        return connector;
                    }
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Метод освобождает указанный коннектор.
        /// </summary>
        /// <param name="connector"></param>
        public static void FreeConnector(Connector connector)
        {
            if (connector != null)
            {
                foreach(var registerSecurity in connector.RegisteredSecurities)
                {
                    connector.UnRegisterSecurity(registerSecurity); //Если имеются зарегистрированные инструменты для Level, то отписываемся от них
                }

                connector.IsAvialable = true;
            }
        }

        //Вспомогательный класс. Наследуется от IQFeedTrader.
        public class Connector: IQFeedTrader
        {
            //Свойство будет содержать Id сеанса, который в данный момент использует коннектор.
            //В случае аварийного завершения сеанса, можно будет освободить коннекторы которые были им заняты.
            //public string SessionId { get; set; }  
            public bool IsAvialable { get; set; }           
        }

        private static void AvialableCount()    //Для мониторинга в процессе разработки
        {
            int free = 0;
            int connected = 0;
            int connecting = 0;
            int failed = 0;
            while(true)
            {
                foreach(var con in _connectors)
                {
                    if(con.IsAvialable)
                        free++;

                    if (con.ConnectionState.ToString() == "Connected")
                        connected++;

                    if (con.ConnectionState.ToString() == "Connecting")
                        connecting++;

                    if (con.ConnectionState.ToString() == "Failed")
                        failed++;
                }
                Console.Clear();
                Console.WriteLine("Avialable connections {0}\nConnected {1}\nConnecting {2}\nFailed {3}", free, connected, connecting, failed);
                Thread.Sleep(500);
                free = 0;
                connected = 0;
                connecting = 0;
                failed = 0;
            }
        }
    }
}
