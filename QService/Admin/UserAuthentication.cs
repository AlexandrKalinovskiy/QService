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
        private static UManager _uManager = new UManager(new UserStore<User>(new IdentityContext()));
        private static bool _isCreated = false;

        public UserAuthentication()
        {           
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
            //var user = _uManager.FindByName(userName);
            var user = _uManager.Find(userName, password);

            if (user == null)
            {
                throw new FaultException("Неверное имя пользователя или пароль.");
            }
            else
            {
                if (!_uManager.SignIn(userName))
                {
                    throw new FaultException("Ошибка подключения. Вероятно сессия уже запущена.");
                }     
            }
        }
    }
}
