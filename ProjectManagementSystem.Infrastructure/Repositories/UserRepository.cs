using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Validation;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class UserRepository(AppDbContext db, IMapper mapper) : IUserRepository
{
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await db.Users.Include(u => u.Role)
            .OrderBy(u => u.Role.Name).ThenBy(u => u.FullName).ToListAsync();
        return mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, string passwordHash, DateTime passwordExpiresAt)
    {
        var roleId = await RoleResolver.GetRoleIdAsync(db, dto.Role);
        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Username = dto.Username,
            Designation = string.Empty,
            RoleId = roleId,
            PasswordHash = passwordHash,
            PasswordExpiresAt = passwordExpiresAt,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Role).LoadAsync();
        return mapper.Map<UserDto>(user);
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, DateTime passwordExpiresAt)
    {
        var user = await db.Users.FindAsync(userId)
                   ?? throw new NotFoundException(ErrorMessages.UserNotFoundById(userId));
        user.PasswordHash = passwordHash;
        user.PasswordExpiresAt = passwordExpiresAt;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int userId, bool isActive)
    {
        var user = await db.Users.FindAsync(userId)
                   ?? throw new NotFoundException(ErrorMessages.UserNotFoundById(userId));
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string username, string email) =>
        await db.Users.AnyAsync(u => u.Username == username || u.Email == email);

    public async Task<(string PasswordHash, DateTime PasswordExpiresAt, bool IsActive, int UserId, string FullName, string Role)?> GetCredentialsAsync(string username)
    {
        var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;
        return (user.PasswordHash, user.PasswordExpiresAt, user.IsActive, user.Id, user.FullName, user.Role.Name);
    }
}
