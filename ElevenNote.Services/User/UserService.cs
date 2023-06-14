using System;
using System.Threading.Tasks;
using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace ElevenNote.Services.User
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> RegisterUserAsync(UserRegister model)
        {
            if (await GetUserEmailAsync(model.Email) != null || await GetUserByUsernameAsync(model.Username) != null)
                return false;

            UserEntity entity = new UserEntity
            {
                Email = model.Email,
                Username = model.Username,
                DateCreated = DateTime.Now
            };

            var PasswordHasher = new PasswordHasher<UserEntity>();
            entity.Password = PasswordHasher.HashPassword(entity, model.Password);
            _context.Users.Add(entity);
            var numberOfChanges = await _context.SaveChangesAsync();

            return numberOfChanges == 1;
        }
        private async Task<UserEntity?> GetUserEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
        }

        private async Task<UserEntity?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Username.ToLower() == username.ToLower());
        }
    }
}
