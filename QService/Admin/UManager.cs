using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QService.Concrete;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace QService.Admin
{
    public class UManager : UserManager<User>
    {
        private static IdentityContext _identityContext = new IdentityContext();
        private static List<string> _activeUsers = new List<string>();
        private static UManager _uManager = new UManager(new UserStore<User>(new IdentityContext()));

        public UManager(IUserStore<User> store) 
            : base(store) 
        {
        }
        public static UManager Create(IdentityFactoryOptions<UManager> options,
                                                IOwinContext context)
        {
            IdentityContext db = context.Get<IdentityContext>();
            UManager manager = new UManager(new UserStore<User>(db));

            return manager;
        }

        /// <summary>
        /// Метод регистрирует активного пользователя
        /// </summary>
        /// <param name="userName"></param>
        public bool SignIn(string userName)
        {
            if (_activeUsers.Contains(userName))
            {
                return false;
            }

            var user = _uManager.FindByName(userName);

            try
            {
                user.Active = true;
                _uManager.Update(user);
                _activeUsers.Add(userName);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Метод удаляет активного пользователя из списка
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool SignOut(string userName)
        {
            var user = _uManager.FindByName(userName);
            user.Active = false;

            try
            {
                _uManager.Update(user);
                _activeUsers.Remove(userName);
            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Метод проверяет содержится ли пользователь в хотябы одной из указанных ролей
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        //public bool IsInRoles(string userId, string[] roles)
        //{
        //    foreach(var role in roles)
        //    {
        //        if(_uManager.IsInRole(userId, role))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}
