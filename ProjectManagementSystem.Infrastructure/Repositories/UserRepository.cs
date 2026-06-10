using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class UserRepository(AppDbContext db, IMapper mapper) : IUserRepository
{
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await db.Users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
        return mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, string passwordHash, bool forcePasswordChange)
    {
        var user = mapper.Map<User>(dto);
        user.PasswordHash = passwordHash;
        user.ForcePasswordChange = forcePasswordChange;
        user.CreatedAt = DateTime.UtcNow;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return mapper.Map<UserDto>(user);
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, bool forcePasswordChange)
    {
        var user = await db.Users.FindAsync(userId)
                   ?? throw new NotFoundException(ErrorMessages.UserNotFoundById(userId));
        user.PasswordHash = passwordHash;
        user.ForcePasswordChange = forcePasswordChange;
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int userId, bool isActive)
    {
        var user = await db.Users.FindAsync(userId)
                   ?? throw new NotFoundException(ErrorMessages.UserNotFoundById(userId));
        user.IsActive = isActive;
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string username, string email) =>
        await db.Users.AnyAsync(u => u.Username == username || u.Email == email);

    public async Task<(string PasswordHash, bool ForcePasswordChange, bool IsActive, int UserId, string FullName, string Role)?> GetCredentialsAsync(string username)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;
        return (user.PasswordHash, user.ForcePasswordChange, user.IsActive, user.Id, user.FullName, user.Role.ToString());
    }
}
