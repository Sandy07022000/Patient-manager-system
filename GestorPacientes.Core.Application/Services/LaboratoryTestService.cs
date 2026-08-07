using GestorPacientes.Core.Application.Interfaces.Repositories;
using GestorPacientes.Core.Application.Interfaces.Services;
using GestorPacientes.Core.Application.ViewModels.LaboratoryTests;
using GestorPacientes.Core.Domain.Entities;

namespace GestorPacientes.Core.Application.Services
{
    public class LaboratoryTestService : ILaboratoryTestService
    {
        private readonly ILaboratoryTestRepository _labTestRepository;

        public LaboratoryTestService(ILaboratoryTestRepository labTestRepository)
        {
            this._labTestRepository = labTestRepository;
        }

        public async Task<SaveLaboratoryTestViewModel> Add(SaveLaboratoryTestViewModel vm)
        {
            LaboratoryTest labTest = new();
            labTest.Name = vm.Name;

            labTest = await _labTestRepository.AddAsync(labTest);

            SaveLaboratoryTestViewModel labTestVm = new();
            labTestVm.Id = labTest.Id;
            labTestVm.Name = labTest.Name;

            return labTestVm;
        }

        public async Task Update(SaveLaboratoryTestViewModel vm)
        {
            LaboratoryTest labTest = await _labTestRepository.GetByIdAsync(vm.Id);
            labTest.Id = vm.Id;
            labTest.Name = vm.Name;

            await _labTestRepository.UpdateAsync(labTest);
        }

        public async Task Delete(int id)
        {
            var labTest = await _labTestRepository.GetByIdAsync(id);
            await _labTestRepository.DeleteAsync(labTest);
        }

        public async Task<SaveLaboratoryTestViewModel> GetByIdSaveViewModel(int id)
        {
            var labTest = await _labTestRepository.GetByIdAsync(id);

            SaveLaboratoryTestViewModel vm = new();
            vm.Id = labTest.Id;
            vm.Name = labTest.Name;

            return vm;
        }

        public async Task<List<LaboratoryTestViewModel>> GetAllViewModel()
        {
            var list = await _labTestRepository.GetAllAsync();
            return list.Select(lb => new LaboratoryTestViewModel
            {
                Id = lb.Id,
                Name = lb.Name

            }).ToList();
        }

        public async Task<List<LaboratoryTestViewModel>> SearchUnsafe(string name)
        {
            var list = await _labTestRepository.SearchUnsafe(name);

            return list.Select(labTest => new LaboratoryTestViewModel
            {
                Id = labTest.Id,
                Name = labTest.Name
            }).ToList();
        }

    }
}
