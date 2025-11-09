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

### 3. SAP Business One DI API & UI API
The following SAP assemblies must be registered in your system (typically in `C:\Windows\SysWOW64`):
- `SAPbouiCOM.dll` - SAP Business One UI API
- `SAPbobsCOM90.dll` - SAP Business One DI API (version may vary: 91, 100, etc.)

## Configuration Changes Made

### 1. Project File Updates
- **ToolsVersion**: Updated from 12.0 to 15.0 (VS2019 compatible)
- **Platform Target**: Set to x86 (required for SAP B1 32-bit COM interop)
- **SAP References**: Added proper references to SAPbouiCOM and SAPbobsCOM

### 2. SAP Business One SDK References
The project now includes proper references to:
```xml
<Reference Include="SAPbouiCOM">
  <HintPath>$(SystemRoot)\SysWOW64\SAPbouiCOM.dll</HintPath>
</Reference>
<Reference Include="SAPbobsCOM">
  <HintPath>$(SystemRoot)\SysWOW64\SAPbobsCOM90.dll</HintPath>
</Reference>
```

**Note**: If your SAP B1 version uses a different DI API version (e.g., SAPbobsCOM91.dll, SAPbobsCOM100.dll), update the reference in the .csproj file accordingly.

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

### Issue: "Could not load file or assembly 'SAPbouiCOM'"
**Solution**:
1. Verify SAP Business One Client is installed
2. Check that SAPbouiCOM.dll exists in `C:\Windows\SysWOW64\`
3. Right-click the project → Add Reference → Browse
4. Navigate to `C:\Windows\SysWOW64\` and manually add `SAPbouiCOM.dll`

### Issue: "Could not load file or assembly 'SAPbobsCOM90'"
**Solution**:
1. Check your SAP B1 version
2. Find the correct DI API DLL in `C:\Windows\SysWOW64\`:
   - SAP B1 9.0: `SAPbobsCOM90.dll`
   - SAP B1 9.1: `SAPbobsCOM91.dll`
   - SAP B1 9.2: `SAPbobsCOM92.dll`
   - SAP B1 9.3: `SAPbobsCOM93.dll`
   - SAP B1 10.0: `SAPbobsCOM100.dll`
3. Update the reference in `ContractManagementAddon.csproj` to match your version
4. Rebuild the solution

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
