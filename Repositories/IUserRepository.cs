using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Add(User user);
        void Save();
    }
}
