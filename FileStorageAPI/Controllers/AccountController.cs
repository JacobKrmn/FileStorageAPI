using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AccountDataAcccess;




namespace FileStorageAPI.Controllers
{
    public class AccountController : ApiController
    {
        public IEnumerable<user> Get() {
            using (AccountDBEntities entities = new AccountDBEntities()) {
                return entities.users.ToList();
            }
        }

        public user Get(int id) {
            using (AccountDBEntities entities = new AccountDBEntities()) {
                return entities.users.FirstOrDefault(e => e.id == id);
            }
        }

        public void Post(string username, string password, string email, string fullname) {
            user user = new user
            {
                username = username,
                password = password,
                email = email,
                fullname = fullname
            };
            using (AccountDBEntities entities = new AccountDBEntities()) {
                entities.users.Add(user);
                entities.SaveChanges();
            }
        }

        public void Put(int id, string column, string value) {
            using (AccountDBEntities entities = new AccountDBEntities()) {
                user user = (from usr in entities.users
                             where usr.id == id
                             select usr).First();

                switch (column) {
                    case "username":
                        user.username = value;
                        break;
                    case "password":
                        user.password = value;
                        break;
                    case "email":
                        user.email = value;
                        break;
                    case "fullname":
                        user.fullname = value;
                        break;
                    default:
                        break;
                }
                entities.SaveChanges();
            }
        }

        public void Delete(int id) {
            using (AccountDBEntities entities = new AccountDBEntities()) {
                user user = (from usr in entities.users
                             where usr.id == id
                             select usr).First();
                entities.users.Remove(user);
                entities.SaveChanges();
            }
        }

    }
}
