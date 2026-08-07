using GestorPacientes.Core.Domain.Entities;

namespace GestorPacientes.Core.Application.Interfaces.Repositories
{
    public interface ILaboratoryTestRepository : IGenericRepository<LaboratoryTest>
    {
        Task<List<LaboratoryTest>> SearchUnsafe(string name);
    }
}