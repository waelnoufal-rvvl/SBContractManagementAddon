# Screenshot Capture Guide
## Contract Management Add-on for SAP Business One
**Version:** 1.0.0
**Date:** January 2025
**Purpose:** Step-by-step guide for capturing high-quality screenshots

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Tools and Setup](#tools-and-setup)
3. [Screenshot Standards](#screenshot-standards)
4. [Capture Instructions by Form](#capture-instructions-by-form)
5. [Editing Screenshots](#editing-screenshots)
6. [File Management](#file-management)
7. [Quality Checklist](#quality-checklist)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### System Requirements

Before capturing screenshots, ensure:

1. **SAP Business One Environment:**
   - Clean installation of SAP B1 9.3 or higher
   - Contract Management Add-on installed and registered
   - Test database with sample data loaded
   - No other add-ons running (to avoid visual conflicts)

2. **Display Settings:**
   - Screen resolution: **1920 x 1080** (Full HD) minimum
   - Scaling: **100%** (no DPI scaling)
   - Color depth: **32-bit** (True Color)
   - Theme: **Windows 10/11 Standard Theme**

3. **SAP B1 Settings:**
   - UI Theme: **Blue** (default SAP theme)
   - Font size: **Default** (9pt Segoe UI)
   - Language: **English**
   - Date format: **DD/MM/YYYY**
   - Number format: **1,000.00**

4. **Test Data:**
   - Load sample contracts from `Database/SampleData.sql`
   - Ensure contracts have realistic values
   - Use company name "Acme Corporation" for demos
   - All dates should be current year

### Access Requirements

- System Administrator access to SAP B1
- Add-on Manager rights
- Ability to create/edit test data
- Access to all add-on forms and menus

---

## Tools and Setup

### Recommended Screenshot Tools

#### Option 1: Snagit (Recommended)

**Why:** Professional screenshot tool with built-in editing

**Download:** https://www.techsmith.com/screen-capture.html

**License:** Paid (30-day trial available)

**Settings:**
```
Capture Method: Window
Format: PNG
Quality: High (no compression)
Include cursor: No
Include drop shadow: Yes
Auto-copy to clipboard: Yes
Auto-save: Yes to Screenshots folder
```

**Advantages:**
- High-quality captures
- Built-in editor for annotations
- Batch processing
- Scrolling window capture
- Video recording capability

#### Option 2: Greenshot (Free Alternative)

**Why:** Free, open-source with good quality

**Download:** https://getgreenshot.org/

**License:** Free (GPL)

**Settings:**
```
Capture Method: Window
Destination: Save and open in editor
Include cursor: No
Include shadow: Yes
Format: PNG
Quality: Best
```

**Advantages:**
- Free and open-source
- Good quality
- Basic editing tools
- Lightweight

#### Option 3: Windows Built-in Tools

**Snipping Tool** (Windows 10):
- Press `Windows + Shift + S`
- Select window or area
- Basic capture only
- Requires separate editor

**Snip & Sketch** (Windows 11):
- Press `Windows + Shift + S`
- Select window or area
- Basic annotation tools

**Note:** Built-in tools are acceptable but lack professional features.

### Image Editing Tools

#### Primary Editor: Paint.NET (Recommended)

**Download:** https://www.getpaint.net/

**License:** Free

**Use for:**
- Adding callouts and arrows
- Highlighting important areas
- Adding numbered steps
- Blurring sensitive data
- Resizing images

#### Alternative: GIMP

**Download:** https://www.gimp.org/

**License:** Free (GPL)

**Use for:**
- Advanced editing
- Batch processing
- Complex annotations

---

## Screenshot Standards

### Technical Specifications

| Specification | Value | Notes |
|--------------|-------|-------|
| Format | PNG | Lossless, supports transparency |
| Resolution | 1920x1080 base | Can be higher, never lower |
| DPI | 96 dpi | Standard screen resolution |
| Color Mode | RGB | 24-bit or 32-bit |
| Compression | None or minimal | Maintain clarity |
| Max File Size | 2 MB | Compress only if necessary |

### Visual Standards

1. **Window Position:**
   - Center form on screen
   - Ensure full form is visible
   - No overlapping windows
   - No desktop icons visible in background

2. **Form State:**
   - Forms should be in "ready" state
   - No validation errors unless demonstrating errors
   - All required fields populated with realistic data
   - Scrollbars visible only when necessary

3. **Content Guidelines:**
   - Use realistic business data
   - Avoid placeholder text like "Test" or "XXX"
   - Use proper company names (Acme Corporation, TechStart Inc, etc.)
   - Use realistic amounts ($1,000.00 not $1.00)
   - Use current dates (within last 30 days)

4. **What NOT to Capture:**
   - Personal information (real names, emails)
   - Real customer data
   - Real financial amounts (if using production)
   - Real contract details
   - Company proprietary information
   - Error messages (unless specifically needed)

---

## Capture Instructions by Form

### General Capture Process

**For Each Screenshot:**

1. **Prepare:**
   - Clear clipboard
   - Close unnecessary windows
   - Position target form center screen
   - Verify data is realistic
   - Check form state

2. **Capture:**
   - Use screenshot tool (Snagit/Greenshot)
   - Select "Window" capture mode
   - Click on target form window
   - Verify capture in clipboard/preview

3. **Review:**
   - Check image quality
   - Verify all content visible
   - Ensure no sensitive data
   - Confirm proper resolution

4. **Save:**
   - Use correct file name (see naming convention)
   - Save to Screenshots folder
   - Keep original (no edits yet)

### Screenshot List by Category

#### Category 1: Setup & Login

##### 001_SAP_Login.png
**What to Capture:** SAP B1 login screen

**Steps:**
1. Start SAP Business One
2. Wait for login screen to appear
3. Fill in server, database, user fields with sample data
4. Do NOT enter real credentials
5. Use: Server: "SBO-DEMO", Company: "Demo_US", User: "manager"
6. Capture full window

**Notes:**
- Show dropdown open for "Company" field
- Cursor should not be visible

##### 002_SAP_Main_Window.png
**What to Capture:** SAP B1 main window after login

**Steps:**
1. Log into SAP B1
2. Close all open documents
3. Ensure menu bar is visible
4. Show "Modules" menu area
5. Capture full SAP window

**Annotations Needed:**
- Highlight menu bar
- Arrow pointing to "Add-Ons" menu

##### 003_Addon_Menu_Location.png
**What to Capture:** Add-Ons menu with Contract Management visible

**Steps:**
1. Click "Add-Ons" in menu bar
2. Wait for menu to expand
3. Ensure "Contract Management" is visible in list
4. Capture SAP window with menu open

**Annotations Needed:**
- Red box around "Contract Management" menu item
- Arrow pointing to menu item

#### Category 2: Contract Forms

##### 005_Contract_Form_Header.png
**What to Capture:** New contract form header section

**Steps:**
1. Navigate: Add-Ons → Contract Management → Contracts → New
2. Fill header with sample data:
   ```
   Contract Code: AUTO-GENERATED
   Contract Name: Web Development Project - Phase 1
   Customer Code: C00001
   Customer Name: Acme Corporation
   Contact Person: John Smith
   Phone: +1-555-0100
   Email: john.smith@acme.com
   Start Date: 01/01/2025
   End Date: 31/12/2025
   Contract Type: Fixed Price (selected)
   Currency: USD
   Exchange Rate: 1.0000
   ```
3. Ensure all fields visible
4. Capture form from top to just above "Financial Information"

**Annotations Needed:**
- Number each field group (1, 2, 3)
- Red asterisk (*) next to required fields

##### 006_Contract_Form_Lines.png
**What to Capture:** Contract lines grid

**Steps:**
1. In same contract form, scroll to "Contract Lines" section
2. Add 3-5 sample lines:
   ```
   Line 1: SRV-001, Consulting Service, 100 hours @ $500
   Line 2: SRV-002, Implementation, 50 hours @ $800
   Line 3: LIC-001, Software License, 1 @ $12,500
   ```
3. Ensure grid toolbar visible
4. Select one line
5. Capture entire grid section

**Annotations Needed:**
- Label toolbar buttons
- Highlight selected row
- Show total at bottom

##### 007_Contract_Form_Complete.png
**What to Capture:** Complete filled contract form

**Steps:**
1. Scroll to show entire form (may need scrolling screenshot)
2. All sections filled
3. Status bar at bottom visible
4. Capture full form

**Notes:**
- Use Snagit's scrolling window feature
- Ensure continuous image (no breaks)

##### 008_Contract_Line_Details.png
**What to Capture:** Contract line detail modal

**Steps:**
1. From contract lines grid, double-click a line OR click "Details"
2. Modal window opens
3. Fill all fields with sample data
4. Show Revenue Recognition section
5. Capture full modal window

**Annotations Needed:**
- Highlight Revenue Recognition section
- Show calculation formula

##### 009_Contract_Browse_Customer.png
**What to Capture:** Customer browse window

**Steps:**
1. In contract form, click Browse button next to Customer Code
2. Customer list opens
3. Show multiple customers in list
4. Select one customer (highlighted)
5. Capture browse window

**Annotations Needed:**
- Arrow showing how to filter
- Highlight selected row

##### 010_Contract_Validation_Error.png
**What to Capture:** Validation error message

**Steps:**
1. In contract form, leave Contract Name empty
2. Try to save
3. Error message appears
4. Capture error message dialog

**Notes:**
- Show both error dialog and form in background
- Error should be clearly visible

##### 011_Contract_Saved_Success.png
**What to Capture:** Success message after save

**Steps:**
1. Fill all required fields correctly
2. Click Save
3. Success message appears
4. Capture success dialog

**Notes:**
- Show contract code in success message

#### Category 3: IPC Forms

##### 015_IPC_Form_Header.png
**What to Capture:** IPC form header section

**Steps:**
1. Navigate: Add-Ons → Contract Management → IPC → New
2. Fill header:
   ```
   IPC Number: AUTO-GENERATED
   Contract Code: [Use contract from previous screenshots]
   IPC Date: Current date
   Period From: Start of month
   Period To: End of month
   IPC Type: Regular (checked)
   ```
3. Ensure Work Summary visible
4. Capture header and summary sections

**Annotations Needed:**
- Highlight progress bar
- Label cumulative calculations

##### 016_IPC_Performance_Obligations.png
**What to Capture:** Performance obligations grid in IPC

**Steps:**
1. In same IPC form, show POs section
2. Grid should show 2-3 POs:
   ```
   PO-001: Phase 1: Design, 100% complete
   PO-002: Phase 2: Development, 75% complete
   PO-003: Phase 3: Testing, 0% complete
   ```
3. Show checkmarks for completed POs
4. Capture grid section

**Annotations Needed:**
- Explain completion percentage calculation
- Highlight checkmarks

##### 017_IPC_Financial_Calculation.png
**What to Capture:** Financial calculation section

**Steps:**
1. Scroll to Financial Calculation section
2. Show all calculations:
   ```
   Gross Amount: $250,000.00
   Less Retention: -$25,000.00
   Subtotal: $225,000.00
   Add VAT: +$45,000.00
   Net Payable: $270,000.00
   ```
3. Ensure formulas visible
4. Capture entire section

**Annotations Needed:**
- Show calculation flow with arrows
- Label each component

##### 018_IPC_Attachments.png
**What to Capture:** Supporting documents section

**Steps:**
1. In IPC form, add 2-3 sample attachments
2. Use file names like "Work_Report_Jan2025.pdf"
3. Show checkmarks for attached
4. Capture attachments section

**Notes:**
- Use realistic file names
- Show different file types (PDF, Excel, images)

##### 019_IPC_Approval_Workflow.png
**What to Capture:** Approval workflow grid

**Steps:**
1. Show approval workflow section
2. Grid should show:
   ```
   Approver 1: Approved (green)
   Approver 2: Pending (yellow)
   Approver 3: Not Started (gray)
   ```
3. Show status colors
4. Capture workflow section

**Annotations Needed:**
- Explain workflow sequence
- Label status indicators

##### 020_IPC_Status_Draft.png
**What to Capture:** IPC in Draft status

**Steps:**
1. Create new IPC
2. Leave in Draft status
3. Status dropdown showing "Draft"
4. Capture form with status highlighted

##### 021_IPC_Status_Approved.png
**What to Capture:** IPC in Approved status

**Steps:**
1. Open an approved IPC
2. Status shows "Approved"
3. All approval signatures present
4. Capture form

**Notes:**
- Compare with draft to show differences

##### 022_IPC_Print_Preview.png
**What to Capture:** IPC print preview

**Steps:**
1. Open IPC
2. Click Print button
3. Print preview opens
4. Show formatted IPC document
5. Capture preview window

**Notes:**
- Should look like professional certificate
- Company letterhead visible

#### Category 4: Change Order Forms

##### 025_Change_Order_Form.png
**What to Capture:** New change order form

**Steps:**
1. Navigate: Add-Ons → Contract Management → Change Orders → New
2. Fill form:
   ```
   CO Number: AUTO-GENERATED
   Contract: [Select from previous]
   CO Date: Current date
   Change Type: Scope + Cost (both selected)
   Subject: Additional Mobile App Development
   Reason: Customer requested iOS platform support
   Impact: Adds 8 weeks, requires 2 developers
   ```
3. Show all sections
4. Capture full form

**Annotations Needed:**
- Highlight financial impact
- Show before/after comparison

##### 026_Change_Order_Financial_Impact.png
**What to Capture:** Financial impact section

**Steps:**
1. In change order, show calculations:
   ```
   Original: $1,000,000
   Previous Changes: $50,000
   Current: $1,050,000
   This CO: $150,000
   Revised: $1,200,000 (+20%)
   ```
2. Ensure formulas clear
3. Capture section

**Annotations Needed:**
- Show percentage calculation
- Highlight total change

##### 027_Change_Order_Approval_Matrix.png
**What to Capture:** Approval workflow based on change amount

**Steps:**
1. Show approval grid
2. Multiple approvers visible
3. Different statuses
4. Capture workflow section

**Notes:**
- Should show cascading approval
- Status colors clear

##### 028_Change_Order_Attachments.png
**What to Capture:** Attached documents

**Steps:**
1. Add sample files:
   ```
   - Revised_Scope_v2.pdf
   - Cost_Breakdown.xlsx
   - Customer_Approval_Email.msg
   ```
2. Show file list
3. Capture attachments section

#### Category 5: Revenue Recognition Forms

##### 030_Performance_Obligation_Form.png
**What to Capture:** New performance obligation form

**Steps:**
1. Navigate: Add-Ons → Contract Management → Performance Obligations → New
2. Fill form:
   ```
   PO Code: AUTO-GENERATED
   Contract: [Select]
   Description: Phase 1: Requirements & Design
   Performance Period: Q1 2025
   Allocated Amount: $300,000
   Recognition Method: Over Time
   Progress Method: Input Method (Cost-to-Cost)
   ```
3. Capture full form

**Annotations Needed:**
- Explain recognition methods
- Highlight progress calculation

##### 031_Performance_Obligation_Progress.png
**What to Capture:** Progress tracking section

**Steps:**
1. In PO form, show progress section:
   ```
   Estimated Total Cost: $250,000
   Actual Costs to Date: $187,500
   Completion %: 75%
   Revenue to Date: $225,000
   Revenue Remaining: $75,000
   ```
2. Show progress bar
3. Capture section

**Annotations Needed:**
- Show calculation formula
- Highlight progress bar

##### 032_PO_Revenue_Recognition_Methods.png
**What to Capture:** Recognition method selection

**Steps:**
1. Show radio buttons:
   ```
   ○ Point in Time
   ● Over Time
   ```
2. When "Over Time" selected, show sub-options:
   ```
   ● Input Method
   ○ Output Method
   ○ Time-Based
   ```
3. Capture options

**Annotations Needed:**
- Explain each method
- Show when to use each

##### 033_Revenue_Schedule_Form.png
**What to Capture:** Revenue schedule form

**Steps:**
1. Navigate: Revenue Schedules → New
2. Fill header
3. Show schedule lines grid:
   ```
   Q1 2025: Planned $75k, Recognized $75k, Deferred $0
   Q2 2025: Planned $75k, Recognized $60k, Deferred $15k
   Q3 2025: Planned $75k, Recognized $0, Deferred $75k
   Q4 2025: Planned $75k, Recognized $0, Deferred $75k
   ```
4. Capture full form

**Annotations Needed:**
- Highlight totals
- Show status colors

##### 034_Revenue_Schedule_Chart.png
**What to Capture:** Revenue recognition chart

**Steps:**
1. In schedule form, scroll to chart section
2. Chart shows planned vs. recognized revenue over time
3. Clear legend visible
4. Capture chart

**Notes:**
- Chart should be clear and readable
- Colors distinguishable

##### 035_Revenue_Variance_Analysis.png
**What to Capture:** Variance analysis section

**Steps:**
1. Show variance calculations
2. Select a period with variance
3. Show reason field filled
4. Capture section

**Annotations Needed:**
- Highlight variance amount
- Explain reason importance

#### Category 6: Multi-Currency Forms

##### 040_Currency_Master_Form.png
**What to Capture:** Currency master form

**Steps:**
1. Navigate: Setup → Currency Master → New/Edit
2. Show currency EUR:
   ```
   Code: EUR
   Name: Euro
   Symbol: €
   Decimal Places: 2
   ISO Code: EUR
   ☑ Active
   ☑ Allow in Contracts
   ```
3. Capture form

##### 041_Currency_Exchange_Rate_Config.png
**What to Capture:** Exchange rate configuration

**Steps:**
1. In currency form, show rate config:
   ```
   Rate Source: Auto-Update
   External Service: European Central Bank
   Update Frequency: Daily
   Current Rate: 0.8500
   Last Updated: [timestamp]
   ```
2. Capture section

**Annotations Needed:**
- Explain auto-update
- Show rate source options

##### 042_Exchange_Rate_Management.png
**What to Capture:** Exchange rate management screen

**Steps:**
1. Navigate: Setup → Exchange Rates
2. Show rate grid for multiple currencies
3. Show current rates, buying/selling
4. Highlight different rate sources
5. Capture full screen

**Annotations Needed:**
- Label columns
- Explain buying vs. selling

##### 043_Exchange_Rate_History_Chart.png
**What to Capture:** Historical rate chart

**Steps:**
1. In exchange rate form, show chart section
2. Select currency pair (EUR/USD)
3. Chart shows 30-day trend
4. Statistics visible
5. Capture chart

**Notes:**
- Ensure chart is clear
- Show trend line

##### 044_Currency_Converter.png
**What to Capture:** Quick currency converter

**Steps:**
1. Show converter section:
   ```
   10,000.00 EUR = 8,500.00 USD
   ```
2. Both dropdowns visible
3. Conversion arrow clear
4. Rate and date shown
5. Capture converter

##### 045_Multi_Currency_Contract.png
**What to Capture:** Contract in foreign currency

**Steps:**
1. Create contract in EUR
2. Show exchange rate field active
3. Show both EUR and USD amounts
4. Capture form highlighting currency fields

**Annotations Needed:**
- Show exchange rate calculation
- Highlight converted amounts

#### Category 7: Reports

##### 050_Revenue_Recognition_Report.png
**What to Capture:** Revenue recognition summary report

**Steps:**
1. Navigate: Reports → Revenue Recognition Summary
2. Set parameters:
   ```
   Period: Q1 2025
   Contract: All
   Customer: All
   Currency: USD
   ```
3. Click Generate
4. Report displays
5. Capture full report

**Notes:**
- Should show multiple contracts
- Summary section visible

##### 051_Revenue_Report_Parameters.png
**What to Capture:** Report parameter selection screen

**Steps:**
1. Before generating report, show parameter form
2. All dropdowns and date pickers visible
3. Capture parameter section

##### 052_Revenue_Report_Executive_Summary.png
**What to Capture:** Executive summary section

**Steps:**
1. In generated report, show top summary boxes:
   ```
   Total Contract Value: $5,250,000
   Recognized Revenue: $2,850,000
   Deferred Revenue: $2,400,000
   ```
2. Show trend indicator
3. Capture summary

**Annotations Needed:**
- Highlight key metrics
- Explain percentages

##### 053_Revenue_Report_Contract_Detail.png
**What to Capture:** Revenue by contract table

**Steps:**
1. Show detailed contract list
2. Multiple contracts visible
3. Sort/filter visible
4. Pagination shown
5. Capture grid

##### 054_Revenue_Report_Chart.png
**What to Capture:** Revenue trend chart

**Steps:**
1. Scroll to chart section
2. Chart shows revenue over multiple periods
3. Legend clear
4. Capture chart

**Notes:**
- Chart should be large and clear
- Both deferred and recognized visible

##### 055_Revenue_Aging_Analysis.png
**What to Capture:** Aging analysis table

**Steps:**
1. Show aging buckets:
   ```
   0-30 days
   31-60 days
   61-90 days
   etc.
   ```
2. Amounts and percentages visible
3. Capture table

##### 056_Contract_Summary_Report.png
**What to Capture:** Contract summary report

**Steps:**
1. Navigate: Reports → Contract Summary
2. Generate report
3. Show contract list with status
4. Capture full report

##### 057_Backlog_Report.png
**What to Capture:** Contract backlog report

**Steps:**
1. Navigate: Reports → Contract Backlog
2. Show remaining revenue by contract
3. Aging and forecast visible
4. Capture report

##### 058_Custom_Report_Builder.png
**What to Capture:** Custom report builder (if available)

**Steps:**
1. Navigate: Reports → Custom Report
2. Show field selection
3. Show filter options
4. Capture builder interface

#### Category 8: Setup & Configuration

##### 060_Addon_Settings.png
**What to Capture:** Add-on settings screen

**Steps:**
1. Navigate: Setup → Add-on Settings
2. Show configuration options:
   ```
   - Default currency
   - Auto-numbering format
   - Retention default %
   - Approval thresholds
   - Email notifications
   ```
3. Capture settings

##### 061_User_Permissions.png
**What to Capture:** User permissions configuration

**Steps:**
1. Navigate: Setup → User Permissions
2. Show permission matrix
3. Different roles visible
4. Capture permission grid

##### 062_Approval_Workflow_Setup.png
**What to Capture:** Approval workflow configuration

**Steps:**
1. Navigate: Setup → Approval Workflows
2. Show workflow designer
3. Multiple approval levels
4. Capture workflow

##### 063_Email_Template_Setup.png
**What to Capture:** Email template configuration

**Steps:**
1. Navigate: Setup → Email Templates
2. Show template editor
3. Sample template selected
4. Capture editor

##### 064_Audit_Log.png
**What to Capture:** Audit log viewer

**Steps:**
1. Navigate: Tools → Audit Log
2. Show log entries
3. Filter options visible
4. Capture log screen

##### 065_Data_Import_Wizard.png
**What to Capture:** Data import wizard

**Steps:**
1. Navigate: Tools → Import Data
2. Show wizard steps
3. Sample file template
4. Capture wizard

#### Category 9: Lists and Browsers

##### 070_Contract_List.png
**What to Capture:** Contract list/browser

**Steps:**
1. Navigate: Contracts → List
2. Show multiple contracts
3. Different statuses
4. Search/filter visible
5. Capture list

**Annotations Needed:**
- Show filter button
- Highlight search box

##### 071_IPC_List.png
**What to Capture:** IPC list

**Steps:**
1. Navigate: IPC → List
2. Show IPCs with different statuses
3. Group by contract option visible
4. Capture list

##### 072_Performance_Obligation_List.png
**What to Capture:** Performance obligation list

**Steps:**
1. Navigate: Performance Obligations → List
2. Show multiple POs
3. Progress bars visible
4. Capture list

##### 073_Change_Order_List.png
**What to Capture:** Change order list

**Steps:**
1. Navigate: Change Orders → List
2. Show COs with approval status
3. Pending items highlighted
4. Capture list

---

## Editing Screenshots

### Annotation Guidelines

#### Adding Callouts

**Purpose:** Highlight important elements

**How to Add:**
1. Open screenshot in Paint.NET or Snagit Editor
2. Select "Shapes" → "Callout"
3. Choose style: Rectangle with arrow
4. Color: Red (#DC3545) for emphasis
5. Border: 2-3px solid
6. Fill: Semi-transparent or white
7. Add number or text inside

**Example:**
```
┌─────────────┐
│   Contract  │ ← Red callout box
│    Code     │   with white fill
└─────────────┘
```

#### Adding Arrows

**Purpose:** Direct attention to specific fields

**How to Add:**
1. Select "Shapes" → "Arrow"
2. Color: Red (#DC3545)
3. Width: 3-4px
4. Style: Solid line with arrowhead
5. Draw from callout to target

#### Adding Numbers

**Purpose:** Create step-by-step sequences

**How to Add:**
1. Use circles with numbers
2. Color: Blue (#0070C0) with white text
3. Size: 24-30px diameter
4. Font: Bold, 14pt
5. Place in sequence order

**Example:**
```
① Fill Contract Code
② Select Customer
③ Enter Dates
```

#### Adding Text Labels

**Purpose:** Explain form elements

**How to Add:**
1. Select "Text" tool
2. Font: Segoe UI, 10pt, Bold
3. Color: Dark gray (#333333)
4. Background: White with border
5. Keep text concise (3-5 words)

#### Highlighting Areas

**Purpose:** Emphasize sections

**How to Add:**
1. Use semi-transparent overlay
2. Color: Yellow (#FFEB3B) at 30% opacity
3. OR use border: 3px solid color around section
4. Don't obscure text

#### Blurring Sensitive Data

**Purpose:** Hide confidential information

**How to Add:**
1. Select area to blur
2. Apply Gaussian blur (radius: 15-20)
3. OR use black bar overlay
4. Verify data not readable

### Editing Workflow

**For Each Screenshot:**

1. **Open Original:**
   - Use Paint.NET or Snagit Editor
   - Keep original file safe (make copy)

2. **Crop (if needed):**
   - Remove unnecessary borders
   - Keep focus on form
   - Maintain context

3. **Add Annotations:**
   - Follow priority: Numbers → Arrows → Callouts → Text
   - Don't over-annotate
   - Keep it clean

4. **Review:**
   - Zoom to 100% to check clarity
   - Verify all text readable
   - Check annotation alignment

5. **Save:**
   - Save as PNG (high quality)
   - File name: [original]_annotated.png
   - Keep both versions

### Annotation Examples

#### Example 1: Form Field Numbering

```
Before:
┌──────────────────────────┐
│ Contract Code: [____]    │
│ Contract Name: [____]    │
│ Customer Code: [____]    │
└──────────────────────────┘

After:
┌──────────────────────────┐
│ ① Contract Code: [____]  │
│ ② Contract Name: [____]  │
│ ③ Customer Code: [____]  │
└──────────────────────────┘
```

#### Example 2: Process Flow

```
  ┌─────────┐    ┌──────────┐    ┌─────────┐
  │ Create  │ →  │ Validate │ →  │  Save   │
  │Contract │    │  Data    │    │Contract │
  └─────────┘    └──────────┘    └─────────┘
      ①              ②                ③
```

#### Example 3: Highlighting Button

```
┌─────────────────────────────────┐
│  [Save]  [Cancel]  [Print]      │ ← Highlight Save button
└─────────────────────────────────┘   with red border
     ↑
   Click here
```

---

## File Management

### Naming Convention

**Format:** `###_Description_Optional.png`

| Component | Description | Example |
|-----------|-------------|---------|
| ### | Three-digit number | 001, 015, 042 |
| Description | Short descriptive name | Contract_Form_Header |
| Optional | Variant or state | _Approved, _Error |
| Extension | Always PNG | .png |

**Examples:**
- `005_Contract_Form_Header.png`
- `010_Contract_Validation_Error.png`
- `021_IPC_Status_Approved.png`
- `050_Revenue_Recognition_Report.png`

### Folder Structure

```
Screenshots/
├── Original/
│   ├── 001_SAP_Login.png
│   ├── 002_SAP_Main_Window.png
│   └── ...
├── Annotated/
│   ├── 001_SAP_Login_annotated.png
│   ├── 002_SAP_Main_Window_annotated.png
│   └── ...
├── Draft/
│   └── (work in progress)
└── Archive/
    └── (old versions)
```

### Version Control

**Track Changes:**
```
File: 005_Contract_Form_Header.png
Version 1.0: Initial capture - 2025-01-15
Version 1.1: Added annotations - 2025-01-16
Version 1.2: Fixed blur on phone - 2025-01-17
Version 2.0: Recaptured with new data - 2025-01-20
```

**Use Metadata:**
- Windows: Right-click → Properties → Details
- Add tags: version, category, status
- Add comments: what changed

### Backup Strategy

1. **Primary Backup:**
   - Copy to network drive daily
   - Path: `\\server\Screenshots\ContractMgmt\`

2. **Cloud Backup:**
   - Upload to SharePoint/OneDrive
   - Sync weekly

3. **Local Backup:**
   - Copy to external drive
   - Before major edits

---

## Quality Checklist

### Before Capture

- [ ] System requirements met (resolution, scaling, theme)
- [ ] SAP B1 configured correctly (language, format)
- [ ] Test data loaded and realistic
- [ ] Form is in correct state
- [ ] No other windows overlapping
- [ ] No personal/sensitive data visible

### During Capture

- [ ] Form centered on screen
- [ ] Full form visible (no cutoffs)
- [ ] Cursor not visible (unless needed)
- [ ] No unnecessary UI elements
- [ ] Screenshot tool set to correct settings
- [ ] Image saved to correct location

### After Capture

- [ ] Image quality is high (clear, sharp)
- [ ] Resolution is correct (1920x1080 or better)
- [ ] File size is reasonable (< 2MB)
- [ ] Correct file name applied
- [ ] Data is realistic (not "test" or "xxx")
- [ ] No validation errors (unless intended)
- [ ] All text is readable at 100% zoom

### After Editing

- [ ] Annotations are clear and visible
- [ ] Colors follow guidelines (red for emphasis, blue for steps)
- [ ] Text is legible (font size 10pt+)
- [ ] Arrows point to correct targets
- [ ] Numbering is sequential
- [ ] No spelling errors in annotations
- [ ] Both original and annotated versions saved
- [ ] Files backed up

### Final Review

- [ ] Screenshot matches description in guide
- [ ] All required elements visible
- [ ] Annotations helpful (not cluttered)
- [ ] Professional appearance
- [ ] Ready for documentation
- [ ] Approved by reviewer

---

## Troubleshooting

### Common Issues and Solutions

#### Issue 1: Screenshot is Blurry

**Causes:**
- Low screen resolution
- DPI scaling enabled
- Screenshot tool using compression
- Captured from remote desktop

**Solutions:**
- Set screen resolution to 1920x1080 or higher
- Disable DPI scaling (set to 100%)
- Use PNG format without compression
- Capture from local machine, not RDP

#### Issue 2: Colors Look Different

**Causes:**
- Monitor color calibration
- Wrong color profile
- Screenshot tool settings

**Solutions:**
- Use standard sRGB color profile
- Calibrate monitor
- Check screenshot tool color settings
- Use same monitor for all captures

#### Issue 3: Form Not Fully Visible

**Causes:**
- Form larger than screen
- Scrollbars present
- Zoom level incorrect

**Solutions:**
- Use higher resolution monitor
- Use Snagit scrolling capture
- Zoom SAP B1 to 100%
- Capture in sections and stitch

#### Issue 4: Unwanted Elements in Screenshot

**Causes:**
- Desktop icons visible
- Other windows overlapping
- System notifications

**Solutions:**
- Hide desktop icons (Right-click desktop → View → Show desktop icons)
- Close all other windows
- Enable "Do Not Disturb" mode
- Use Window capture mode (not fullscreen)

#### Issue 5: File Size Too Large

**Causes:**
- High resolution
- No compression
- Complex graphics

**Solutions:**
- Use PNG with minimal compression
- Crop unnecessary areas
- If > 2MB, use slight compression (still PNG)
- Don't use JPEG (lossy)

#### Issue 6: Can't Capture Dropdown Menus

**Causes:**
- Menu closes when switching to screenshot tool

**Solutions:**
- Use delay capture (Snagit: 5-second delay)
- Use keyboard shortcut (Alt+PrintScreen)
- Use Greenshot's dropdown capture mode
- Practice timing

#### Issue 7: SAP B1 Forms Look Different

**Causes:**
- Different SAP version
- Custom theme applied
- Wrong language

**Solutions:**
- Verify SAP B1 version (should be 9.3+)
- Reset to default Blue theme
- Change language to English
- Check UI settings

#### Issue 8: Test Data Looks Unrealistic

**Causes:**
- Auto-generated random data
- Using default "Test" values

**Solutions:**
- Use provided sample data script
- Create realistic company names (Acme, TechStart)
- Use realistic amounts (round numbers like $100,000)
- Use current dates
- Fill all fields completely

#### Issue 9: Annotations Not Clear

**Causes:**
- Wrong color choice
- Too small text
- Overlapping elements

**Solutions:**
- Use high-contrast colors (red, blue)
- Minimum font size 10pt
- Use white background for text
- Space annotations properly

#### Issue 10: Screenshot Doesn't Match Guide Description

**Causes:**
- Wrong form captured
- Missing required elements
- Different data

**Solutions:**
- Re-read capture instructions
- Verify form navigation path
- Check all fields match example
- Compare with wireframe

---

## Best Practices

### Do's

✓ **Use high resolution** - 1920x1080 minimum
✓ **Use realistic data** - Company names, amounts, dates
✓ **Capture at 100% zoom** - No scaling
✓ **Keep originals** - Never overwrite source files
✓ **Follow naming convention** - Consistent file names
✓ **Annotate clearly** - Use colors and numbers
✓ **Review before saving** - Check quality
✓ **Backup regularly** - Multiple locations
✓ **Test on different screens** - Verify readability
✓ **Get approval** - Have reviewer check

### Don'ts

✗ **Don't use JPEG** - Lossy compression
✗ **Don't capture real data** - Privacy concerns
✗ **Don't use personal info** - Names, emails, phones
✗ **Don't over-annotate** - Keep it simple
✗ **Don't use low resolution** - Below 1080p
✗ **Don't skip backups** - Risk losing work
✗ **Don't use dark theme** - Stick to standard
✗ **Don't capture errors** - Unless specifically needed
✗ **Don't use placeholder text** - "XXX", "Test", etc.
✗ **Don't rush** - Take time for quality

---

## Appendix A: Screenshot Checklist

### Quick Reference Checklist

Print this checklist for each screenshot session:

```
SCREENSHOT CAPTURE CHECKLIST
Date: ___________  Captured by: ___________

PREPARATION
□ SAP B1 running with add-on loaded
□ Screen resolution: 1920x1080 or higher
□ DPI scaling: 100%
□ SAP theme: Blue (default)
□ Test data loaded
□ Screenshot tool ready
□ Reference guide open

CAPTURE
□ Form navigated to correctly
□ All required fields filled
□ Data is realistic
□ No other windows visible
□ Form centered on screen
□ Cursor hidden
□ Screenshot captured
□ Preview checked

SAVE
□ Correct file name format
□ Saved to Screenshots/Original folder
□ Quality verified
□ Resolution checked
□ Backed up

EDIT
□ Annotations added (if needed)
□ Colors follow guidelines
□ Text is readable
□ Saved to Screenshots/Annotated folder
□ Both versions retained

REVIEW
□ Matches guide description
□ All elements visible
□ Professional appearance
□ Ready for documentation
□ Approved by reviewer

NOTES:
_________________________________
_________________________________
_________________________________
```

---

## Appendix B: Sample Data Reference

### Standard Test Data

**Use these consistent values across all screenshots:**

#### Companies
```
Primary Customer:
- Code: C00001
- Name: Acme Corporation
- Contact: John Smith
- Email: john.smith@acme.com
- Phone: +1-555-0100

Secondary Customers:
- C00002: TechStart Inc, Sarah Johnson, sarah.j@techstart.com
- C00003: Global Solutions Ltd, Michael Chen, m.chen@globalsol.com
- C00004: MegaSoft Industries, Lisa Anderson, l.anderson@megasoft.com
```

#### Contracts
```
Contract 1:
- Code: CNT-2025-00001
- Name: Web Development Project - Phase 1
- Customer: Acme Corporation (C00001)
- Value: $1,000,000
- Period: 01/01/2025 - 31/12/2025
- Type: Fixed Price

Contract 2:
- Code: CNT-2025-00002
- Name: ERP Implementation Services
- Customer: TechStart Inc (C00002)
- Value: $850,000
- Period: 01/02/2025 - 31/07/2025
- Type: Time & Materials
```

#### Items/Services
```
SRV-001: Consulting Services - Hourly rate $500
SRV-002: Implementation Services - Hourly rate $800
SRV-003: Training Services - Hourly rate $300
LIC-001: Software License - Annual - $12,500
LIC-002: Support & Maintenance - Annual - $5,000
```

#### Users
```
Project Manager: John Doe (Manager role)
Finance Manager: Jane Smith (Finance role)
Sales Director: Bob Johnson (Sales role)
System Admin: Administrator (Admin role)
```

---

## Appendix C: Tool Settings Reference

### Snagit Settings

```
File → Preferences → Capture

Image Capture:
✓ Include cursor: OFF
✓ Include layered windows: ON
✓ Capture with transparency: OFF
✓ Color scheme: Full color
✓ Resolution: Same as screen

Auto-save:
✓ Enable auto-save: ON
✓ Location: C:\Screenshots\ContractMgmt\Original\
✓ File name: Ask every time
✓ Format: PNG
✓ Quality: Best

Hotkeys:
- Capture window: Ctrl+Shift+W
- Capture region: Ctrl+Shift+R
- Capture scrolling: Ctrl+Shift+S
```

### Greenshot Settings

```
Settings → Output

Preferred Output:
✓ Save as: Save and open in editor
✓ File format: PNG
✓ JPEG quality: N/A (using PNG)
✓ Prompt for file name: Yes

Capture Settings:
✓ Include window: Yes
✓ Include cursor: No
✓ Window capture mode: Auto
✓ Capture delay: 0 seconds (adjust for dropdowns)

Hotkeys:
- Capture window: Ctrl+Alt+W
- Capture region: Ctrl+Alt+R
- Capture full screen: Ctrl+Alt+F
```

### Paint.NET Common Operations

```
Resize Image:
Image → Resize → Enter new dimensions → Maintain aspect ratio

Add Callout:
Tools → Shapes → Select Callout → Draw on canvas
Fill: Solid color or transparent
Outline: 2-3px, Color: #DC3545 (red)

Add Arrow:
Tools → Line/Curve → Draw line
Edit → Line width: 3-4px
Edit → End cap: Arrow

Add Text:
Tools → Text → Click on image
Font: Segoe UI, 10-12pt, Bold
Color: #333333 (dark gray)
Background: White rectangle first

Blur Area:
Tools → Selection → Select area
Effects → Blur → Gaussian Blur → Radius: 15-20
```

---

## Appendix D: Contact Information

### Documentation Team

**Screenshot Coordinator:**
Name: [Your Name]
Email: screenshots@company.com
Phone: [Extension]

**Quality Reviewer:**
Name: [Reviewer Name]
Email: review@company.com
Phone: [Extension]

**Technical Support:**
Name: [IT Support]
Email: itsupport@company.com
Phone: [Extension]

### Escalation

If you encounter issues:
1. First: Check Troubleshooting section (page XX)
2. Second: Contact Screenshot Coordinator
3. Third: Escalate to Quality Reviewer
4. Emergency: Contact Technical Support

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-01-XX | Documentation Team | Initial version |

---

**END OF GUIDE**

---

**Next Steps:**

1. Review this guide thoroughly
2. Set up your capture environment
3. Practice with 2-3 test screenshots
4. Review with coordinator
5. Begin systematic capture per screenshot list
6. Submit for quality review batch by batch
7. Incorporate feedback
8. Deliver final screenshot package

**Estimated Time:**
- Setup: 2 hours
- Capture all screenshots (65+): 8-12 hours
- Editing and annotation: 6-10 hours
- Review and revisions: 2-4 hours
- **Total: 18-28 hours**

**Good luck with your screenshot captures!**
