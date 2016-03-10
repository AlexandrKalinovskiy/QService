using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Security.Authentication;
using System.ServiceModel;

namespace QService.Admin
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        List<User> activeUsers = new List<User>();  //Список всех активных пользователей

        /// <summary>
        /// Метод принимает имя пользователя и пароль, после чего проверяет в базе на соответствие.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public override void Validate(string userName, string password)
        {
            var user = new User
            {
                UserName = userName,
                Password = password
            };

            var find = activeUsers.Find(u => u.UserName == userName);

            if (find == null)
            {
                Console.WriteLine("Пользователь авторизован.");
                activeUsers.Add(user);
            }
            else
            {
                Console.WriteLine("Пользователь уже подключен");
                throw new FaultException("Пользователь уже подключен");
            }              
        }
    }
}
