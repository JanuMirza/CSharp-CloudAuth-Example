# CloudAuth C# Desktop Application Example

Complete example of integrating CloudAuth into your C# Windows Forms application.

## Features

- ✅ **License Verification** - Verify user licenses with CloudAuth API
- ✅ **HWID Binding** - Hardware ID generation and device binding
- ✅ **Auto-Login** - Saves license locally for automatic verification
- ✅ **Dark Theme UI** - Modern, professional interface
- ✅ **Secure Communication** - SHA256 hashing for hardware IDs
- ✅ **Error Handling** - Comprehensive error messages

## Requirements

- Visual Studio 2022 (or Visual Studio 2019)
- .NET 6.0 or later
- Windows 10/11
- NuGet Package: `Newtonsoft.Json` (version 13.0.3 or later)

## Setup Instructions

### 1. Open Project in Visual Studio

1. Open Visual Studio
2. Click "Open a project or solution"
3. Open `CloudAuthExample.sln`

### 2. Restore NuGet Packages

Visual Studio will automatically restore packages. If not:
- Right-click on Solution → "Restore NuGet Packages"

### 3. Configure Your App Key

Open `LoginForm.cs` and replace the placeholder:

```csharp
private const string APP_KEY = "YOUR_APP_KEY_HERE"; 
```

**Where to get your App Key:**
1. Login to CloudAuth dashboard: `https://cloudauthx.xyz/`
2. Go to "Applications" page
3. Create a new application or use existing one
4. Copy the **App Key**

Example:
```csharp
private const string APP_KEY = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"; 
```

### 4. Update Base URL (if needed)

If your CloudAuth is hosted elsewhere:

```csharp
private const string BASE_URL = "https://cloudauthx.xyz/";
```

### 5. Build and Run

1. Press `F5` or click "Start" in Visual Studio
2. The login form will appear

#### Method A: Via Dashboard
1. Login to CloudAuth dashboard
2. Create an application
3. Go to applications page
4. You'll need to use the API to create licenses (next step)

#### Method B: Via Postman/API

### Step 2: Test the Application

1. Run the C# application
2. Enter the license key: `A1B2C-D3E4F-G5H6I-J7K8L-M9N0O`
3. Click "Verify License"
4. If valid, main application window will open
5. License is saved to `license.dat` for auto-login

## Project Structure

```
CloudAuthExample/
├── CloudAuthClient.cs      - API client for CloudAuth communication
├── LoginForm.cs            - License verification form
├── MainAppForm.cs          - Main protected application window
├── Program.cs              - Application entry point
└── CloudAuthExample.csproj - Project configuration
```

## Key Classes

### CloudAuthClient.cs

Main API client with methods:

```csharp
// Verify a license key
var result = await authClient.VerifyLicense("LICENSE-KEY-HERE");

// Get hardware ID
string hwid = authClient.GetHardwareId();

// Create new license (requires app_secret)
var result = await authClient.CreateLicense(appSecret, "username", "email@test.com", 30);
```

### LoginForm.cs

- License key input
- Verification UI
- Auto-login from saved license
- HWID display

### MainAppForm.cs

- Protected application interface
- Available after successful verification
- Logout functionality

## HWID (Hardware ID) Generation

The app generates a unique hardware ID based on:
- CPU Processor ID
- Motherboard Serial Number
- MAC Address
- Hashed with SHA256

This ensures:
- One license = One device
- Cannot be shared across multiple computers
- Secure device binding

## Auto-Login Feature

After first successful verification:
- License is saved to `license.dat`
- Next time app starts, auto-verifies
- If license expired/invalid, user must re-enter

## Customization

### Change UI Colors

Edit colors in `LoginForm.cs` and `MainAppForm.cs`:

```csharp
BackColor = Color.FromArgb(10, 10, 15);      // Background
ForeColor = Color.FromArgb(99, 102, 241);    // Primary accent
```

### Add More Features

In `MainAppForm.cs`, add more buttons:

```csharp
Button btnNewFeature = new Button
{
    Text = "🎯 My New Feature",
    // ... styling ...
};
btnNewFeature.Click += (s, e) => {
    // Your feature code here
};
```

## Error Handling

Common errors and solutions:

| Error | Solution |
|-------|----------|
| "Connection failed" | Check if XAMPP Apache is running |
| "Invalid app key" | Verify APP_KEY matches your CloudAuth app |
| "License expired" | Generate new license with longer expiry |
| "Device mismatch" | License bound to different HWID |
| "Rate limit exceeded" | Wait 1 hour or increase rate limit |

## Building for Release

1. Change to **Release** configuration
2. Build → Build Solution
3. EXE location: `bin\Release\net6.0\CloudAuthExample.exe`
4. Distribute this EXE to your users

## Important Notes

⚠️ **Security:**
- Never hardcode `app_secret` in the client app
- Only use `app_key` in desktop applications
- `app_secret` should only be used server-side

⚠️ **Distribution:**
- Users need .NET 6.0 Runtime installed
- Or publish as self-contained (includes runtime)

⚠️ **Testing:**
- Test with expired licenses
- Test with wrong HWID
- Test rate limiting

## Advanced: Self-Contained Publishing

To distribute without requiring .NET runtime:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: Single EXE file with everything included!

## Support

For issues:
1. Check CloudAuth is running (XAMPP)
2. Verify database is imported
3. Check API endpoints in browser
4. Enable debug mode in Visual Studio

## Next Steps

- Add more features to your protected app
- Implement license renewal
- Add analytics tracking
- Create license management panel
- Add update checker

---

**Made with CloudAuth** 🔐
