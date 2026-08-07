using GestorPacientes.Core.Application.Interfaces.Services;
using GestorPacientes.Core.Application.ViewModels.LaboratoryTests;
using GestorPacientes.Core.Application.ViewModels.Users;
using GestorPacientes.Core.Application.Helpers;
using Microsoft.AspNetCore.Mvc;
using GestorPacientes.Middlewares;
using GestorPacientes.Core.Application.Enums;

namespace GestorPacientes.Controllers
{
    public class LaboratoryTestController : Controller
    {
        private readonly ILaboratoryTestService _labTestService;
        private readonly ValidateUserSession _validateUserSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserViewModel userViewModel;

        public LaboratoryTestController(ILaboratoryTestService labTestService, ValidateUserSession validateUserSession,
            IHttpContextAccessor httpContextAccessor)
        {
            this._labTestService = labTestService;
            this._validateUserSession = validateUserSession;
            this._httpContextAccessor = httpContextAccessor;
            this.userViewModel = _httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
        }

        public async Task<IActionResult> Index()
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            var list = await _labTestService.GetAllViewModel();
            return View(list);
        }

        public IActionResult Create()
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            SaveLaboratoryTestViewModel vm = new();
            return View("SaveLaboratoryTest", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveLaboratoryTestViewModel vm)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                return View("SaveLaboratoryTest", vm);
            }

            await _labTestService.Add(vm);
            return RedirectToRoute(new { controller = "LaboratoryTest", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            var labTest = await _labTestService.GetByIdSaveViewModel(id);
            return View("SaveLaboratoryTest", labTest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveLaboratoryTestViewModel vm)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                return View("SaveLaboratoryTest", vm);
            }
            
            await _labTestService.Update(vm);
            return RedirectToRoute(new { controller = "LaboratoryTest", action = "Index" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            var labTest = await _labTestService.GetByIdSaveViewModel(id);
            return View(labTest);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLabTest(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            await _labTestService.Delete(id);
            return RedirectToRoute(new { controller = "LaboratoryTest", action = "Index" });
        }
        [HttpGet]
        public async Task<IActionResult> QuickRename(int id)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var laboratoryTest =
                await _labTestService.GetByIdSaveViewModel(id);

            return View(laboratoryTest);
        }

        [HttpPost]
        public async Task<IActionResult> QuickRename(
            int id,
            string name)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var laboratoryTest =
                await _labTestService.GetByIdSaveViewModel(id);

            laboratoryTest.Name = name;

            await _labTestService.Update(laboratoryTest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> SearchUnsafe(string name)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return View(new List<LaboratoryTestViewModel>());
            }

            var results = await _labTestService.SearchUnsafe(name);

            return View(results);
        }
    }
}
