using GestorPacientes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.DirectoryServices.Protocols;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Security.Cryptography;
using GestorPacientes.Middlewares;

namespace GestorPacientes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private const string ExternalHealthApiKey = "HARDCODED_HEALTH_API_KEY_123456";
        private readonly ValidateUserSession _validateUserSession;

        public HomeController(
    ILogger<HomeController> logger,
    IConfiguration configuration,
    ValidateUserSession validateUserSession)
        {
            _logger = logger;
            _configuration = configuration;
            _validateUserSession = validateUserSession;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult InvokeUnsafe(string typeName, string methodName)
        {
            if (string.IsNullOrWhiteSpace(typeName) ||
                string.IsNullOrWhiteSpace(methodName))
            {
                return BadRequest("Type and method are required.");
            }

            var allowedOperations =
                new Dictionary<string, Func<string>>
                {
                    ["System.DateTime:GetCurrentDate"] =
                        () => DateTime.UtcNow.ToString("yyyy-MM-dd")
                };

            string operationKey =
                $"{typeName}:{methodName}";

            if (!allowedOperations.TryGetValue(
                    operationKey,
                    out var operation))
            {
                return BadRequest(
                    "Requested operation is not allowed.");
            }

            return Content(operation());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private static string EscapeLdapFilterValue(string value)
        {
            if (value == null)
                return string.Empty;

            return value
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        private static bool IsPrivateIp(IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                byte[] bytes = ip.GetAddressBytes();

                return bytes[0] == 10 ||
                       (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                       (bytes[0] == 192 && bytes[1] == 168) ||
                       (bytes[0] == 169 && bytes[1] == 254);
            }

            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return ip.IsIPv6LinkLocal ||
                       ip.IsIPv6SiteLocal ||
                       ip.IsIPv6UniqueLocal;
            }

            return false;
        }

        public IActionResult PingHost(string host)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new
                {
                    controller = "User",
                    action = "Login"
                });
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                return BadRequest("Host is required.");
            }

            if (!System.Net.IPAddress.TryParse(host, out _))
            {
                return BadRequest("Only valid IP addresses are allowed.");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "ping",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("-n");
            startInfo.ArgumentList.Add("1");
            startInfo.ArgumentList.Add(host);

            using var process = Process.Start(startInfo);

            if (process == null)
            {
                return StatusCode(500, "Unable to start ping.");
            }

            process.WaitForExit();

            return Content("Ping command executed safely.");
        }

        [HttpGet]
        public IActionResult FetchUrl()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchUrl(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                ViewBag.Error = "Please enter a URL.";
                return View();
            }

            if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
            {
                ViewBag.Error = "Invalid URL.";
                return View();
            }

            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                ViewBag.Error = "Only HTTPS URLs are allowed.";
                return View();
            }

            if (uri.IsLoopback)
            {
                ViewBag.Error = "Loopback addresses are not allowed.";
                return View();
            }

            var addresses = await Dns.GetHostAddressesAsync(uri.Host);

            bool privateAddressDetected = addresses.Any(ip =>
                IPAddress.IsLoopback(ip) ||
                ip.Equals(IPAddress.Any) ||
                ip.Equals(IPAddress.IPv6Any) ||
                ip.Equals(IPAddress.None) ||
                IsPrivateIp(ip));

            if (privateAddressDetected)
            {
                ViewBag.Error = "Private or internal network addresses are not allowed.";
                return View();
            }

            using HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            string responseBody = await client.GetStringAsync(uri);

            ViewBag.TargetUrl = targetUrl;
            ViewBag.ResponseBody = responseBody;

            return View();
        }

        public IActionResult ErrorLeak()
        {
            try
            {
                throw new Exception("Simulated application error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An internal application error occurred.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal error occurred.");
            }
        }

        [HttpGet]
        public IActionResult RedirectTest()
        {
            return View();
        }

        public IActionResult GoTo(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return BadRequest("External redirects are not allowed.");
        }

        public IActionResult SearchPreview(string q)
        {
            ViewBag.SearchQuery = q ?? string.Empty;
            return View();
        }

        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name is required.");
            }

            string uploadsRoot = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads"));

            string safeFileName = Path.GetFileName(fileName);

            if (!string.Equals(fileName, safeFileName, StringComparison.Ordinal))
            {
                return BadRequest("Invalid file name.");
            }

            string fullPath = Path.GetFullPath(
                Path.Combine(uploadsRoot, safeFileName));

            string requiredPrefix =
                uploadsRoot.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(
                    requiredPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file path.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);

            return File(
                fileBytes,
                "application/octet-stream",
                safeFileName);
        }

        [HttpGet]
        public IActionResult LdapSearch()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LdapSearch(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Please enter a username.";
                return View();
            }

            string safeUsername = EscapeLdapFilterValue(username);

            string filter =
                "(&(objectClass=user)(sAMAccountName=" + safeUsername + "))";

            var request = new SearchRequest(
                "DC=example,DC=local",
                filter,
                SearchScope.Subtree,
                new[] { "sAMAccountName", "displayName", "mail" });

            ViewBag.Username = username;
            ViewBag.Filter = request.Filter;
            ViewBag.Message =
                "LDAP request was created using an escaped username.";

            return View();
        }

        [HttpGet]
        public IActionResult XmlImport()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader, settings);

                var document = new XmlDocument
                {
                    XmlResolver = null
                };

                document.Load(xmlReader);

                ViewBag.XmlContent = xmlContent;
                ViewBag.ParsedValue =
                    document.DocumentElement?.InnerText ?? "No value found";
            }
            catch (XmlException)
            {
                ViewBag.XmlContent = xmlContent;
                ViewBag.Error = "Invalid or unsafe XML content.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult DeserializeData()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    TypeNameHandling = TypeNameHandling.None,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var result =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        jsonContent,
                        settings);

                ViewBag.JsonContent = jsonContent;
                ViewBag.ResultType =
                    result?.GetType().FullName ?? "No object created";

                ViewBag.Result =
                    result != null
                        ? JsonConvert.SerializeObject(result)
                        : "No result";
            }
            catch (JsonException)
            {
                ViewBag.JsonContent = jsonContent;
                ViewBag.Error = "Invalid JSON content.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult LoginAudit()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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

            _logger.LogInformation(
                "Login attempt recorded for Username={Username}",
                username);

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
        [ValidateAntiForgeryToken]
        public IActionResult EncryptPatientData(string patientData)
        {
            if (string.IsNullOrWhiteSpace(patientData))
            {
                ViewBag.Error = "Please enter patient data.";
                return View();
            }

            try
            {
                string? keyBase64 =
                    _configuration["Encryption:Key"];

                if (string.IsNullOrWhiteSpace(keyBase64))
                {
                    ViewBag.Error = "Encryption key is not configured.";
                    return View();
                }

                byte[] key = Convert.FromBase64String(keyBase64);

                if (key.Length != 32)
                {
                    ViewBag.Error = "Encryption key must be 256 bits.";
                    return View();
                }

                byte[] plaintext =
                    Encoding.UTF8.GetBytes(patientData);

                byte[] nonce =
                    RandomNumberGenerator.GetBytes(12);

                byte[] ciphertext =
                    new byte[plaintext.Length];

                byte[] tag =
                    new byte[16];

                using var aesGcm =
                    new AesGcm(key, tag.Length);

                aesGcm.Encrypt(
                    nonce,
                    plaintext,
                    ciphertext,
                    tag);

                byte[] combined =
                    nonce
                        .Concat(tag)
                        .Concat(ciphertext)
                        .ToArray();

                ViewBag.PatientData = patientData;
                ViewBag.EncryptedData =
                    Convert.ToBase64String(combined);
            }
            catch (Exception)
            {
                ViewBag.PatientData = patientData;
                ViewBag.Error = "Unable to encrypt patient data.";
            }

            return View();
        }

        public IActionResult DisableCertificateValidation()
        {
            return Content("Certificate validation uses the default secure platform policy.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
