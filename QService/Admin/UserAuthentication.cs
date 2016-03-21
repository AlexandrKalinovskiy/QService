using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QService.Concrete;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.Threading.Tasks;

namespace QService.Admin
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        private IdentityContext _identityContext;
        private UserManager<User> _userManager;
        public static List<User> _activeUsers;

        public UserAuthentication()
        {
            _identityContext = new IdentityContext();
            _userManager = new UserManager<User>(new UserStore<User>(_identityContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_identityContext));
            _activeUsers = new List<User>();
            Console.WriteLine("CONNECT NEW USER");
        }

        ~UserAuthentication()
        {
            Console.WriteLine("Destroy, active users {0}", _activeUsers.Count);
        }

        /// <summary>
        /// Метод принимает имя пользователя и пароль, после чего проверяет в базе на соответствие. Если пользователь найден в базе и еще не подключен к сервису, 
        /// срабатывает создание экземпляра и установление сеанса связи.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public override void Validate(string userName, string password)
        {
            var user = _userManager.FindByName(userName);

            Console.WriteLine("Users count {0}", _activeUsers.Count);

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

        public bool IsInRole(string userName, string roleName)
        {
            var user = _userManager.FindByName(userName);
            if (_userManager.IsInRole(user.Id, roleName))
            {
                return true;
            }

            return false;
        }
    }
}
