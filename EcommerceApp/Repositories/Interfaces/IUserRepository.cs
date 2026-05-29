using EcommerceApp.Models.Database;

namespace EcommerceApp.Repositories.Interfaces
{
    public interface IUserRepository
    {
        void Add(User user);
        Task<User> GetByEmail(string email);
        Task<User> GetById(string identifier);
    }
}
