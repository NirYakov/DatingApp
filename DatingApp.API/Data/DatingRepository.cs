using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext r_Context;

        public DatingRepository(DataContext i_Context)
        {
            r_Context = i_Context;
        }

        public void Add<T>(T entity) where T : class
        {
            r_Context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            r_Context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await r_Context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await r_Context.Users.Include(p => p.Photos).ToListAsync();
            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await r_Context.SaveChangesAsync() > 0;
        }
    }
}