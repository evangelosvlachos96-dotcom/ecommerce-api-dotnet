using EcommerceApp.Data;
using EcommerceApp.Models.Database;
using EcommerceApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EcommerceContext _context;
        public UserRepository(EcommerceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }
        public async Task<User> GetByEmail(string email)
        {   
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User> GetById(string identifier)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == identifier);
        }
    }
}
