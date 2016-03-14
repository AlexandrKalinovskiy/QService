using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QService.Concrete;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace QService.Admin
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        private IdentityContext _identityContext;
        private UserManager<User> _userManager;
        private static List<User> _activeUsers;

        public UserAuthentication()
        {
            _identityContext = new IdentityContext();
            _userManager = new UserManager<User>(new UserStore<User>(_identityContext));
            _activeUsers = new List<User>();
        }

        ~UserAuthentication()
        {
            Console.WriteLine("Destroy");
        }

        /// <summary>
        /// Метод принимает имя пользователя и пароль, после чего проверяет в базе на соответствие.
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
                var find = _activeUsers.Find(u => u.Id == user.Id);
                if (find != null)
                {
                    Console.WriteLine("Пользователь уже подключен");
                    throw new FaultException("Пользователь уже подключен");
                }
            }
        }

        public void SignIn(string userName)
        {
            var user = _userManager.FindByName(userName);
            _activeUsers.Add(user);
        }

        public bool SignOut(string userName)
        {
            var user = _userManager.FindByName(userName);
            if(_activeUsers.Remove(user) == true)
                return true;

            return false;
        }
    }
}
