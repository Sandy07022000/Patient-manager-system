using GestorPacientes.Core.Application.Interfaces.Repositories;
using GestorPacientes.Core.Domain.Entities;
using GestorPacientes.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GestorPacientes.Infrastructure.Persistence.Repositories
{
    public class LaboratoryTestRepository : GenericRepository<LaboratoryTest>, ILaboratoryTestRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public LaboratoryTestRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<List<LaboratoryTest>> SearchUnsafe(string name)
        {
            string sql = "SELECT * FROM LaboratoryTests WHERE Name = '" + name + "'";
            return await _dbContext.LaboratoryTests
                .FromSqlRaw(sql)
                .ToListAsync();
        }

}
}
