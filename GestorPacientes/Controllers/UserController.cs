using GestorPacientes.Core.Application.Interfaces.Services;
using GestorPacientes.Core.Application.ViewModels.Users;
using GestorPacientes.Core.Application.Helpers;
using Microsoft.AspNetCore.Mvc;
using GestorPacientes.Middlewares;
using GestorPacientes.Core.Application.Enums;

namespace GestorPacientes.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ValidateUserSession _validateUserSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserViewModel? userViewModel;

        public UserController(IUserService userService, ValidateUserSession validateUserSession,
            IHttpContextAccessor httpContextAccessor)
        {
            this._userService = userService;
            this._validateUserSession = validateUserSession;
            this._httpContextAccessor = httpContextAccessor;
            this.userViewModel = _httpContextAccessor.HttpContext.Session.Get<UserViewModel>("user");
        }


        public IActionResult Login()
        {
            if (_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            UserViewModel userVm = await _userService.Login(vm);

            if (userVm != null)
            {
                HttpContext.Session.Set<UserViewModel>("user", userVm);
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            else
            {
                ModelState.AddModelError(nameof(userVm), "Datos de acceso incorrecto");
            }

            Console.WriteLine($"User password submitted: {userVm.Password}");

            return View(vm);
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("user");
            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        public async Task<IActionResult> Index()
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            var list = await _userService.GetAllViewModel();
            return View(list);
        }

        public IActionResult Register()
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return View("Register", new SaveUserViewModel());
            }

            return View("SaveUser", new SaveUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(SaveUserViewModel userVm)
        {
            if (!ModelState.IsValid)
            {
                return View(userVm);
            }

            userVm.TypeUserId = Roles.Admin;

            await _userService.Add(userVm);
            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveUserViewModel userVm)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                return View(userVm);
            }

            await _userService.Add(userVm);
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var user = await _userService.GetByIdSaveViewModel(id);
            return View("Register", user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            if (!ModelState.IsValid)
            {
                return View("Register", vm);
            }

            await _userService.Update(vm);
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            var user = await _userService.GetByIdSaveViewModel(id);
            return View("Delete", user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!_validateUserSession.HasUser() || userViewModel.TypeUserId != Roles.Admin)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            await _userService.Delete(id);
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }
    }
}
