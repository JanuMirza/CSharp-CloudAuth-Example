using System;
using System.Threading.Tasks;

namespace CloudAuthExample
{
    class Program
    {
        private static class CloudConfig
        {
            public static readonly string BaseUrl = "https://cloudauthx.xyz/";
            public static readonly string AppName = "Appliction Name";
            public static readonly string AppKey = "Your-App-Key"; // Your App Key
            public static readonly string AppSecret = "Your-App-Secret"; // Your App Secret
            public static readonly string Version = "1.0.0";
            public static bool secureMode = true;// Enable secure mode by default
        }
        static async Task Main(string[] args)
        {
            Console.Title = "CloudAuth - License Verification System";
            Console.ForegroundColor = ConsoleColor.Magenta;
            
            PrintHeader();
            
            // Initialize CloudAuth Client
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[INFO] Initializing CloudAuth Client...");
            Console.ResetColor();
            
          
            
            var client = new CloudAuthClient(
                CloudConfig.BaseUrl,
                CloudConfig.AppName,
                CloudConfig.AppKey,
                CloudConfig.AppSecret,
                CloudConfig.Version,
                CloudConfig.secureMode
            );
            // Display Hardware ID
            string hwid = client.GetHardwareId();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n[HWID] Your Hardware ID: {hwid.Substring(0, 16)}...");
            Console.ResetColor();
            
            // Display security mode status
            if (client.SecureMode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[SECURITY] Secure Mode: ENABLED ✓");
                Console.WriteLine("  - Request Signing: ON");
                Console.WriteLine("  - Replay Protection: ON");
                Console.WriteLine("  - Response Encryption: ON");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[SECURITY] Secure Mode: DISABLED");
                Console.ResetColor();
            }
            
            // Initialize Application
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[*] Connecting to CloudAuth Server...");
            Console.ResetColor();
            
            var initResult = await client.InitializeApp();
            
            if (!initResult.success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] Failed to connect: {initResult.message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Connected to CloudAuth!");
            Console.WriteLine($"[INFO] App: {initResult.data?.app_name}");
            Console.ResetColor();
            
            // Version Check
            if (initResult.data?.status == "version_mismatch")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              ⚠️  VERSION MISMATCH WARNING  ⚠️               ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"\n[WARNING] Your Version: {initResult.data?.your_version}");
                Console.WriteLine($"[WARNING] Latest Version: {initResult.data?.current_version}");
                Console.WriteLine($"[WARNING] {initResult.data?.message}");
                Console.WriteLine("\nYou can continue, but some features may not work properly.");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nPress ENTER to continue or type 'exit' to quit: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                string? choice = Console.ReadLine()?.Trim().ToLower();
                Console.ResetColor();
                
                if (choice == "exit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nApplication closed due to version mismatch.");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INFO] Version: {initResult.data?.version} ✓");
                Console.ResetColor();
            }
            
            // Check for saved credentials
            var savedCreds = LoadSavedCredentials();
            
            if (savedCreds.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n[*] Found saved credentials. Logging in...");
                Console.ResetColor();
                
                var loginResult = await client.LoginUser(savedCreds.Value.username, savedCreds.Value.password);
                
                if (loginResult.success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Welcome back, {loginResult.data?.username}!");
                    Console.ResetColor();
                    Console.Write("✓ ");
                    DisplayExpiryInfo(loginResult.data?.expires_at);
                    
                    RunProtectedApplication(loginResult.data?.username ?? "User", loginResult.data);
                    return;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n[WARNING] Auto-login failed: {loginResult.message}");
                    Console.ResetColor();
                    DeleteSavedCredentials();
                }
            }
       
