using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QService.Concrete;
using QService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace QService.Admin
{
    public class UManager : UserManager<User>
    {
        private static IdentityContext _identityContext = new IdentityContext();
        private static List<string> _activeUsers = new List<string>();
        private static UManager _uManager = new UManager(new UserStore<User>(new IdentityContext()));
        private static RoleManager<ApplicationRole> _rManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new IdentityContext()));

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

        public List<ApplicationRole> GetUserRoles(string userId)
        {
            var user = _uManager.FindById(userId);
            var userRoles = _uManager.GetRoles(userId);
            var allRoles = _rManager.Roles.ToList();

            var rolesList = new List<ApplicationRole>();
            foreach(var role in userRoles)
            {
                var r = _rManager.FindByName(role);
                rolesList.Add(r);   
            }

            return rolesList;
        }
    }
}
