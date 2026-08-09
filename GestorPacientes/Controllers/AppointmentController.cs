using GestorPacientes.Core.Application.Enums;
using GestorPacientes.Core.Application.Interfaces.Services;
using GestorPacientes.Core.Application.ViewModels.Appointment;
using GestorPacientes.Core.Application.ViewModels.LaboratoryResult;
using GestorPacientes.Core.Application.Helpers;
using GestorPacientes.Middlewares;
using Microsoft.AspNetCore.Mvc;
using GestorPacientes.Core.Application.ViewModels.Users;
using Microsoft.AspNetCore.Http;

namespace GestorPacientes.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly ILaboratoryResultService _labResultService;
        private readonly ILaboratoryTestService _labTestService;
        private readonly ValidateUserSession _validateUserSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserViewModel? userViewModel;

        public AppointmentController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService,
            ILaboratoryResultService labResultService,
            ILaboratoryTestService labTestService,
            ValidateUserSession validateUserSession,
            IHttpContextAccessor httpContextAccessor)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
            _labResultService = labResultService;
            _labTestService = labTestService;
            _validateUserSession = validateUserSession;
            _httpContextAccessor = httpContextAccessor;

            userViewModel =
                _httpContextAccessor.HttpContext?
                    .Session.Get<UserViewModel>("user");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var list = await _appointmentService.GetAllViewModel();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var listPatients = await _patientService.GetAllViewModel();
            var listDoctors = await _doctorService.GetAllViewModel();

            var model = new SaveAppointmentViewModel
            {
                Patients = listPatients,
                Doctors = listDoctors
            };

            return View("SaveAppointment", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            SaveAppointmentViewModel vm)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            if (!ModelState.IsValid)
            {
                vm.Patients =
                    await _patientService.GetAllViewModel();

                vm.Doctors =
                    await _doctorService.GetAllViewModel();

                return View("SaveAppointment", vm);
            }

            await _appointmentService.Add(vm);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Consult(int appointmentid)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var model = new SaveLaboratoryResultViewModel
            {
                AppointmentId = appointmentid,
                LaboratoryTests =
                    await _labTestService.GetAllViewModel()
            };

            return View("ConsultPatient", model);
        }

        [HttpPost]
        public async Task<IActionResult> Consult(
            SaveLaboratoryResultViewModel vm)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            if (!ModelState.IsValid)
            {
                vm.LaboratoryTests =
                    await _labTestService.GetAllViewModel();

                return View("ConsultPatient", vm);
            }

            await _labResultService.Add(vm);

            var appointment =
                await _appointmentService
                    .GetByIdSaveViewModel(vm.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.status = Status.PendingResults;

            await _appointmentService.Update(appointment);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CheckResults(
            FilterLabResultViewModel filterLabResult,
            string? status)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var list =
                await _labResultService
                    .GetAllViewModelWithFilter(filterLabResult);

            if (status != null)
            {
                ViewBag.Status = status;
            }

            return View("LaboratoryResults", list);
        }

        // Security remediation:
        // State-changing operation is now POST-only.
        // AutoValidateAntiforgeryToken validates the CSRF token.
        [HttpPost]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var appointment =
                await _appointmentService
                    .GetByIdSaveViewModel(id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.status = Status.Completed;

            await _appointmentService.Update(appointment);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            var appointment =
                await _appointmentService
                    .GetByIdSaveViewModel(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            if (!_validateUserSession.HasUser() ||
                userViewModel?.TypeUserId != Roles.Assistant)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            await _appointmentService.Delete(id);

            return RedirectToAction("Index");
        }
    }
}