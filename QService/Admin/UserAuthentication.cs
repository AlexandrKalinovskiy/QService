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
        private IdentityContext _identityContext;
        private UserManager<User> _userManager;
        private static bool _isCreated = false;

        public UserAuthentication()
        {
            _identityContext = new IdentityContext();
            _userManager = new UserManager<User>(new UserStore<User>(_identityContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_identityContext));
            Console.WriteLine("UserAuthentication created");
            if (!_isCreated)
            {
                var connector = GetAvialableConnector();
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
        public async override void Validate(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new FaultException("Пользователь не найден.");
            }
            else
            {
                if (user.Active)
                {
                    Console.WriteLine("Пользователь уже подключен");
                    throw new FaultException("Пользователь уже подключен");
                }
            }
        }

        /// <summary>
        /// Метод регистрирует активного пользователя
        /// </summary>
        /// <param name="userName"></param>
        public void SignIn(string userName)
        {
            var user = _userManager.FindByName(userName);
            user.Active = true;
            _userManager.Update(user);
        }

        /// <summary>
        /// Метод удаляет активного пользователя из списка
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool SignOut(string userName)
        {
            var user = _userManager.FindByName(userName);
            user.Active = false;
            var result = _userManager.Update(user);

            if (result.Succeeded)
                return true;

            return false;
        }

        /// <summary>
        /// Метод проверяет входит ли пользователь в указанную роль или нет.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsInRole(string userName, string[] roles)
        {
            var user = _userManager.FindByName(userName);
            foreach (var role in roles)
            {
                if (_userManager.IsInRole(user.Id, role))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
