using MusicPlayer.Domain.Entities;

namespace MusicPlayer.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}