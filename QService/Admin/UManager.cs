using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QService.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QService.Admin
{
    public class UManager : UserManager<User>
    {
        private static IdentityContext _identityContext = new IdentityContext();
        private static List<string> _activeUsers;

        public UManager()
                : base(new UserStore<User>(_identityContext))
        {
            _activeUsers = new List<string>();
        }

        /// <summary>
        /// Метод регистрирует активного пользователя
        /// </summary>
        /// <param name="userName"></param>
        public async Task<bool> SignIn(string userName)
        {
            if (_activeUsers.Contains(userName))
            {
                return false;
            }
            var user = await base.FindByNameAsync(userName);
            user.Active = true;

            await base.UpdateAsync(user);
            _activeUsers.Add(userName);
            return true;
        }

        /// <summary>
        /// Метод удаляет активного пользователя из списка
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> SignOut(string userName)
        {
            var user = await base.FindByNameAsync(userName);
            user.Active = false;
            var result = await base.UpdateAsync(user);

            if (result.Succeeded)
            {
                _activeUsers.Remove(userName);
                return true;
            }

            return false;
        }
    }
}
