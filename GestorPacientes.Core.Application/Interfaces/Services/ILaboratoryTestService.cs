using GestorPacientes.Core.Application.ViewModels.LaboratoryTests;

namespace GestorPacientes.Core.Application.Interfaces.Services
{
    public interface ILaboratoryTestService : IGenericService<SaveLaboratoryTestViewModel, LaboratoryTestViewModel>
    {
        Task<List<LaboratoryTestViewModel>> SearchUnsafe(string name);
    }
}
