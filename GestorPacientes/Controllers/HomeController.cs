using GestorPacientes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Security.Cryptography;

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

        [HttpGet]
        public IActionResult FetchUrl()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FetchUrl(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                ViewBag.Error = "Please enter a URL.";
                return View();
            }

            using HttpClient client = new HttpClient();

            string responseBody = await client.GetStringAsync(targetUrl);

            ViewBag.TargetUrl = targetUrl;
            ViewBag.ResponseBody = responseBody;

            return View();
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

        [HttpGet]
        public IActionResult RedirectTest()
        {
            return View();
        }

        public IActionResult GoTo(string returnUrl)
        {
            return Redirect(returnUrl);
        }

        public IActionResult SearchPreview(string q)
        {
            ViewBag.SearchQuery = q ?? string.Empty;
            return View();
        }

        public IActionResult DownloadFile(string fileName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public IActionResult LdapSearch()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LdapSearch(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Please enter a username.";
                return View();
            }

            string filter =
                "(&(objectClass=user)(sAMAccountName=" + username + "))";

            var request = new SearchRequest(
                "DC=example,DC=local",
                filter,
                SearchScope.Subtree,
                new[] { "sAMAccountName", "displayName", "mail" });

            ViewBag.Username = username;
            ViewBag.Filter = request.Filter;
            ViewBag.Message =
                "LDAP request was created using the unvalidated username.";

            return View();
        }

        [HttpGet]
        public IActionResult XmlImport()
        {
            return View();
        }

        [HttpPost]
        public IActionResult XmlImport(string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                ViewBag.Error = "Please enter XML content.";
                return View();
            }

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse,
                    XmlResolver = new XmlUrlResolver()
                };

                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader, settings);

                var document = new XmlDocument
                {
                    XmlResolver = new XmlUrlResolver()
                };

                document.Load(xmlReader);

                ViewBag.XmlContent = xmlContent;
                ViewBag.ParsedValue =
                    document.DocumentElement?.InnerText ?? "No value found";
            }
            catch (Exception ex)
            {
                ViewBag.XmlContent = xmlContent;
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        [HttpGet]
        public IActionResult DeserializeData()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeserializeData(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                ViewBag.Error = "Please enter JSON content.";
                return View();
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                object? result =
                    JsonConvert.DeserializeObject<object>(jsonContent, settings);

                ViewBag.JsonContent = jsonContent;
                ViewBag.ResultType =
                    result?.GetType().FullName ?? "No object created";

                ViewBag.Result =
                    result?.ToString() ?? "No result";
            }
            catch (Exception ex)
            {
                ViewBag.JsonContent = jsonContent;
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        [HttpGet]
        public IActionResult LoginAudit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginAudit(
            string username,
            string password,
            string patientId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // Deliberately vulnerable:
            // sensitive authentication and patient data are written to logs.
            _logger.LogInformation(
                "Login attempt: Username={Username}, Password={Password}, PatientId={PatientId}",
                username,
                password,
                patientId);

            ViewBag.Message = "Login attempt recorded.";
            ViewBag.Username = username;
            ViewBag.PatientId = patientId;

            return View();
        }

        [HttpGet]
        public IActionResult EncryptPatientData()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EncryptPatientData(string patientData)
        {
            if (string.IsNullOrWhiteSpace(patientData))
            {
                ViewBag.Error = "Please enter patient data.";
                return View();
            }

            try
            {
                byte[] key =
                    Encoding.UTF8.GetBytes("12345678901234567890123456789012");

                using Aes aes = Aes.Create();

                // Deliberately insecure cryptographic configuration.
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using ICryptoTransform encryptor = aes.CreateEncryptor();

                byte[] plaintextBytes =
                    Encoding.UTF8.GetBytes(patientData);

                byte[] encryptedBytes =
                    encryptor.TransformFinalBlock(
                        plaintextBytes,
                        0,
                        plaintextBytes.Length);

                ViewBag.PatientData = patientData;
                ViewBag.EncryptedData =
                    Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                ViewBag.PatientData = patientData;
                ViewBag.Error = ex.Message;
            }

            return View();
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
