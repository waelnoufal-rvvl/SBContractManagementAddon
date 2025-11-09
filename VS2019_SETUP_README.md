# Visual Studio 2019 Setup Guide for Contract Management Add-on

## Overview
This SAP Business One add-on project has been configured for Visual Studio 2019 compatibility.

## Prerequisites

### 1. SAP Business One Installation
- SAP Business One Client must be installed on your machine
- SAP Business One SDK must be installed
- Default SDK location: `C:\Program Files (x86)\SAP\SAP Business One SDK\`

### 2. Visual Studio 2019
- Visual Studio 2019 (any edition)
- .NET Framework 4.8 Development Tools
- NuGet Package Manager

### 3. SAP Business One SDK Assembly
The SAP Business One SDK must be installed, which includes:
- `SAPBusinessOneSDK.dll` - The unified SDK assembly containing:
  - SAPbouiCOM (UI API)
  - SAPbobsCOM (DI API)
  - SAPbouiCOM.Framework (Framework classes)

## Configuration Changes Made

### 1. Project File Updates
- **ToolsVersion**: Updated from 12.0 to 15.0 (VS2019 compatible)
- **Platform Target**: Set to x86 (required for SAP B1 32-bit COM interop)
- **SAP References**: Uses SAPBusinessOneSDK (standard SDK assembly)

### 2. SAP Business One SDK Reference
The project uses the standard SAP Business One SDK assembly:
```xml
<Reference Include="SAPBusinessOneSDK">
  <HintPath>$(ProgramFiles)\SAP\SAP Business One SDK\Assemblies\SAPBusinessOneSDK.dll</HintPath>
  <Private>False</Private>
</Reference>
```

**Benefits of using SAPBusinessOneSDK:**
- Single unified reference instead of multiple COM DLLs
- Includes SAPbouiCOM.Framework for template support
- Version-independent (works with all SAP B1 versions)
- Standard approach recommended by SAP

**Default Installation Path:** `C:\Program Files (x86)\SAP\SAP Business One SDK\Assemblies\`

### 3. Assembly Information
- Updated AssemblyInfo.cs with correct project information
- Removed template placeholder values

## Opening the Project

1. **Launch Visual Studio 2019**

2. **Open the Solution**
   - File → Open → Project/Solution
   - Navigate to `ContractManagementAddon.sln`
   - Click Open

3. **Restore NuGet Packages**
   - Right-click on the solution in Solution Explorer
   - Select "Restore NuGet Packages"
   - Wait for all packages to download

4. **Verify SAP References**
   - Expand "References" in Solution Explorer
   - Check that SAPbouiCOM and SAPbobsCOM show without warning icons
   - If there are warnings, see "Troubleshooting" below

## Building the Project

### Debug Build
1. Set Configuration to **Debug**
2. Build → Build Solution (Ctrl+Shift+B)
3. Output will be in `bin\Debug\`

### Release Build
1. Set Configuration to **Release**
2. Build → Build Solution (Ctrl+Shift+B)
3. Output will be in `bin\Release\`

## Running the Add-on

### Method 1: With SAP Business One Running
1. Start SAP Business One Client
2. Login to your company database
3. Run the add-on executable: `ContractManagementAddon.exe`

### Method 2: With Command Line Connection String
```cmd
ContractManagementAddon.exe "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056"
```

### Method 3: Debug from Visual Studio
1. Start SAP Business One Client first
2. Login to your company
3. In Visual Studio, press F5 or click Debug → Start Debugging
4. The add-on will connect to the running SAP B1 instance

## Troubleshooting

### Issue: "Could not load file or assembly 'SAPBusinessOneSDK'"
**Solution**:
1. Verify SAP Business One SDK is installed
2. Check SDK installation path: `C:\Program Files (x86)\SAP\SAP Business One SDK\`
3. Verify SAPBusinessOneSDK.dll exists in: `C:\Program Files (x86)\SAP\SAP Business One SDK\Assemblies\`
4. If SDK is not installed:
   - Download SAP Business One SDK from SAP Service Marketplace
   - Run the SDK installer
   - Rebuild the solution after installation

### Issue: SDK Reference Shows Warning in Visual Studio
**Solution**:
1. Right-click the project → Add Reference
2. Click "Browse" button
3. Navigate to: `C:\Program Files (x86)\SAP\SAP Business One SDK\Assemblies\`
4. Select `SAPBusinessOneSDK.dll`
5. Click Add
6. Rebuild the solution

**Note**: The SAPBusinessOneSDK.dll works with all SAP B1 versions (9.0, 9.1, 9.2, 9.3, 10.0, etc.) - no version-specific configuration needed!

### Issue: Platform Mismatch Errors
**Solution**:
- SAP Business One requires x86 (32-bit) builds
- Verify Configuration Manager shows Platform = x86 or Any CPU with x86 target
- Do NOT build as x64

### Issue: NuGet Package Restore Fails
**Solution**:
1. Check internet connection
2. Tools → Options → NuGet Package Manager → Package Sources
3. Ensure "nuget.org" is enabled
4. Click "Restore NuGet Packages" again
5. If issues persist, delete the `packages` folder and restore again

### Issue: Missing packages folder
**Solution**:
The packages folder has been created. Visual Studio will automatically populate it when you:
1. Open the solution
2. Restore NuGet packages
3. Build the project

### Issue: Visual Studio 2022 Compatibility
**Note**: SAP Business One SDK tools are NOT compatible with Visual Studio 2022. You must use Visual Studio 2019 or earlier for SAP B1 add-on development.

## Project Structure
```
ContractManagementAddon/
├── Core/                    # Core application components
├── DataAccess/             # Repository pattern for data access
├── Forms/                  # SAP B1 UI forms
├── Models/                 # Data models
├── Services/               # Business logic services
├── Utilities/              # Helper classes
├── Tests/                  # Unit and integration tests
├── SAP/                    # SAP-specific files (addon.xml, addon.srf)
├── App.config              # Application configuration
├── packages.config         # NuGet package references
└── ContractManagementAddon.csproj
```

## Additional Notes

### Connection Configuration
Edit `App.config` to configure:
- ODBC connection to SAP HANA
- Database server and credentials
- Logging settings

### SAP Add-on Registration
To register as an SAP B1 Add-on:
1. Use the addon.xml and addon.srf files in the SAP folder
2. Follow SAP Business One Add-on Administration documentation
3. Register through SAP B1 Add-on Manager

## Support
For SAP Business One SDK documentation, visit:
https://help.sap.com/viewer/product/SAP_BUSINESS_ONE_SDK/

## Version History
- 1.0.0 - Initial VS2019 compatible version
  - Updated ToolsVersion to 15.0
  - Added x86 platform target
  - Fixed SAP B1 SDK references
  - Updated assembly information
