# Contract Management Add-on - Architecture Flow

## Overview
This document describes the corrected architecture flow after fixing the VS2019 compatibility issues and template conflicts.

## Application Startup Flow

### 1. Entry Point: Program.cs
```
Main(string[] args)
  ├─> Creates ContractManagementApplication instance
  ├─> Calls application.Run()
  └─> Keeps application running with Application.Run()
```

**Key Changes from Template:**
- ❌ Removed: `SAPbouiCOM.Framework.Application` (template class)
- ❌ Removed: Template `Menu.cs` usage
- ✅ Added: Direct instantiation of `ContractManagementApplication`
- ✅ Added: Proper exception handling and logging
- ✅ Added: Cleanup in finally block

### 2. Application Initialization: Core/Application.cs
```
ContractManagementApplication.Run()
  ├─> ConnectionManager.GetUIApplication()
  ├─> ConnectionManager.GetCompany()
  ├─> UDOManager.CreateUDOs()
  ├─> Initialize all services:
  │   ├─> AuthorizationManager
  │   ├─> CurrencyService
  │   ├─> RevenueRecognitionService
  │   ├─> ApprovalWorkflowManager
  │   ├─> EmailNotificationService
  │   ├─> AttachmentService
  │   ├─> ContractService
  │   ├─> IPCService
  │   └─> ChangeOrderService
  ├─> MenuManager.AddMenuItems()
  └─> EventManager.RegisterEvents()
```

### 3. Menu Creation: Core/MenuManager.cs
```
MenuManager.AddMenuItems()
  ├─> Create main menu "Contract Management" under Modules
  └─> Create sub-menus:
      ├─> CMADDON_CONTRACTS - "Contracts"
      ├─> CMADDON_IPC - "Interim Payment Certificates"
      ├─> CMADDON_CO - "Change Orders"
      └─> CMADDON_DASH - "Dashboard"
```

### 4. Event Registration: Core/EventManager.cs
```
EventManager.RegisterEvents()
  ├─> Register MenuEvent handler (OnMenuEvent)
  ├─> Register ItemEvent handler (OnItemEvent)
  └─> Register AppEvent handler (OnAppEvent)
```

## Runtime Event Handling

### Menu Click Events
```
User clicks menu item
  ↓
OnMenuEvent(MenuEvent pVal)
  ↓
Switch on pVal.MenuUID:
  ├─> CMADDON_CONTRACTS → Opens ContractForm
  ├─> CMADDON_IPC → Opens IPCForm
  ├─> CMADDON_CO → Opens ChangeOrderForm
  └─> CMADDON_DASH → Opens DashboardForm
```

### Form Events
```
User interacts with form
  ↓
OnItemEvent(string formUID, ItemEvent pVal)
  ↓
Forms handle their own events
(EventManager logs global events like form close)
```

### Application Events
```
SAP B1 event occurs
  ↓
OnAppEvent(BoAppEventTypes eventType)
  ↓
Switch on eventType:
  ├─> aet_ShutDown → Calls app.Shutdown() and Application.Exit()
  ├─> aet_CompanyChanged → Logs company change
  ├─> aet_LanguageChanged → Logs language change
  └─> aet_ServerTerminition → Logs server termination
```

## Application Shutdown Flow

### 1. Shutdown Trigger
```
User closes SAP B1 or Ctrl+C
  ↓
OnAppEvent(aet_ShutDown)
  ↓
ContractManagementApplication.Shutdown()
  ├─> EventManager.UnregisterEvents()
  └─> Company.Disconnect()
```

### 2. Program.cs Finally Block
```
Application exits
  ↓
finally block in Program.Main()
  ↓
Calls application.Shutdown() if not already called
  ↓
Logs termination
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                   Program.cs                         │
│              (Entry Point - Main)                    │
└───────────────────┬─────────────────────────────────┘
                    │ creates
                    ↓
┌─────────────────────────────────────────────────────┐
│         ContractManagementApplication               │
│              (Core/Application.cs)                   │
└───┬──────────┬──────────┬──────────┬───────────────┘
    │          │          │          │
    │          │          │          │
    ↓          ↓          ↓          ↓
┌─────────┐┌──────────┐┌──────────┐┌───────────────┐
│Connection││  Menu    ││  Event   ││   Services    │
│ Manager  ││ Manager  ││ Manager  ││   (Phase 1)   │
└─────────┘└──────────┘└──────────┘└───────────────┘
    │          │          │                │
    │          │          │                │
    ↓          ↓          ↓                ↓
┌─────────┐┌──────────┐┌──────────┐┌───────────────┐
│SAP B1   ││SAP Menus ││SAP Events││  Repositories │
│UI & DI  ││          ││          ││  & Forms      │
└─────────┘└──────────┘└──────────┘└───────────────┘
```

## Key Files and Their Roles

### Core Files
- **Program.cs** - Entry point, creates and runs main application
- **Core/Application.cs** - Main application coordinator
- **Core/ConnectionManager.cs** - SAP B1 connection management
- **Core/MenuManager.cs** - Menu creation and structure
- **Core/EventManager.cs** - Event handling (menu, item, app events)
- **Core/AuthorizationManager.cs** - User permissions

### Obsolete Files
- **Menu.cs** - ❌ OBSOLETE (replaced by Core/MenuManager.cs)

