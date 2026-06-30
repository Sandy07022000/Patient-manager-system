using GestorPacientes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Reflection;

namespace GestorPacientes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string ExternalHealthApiKey = "HARDCODED_HEALTH_API_KEY_123456";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult InvokeUnsafe(string typeName, string methodName)
        {
            Type type = Type.GetType(typeName);
            object instance = Activator.CreateInstance(type);
            MethodInfo method = type.GetMethod(methodName);
            object result = method.Invoke(instance, null);

            return Content(result?.ToString() ?? "No result");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult PingHost(string host)
        {
            Process.Start("cmd.exe", "/c ping " + host);
            return Content("Ping command executed");
        }

        public IActionResult ErrorLeak()
        {
            try
            {
                throw new Exception("Database error: internal path C:\\PatientManager\\Secrets\\config.json");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public IActionResult GoTo(string returnUrl)
        {
            return Redirect(returnUrl);
        }

        public IActionResult SearchPreview(string q)
        {
            return Content($"Search result for: {q}", "text/html");
        }

        public IActionResult DownloadFile(string fileName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        public IActionResult BuildLdapFilter(string username)
        {
            string filter = "(&(objectClass=user)(sAMAccountName=" + username + "))";
            return Content(filter);
        }

        public IActionResult DisableCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;

            return Content("Certificate validation disabled");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
