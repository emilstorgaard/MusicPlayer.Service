using MusicPlayer.Domain.Entities;

namespace MusicPlayer.Domain.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllUsers();
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(int id);
    Task AddUser(User user);
    Task Delete(User user);
}