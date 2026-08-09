using GestorPacientes.Core.Application.Helpers;
using GestorPacientes.Core.Application.Interfaces.Repositories;
using GestorPacientes.Core.Application.ViewModels.Users;
using GestorPacientes.Core.Domain.Entities;
using GestorPacientes.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestorPacientes.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();
        }

        public override async Task<User> AddAsync(User entity)
        {
            entity.Password =
                _passwordHasher.HashPassword(
                    entity,
                    entity.Password);

            await base.AddAsync(entity);

            return entity;
        }

        public async Task<User> LoginAsync(LoginViewModel loginVm)
        {
            User? user = await _dbContext
                .Set<User>()
                .FirstOrDefaultAsync(
                    u => u.Username == loginVm.Username);

            if (user == null)
            {
                return null;
            }

            PasswordVerificationResult result;

            try
            {
                result =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.Password,
                        loginVm.Password);
            }
            catch (FormatException)
            {
                // Legacy or invalid password hashes are no longer accepted.
                return null;
            }

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password =
                    _passwordHasher.HashPassword(
                        user,
                        loginVm.Password);

                await _dbContext.SaveChangesAsync();
            }

            return user;
        }

    }
}