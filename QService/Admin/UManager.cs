using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QService.Concrete;
using System.Collections.Generic;
using System.ServiceModel;

namespace QService.Admin
{
    public class UManager : UserManager<User>
    {
        private static IdentityContext _identityContext = new IdentityContext();
        private static List<string> _activeUsers = new List<string>();

        //public UManager()
        //        : base(new UserStore<User>(_identityContext))
        //{

        //}

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
        public async void SignInAsync(string userName)
        {
            if (_activeUsers.Contains(userName))
            {
                throw new FaultException("Пользователь уже подключен");
            }

            var user = await base.FindByNameAsync(userName);
            user.Active = true;

            await base.UpdateAsync(user);
            _activeUsers.Add(userName);
        }

        /// <summary>
        /// Метод удаляет активного пользователя из списка
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async void SignOutAsync(string userName)
        {
            var user = await base.FindByNameAsync(userName);
            user.Active = false;
            var result = await base.UpdateAsync(user);

            if (result.Succeeded)
            {
                _activeUsers.Remove(userName);
            }
        }

        /// <summary>
        /// Метод проверяет входит ли пользователь в указанную роль или нет.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        //public bool IsInRole(string userName, string[] roles)
        //{
        //    var user = _uManager.FindByName(userName);
        //    foreach (var role in roles)
        //    {
        //        if (_uManager.IsInRole(user.Id, role))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        /// <summary>
        /// Метод проверяет подключен ли пользователь или нет.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool IsUserConnected(string userName)
        {
            if (_activeUsers.Contains(userName))
            {
                return true;
            }

            return false;
        }
    }
}