### Service Layer (Phase 1)
- **Services/ContractService.cs** - Contract business logic
- **Services/IPCService.cs** - IPC business logic
- **Services/ChangeOrderService.cs** - Change order business logic
- **Services/ApprovalWorkflowManager.cs** - Approval workflows
- **Services/EmailNotificationService.cs** - Email notifications
- **Services/AttachmentService.cs** - Document attachments
- **Services/CurrencyService.cs** - Multi-currency support
- **Services/RevenueRecognitionService.cs** - Revenue calculations

### Data Access Layer
- **DataAccess/ContractRepository.cs** - Contract data access
- **DataAccess/IPCRepository.cs** - IPC data access
- **DataAccess/ChangeOrderRepository.cs** - Change order data access
- **DataAccess/UDOManager.cs** - User Defined Objects setup

### Forms (UI Layer)
- **Forms/ContractForm.cs** - Contract management form
- **Forms/IPCForm.cs** - IPC form
- **Forms/ChangeOrderForm.cs** - Change order form
- **Forms/DashboardForm.cs** - Dashboard form

### Utilities
- **Utilities/Logger.cs** - Logging
- **Utilities/DatabaseHelper.cs** - Database utilities
- **Utilities/ValidationHelper.cs** - Input validation

## Event Handler Summary

### MenuEvent Handler
**Location:** `Core/EventManager.cs:OnMenuEvent()`
**Triggers:** User clicks menu item
**Actions:**
- Opens appropriate form based on menu ID
- Logs menu action
- Displays errors in status bar if needed

### ItemEvent Handler
**Location:** `Core/EventManager.cs:OnItemEvent()`
**Triggers:** User interacts with form controls
**Actions:**
- Forms handle their own item events
- EventManager logs global events (form close, etc.)

### AppEvent Handler
**Location:** `Core/EventManager.cs:OnAppEvent()`
**Triggers:** SAP B1 application events
**Actions:**
- Shutdown: Cleanup and exit
- Company changed: Reload connection
- Language changed: Reload UI (if multi-language)
- Server termination: Log warning

## Configuration

### App.config Settings
```xml
<appSettings>
  <add key="AddonName" value="Contract Management" />
  <add key="AddonVersion" value="1.0.0" />
  <add key="LogPath" value="C:\Logs\ContractManagement\" />
  <add key="EnableDebugMode" value="true" />
  <add key="UseODBCConnection" value="true" />
  <!-- HANA connection settings -->
</appSettings>
```

### Connection Options
1. **SAP UI API** - Through ConnectionManager
2. **SAP DI API** - Direct company connection
3. **ODBC** - For complex queries (optional)

## Error Handling Strategy

### Level 1: Program.cs
- Catches all unhandled exceptions
- Shows MessageBox to user
- Logs to file
- Ensures cleanup

### Level 2: Application.cs
- Catches initialization errors
- Shows status bar message
- Logs error details
- Re-throws to Program.cs

### Level 3: Managers & Services
- Catch specific exceptions
- Log detailed error info
- Return error status or throw
- Update UI with error message

### Level 4: Event Handlers
- Catch all exceptions in event handlers
- Set bubbleEvent = false on error
- Log error details
- Display status bar message

## Best Practices Implemented

1. ✅ **Separation of Concerns**
   - Core logic in Application.cs
   - Menu handling in MenuManager.cs
   - Event handling in EventManager.cs
   - Business logic in Services
   - Data access in Repositories

2. ✅ **Dependency Injection**
   - Services receive Company in constructor
   - Forms receive ContractManagementApplication
   - Managers receive dependencies explicitly

3. ✅ **Error Handling**
   - Try-catch at all levels
   - Comprehensive logging
   - User-friendly error messages
   - Proper cleanup in finally blocks

4. ✅ **Resource Management**
   - Proper disposal of SAP objects
   - Event unregistration on shutdown
   - Company disconnection on exit

5. ✅ **Logging**
   - Info level for normal operations
   - Warning level for recoverable issues
   - Error level for failures
   - Logs include timestamps and context

## Testing the Flow

### Manual Test Steps
1. ✅ Start SAP Business One
2. ✅ Login to company database
3. ✅ Run ContractManagementAddon.exe
4. ✅ Verify menu appears under Modules
5. ✅ Click each menu item
6. ✅ Verify forms open correctly
7. ✅ Check logs for errors
8. ✅ Close SAP B1
9. ✅ Verify clean shutdown in logs

### Expected Log Output
```
=================================================================
Starting Contract Management Add-On for SAP Business One
=================================================================
Initializing Contract Management Add-On...
Connected to company: SBODEMOGB
Database: HANA - SBODEMOGB
Initializing Phase 1 services...
Phase 1 services initialized successfully
Adding menu items...
Main menu added successfully
All menu items added successfully
Registering event handlers...
Event handlers registered successfully
Contract Management Add-On started successfully
Application is running. Press Ctrl+C or close SAP B1 to exit.
```

## Version History

### v2.0.0 (Current - November 2025)
- ✅ Fixed VS2019 compatibility
- ✅ Rewrote Program.cs to use proper architecture
- ✅ Deprecated template Menu.cs
- ✅ Integrated all Phase 1 services
- ✅ Proper event handling flow
- ✅ Comprehensive error handling
- ✅ Full logging implementation

### v1.0.0 (Template Version)
- ❌ Used SAP template structure
- ❌ Had conflicts between template and actual code
- ❌ Referenced non-existent Form1
- ❌ Incomplete service integration

---
**Last Updated:** November 9, 2025
**Architecture Status:** ✅ Production Ready
