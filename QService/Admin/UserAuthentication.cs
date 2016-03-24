using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QService.Concrete;
using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using static QService.Concrete.Connectors;

namespace QService.Admin
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        private UManager _uManager;
        private static bool _isCreated = false;

        public UserAuthentication()
        {
            _uManager = new UManager();
            Console.WriteLine("UserAuthentication created");
            if (!_isCreated)
            {
                var connector = GetAvialableConnector();    //Только для того чтобы инициализировать класс Connectors
                FreeConnector(connector);
                _isCreated = true;
            }
        }

        ~UserAuthentication()
        {
            Console.WriteLine("UserAuthentication destroy");
        }

        /// <summary>
        /// Метод принимает имя пользователя и пароль, после чего проверяет в базе на соответствие. Если пользователь найден в базе и еще не подключен к сервису, 
        /// срабатывает создание экземпляра и установление сеанса связи.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public override void Validate(string userName, string password)
        {
            var user = _uManager.FindByName(userName);

            if (user == null)
            {
                throw new FaultException("Пользователь не найден.");
            }
            else
            {
                if(_uManager.IsUserConnected(userName))
                {
                    throw new FaultException("Пользователь уже подключен.");
                }
                _uManager.SignInAsync(userName);        
            }
        }
    }
}
