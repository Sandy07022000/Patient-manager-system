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

            // First try the new secure password format.
            try
            {
                PasswordVerificationResult result =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.Password,
                        loginVm.Password);

                if (result == PasswordVerificationResult.Success ||
                    result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    if (result ==
                        PasswordVerificationResult.SuccessRehashNeeded)
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
            catch (FormatException)
            {
                // Existing legacy hashes are checked below.
            }

            // Temporary migration support for existing SHA-256 accounts.
            string legacyHash =
                PasswordEncryptation.ComputeSha256Hash(
                    loginVm.Password);

            if (user.Password == legacyHash)
            {
                // Password was correct using legacy SHA-256.
                // Immediately upgrade it to the secure password hash.
                user.Password =
                    _passwordHasher.HashPassword(
                        user,
                        loginVm.Password);

                await _dbContext.SaveChangesAsync();

                return user;
            }

            return null;
        }
    }
}