            // Main Menu Loop
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("Choose an option:");
                Console.WriteLine("  1. Login with Username & Password");
                Console.WriteLine("  2. Register New Account (with License Key)");
                Console.WriteLine($"  3. Toggle Secure Mode (Currently: {(client.SecureMode ? "ON" : "OFF")})");
                Console.WriteLine("  4. Exit");
                Console.Write("\n> ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                
                string? choice = Console.ReadLine()?.Trim();
                Console.ResetColor();
                
                if (choice == "1")
                {
                    // Login Flow
                    if (await LoginFlow(client))
                    {
                        break; // Exit to protected app
                    }
                }
                else if (choice == "2")
                {
                    // Registration Flow
                    if (await RegisterFlow(client))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n✓ Registration successful! Please login with your credentials.");
                        Console.ResetColor();
                    }
                }
                else if (choice == "3")
                {
                    // Toggle Secure Mode
                    client.SecureMode = !client.SecureMode;
                    
                    if (client.SecureMode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[✓] Secure Mode ENABLED");
                        Console.WriteLine("    - Request signing active");
                        Console.WriteLine("    - Replay attack protection active");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n[!] Secure Mode DISABLED");
                        Console.WriteLine("    - Using basic authentication");
                        Console.ResetColor();
                    }
                }
                else if (choice == "4")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nGoodbye!");
                    Console.ResetColor();
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[ERROR] Invalid choice. Please enter 1-4.");
                    Console.ResetColor();
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static void PrintHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║      ██████╗██╗      ██████╗ ██╗   ██╗██████╗               ║
║     ██╔════╝██║     ██╔═══██╗██║   ██║██╔══██╗              ║
║     ██║     ██║     ██║   ██║██║   ██║██║  ██║              ║
║     ██║     ██║     ██║   ██║██║   ██║██║  ██║              ║
║     ╚██████╗███████╗╚██████╔╝╚██████╔╝██████╔╝              ║
║      ╚═════╝╚══════╝ ╚═════╝  ╚═════╝ ╚═════╝               ║
║                                                              ║
║              █████╗ ██╗   ██╗████████╗██╗  ██╗              ║
║             ██╔══██╗██║   ██║╚══██╔══╝██║  ██║              ║
║             ███████║██║   ██║   ██║   ███████║              ║
║             ██╔══██║██║   ██║   ██║   ██╔══██║              ║
║             ██║  ██║╚██████╔╝   ██║   ██║  ██║              ║
║             ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝              ║
║                                                              ║
║           License Verification System v1.0.0                ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }
        
        static void RunProtectedApplication(string username, LoginData? userData = null)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║                    ✓ ACCESS GRANTED ✓                        ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n🎉 Welcome to the protected application, {username}!");
            Console.ResetColor();
            
            Console.WriteLine("\n" + new string('-', 60));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("This is your protected application area.");
            Console.WriteLine("Only users with valid licenses can access this section.");
            Console.ResetColor();
            Console.WriteLine(new string('-', 60));
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nAvailable Commands:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  1. Show User Info");
            Console.WriteLine("  2. Check License Status");
            Console.WriteLine("  3. View Application Info");
            Console.WriteLine("  4. Logout");
            Console.ResetColor();
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nEnter command (1-4): ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                string? input = Console.ReadLine();
                Console.ResetColor();
                
                switch (input)
                {
                    case "1":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n[User Info]");
                        Console.ResetColor();
                        Console.WriteLine($"  Username: {username}");
                        if (userData != null)
                        {
                            Console.WriteLine($"  Email: {userData.email}");
                            Console.WriteLine($"  License Key: {userData.license_key}");
                            Console.Write("  ");
                            DisplayExpiryInfo(userData.expires_at);
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  Status: Active");
                        Console.WriteLine($"  Access Level: Premium");
                        Console.ResetColor();
                        break;
                        
                    case "2":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n[License Status]");
                        Console.ResetColor();
                        if (userData != null)
                        {
                            Console.Write("  ");
                            DisplayExpiryInfo(userData.expires_at);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("  ✓ License: Valid");
                            Console.WriteLine("  ✓ Hardware Binding: Active");
                            Console.WriteLine("  ✓ Connection: Secure");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("  ✓ License: Valid");
                            Console.WriteLine("  ✓ Hardware Binding: Active");
                            Console.WriteLine("  ✓ Connection: Secure");
                            Console.ResetColor();
                        }
                        break;
                        
                    case "3":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n[Application Info]");
                        Console.WriteLine($"  App Name: {CloudConfig.AppName}");
                        Console.WriteLine($"  Version: {CloudConfig.Version}");
                        Console.WriteLine($"  Protected By: CloudAuth");
                        Console.ResetColor();
                        break;
                        
                    case "4":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n[*] Logging out...");
                        Console.ResetColor();
                        return;
                        
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[ERROR] Invalid command. Please enter 1-4.");
                        Console.ResetColor();
                        break;
                }
            }
        }
        
        static async Task<bool> LoginFlow(CloudAuthClient client)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        LOGIN                               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nUsername: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string? username = Console.ReadLine()?.Trim();
            Console.ResetColor();
            
            if (string.IsNullOrEmpty(username))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Username cannot be empty!");
                Console.ResetColor();
                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Password: ");
            string password = ReadPassword();
            Console.ResetColor();
            
            if (string.IsNullOrEmpty(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR] Password cannot be empty!");
                Console.ResetColor();
                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n\n[*] Logging in");
            if (client.SecureMode)
            {
                Console.Write(" (Secure Mode)");
            }
            Console.WriteLine("...");
            Console.ResetColor();
            
            var result = await client.LoginUser(username, password);
            
            if (result.success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                  ✓ LOGIN SUCCESSFUL ✓                      ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nWelcome, {result.data?.username}!");
                Console.ResetColor();
                
                Console.WriteLine($"Email: {result.data?.email}");
                Console.WriteLine($"License Key: {result.data?.license_key}");
                
                // Display expiry with color coding
                DisplayExpiryInfo(result.data?.expires_at);
                
                // Save credentials
                SaveCredentials(username, password);
                
                // Run protected app with full user data
                RunProtectedApplication(result.data?.username ?? "User", result.data);
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ LOGIN FAILED: {result.message}");
                Console.ResetColor();
                return false;
            }
        }
        
        static async Task<bool> RegisterFlow(CloudAuthClient client)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  REGISTER NEW ACCOUNT                      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nLicense Key: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string? licenseKey = Console.ReadLine()?.Trim().ToUpper();
            Console.ResetColor();
            
            if (string.IsNullOrEmpty(licenseKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] License key cannot be empty!");
                Console.ResetColor();
                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Username: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string? username = Console.ReadLine()?.Trim();
            Console.ResetColor();
            
            if (string.IsNullOrEmpty(username))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Username cannot be empty!");
                Console.ResetColor();
                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Password (min 6 chars): ");
            string password = ReadPassword();
            
            Console.Write("\nConfirm Password: ");
            string confirmPassword = ReadPassword();
            Console.ResetColor();
            
            if (password != confirmPassword)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n[ERROR] Passwords do not match!");
                Console.ResetColor();
                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nEmail (optional, press Enter to skip): ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string? email = Console.ReadLine()?.Trim();
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[*] Registering account...");
            Console.ResetColor();
            
            var result = await client.RegisterUser(licenseKey, username, password, email ?? "");
            
            if (result.success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║            ✓ REGISTRATION SUCCESSFUL ✓                    ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nUsername: {result.data?.username}");
                Console.ResetColor();
                Console.WriteLine($"License Key: {result.data?.license_key}");
                DisplayExpiryInfo(result.data?.expires_at);
                Console.WriteLine($"Status: {result.data?.status}");
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ You can now login with your credentials!");
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ REGISTRATION FAILED: {result.message}");
                Console.ResetColor();
                return false;
            }
        }
        
        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            
            return password;
        }
        
        static void SaveCredentials(string username, string password)
        {
            try
            {
                string data = $"{username}|{password}";
                System.IO.File.WriteAllText("credentials.dat", data);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n[INFO] Credentials saved for auto-login.");
                Console.ResetColor();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("\n[WARNING] Could not save credentials.");
                Console.ResetColor();
            }
        }
        
        static (string username, string password)? LoadSavedCredentials()
        {
            try
            {
                if (System.IO.File.Exists("credentials.dat"))
                {
                    string data = System.IO.File.ReadAllText("credentials.dat");
                    string[] parts = data.Split('|');
                    if (parts.Length == 2)
                    {
                        return (parts[0], parts[1]);
                    }
                }
            }
            catch { }
            
            return null;
        }
        
        static void DeleteSavedCredentials()
        {
            try
            {
                if (System.IO.File.Exists("credentials.dat"))
                {
                    System.IO.File.Delete("credentials.dat");
                }
            }
            catch { }
        }
        
        static void DisplayExpiryInfo(string? expiresAt)
        {
            if (string.IsNullOrEmpty(expiresAt))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Expires: Lifetime License");
                Console.ResetColor();
                return;
            }
            
            try
            {
                DateTime expiryDate = DateTime.Parse(expiresAt);
                DateTime now = DateTime.Now;
                TimeSpan remaining = expiryDate - now;
                
                Console.Write("Expires: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{expiryDate:yyyy-MM-dd HH:mm:ss}");
                Console.ResetColor();
                
                if (remaining.TotalDays < 0)
                {
                    // Expired
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" (⚠️ EXPIRED {Math.Abs(remaining.Days)} days ago)");
                    Console.ResetColor();
                }
                else if (remaining.TotalDays <= 7)
                {
                    // Expiring soon (within 7 days)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" (⚠️ Expires in {remaining.Days} days, {remaining.Hours} hours)");
                    Console.ResetColor();
                }
                else if (remaining.TotalDays <= 30)
                {
                    // Expiring this month
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($" (✓ {remaining.Days} days remaining)");
                    Console.ResetColor();
                }
                else
                {
                    // Long time remaining
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" (✓ {remaining.Days} days remaining)");
                    Console.ResetColor();
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Expires: {expiresAt}");
                Console.ResetColor();
            }
        }
    }
}
