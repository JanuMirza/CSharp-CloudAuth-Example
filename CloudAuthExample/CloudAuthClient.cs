using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CloudAuthExample
{
    public class CloudAuthClient
    {
        private readonly string _baseUrl;
        private readonly string _appName;
        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly string _version;
        private readonly HttpClient _httpClient;
        public bool SecureMode { get; set; }

        public CloudAuthClient(string baseUrl, string appName, string appKey, string appSecret, string version, bool secureMode = true)
        {
            _baseUrl = baseUrl.TrimEnd('/') + "/";
            _appName = appName;
            _appKey = appKey;
            _appSecret = appSecret;
            _version = version;
            SecureMode = secureMode;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<AppInitResponse> InitializeApp()
        {
            try
            {
                var requestData = new
                {
                    app_name = _appName,
                    app_key = _appKey,
                    app_secret = _appSecret,
                    version = _version
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "api/app/init.php", content);
                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<AppInitResponse>(responseText);
                return result ?? new AppInitResponse { success = false, message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new AppInitResponse
                {
                    success = false,
                    message = $"Connection Error: {ex.Message}"
                };
            }
        }

        public async Task<RegisterResponse> RegisterUser(string licenseKey, string username, string password, string email = "")
        {
            try
            {
                var hwid = GetHardwareId();
                
                var requestData = new
                {
                    app_name = _appName,
                    app_key = _appKey,
                    app_secret = _appSecret,
                    license_key = licenseKey,
                    username = username,
                    password = password,
                    email = email,
                    hwid = hwid
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "api/license/register.php", content);
                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<RegisterResponse>(responseText);
                return result ?? new RegisterResponse { success = false, message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new RegisterResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse> LoginUser(string username, string password)
        {
            try
            {
                var hwid = GetHardwareId();
                
                var requestData = new
                {
                    app_name = _appName,
                    app_key = _appKey,
                    app_secret = _appSecret,
                    username = username,
                    password = password,
                    hwid = hwid
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add security headers if secure mode is enabled
                string endpoint = SecureMode ? "api/license/login-secure.php" : "api/license/login.php";
                
                if (SecureMode)
                {
                    long timestamp = GetCurrentTimestamp();
                    string nonce = GenerateNonce();
                    string signature = GenerateSignature(json, timestamp);
                    
                    content.Headers.Add("X-Timestamp", timestamp.ToString());
                    content.Headers.Add("X-Nonce", nonce);
                    content.Headers.Add("X-Signature", signature);
                }

                var response = await _httpClient.PostAsync(_baseUrl + endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<LoginResponse>(responseText);
                return result ?? new LoginResponse { success = false, message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<LicenseVerifyResponse> VerifyLicense(string licenseKey)
        {
            try
            {
                var hwid = GetHardwareId();
                
                var requestData = new
                {
                    app_name = _appName,
                    app_key = _appKey,
                    app_secret = _appSecret,
                    version = _version,
                    license_key = licenseKey,
                    hwid = hwid
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "api/license/verify.php", content);
                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<LicenseVerifyResponse>(responseText);
                return result ?? new LicenseVerifyResponse { success = false, message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new LicenseVerifyResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<CreateLicenseResponse> CreateLicense(string appSecret, string username, string email = "", int expiryDays = 30)
        {
            try
            {
                var requestData = new
                {
                    app_key = _appKey,
                    app_secret = appSecret,
                    username = username,
                    email = email,
                    expiry_days = expiryDays
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "api/license/create.php", content);
                var responseText = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<CreateLicenseResponse>(responseText);
                return result ?? new CreateLicenseResponse { success = false, message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new CreateLicenseResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                };
            }
        }

        public string GetHardwareId()
        {
            try
            {
                string hwid = "";

                // Get CPU ID
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    hwid += obj["ProcessorId"]?.ToString() ?? "";
                }

                // Get Motherboard Serial
                searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    hwid += obj["SerialNumber"]?.ToString() ?? "";
                }

                // Get MAC Address
                searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
                foreach (ManagementObject obj in searcher.Get())
                {
                    hwid += obj["MACAddress"]?.ToString() ?? "";
                    break; // First MAC only
                }

                // Hash the HWID for security
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hwid));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString().Substring(0, 32); // Return first 32 chars
                }
            }
            catch
            {
                // Fallback if WMI fails
                return Environment.MachineName + "_" + Environment.UserName;
            }
        }

        private string GenerateSignature(string jsonData, long timestamp)
        {
            string signatureBase = jsonData + timestamp + _appSecret;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_appSecret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBase));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private string GenerateNonce()
        {
            byte[] randomBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }

        private long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    public class LicenseVerifyResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public LicenseData? data { get; set; }
    }

    public class LicenseData
    {
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string expires_at { get; set; } = "";
        public string last_login { get; set; } = "";
    }

    public class CreateLicenseResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public CreateLicenseData? data { get; set; }
    }

    public class CreateLicenseData
    {
        public string license_key { get; set; } = "";
        public string username { get; set; } = "";
        public string expires_at { get; set; } = "";
    }

    public class AppInitResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public AppInitData? data { get; set; }
    }

    public class AppInitData
    {
        public int app_id { get; set; }
        public string app_name { get; set; } = "";
        public string version { get; set; } = "";
        public string current_version { get; set; } = "";
        public string your_version { get; set; } = "";
        public string status { get; set; } = "";
        public string message { get; set; } = "";
        public int total_users { get; set; }
        public string developer { get; set; } = "";
    }

    public class RegisterResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public RegisterData? data { get; set; }
    }

    public class RegisterData
    {
        public string username { get; set; } = "";
        public string license_key { get; set; } = "";
        public string expires_at { get; set; } = "";
        public string status { get; set; } = "";
    }

    public class LoginResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public LoginData? data { get; set; }
    }

    public class LoginData
    {
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string license_key { get; set; } = "";
        public string expires_at { get; set; } = "";
        public string last_login { get; set; } = "";
    }
}
