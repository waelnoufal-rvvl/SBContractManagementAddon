# Form Wireframes and Mockups
## Contract Management Add-on for SAP Business One
**Version:** 1.0.0
**Date:** January 2025
**Purpose:** Detailed wireframes and mockups for all add-on forms

---

## Table of Contents

1. [Wireframe Conventions](#wireframe-conventions)
2. [Contract Management Forms](#contract-management-forms)
3. [IPC Forms](#ipc-forms)
4. [Change Order Forms](#change-order-forms)
5. [Revenue Recognition Forms](#revenue-recognition-forms)
6. [Multi-Currency Forms](#multi-currency-forms)
7. [Reports](#reports)
8. [Design Specifications](#design-specifications)

---

## Wireframe Conventions

### Element Legend

```
[Button]          - Standard button
[_________]       - Text input field
[▼]               - Dropdown/ComboBox
[☑]               - Checkbox
[○]               - Radio button
[📅]              - Date picker
[🔍]              - Search/Browse button
[+] [-]           - Add/Remove buttons
[▶] [◀]           - Navigation buttons
[💾]              - Save button
[❌]              - Cancel/Close button
[🖨]               - Print button
[↻]               - Refresh button
═══════           - Group box border
───────           - Separator line
```

### Color Scheme (SAP B1 Standard)

- **Primary Color:** SAP Blue (#0070C0)
- **Secondary Color:** Light Gray (#F0F0F0)
- **Accent Color:** Dark Gray (#333333)
- **Success:** Green (#28A745)
- **Warning:** Yellow (#FFC107)
- **Error:** Red (#DC3545)
- **Disabled:** Light Gray (#CCCCCC)

### Typography

- **Headers:** Segoe UI, Bold, 11pt
- **Labels:** Segoe UI, Regular, 9pt
- **Input Fields:** Segoe UI, Regular, 9pt
- **Buttons:** Segoe UI, Semibold, 9pt

---

## Contract Management Forms

### 1. Contract Master Form

**Form ID:** `frmContract`
**Dimensions:** 1000px × 700px
**Type:** Modal Dialog

#### Layout Structure

```
╔═══════════════════════════════════════════════════════════════════════════════════════════════╗
║  CONTRACT MANAGEMENT - NEW CONTRACT                                          [─][□][✕]         ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                                 ║
║  ┌─ General Information ───────────────────────────────────────────────────────────────────┐  ║
║  │                                                                                           │  ║
║  │  Contract Code: *    [AUTO-GENERATED          ] [🔍]      Status: [Active    ▼]         │  ║
║  │  Contract Name: *    [________________________________________________]                  │  ║
║  │                                                                                           │  ║
║  │  Customer Code: *    [C00001        ] [🔍]      Customer Name: Acme Corporation         │  ║
║  │  Contact Person:     [John Smith              ]      Phone: [+1-555-0100    ]           │  ║
║  │  Email:              [john.smith@acme.com                              ]                 │  ║
║  │                                                                                           │  ║
║  │  Start Date: *       [01/01/2025] [📅]          End Date: * [31/12/2025] [📅]          │  ║
║  │  Contract Type:      [○ Fixed Price  ○ Time & Materials  ○ Cost Plus]                   │  ║
║  │                                                                                           │  ║
║  └───────────────────────────────────────────────────────────────────────────────────────────┘  ║
║                                                                                                 ║
║  ┌─ Financial Information ──────────────────────────────────────────────────────────────────┐  ║
║  │                                                                                           │  ║
║  │  Currency: *         [USD - US Dollar          ▼]                                        │  ║
║  │  Exchange Rate:      [1.0000    ]               [↻ Refresh]                              │  ║
║  │                                                                                           │  ║
║  │  Contract Value:     [100,000.00        ]       Local Currency: [100,000.00]            │  ║
║  │  Payment Terms:      [Net 30            ▼]                                               │  ║
║  │                                                                                           │  ║
║  │  Tax Group:          [VAT 20%           ▼]      Tax Amount: [20,000.00]                 │  ║
║  │  Total Value:        [120,000.00]                                                        │  ║
║  │                                                                                           │  ║
║  └───────────────────────────────────────────────────────────────────────────────────────────┘  ║
║                                                                                                 ║
║  ┌─ Contract Lines ─────────────────────────────────────────────────────────────────────────┐  ║
║  │                                                                                           │  ║
║  │  [+Add] [−Remove] [↑] [↓]                                      [💾 Save Lines]           │  ║
║  │  ┌────────────────────────────────────────────────────────────────────────────────────┐  │  ║
║  │  │ # │ Item Code   │ Description        │ Quantity │ Unit Price │ Discount │ Total  │  │  ║
║  │  ├───┼─────────────┼───────────────────┼──────────┼────────────┼──────────┼────────┤  │  ║
║  │  │ 1 │ SRV-001     │ Consulting Service │   100.00 │    500.00  │     5%   │ 47,500 │  │  ║
║  │  │ 2 │ SRV-002     │ Implementation     │    50.00 │    800.00  │     0%   │ 40,000 │  │  ║
║  │  │ 3 │ LIC-001     │ Software License   │     1.00 │  12,500.00 │    10%   │ 11,250 │  │  ║
║  │  │   │             │                    │          │            │          │        │  │  ║
║  │  └────────────────────────────────────────────────────────────────────────────────────┘  │  ║
║  │                                                          Subtotal: [100,000.00]           │  ║
║  │                                                                                           │  ║
║  └───────────────────────────────────────────────────────────────────────────────────────────┘  ║
║                                                                                                 ║
║  ┌─ Additional Information ─────────────────────────────────────────────────────────────────┐  ║
║  │                                                                                           │  ║
║  │  Owner:              [John Doe          ▼]       Department: [Sales       ▼]            │  ║
║  │  Remarks:            [________________________________________________]                  │  ║
║  │                      [________________________________________________]                  │  ║
║  │                                                                                           │  ║
║  │  Attachments:        [No files attached                 ] [📎 Browse] [🗑 Remove]        │  ║
║  │                                                                                           │  ║
║  └───────────────────────────────────────────────────────────────────────────────────────────┘  ║
║                                                                                                 ║
║  ┌─────────────────────────────────────────────────────────────────────────────────────────┐  ║
║  │  [💾 Save]  [❌ Cancel]  [🖨 Print]        Created: 01/15/2025 by Admin                  │  ║
║  └─────────────────────────────────────────────────────────────────────────────────────────┘  ║
║                                                                                                 ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════╝
```

#### Field Specifications

| Field | Type | Width | Required | Validation | Default |
|-------|------|-------|----------|------------|---------|
| Contract Code | Text | 20 | Yes | Alphanumeric, unique | Auto-generated |
| Contract Name | Text | 100 | Yes | Not empty | - |
| Customer Code | Browse | 15 | Yes | Valid BP code | - |
| Contact Person | Text | 50 | No | - | - |
| Email | Text | 100 | No | Valid email format | - |
| Phone | Text | 20 | No | - | - |
| Start Date | Date | - | Yes | Valid date | Current date |
| End Date | Date | - | Yes | >= Start Date | Start + 1 year |
| Contract Type | Radio | - | Yes | One selected | Fixed Price |
| Currency | ComboBox | 3 | Yes | Valid currency code | System currency |
| Exchange Rate | Numeric | 10,6 | Yes | > 0 | 1.0 |
| Contract Value | Numeric | 15,2 | Yes | >= 0 | 0.00 |
| Payment Terms | ComboBox | - | No | - | Net 30 |
| Tax Group | ComboBox | - | No | Valid tax group | Default tax |
| Owner | ComboBox | - | No | Valid user | Current user |
| Department | ComboBox | - | No | Valid department | - |
| Remarks | Memo | 500 | No | - | - |

#### Button Actions

| Button | Action | Keyboard Shortcut |
|--------|--------|-------------------|
| Save | Validate and save contract | Ctrl+S |
| Cancel | Close without saving | Esc |
| Print | Open print preview | Ctrl+P |
| Add (Line) | Add new contract line | Ctrl+N |
| Remove (Line) | Delete selected line | Delete |
| Browse (Customer) | Open customer list | F2 |
| Refresh (Rate) | Update exchange rate | - |

#### Validation Rules

1. **Contract Code:** Must be unique in @CM_CONTRACTS table
2. **End Date:** Must be >= Start Date
3. **Contract Value:** Must equal sum of line totals
4. **Currency:** If not system currency, exchange rate required
5. **Lines:** Minimum 1 line required
6. **Customer:** Must be active business partner

#### Tab Order

1. Contract Name
2. Customer Code
3. Contact Person
4. Email
5. Phone
6. Start Date
7. End Date
8. Contract Type
9. Currency
10. Contract Value
11. Payment Terms
12. Tax Group
13. Owner
14. Department
15. Remarks
16. Save Button

---

### 2. Contract Line Details Modal

**Form ID:** `frmContractLine`
**Dimensions:** 600px × 500px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║  CONTRACT LINE DETAILS                                           [─][□][✕]    ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                                 ║
║  ┌─ Item Information ────────────────────────────────────────────────────┐    ║
║  │                                                                         │    ║
║  │  Line Number:        [1         ]                                      │    ║
║  │                                                                         │    ║
║  │  Item Code: *        [SRV-001       ] [🔍]                             │    ║
║  │  Description: *      [Consulting Services - Phase 1        ]           │    ║
║  │                                                                         │    ║
║  │  Item Type:          [○ Service  ○ Item  ○ Resource]                   │    ║
║  │  Category:           [Professional Services    ▼]                      │    ║
║  │                                                                         │    ║
║  └─────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                 ║
║  ┌─ Quantity & Pricing ───────────────────────────────────────────────────┐    ║
║  │                                                                         │    ║
║  │  Quantity: *         [100.00    ]       Unit: [Hours      ▼]          │    ║
║  │  Unit Price: *       [500.00    ]                                      │    ║
║  │                                                                         │    ║
║  │  Gross Total:        [50,000.00 ]                                      │    ║
║  │  Discount %:         [5.00      ]       Discount Amt: [2,500.00]      │    ║
║  │  Tax %:              [20.00     ]       Tax Amount: [9,500.00]        │    ║
║  │                                                                         │    ║
║  │  Line Total:         [47,500.00 ]                                      │    ║
║  │                                                                         │    ║
║  └─────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                 ║
║  ┌─ Revenue Recognition ──────────────────────────────────────────────────┐    ║
║  │                                                                         │    ║
║  │  Recognition Method: [○ Point in Time  ○ Over Time]                    │    ║
║  │                                                                         │    ║
║  │  Start Date:         [01/01/2025] [📅]                                │    ║
║  │  End Date:           [31/03/2025] [📅]                                │    ║
║  │                                                                         │    ║
║  │  Allocation %:       [25.00     ]       of total contract              │    ║
║  │                                                                         │    ║
║  └─────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                 ║
║  ┌─ Additional Details ───────────────────────────────────────────────────┐    ║
║  │                                                                         │    ║
║  │  GL Account:         [4000 - Service Revenue       ▼]                  │    ║
║  │  Cost Center:        [CC-001 - Professional Services ▼]                │    ║
║  │  Project:            [PRJ-2025-001                 ▼]                  │    ║
║  │                                                                         │    ║
║  │  Line Remarks:       [____________________________________________]     │    ║
║  │                                                                         │    ║
║  └─────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                 ║
║  ┌─────────────────────────────────────────────────────────────────────────┐    ║
║  │  [💾 OK]  [❌ Cancel]                                                   │    ║
║  └─────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                 ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

#### Field Specifications

| Field | Type | Width | Required | Validation |
|-------|------|-------|----------|------------|
| Item Code | Browse | 20 | Yes | Valid item master |
| Description | Text | 100 | Yes | Not empty |
| Quantity | Numeric | 10,2 | Yes | > 0 |
| Unit | ComboBox | - | Yes | Valid UoM |
| Unit Price | Numeric | 15,2 | Yes | >= 0 |
| Discount % | Numeric | 5,2 | No | 0-100 |
| Recognition Method | Radio | - | Yes | One selected |
| Allocation % | Numeric | 5,2 | No | 0-100 |
| GL Account | ComboBox | - | No | Valid G/L account |

#### Calculations

```
Gross Total = Quantity × Unit Price
Discount Amount = Gross Total × (Discount % / 100)
Taxable Amount = Gross Total - Discount Amount
Tax Amount = Taxable Amount × (Tax % / 100)
Line Total = Taxable Amount + Tax Amount
```

---

## IPC Forms

### 3. Interim Payment Certificate (IPC) Form

**Form ID:** `frmIPC`
**Dimensions:** 1100px × 800px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════════════════════════╗
║  INTERIM PAYMENT CERTIFICATE (IPC)                                              [─][□][✕]         ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                                     ║
║  ┌─ IPC Header ──────────────────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  IPC Number: *       [IPC-2025-00001      ]       Status: [Draft      ▼]                  │    ║
║  │  Contract Code: *    [CNT-2025-00001      ] [🔍]  Contract: Web Development Project       │    ║
║  │                                                                                             │    ║
║  │  Customer:           [Acme Corporation                    ]                                │    ║
║  │  IPC Date: *         [31/01/2025] [📅]                                                    │    ║
║  │  Period:             [01/01/2025] [📅] to [31/01/2025] [📅]                               │    ║
║  │                                                                                             │    ║
║  │  IPC Type:           [☑] Regular  [☐] Final  [☐] Retention Release                        │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─ Work Summary ─────────────────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  Contract Value:               [1,000,000.00] USD                                          │    ║
║  │  Previous IPCs (Cumulative):   [  500,000.00] USD  (50.00%)                               │    ║
║  │  This IPC:                     [  250,000.00] USD  (25.00%)                               │    ║
║  │  ───────────────────────────────────────────────────────────────────                       │    ║
║  │  Cumulative to Date:           [  750,000.00] USD  (75.00%)                               │    ║
║  │  Balance Remaining:            [  250,000.00] USD  (25.00%)                               │    ║
║  │                                                                                             │    ║
║  │  Progress Bar:  [▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░] 75%                            │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─ Performance Obligations ──────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  [+Add] [−Remove] [📝 Details] [↻ Refresh]                                                 │    ║
║  │  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │    ║
║  │  │ PO Code     │ Description        │ PO Value  │ Previous │ This Period│ Total % │✓│   │    ║
║  │  ├─────────────┼───────────────────┼───────────┼──────────┼────────────┼─────────┼─┤   │    ║
║  │  │ PO-001      │ Phase 1: Design    │ 300,000   │ 300,000  │      0     │  100%   │✓│   │    ║
║  │  │ PO-002      │ Phase 2: Dev       │ 500,000   │ 200,000  │ 250,000    │   90%   │ │   │    ║
║  │  │ PO-003      │ Phase 3: Testing   │ 200,000   │      0   │      0     │    0%   │ │   │    ║
║  │  │             │                    │           │          │            │         │ │   │    ║
║  │  └─────────────────────────────────────────────────────────────────────────────────────┘   │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─ Financial Calculation ────────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  Gross Amount (This IPC):      [  250,000.00]                                              │    ║
║  │                                                                                             │    ║
║  │  Less: Retention (10%):        [  -25,000.00]                                              │    ║
║  │  Less: Previous Advances:      [  -10,000.00]                                              │    ║
║  │                                                                                             │    ║
║  │  Subtotal:                     [  215,000.00]                                              │    ║
║  │  Add: VAT (20%):               [  +43,000.00]                                              │    ║
║  │  ═══════════════════════════════════════                                                   │    ║
║  │  Net Amount Payable:           [  258,000.00] USD                                          │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─ Supporting Documents ─────────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  Work Reports:         [☑] Attached (3 files)    [📎 Browse] [👁 View] [🗑 Remove]        │    ║
║  │  Photos/Evidence:      [☑] Attached (12 files)   [📎 Browse] [👁 View] [🗑 Remove]        │    ║
║  │  Approval Docs:        [☐] Not attached           [📎 Browse] [👁 View] [🗑 Remove]        │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─ Remarks & Approval ───────────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                             │    ║
║  │  Remarks:  [___________________________________________________________________]            │    ║
║  │            [___________________________________________________________________]            │    ║
║  │                                                                                             │    ║
║  │  Prepared By:  [John Doe       ]  Date: [31/01/2025]  Signature: _____________            │    ║
║  │  Approved By:  [Jane Smith     ]  Date: [          ]  Signature: _____________            │    ║
║  │                                                                                             │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
║  ┌─────────────────────────────────────────────────────────────────────────────────────────────┐    ║
║  │  [💾 Save]  [✔ Submit for Approval]  [🖨 Print]  [❌ Cancel]                               │    ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                                     ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════════╝
```

#### IPC Status Workflow

```
┌─────────┐     ┌──────────────┐     ┌──────────┐     ┌─────────┐     ┌──────────┐
│  Draft  │ ──→ │ Pending      │ ──→ │ Approved │ ──→ │ Posted  │ ──→ │ Paid     │
│         │     │ Approval     │     │          │     │         │     │          │
└─────────┘     └──────────────┘     └──────────┘     └─────────┘     └──────────┘
     ↓                ↓                     ↓
     └────────────────┴─────────────────────┴──→ [Rejected] ──→ [Cancelled]
```

#### Field Specifications

| Field | Type | Width | Required | Validation |
|-------|------|-------|----------|------------|
| IPC Number | Text | 20 | Yes | Auto-generated, unique |
| Contract Code | Browse | 20 | Yes | Valid active contract |
| IPC Date | Date | - | Yes | Within contract period |
| Period From | Date | - | Yes | >= Contract start |
| Period To | Date | - | Yes | <= Contract end, >= Period From |
| IPC Type | Checkbox | - | Yes | One selected |
| This IPC Amount | Numeric | 15,2 | Yes | > 0, <= Balance |
| Retention % | Numeric | 5,2 | No | 0-100 |

#### Business Rules

1. **Cumulative Validation:** Total of all IPCs cannot exceed contract value
2. **Retention:** Cumulative retention tracked separately
3. **Period Overlap:** IPC periods cannot overlap for same contract
4. **Approval Required:** Status must be "Approved" before posting
5. **Final IPC:** Auto-calculated to complete 100% of contract
6. **Currency:** Must match contract currency

---

## Change Order Forms

### 4. Change Order Form

**Form ID:** `frmChangeOrder`
**Dimensions:** 950px × 650px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════════════╗
║  CHANGE ORDER                                                          [─][□][✕]       ║
╠═══════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                         ║
║  ┌─ Change Order Details ────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  CO Number: *        [CO-2025-00001   ]       Status: [Draft        ▼]        │    ║
║  │  Contract Code: *    [CNT-2025-00001  ] [🔍]  Contract: Web Development       │    ║
║  │                                                                                 │    ║
║  │  CO Date: *          [15/02/2025] [📅]                                        │    ║
║  │  Effective Date:     [20/02/2025] [📅]                                        │    ║
║  │                                                                                 │    ║
║  │  Change Type:        [○ Scope  ○ Schedule  ○ Cost  ○ All]                     │    ║
║  │  Priority:           [○ High  ○ Medium  ○ Low]                                 │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─ Change Description ───────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  Subject: *          [Additional Mobile App Development                    ]   │    ║
║  │                                                                                 │    ║
║  │  Reason for Change: *                                                          │    ║
║  │  [_____________________________________________________________________]       │    ║
║  │  [Customer requested additional mobile platform support for iOS        ]       │    ║
║  │  [_____________________________________________________________________]       │    ║
║  │                                                                                 │    ║
║  │  Impact Description:                                                           │    ║
║  │  [_____________________________________________________________________]       │    ║
║  │  [Adds 8 weeks to schedule, requires 2 additional developers          ]       │    ║
║  │  [_____________________________________________________________________]       │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─ Financial Impact ─────────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  Original Contract Value:     [1,000,000.00] USD                               │    ║
║  │  Previous Changes:            [   50,000.00] USD                               │    ║
║  │  ───────────────────────────────────────────────────                           │    ║
║  │  Current Contract Value:      [1,050,000.00] USD                               │    ║
║  │                                                                                 │    ║
║  │  This Change Order:           [  150,000.00] USD                               │    ║
║  │                                                                                 │    ║
║  │  ═══════════════════════════════════════════════════                           │    ║
║  │  Revised Contract Value:      [1,200,000.00] USD  (+20.00%)                   │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─ Schedule Impact ──────────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  Original End Date:           [30/06/2025] [📅]                               │    ║
║  │  Days Extension:              [56        ] days (8 weeks)                      │    ║
║  │  Revised End Date:            [25/08/2025] [📅]                               │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─ Approval Workflow ────────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  ┌──────────────┬─────────────────┬──────────────┬─────────────┬──────────┐   │    ║
║  │  │ Approver     │ Role            │ Status       │ Date        │ Comments │   │    ║
║  │  ├──────────────┼─────────────────┼──────────────┼─────────────┼──────────┤   │    ║
║  │  │ John Doe     │ Project Manager │ Approved     │ 15/02/2025  │ OK       │   │    ║
║  │  │ Jane Smith   │ Finance Manager │ Pending      │ -           │ -        │   │    ║
║  │  │ Bob Johnson  │ Sales Director  │ Not Started  │ -           │ -        │   │    ║
║  │  └──────────────┴─────────────────┴──────────────┴─────────────┴──────────┘   │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─ Attachments ──────────────────────────────────────────────────────────────────┐    ║
║  │                                                                                 │    ║
║  │  [📎 Revised_Scope_v2.pdf]      [👁 View] [🗑 Remove]                          │    ║
║  │  [📎 Cost_Breakdown.xlsx]       [👁 View] [🗑 Remove]                          │    ║
║  │                                                                                 │    ║
║  │  [📎 Browse Files]                                                             │    ║
║  │                                                                                 │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
║  ┌─────────────────────────────────────────────────────────────────────────────────┐    ║
║  │  [💾 Save]  [✔ Submit for Approval]  [🖨 Print]  [❌ Cancel]                   │    ║
║  └─────────────────────────────────────────────────────────────────────────────────┘    ║
║                                                                                         ║
╚═══════════════════════════════════════════════════════════════════════════════════════╝
```

#### Change Order Approval Matrix

| Change Amount (%) | Required Approvers |
|------------------|-------------------|
| 0% - 5% | Project Manager |
| 5% - 10% | Project Manager + Finance Manager |
| 10% - 25% | Project Manager + Finance Manager + Sales Director |
| > 25% | Project Manager + Finance Manager + Sales Director + CEO |

---

## Revenue Recognition Forms

### 5. Performance Obligation Form

**Form ID:** `frmPerformanceObligation`
**Dimensions:** 900px × 600px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║  PERFORMANCE OBLIGATION                                            [─][□][✕]      ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  ┌─ Obligation Header ────────────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  PO Code: *          [PO-2025-00001   ]       Status: [Active      ▼]     │   ║
║  │  Contract Code: *    [CNT-2025-00001  ] [🔍]  Description:                │   ║
║  │  PO Description: *   [Phase 1: Requirements & Design               ]       │   ║
║  │                                                                             │   ║
║  │  Performance Period:                                                        │   ║
║  │  From: *             [01/01/2025] [📅]    To: * [31/03/2025] [📅]        │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Financial Allocation ─────────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Total Contract Value:    [1,000,000.00] USD                               │   ║
║  │                                                                             │   ║
║  │  PO Standalone Price:     [  300,000.00] USD                               │   ║
║  │  Allocation %:            [      30.00] %                                  │   ║
║  │  Allocated Amount:        [  300,000.00] USD                               │   ║
║  │                                                                             │   ║
║  │  [☑] Use Standalone Selling Price                                          │   ║
║  │  [☐] Use Relative Allocation                                               │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Revenue Recognition Method ───────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Recognition Timing:                                                        │   ║
║  │  [○] Point in Time - Revenue recognized when control transfers             │   ║
║  │  [●] Over Time - Revenue recognized as performance is satisfied            │   ║
║  │                                                                             │   ║
║  │  Progress Measurement Method (if Over Time):                               │   ║
║  │  [●] Input Method - Based on costs incurred                                │   ║
║  │  [○] Output Method - Based on units delivered                              │   ║
║  │  [○] Time-Based - Straight line over period                                │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Progress Tracking ────────────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Estimated Total Cost:    [  250,000.00] USD                               │   ║
║  │  Actual Costs to Date:    [  187,500.00] USD                               │   ║
║  │  Completion %:            [      75.00] %                                  │   ║
║  │                                                                             │   ║
║  │  Revenue to Date:         [  225,000.00] USD  (75% of 300,000)            │   ║
║  │  Revenue Remaining:       [   75,000.00] USD                               │   ║
║  │                                                                             │   ║
║  │  Progress Bar:  [▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░] 75%            │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Accounting Integration ───────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Revenue G/L Account:     [4000 - Service Revenue          ▼]             │   ║
║  │  Deferred Revenue A/c:    [2400 - Deferred Revenue         ▼]             │   ║
║  │  Cost G/L Account:        [5000 - Cost of Services         ▼]             │   ║
║  │                                                                             │   ║
║  │  Cost Center:             [CC-001 - Professional Services  ▼]             │   ║
║  │  Profit Center:           [PC-001 - Consulting Division    ▼]             │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─────────────────────────────────────────────────────────────────────────────┐   ║
║  │  [💾 Save]  [📊 View Revenue Schedule]  [🖨 Print]  [❌ Cancel]            │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

#### Progress Measurement Formulas

**Input Method (Cost-to-Cost):**
```
Completion % = Actual Costs to Date / Estimated Total Cost × 100
Revenue to Date = Allocated Amount × Completion %
```

**Output Method (Units Delivered):**
```
Completion % = Units Delivered / Total Units × 100
Revenue to Date = Allocated Amount × Completion %
```

**Time-Based (Straight Line):**
```
Days Elapsed = Today - Start Date
Total Days = End Date - Start Date
Completion % = Days Elapsed / Total Days × 100
Revenue to Date = Allocated Amount × Completion %
```

---

### 6. Revenue Schedule Form

**Form ID:** `frmRevenueSchedule`
**Dimensions:** 1000px × 650px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════════════╗
║  REVENUE SCHEDULE                                                      [─][□][✕]       ║
╠═══════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                         ║
║  ┌─ Schedule Header ──────────────────────────────────────────────────────────────┐   ║
║  │                                                                                 │   ║
║  │  Schedule Code:      [RS-2025-00001   ]       Status: [Active      ▼]         │   ║
║  │  Contract:           [CNT-2025-00001  ] [🔍] Web Development Project           │   ║
║  │  PO Code:            [PO-2025-00001   ] [🔍] Phase 1: Requirements & Design    │   ║
║  │                                                                                 │   ║
║  │  Schedule Type:      [○] Monthly  [●] Quarterly  [○] Custom                    │   ║
║  │  Auto-Generate:      [☑] Generate schedule based on period                     │   ║
║  │                                                                                 │   ║
║  └─────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                         ║
║  ┌─ Revenue Schedule Lines ───────────────────────────────────────────────────────┐   ║
║  │                                                                                 │   ║
║  │  [+Add] [−Remove] [⚙ Auto-Generate] [↻ Recalculate]                           │   ║
║  │  ┌──────────────────────────────────────────────────────────────────────────┐  │   ║
║  │  │ Period      │ Period Date│ Planned    │ Recognized │ Deferred  │ Status │  │   ║
║  │  ├─────────────┼────────────┼────────────┼────────────┼───────────┼────────┤  │   ║
║  │  │ 2025-Q1     │ 31/03/2025 │  75,000.00 │  75,000.00 │       0.00│ Posted │  │   ║
║  │  │ 2025-Q2     │ 30/06/2025 │  75,000.00 │  60,000.00 │  15,000.00│ Partial│  │   ║
║  │  │ 2025-Q3     │ 30/09/2025 │  75,000.00 │       0.00 │  75,000.00│ Planned│  │   ║
║  │  │ 2025-Q4     │ 31/12/2025 │  75,000.00 │       0.00 │  75,000.00│ Planned│  │   ║
║  │  ├─────────────┴────────────┼────────────┼────────────┼───────────┼────────┤  │   ║
║  │  │ Total:                   │ 300,000.00 │ 135,000.00 │ 165,000.00│        │  │   ║
║  │  └──────────────────────────┴────────────┴────────────┴───────────┴────────┘  │   ║
║  │                                                                                 │   ║
║  └─────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                         ║
║  ┌─ Summary & Analytics ──────────────────────────────────────────────────────────┐   ║
║  │                                                                                 │   ║
║  │  Total Allocated:         [  300,000.00] USD                                   │   ║
║  │  Total Recognized:        [  135,000.00] USD  (45.00%)                         │   ║
║  │  Total Deferred:          [  165,000.00] USD  (55.00%)                         │   ║
║  │                                                                                 │   ║
║  │  Recognition Chart:                                                            │   ║
║  │  ┌───────────────────────────────────────────────────────────────────┐         │   ║
║  │  │ 300k│                                              ╱──────         │         │   ║
║  │  │ 250k│                                        ╱─────               │         │   ║
║  │  │ 200k│                                  ╱─────                     │         │   ║
║  │  │ 150k│                            ╱─────                           │         │   ║
║  │  │ 100k│                      ╱─────                                 │         │   ║
║  │  │  50k│                ╱─────                                       │         │   ║
║  │  │    0│──────┬───────┬───────┬───────┬───────┬───────┬─────────    │         │   ║
║  │  │     │  Q1  │  Q2   │  Q3   │  Q4   │  Q1   │  Q2   │  Q3         │         │   ║
║  │  │     └──────────────────────────────────────────────────────      │         │   ║
║  │  │     [─] Planned    [─] Recognized    [─] Deferred                │         │   ║
║  │  └───────────────────────────────────────────────────────────────────┘         │   ║
║  │                                                                                 │   ║
║  └─────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                         ║
║  ┌─ Variance Analysis ────────────────────────────────────────────────────────────┐   ║
║  │                                                                                 │   ║
║  │  Period: [2025-Q2         ▼]                                                   │   ║
║  │                                                                                 │   ║
║  │  Planned Revenue:         [   75,000.00] USD                                   │   ║
║  │  Recognized Revenue:      [   60,000.00] USD                                   │   ║
║  │  Variance:                [  -15,000.00] USD  (-20.00%)                        │   ║
║  │                                                                                 │   ║
║  │  Reason:  [Delayed completion of milestone 2                          ]        │   ║
║  │                                                                                 │   ║
║  └─────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                         ║
║  ┌─────────────────────────────────────────────────────────────────────────────────┐   ║
║  │  [💾 Save]  [📊 Generate Report]  [🖨 Print]  [📤 Export]  [❌ Cancel]         │   ║
║  └─────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                         ║
╚═══════════════════════════════════════════════════════════════════════════════════════╝
```

---

## Multi-Currency Forms

### 7. Currency Master Form

**Form ID:** `frmCurrency`
**Dimensions:** 700px × 500px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════╗
║  CURRENCY MASTER                                               [─][□][✕]  ║
╠═══════════════════════════════════════════════════════════════════════════╣
║                                                                             ║
║  ┌─ Currency Information ──────────────────────────────────────────────┐  ║
║  │                                                                       │  ║
║  │  Currency Code: *    [USD     ]       ☑ Active                       │  ║
║  │  Currency Name: *    [US Dollar                           ]          │  ║
║  │  Symbol:             [$       ]                                      │  ║
║  │  Decimal Places:     [2  ▼]                                          │  ║
║  │                                                                       │  ║
║  │  ISO Code:           [USD     ]       Numeric Code: [840     ]       │  ║
║  │                                                                       │  ║
║  │  [☑] System Currency                                                 │  ║
║  │  [☐] Allow in Contracts                                              │  ║
║  │                                                                       │  ║
║  └───────────────────────────────────────────────────────────────────────┘  ║
║                                                                             ║
║  ┌─ Exchange Rate Configuration ───────────────────────────────────────┐  ║
║  │                                                                       │  ║
║  │  Rate Source:        [○] Manual  [●] Auto-Update  [○] Fixed         │  ║
║  │                                                                       │  ║
║  │  External Service:   [European Central Bank    ▼]                    │  ║
║  │  Update Frequency:   [Daily             ▼]                           │  ║
║  │  Last Updated:       [31/01/2025 10:30 AM]                           │  ║
║  │                                                                       │  ║
║  │  Current Rate:       [1.0000    ]       vs [USD - System Currency]   │  ║
║  │                                                                       │  ║
║  └───────────────────────────────────────────────────────────────────────┘  ║
║                                                                             ║
║  ┌─ Rounding Rules ─────────────────────────────────────────────────────┐  ║
║  │                                                                       │  ║
║  │  Rounding Method:    [○] Round Up  [●] Round to Nearest  [○] Down   │  ║
║  │  Rounding Precision: [2 decimal places         ▼]                    │  ║
║  │                                                                       │  ║
║  │  Amount Rounding:    [○] No Rounding  [●] Round to 0.01  [○] 0.05   │  ║
║  │                                                                       │  ║
║  └───────────────────────────────────────────────────────────────────────┘  ║
║                                                                             ║
║  ┌─ Rate History (Last 10 Days) ───────────────────────────────────────┐  ║
║  │                                                                       │  ║
║  │  [↻ Refresh]  [📊 View Full History]                                 │  ║
║  │  ┌───────────────┬──────────┬──────────┬──────────┬─────────┐        │  ║
║  │  │ Date          │ Rate     │ High     │ Low      │ Source  │        │  ║
║  │  ├───────────────┼──────────┼──────────┼──────────┼─────────┤        │  ║
║  │  │ 31/01/2025    │ 1.0000   │ 1.0000   │ 1.0000   │ System  │        │  ║
║  │  │ 30/01/2025    │ 1.0000   │ 1.0000   │ 1.0000   │ System  │        │  ║
║  │  └───────────────┴──────────┴──────────┴──────────┴─────────┘        │  ║
║  │                                                                       │  ║
║  └───────────────────────────────────────────────────────────────────────┘  ║
║                                                                             ║
║  ┌───────────────────────────────────────────────────────────────────────┐  ║
║  │  [💾 Save]  [❌ Cancel]  [🔄 Update Rate Now]                         │  ║
║  └───────────────────────────────────────────────────────────────────────┘  ║
║                                                                             ║
╚═══════════════════════════════════════════════════════════════════════════╝
```

---

### 8. Exchange Rate Management Form

**Form ID:** `frmExchangeRate`
**Dimensions:** 950px × 650px
**Type:** Modal Dialog

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║  EXCHANGE RATE MANAGEMENT                                          [─][□][✕]      ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  ┌─ Rate Entry ───────────────────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Effective Date: *   [31/01/2025] [📅]                                     │   ║
║  │  From Currency: *    [EUR - Euro           ▼]                              │   ║
║  │  To Currency:        [USD - US Dollar      ▼]  (System Currency)           │   ║
║  │                                                                             │   ║
║  │  Exchange Rate: *    [0.8500    ]                                          │   ║
║  │  Inverse Rate:       [1.1765    ]  (Auto-calculated)                       │   ║
║  │                                                                             │   ║
║  │  Rate Type:          [○] Buying  [●] Selling  [○] Average                  │   ║
║  │                                                                             │   ║
║  │  [🔄 Get Latest Rate]  [⚖ Calculate Inverse]                               │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Current Rates (Today: 31/01/2025) ────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  [+Add] [✏Edit] [−Delete] [↻ Refresh All] [📥 Import] [📤 Export]         │   ║
║  │  ┌──────────────────────────────────────────────────────────────────────┐  │   ║
║  │  │Currency│Name         │Rate vs USD│Buying  │Selling │Type│Last Updated│  │   ║
║  │  ├────────┼─────────────┼───────────┼────────┼────────┼────┼────────────┤  │   ║
║  │  │ USD    │ US Dollar   │  1.0000   │ 1.0000 │ 1.0000 │Sys │ 31/01 00:00│  │   ║
║  │  │ EUR    │ Euro        │  0.8500   │ 0.8480 │ 0.8520 │Man │ 31/01 09:00│  │   ║
║  │  │ GBP    │ Pound       │  0.7300   │ 0.7285 │ 0.7315 │Auto│ 31/01 10:00│  │   ║
║  │  │ JPY    │ Yen         │110.5000   │110.250 │110.750 │Auto│ 31/01 10:00│  │   ║
║  │  │ AUD    │ Aust Dollar │  1.3500   │ 1.3475 │ 1.3525 │Man │ 31/01 08:00│  │   ║
║  │  │ CAD    │ Canadian $  │  1.2500   │ 1.2480 │ 1.2520 │Auto│ 31/01 10:00│  │   ║
║  │  └────────┴─────────────┴───────────┴────────┴────────┴────┴────────────┘  │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Rate History & Trends ─────────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Currency Pair: [EUR/USD         ▼]    Period: [Last 30 Days    ▼]        │   ║
║  │                                                                             │   ║
║  │  Chart:                                                                     │   ║
║  │  ┌───────────────────────────────────────────────────────────────────┐     │   ║
║  │  │ 0.90│                            ╱╲                                │     │   ║
║  │  │ 0.88│                      ╱────╱  ╲                               │     │   ║
║  │  │ 0.86│                ╱────╱        ╲──╲                            │     │   ║
║  │  │ 0.84│          ╱────╱                  ╲                           │     │   ║
║  │  │ 0.82│    ╱────╱                         ╲──────                    │     │   ║
║  │  │ 0.80│───╱                                                          │     │   ║
║  │  │     │─┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬─│     │   ║
║  │  │     │ 1  3  5  7  9 11 13 15 17 19 21 23 25 27 29 31  (Days)      │     │   ║
║  │  └───────────────────────────────────────────────────────────────────┘     │   ║
║  │                                                                             │   ║
║  │  Statistics:                                                                │   ║
║  │  30-Day High: [0.8950]   30-Day Low: [0.8050]   Average: [0.8500]         │   ║
║  │  Volatility: [±2.5%]     Trend: [↗ Strengthening]                          │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─ Quick Currency Converter ──────────────────────────────────────────────────┐   ║
║  │                                                                             │   ║
║  │  Amount: [10,000.00]  [EUR ▼]  ⇄  [8,500.00 ]  [USD ▼]                    │   ║
║  │                                                                             │   ║
║  │  Using rate: 0.8500  as of 31/01/2025 10:00 AM                             │   ║
║  │                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
║  ┌─────────────────────────────────────────────────────────────────────────────┐   ║
║  │  [💾 Save Rate]  [🔄 Update All Rates]  [📊 View Report]  [❌ Close]       │   ║
║  └─────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                     ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## Reports

### 9. Revenue Recognition Summary Report

**Form ID:** `frmRevenueReport`
**Dimensions:** 1200px × 800px
**Type:** Report Window

```
╔═══════════════════════════════════════════════════════════════════════════════════════════════════╗
║  REVENUE RECOGNITION SUMMARY REPORT                                            [─][□][✕]          ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                                     ║
║  ┌─ Report Parameters ────────────────────────────────────────────────────────────────────────┐   ║
║  │                                                                                             │   ║
║  │  Period:          [Q1 2025         ▼]      From: [01/01/2025][📅] To: [31/03/2025][📅]   │   ║
║  │  Contract:        [All Contracts   ▼]      Customer: [All Customers    ▼]                 │   ║
║  │  Currency:        [USD             ▼]      Status: [☑All ☐Active ☐Completed]              │   ║
║  │                                                                                             │   ║
║  │  [📊 Generate]  [🖨 Print]  [📤 Export Excel]  [📧 Email]  [💾 Save Template]             │   ║
║  │                                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
║  ┌─ Executive Summary ────────────────────────────────────────────────────────────────────────┐   ║
║  │                                                                                             │   ║
║  │  ┌─────────────────────────┬─────────────────────────┬─────────────────────────────────┐  │   ║
║  │  │  Total Contract Value   │  Recognized Revenue     │  Deferred Revenue               │  │   ║
║  │  │  $5,250,000            │  $2,850,000            │  $2,400,000                     │  │   ║
║  │  │  ────────────────────   │  ────────────────────   │  ────────────────────           │  │   ║
║  │  │  12 Active Contracts    │  54.3% Recognized      │  45.7% Deferred                 │  │   ║
║  │  └─────────────────────────┴─────────────────────────┴─────────────────────────────────┘  │   ║
║  │                                                                                             │   ║
║  │  Trend (vs. Prior Period): [↗ +12.5%]    Performance: [🟢 On Track]                       │   ║
║  │                                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
║  ┌─ Revenue by Contract ──────────────────────────────────────────────────────────────────────┐   ║
║  │                                                                                             │   ║
║  │  Sort: [Contract Value ▼]  Filter: [____________] [🔍]                                     │   ║
║  │  ┌────────────────────────────────────────────────────────────────────────────────────┐    │   ║
║  │  │Contract │Customer      │Contract  │Recognized │Deferred  │Recognition│% Complete│    │   ║
║  │  │Code     │Name          │Value     │Revenue    │Revenue   │Date       │          │    │   ║
║  │  ├─────────┼──────────────┼──────────┼───────────┼──────────┼───────────┼──────────┤    │   ║
║  │  │CNT-001  │Acme Corp     │1,000,000 │  750,000  │ 250,000  │31/03/2025 │   75.0%  │    │   ║
║  │  │CNT-002  │TechStart Inc │  850,000 │  500,000  │ 350,000  │31/03/2025 │   58.8%  │    │   ║
║  │  │CNT-003  │Global Ltd    │  750,000 │  450,000  │ 300,000  │31/03/2025 │   60.0%  │    │   ║
║  │  │CNT-004  │MegaSoft      │  650,000 │  325,000  │ 325,000  │31/03/2025 │   50.0%  │    │   ║
║  │  │CNT-005  │DataCorp      │  500,000 │  375,000  │ 125,000  │31/03/2025 │   75.0%  │    │   ║
║  │  │CNT-006  │CloudNet      │  450,000 │  225,000  │ 225,000  │31/03/2025 │   50.0%  │    │   ║
║  │  │CNT-007  │AppWorks      │  350,000 │  175,000  │ 175,000  │31/03/2025 │   50.0%  │    │   ║
║  │  │CNT-008  │DevHub        │  250,000 │   50,000  │ 200,000  │31/03/2025 │   20.0%  │    │   ║
║  │  ├─────────┴──────────────┼──────────┼───────────┼──────────┼───────────┼──────────┤    │   ║
║  │  │ TOTAL (8 of 12 shown)  │5,250,000 │2,850,000  │2,400,000 │           │   54.3%  │    │   ║
║  │  └────────────────────────┴──────────┴───────────┴──────────┴───────────┴──────────┘    │   ║
║  │                                                                                             │   ║
║  │  [◄ Prev]  Page 1 of 2  [Next ►]                                                           │   ║
║  │                                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
║  ┌─ Revenue by Period ────────────────────────────────────────────────────────────────────────┐   ║
║  │                                                                                             │   ║
║  │  Chart:                                                                                     │   ║
║  │  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │   ║
║  │  │  3M │                                          ┌────┐                                │   │   ║
║  │  │     │                                    ┌────┐│████│                                │   │   ║
║  │  │ 2.5M│                              ┌────┐│████││████│                                │   │   ║
║  │  │     │                        ┌────┐│████││████││████│                                │   │   ║
║  │  │  2M │                  ┌────┐│████││████││████││████│                                │   │   ║
║  │  │     │            ┌────┐│████││████││████││████││████│                                │   │   ║
║  │  │ 1.5M│      ┌────┐│████││████││████││████││████││████│                                │   │   ║
║  │  │     │┌────┐│▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒│                                │   │   ║
║  │  │  1M ││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒│                                │   │   ║
║  │  │     ││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒│                                │   │   ║
║  │  │ 500K││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒│                                │   │   ║
║  │  │     ││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒││▒▒▒▒│                                │   │   ║
║  │  │    0│┴────┴┴────┴┴────┴┴────┴┴────┴┴────┴┴────┴┴────┴                                │   │   ║
║  │  │     │ 2024  2024  2024  2024  2025  2025  2025  2025                                 │   │   ║
║  │  │     │  Q3    Q4    Q1    Q2    Q3    Q4    Q1    Q2                                  │   │   ║
║  │  │     └──────────────────────────────────────────────────────                          │   │   ║
║  │  │     [▒] Deferred Revenue    [█] Recognized Revenue                                   │   │   ║
║  │  └─────────────────────────────────────────────────────────────────────────────────────┘   │   ║
║  │                                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
║  ┌─ Aging Analysis ───────────────────────────────────────────────────────────────────────────┐   ║
║  │                                                                                             │   ║
║  │  ┌────────────────┬──────────────┬─────────────┬─────────────┬──────────────┐             │   ║
║  │  │ Age Bucket     │ # Contracts  │ Total Value │ Deferred    │ % of Total   │             │   ║
║  │  ├────────────────┼──────────────┼─────────────┼─────────────┼──────────────┤             │   ║
║  │  │ 0-30 days      │      3       │ 1,200,000   │   900,000   │    37.5%     │             │   ║
║  │  │ 31-60 days     │      4       │ 1,800,000   │   900,000   │    37.5%     │             │   ║
║  │  │ 61-90 days     │      2       │   950,000   │   400,000   │    16.7%     │             │   ║
║  │  │ 91-180 days    │      2       │   800,000   │   150,000   │     6.3%     │             │   ║
║  │  │ > 180 days     │      1       │   500,000   │    50,000   │     2.1%     │             │   ║
║  │  ├────────────────┼──────────────┼─────────────┼─────────────┼──────────────┤             │   ║
║  │  │ Total          │     12       │ 5,250,000   │ 2,400,000   │   100.0%     │             │   ║
║  │  └────────────────┴──────────────┴─────────────┴─────────────┴──────────────┘             │   ║
║  │                                                                                             │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
║  ┌─────────────────────────────────────────────────────────────────────────────────────────────┐   ║
║  │  Generated: 31/01/2025 3:45 PM by Admin           Page 1 of 3                              │   ║
║  └─────────────────────────────────────────────────────────────────────────────────────────────┘   ║
║                                                                                                     ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════════╝
```

---

## Design Specifications

### Color Codes

```css
/* Primary Colors */
--sap-blue: #0070C0;
--sap-blue-dark: #00569E;
--sap-blue-light: #E5F3FF;

/* Semantic Colors */
--success-green: #28A745;
--warning-yellow: #FFC107;
--error-red: #DC3545;
--info-blue: #17A2B8;

/* Neutral Colors */
--background-white: #FFFFFF;
--background-light: #F8F9FA;
--background-gray: #F0F0F0;
--border-gray: #DEE2E6;
--text-dark: #333333;
--text-medium: #666666;
--text-light: #999999;

/* Status Colors */
--status-draft: #6C757D;
--status-active: #28A745;
--status-pending: #FFC107;
--status-approved: #17A2B8;
--status-rejected: #DC3545;
--status-completed: #5CB85C;
```

### Grid System

```
Standard Grid: 12 columns
Column Width: 80px
Gutter: 20px
Container Width: 1200px (max)

Responsive Breakpoints:
- Desktop:  >= 1200px
- Tablet:   768px - 1199px
- Mobile:   < 768px
```

### Spacing

```
Padding/Margin Units:
--space-xs:  4px
--space-sm:  8px
--space-md: 12px
--space-lg: 16px
--space-xl: 24px
--space-xxl: 32px

Form Element Spacing:
- Label to Input: 4px
- Between Fields: 12px
- Between Groups: 24px
```

### Button Specifications

| Button Type | Height | Padding | Font Size | Border Radius |
|-------------|--------|---------|-----------|---------------|
| Primary | 32px | 12px 24px | 9pt | 4px |
| Secondary | 32px | 12px 20px | 9pt | 4px |
| Icon | 28px | 6px | - | 4px |
| Small | 24px | 8px 16px | 8pt | 3px |

### Input Field Specifications

| Field Type | Height | Padding | Font Size | Border |
|-----------|--------|---------|-----------|--------|
| Text Input | 28px | 6px 10px | 9pt | 1px solid #CCC |
| TextArea | Auto | 8px 10px | 9pt | 1px solid #CCC |
| ComboBox | 28px | 6px 30px 6px 10px | 9pt | 1px solid #CCC |
| Date | 28px | 6px 30px 6px 10px | 9pt | 1px solid #CCC |

### Validation States

```css
/* Valid */
.input-valid {
    border-color: #28A745;
    background-color: #F0FFF4;
}

/* Invalid */
.input-invalid {
    border-color: #DC3545;
    background-color: #FFF5F5;
}

/* Required */
.input-required::after {
    content: " *";
    color: #DC3545;
}

/* Disabled */
.input-disabled {
    background-color: #E9ECEF;
    color: #6C757D;
    cursor: not-allowed;
}
```

---

## Accessibility Guidelines

### Keyboard Navigation

- **Tab Order:** Logical left-to-right, top-to-bottom
- **Enter Key:** Activate default button or submit form
- **Escape Key:** Close modal/cancel operation
- **Arrow Keys:** Navigate grids and lists
- **F2:** Edit mode in grids
- **Ctrl+S:** Save
- **Ctrl+P:** Print
- **Ctrl+N:** New record

### Screen Reader Support

- All form fields must have associated labels
- Use ARIA labels for icon-only buttons
- Provide alt text for all images
- Use semantic HTML5 elements
- Provide skip navigation links

### Visual Indicators

- Focus states clearly visible (2px outline)
- Required fields marked with asterisk (*)
- Error messages in red with icon
- Success messages in green with icon
- Loading states with spinners

---

## Notes for Developers

1. **SAP B1 Integration:**
   - All forms must inherit from SAP.B1.Framework.Form
   - Use SAP B1 DI API for database operations
   - Follow SAP namespace conventions (@CM_)

2. **Performance:**
   - Lazy load grids with > 100 rows
   - Implement pagination for large datasets
   - Use async operations for long-running processes
   - Cache frequently accessed data

3. **Security:**
   - Validate all inputs server-side
   - Use parameterized queries
   - Implement role-based access control
   - Log all data modifications

4. **Testing:**
   - Unit test all calculations
   - Integration test workflows
   - Performance test with 1000+ contracts
   - UAT with actual users

---

**Document Version:** 1.0.0
**Last Updated:** January 2025
**Next Review:** March 2025
