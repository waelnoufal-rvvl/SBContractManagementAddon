# Workflow Tutorials
## Contract Management Add-on for SAP Business One
**Version:** 1.0.0
**Date:** January 2025
**Purpose:** Step-by-step tutorials for common workflows

---

## Table of Contents

1. [Tutorial Format](#tutorial-format)
2. [Getting Started Workflows](#getting-started-workflows)
3. [Contract Management Workflows](#contract-management-workflows)
4. [IPC Workflows](#ipc-workflows)
5. [Change Order Workflows](#change-order-workflows)
6. [Revenue Recognition Workflows](#revenue-recognition-workflows)
7. [Multi-Currency Workflows](#multi-currency-workflows)
8. [Reporting Workflows](#reporting-workflows)
9. [Administration Workflows](#administration-workflows)
10. [Troubleshooting Workflows](#troubleshooting-workflows)

---

## Tutorial Format

Each tutorial follows this structure:

```
WORKFLOW: [Name]
DIFFICULTY: [Beginner/Intermediate/Advanced]
TIME: [Estimated time to complete]
PREREQUISITES: [What you need before starting]

OVERVIEW:
[Brief description of what you'll accomplish]

STEPS:
[Detailed step-by-step instructions]

VERIFICATION:
[How to confirm successful completion]

TIPS & BEST PRACTICES:
[Helpful hints and recommendations]

TROUBLESHOOTING:
[Common issues and solutions]

RELATED WORKFLOWS:
[Links to related tutorials]
```

---

## Getting Started Workflows

### Tutorial 1.1: Initial System Setup

**DIFFICULTY:** Beginner
**TIME:** 30 minutes
**PREREQUISITES:**
- SAP B1 Administrator access
- Contract Management Add-on installed

#### OVERVIEW

Learn how to configure the Contract Management Add-on for first-time use, including setting up currencies, numbering series, and default settings.

#### STEPS

**Step 1: Verify Add-on Installation**

1. Log into SAP Business One
2. Navigate to: **Add-Ons** → **Add-On Administration**
3. Locate "Contract Management" in the list
4. Status should show: **Running**
5. If not running, right-click and select **Start**

📸 *Screenshot Reference: 002_SAP_Main_Window.png, 003_Addon_Menu_Location.png*

**Step 2: Access Add-on Settings**

1. Navigate to: **Add-Ons** → **Contract Management** → **Setup** → **Add-on Settings**
2. The Add-on Settings window opens
3. Review tabs: General, Numbering, Defaults, Approval, Email

📸 *Screenshot Reference: 060_Addon_Settings.png*

**Step 3: Configure General Settings**

1. In **General** tab:
   ```
   Default Currency: [Select your base currency, e.g., USD]
   Date Format: DD/MM/YYYY
   Decimal Places: 2
   Enable Audit Log: ☑ Checked
   Auto-Save Interval: 5 minutes
   ```

2. Click **Save**

**Step 4: Setup Numbering Series**

1. Switch to **Numbering** tab
2. Configure series for each document type:

   ```
   Contracts:
   - Prefix: CNT
   - Suffix: Year (2025)
   - Next Number: 00001
   - Format: CNT-2025-00001

   IPCs:
   - Prefix: IPC
   - Suffix: Year
   - Next Number: 00001
   - Format: IPC-2025-00001

   Change Orders:
   - Prefix: CO
   - Suffix: Year
   - Next Number: 00001
   - Format: CO-2025-00001

   Performance Obligations:
   - Prefix: PO
   - Suffix: Year
   - Next Number: 00001
   - Format: PO-2025-00001
   ```

3. Check **Auto-Generate** for all
4. Click **Save**

**Step 5: Set Default Values**

1. Switch to **Defaults** tab
2. Configure:
   ```
   Default Payment Terms: Net 30
   Default Tax Group: [Your default tax]
   Default Retention %: 10
   Default Recognition Method: Over Time
   Default Progress Method: Input Method
   Default Contract Type: Fixed Price
   Default Owner: [Current User]
   ```

3. Click **Save**

**Step 6: Configure Approval Workflows**

1. Switch to **Approval** tab
2. Set approval thresholds:

   ```
   Change Orders:
   ────────────────────────────
   0% - 5%: Project Manager
   5% - 10%: PM + Finance Manager
   10% - 25%: PM + Finance + Sales Director
   > 25%: PM + Finance + Sales + CEO

   IPC Approvals:
   ────────────────────────────
   All IPCs: Project Manager + Finance Manager

   Contract Approvals:
   ────────────────────────────
   < $100,000: Sales Manager
   $100,000 - $500,000: Sales Director
   > $500,000: Sales Director + CFO
   ```

3. Click **Add Rule** for each threshold
4. Click **Save**

**Step 7: Setup Email Notifications**

1. Switch to **Email** tab
2. Configure SMTP settings:
   ```
   SMTP Server: [Your mail server]
   Port: 587 (or 465 for SSL)
   Use SSL: ☑ Checked
   Username: [Email account]
   Password: [Password]
   From Name: Contract Management System
   From Email: contracts@yourcompany.com
   ```

3. Enable notifications:
   ```
   ☑ Contract Created
   ☑ IPC Submitted for Approval
   ☑ IPC Approved
   ☑ IPC Rejected
   ☑ Change Order Submitted
   ☑ Change Order Approved
   ☑ Revenue Recognition Posted
   ```

4. Click **Test Email** to verify
5. Click **Save**

**Step 8: Setup User Permissions**

1. Navigate to: **Setup** → **User Permissions**
2. For each user role, configure:

   ```
   Administrator:
   ────────────────────────────
   ☑ All permissions

   Project Manager:
   ────────────────────────────
   ☑ View Contracts
   ☑ Create/Edit Contracts
   ☑ Create/Edit IPCs
   ☑ Create Change Orders
   ☑ View Reports
   ☐ Delete Records
   ☐ Configure System

   Finance Manager:
   ────────────────────────────
   ☑ View Contracts
   ☑ View IPCs
   ☑ Approve IPCs
   ☑ View Revenue Recognition
   ☑ Post Revenue
   ☑ View Reports
   ☐ Delete Records

   Sales:
   ────────────────────────────
   ☑ View Contracts
   ☑ Create Contracts
   ☑ View Reports
   ☐ Edit IPCs
   ☐ View Costs
   ```

3. Click **Save**

📸 *Screenshot Reference: 061_User_Permissions.png*

**Step 9: Load Sample Data (Optional)**

1. Navigate to: **Tools** → **Import Data**
2. Select **Sample Data** option
3. Choose what to import:
   ```
   ☑ Sample Customers (10)
   ☑ Sample Items (15)
   ☑ Sample Contracts (5)
   ☐ Sample IPCs (don't import yet)
   ```

4. Click **Import**
5. Wait for completion message

📸 *Screenshot Reference: 065_Data_Import_Wizard.png*

**Step 10: Verify Setup**

1. Navigate to: **Contracts** → **New**
2. Check that:
   - Contract code auto-generates (CNT-2025-00001)
   - Default currency shows correctly
   - Default values are populated
3. Cancel (don't save yet)

#### VERIFICATION

✓ Add-on is running
✓ Settings saved successfully
✓ Numbering series configured
✓ Email test successful
✓ User permissions assigned
✓ New contract shows correct defaults

#### TIPS & BEST PRACTICES

💡 **Numbering Series:**
- Use consistent prefixes across all companies
- Include year in suffix for easy sorting
- Reserve number ranges (e.g., 1-1000 for Company A, 1001-2000 for Company B)

💡 **Approval Workflows:**
- Start with simple workflows, add complexity later
- Document approval matrix and share with team
- Use % thresholds rather than absolute amounts (works across all currencies)

💡 **Email Notifications:**
- Test with a small group first
- Don't enable all notifications immediately (can be overwhelming)
- Create email filters for contract notifications

💡 **User Permissions:**
- Follow principle of least privilege
- Review permissions quarterly
- Use SAP B1's built-in authorization groups where possible

#### TROUBLESHOOTING

**Problem:** Add-on not appearing in menu
**Solution:**
1. Check Add-On Administration - ensure it's running
2. Restart SAP B1 client
3. Check user has Add-On access in SAP authorizations

**Problem:** Numbering series conflicts
**Solution:**
1. Check if manual entry is enabled
2. Verify next number is available
3. Check for duplicates in database

**Problem:** Email test fails
**Solution:**
1. Verify SMTP settings with IT
2. Check firewall allows outbound on port 587/465
3. Try without SSL first, then enable

**Problem:** Permissions not applying
**Solution:**
1. User must log out and back in
2. Clear SAP B1 cache
3. Verify no conflicting SAP authorizations

#### RELATED WORKFLOWS

- Tutorial 1.2: Setting Up Currencies and Exchange Rates
- Tutorial 9.1: User Management Best Practices

---

### Tutorial 1.2: Setting Up Currencies and Exchange Rates

**DIFFICULTY:** Beginner
**TIME:** 20 minutes
**PREREQUISITES:**
- Initial System Setup completed (Tutorial 1.1)
- Knowledge of required currencies

#### OVERVIEW

Configure currencies and set up exchange rate management for multi-currency contracts.

#### STEPS

**Step 1: Review Existing Currencies**

1. Navigate to: **Setup** → **Currency Master** → **List**
2. Review currencies already in SAP B1
3. Note the system currency (usually USD)
4. Identify gaps (currencies you need but don't have)

**Step 2: Add New Currency**

1. Click **New** or **Add**
2. Fill in currency details:

   ```
   Example: Euro
   ────────────────────────────
   Currency Code: EUR
   Currency Name: Euro
   Symbol: €
   Decimal Places: 2
   ISO Code: EUR
   Numeric Code: 978

   Status:
   ☑ Active
   ☑ Allow in Contracts

   Rate Configuration:
   ○ Manual
   ● Auto-Update
   ○ Fixed

   External Service: European Central Bank
   Update Frequency: Daily
   Last Updated: [Auto-populated]
   ```

3. Click **Save**

📸 *Screenshot Reference: 040_Currency_Master_Form.png, 041_Currency_Exchange_Rate_Config.png*

**Step 3: Repeat for All Required Currencies**

Common business currencies:
```
USD - US Dollar (usually system currency)
EUR - Euro
GBP - British Pound
JPY - Japanese Yen
AUD - Australian Dollar
CAD - Canadian Dollar
CHF - Swiss Franc
CNY - Chinese Yuan
```

**Step 4: Configure Exchange Rates**

1. Navigate to: **Setup** → **Exchange Rates**
2. Today's date should be selected
3. For each currency, enter rates:

   ```
   Example rates vs. USD (vary by market):
   ────────────────────────────
   EUR: 0.8500 (buying: 0.8480, selling: 0.8520)
   GBP: 0.7300 (buying: 0.7285, selling: 0.7315)
   JPY: 110.5000 (buying: 110.250, selling: 110.750)
   AUD: 1.3500 (buying: 1.3475, selling: 1.3525)
   ```

4. Select **Rate Type:** Selling (use for contracts)
5. Click **Save**

📸 *Screenshot Reference: 042_Exchange_Rate_Management.png*

**Step 5: Setup Auto-Update (Recommended)**

1. For each currency, edit settings:
2. Select **Rate Source:** Auto-Update
3. Choose **External Service:**
   ```
   Options:
   - European Central Bank (EUR, GBP, etc.)
   - US Federal Reserve (USD pairs)
   - Custom API (if configured)
   ```

4. Set **Update Frequency:**
   ```
   Recommended: Daily at 9:00 AM
   (After markets open, before business hours)
   ```

5. Click **Update Rate Now** to test
6. Verify rate populated correctly
7. Click **Save**

**Step 6: View Rate History**

1. In Exchange Rate form, select a currency
2. Click **View History** or switch to History tab
3. Review historical rates (last 30-90 days)
4. Check trend chart
5. Note high/low/average

📸 *Screenshot Reference: 043_Exchange_Rate_History_Chart.png*

**Step 7: Test Currency Converter**

1. In Exchange Rate form, find **Quick Converter**
2. Test conversion:
   ```
   10,000.00 EUR
   ↓ (multiply by 0.8500)
   8,500.00 USD
   ```

3. Verify calculation matches
4. Test inverse:
   ```
   8,500.00 USD
   ↓ (divide by 0.8500 = multiply by 1.1765)
   10,000.00 EUR
   ```

📸 *Screenshot Reference: 044_Currency_Converter.png*

**Step 8: Setup Rate Update Schedule (Advanced)**

1. Navigate to: **Setup** → **Scheduled Tasks**
2. Create new task:
   ```
   Task Name: Update Exchange Rates
   Type: Exchange Rate Update
   Frequency: Daily
   Time: 09:00 AM
   Days: Monday - Friday
   Currencies: All Active
   Notification: On Failure
   ```

3. Click **Save**
4. Click **Run Now** to test
5. Check results

**Step 9: Configure Rate Tolerances (Risk Management)**

1. Navigate to: **Setup** → **Currency Settings** → **Tolerances**
2. Set warning thresholds:
   ```
   Daily Change Warning: ±5%
   Weekly Change Warning: ±10%

   Action on Breach:
   ☑ Send email notification
   ☑ Require approval for new contracts
   ☐ Block new contracts
   ```

3. Add recipients for notifications
4. Click **Save**

**Step 10: Test Multi-Currency Contract**

1. Navigate to: **Contracts** → **New**
2. Select a foreign currency (e.g., EUR)
3. Note exchange rate automatically populates
4. Enter contract value: 100,000 EUR
5. Note local currency equivalent shows: ~85,000 USD
6. Verify calculation is correct
7. Cancel (don't save yet)

📸 *Screenshot Reference: 045_Multi_Currency_Contract.png*

#### VERIFICATION

✓ All required currencies added and active
✓ Exchange rates populated for all currencies
✓ Auto-update configured and tested
✓ Historical data visible
✓ Currency converter works correctly
✓ Multi-currency contract shows correct conversion
✓ Scheduled task running successfully

#### TIPS & BEST PRACTICES

💡 **Exchange Rate Management:**
- Update rates before creating contracts (don't use stale rates)
- For large contracts, consider locking rate at contract date
- Review rates weekly for anomalies
- Keep historical data for at least 2 years (audit requirements)

💡 **Auto-Update:**
- Always have a backup manual update process
- Monitor update job failures
- Use reliable data sources (central banks preferred)
- Update during off-peak hours

💡 **Rate Types:**
- Use **Selling** rate for customer contracts (higher rate = more local currency)
- Use **Buying** rate for supplier contracts
- Use **Average** for internal reporting

💡 **Risk Management:**
- For long-term contracts, consider currency clauses
- Monitor volatile currency pairs closely
- Document exchange rate policy
- Consider hedging for large exposures

#### TROUBLESHOOTING

**Problem:** Auto-update fails
**Solution:**
1. Check internet connectivity
2. Verify API key/credentials (if using custom source)
3. Check firewall allows outbound HTTPS
4. Fall back to manual update
5. Contact external service provider

**Problem:** Rate seems incorrect
**Solution:**
1. Compare with reliable source (e.g., xe.com, Bloomberg)
2. Check if inverse rate was entered by mistake
3. Verify currency pair (USD/EUR vs EUR/USD)
4. Check update timestamp
5. Manually correct if needed

**Problem:** Historical data missing
**Solution:**
1. Historical data populates from first update
2. Import historical rates if needed:
   - Navigate: Tools → Import Rates
   - Select CSV file with historical data
   - Map columns: Date, Currency, Rate
   - Import

**Problem:** Currency not appearing in contract dropdown
**Solution:**
1. Verify currency is **Active**
2. Check **Allow in Contracts** is enabled
3. Refresh SAP B1 (Ctrl+Shift+F5)
4. Check user has currency access (SAP authorization)

**Problem:** Conversion calculation doesn't match manual calculation
**Solution:**
1. Check rounding settings (Setup → Currency Settings → Rounding)
2. Verify correct rate is being used (selling vs. buying)
3. Check if rate has more decimal places (system may use 4-6 decimals)
4. Review formula: Foreign Amount × Rate = Local Amount

#### RELATED WORKFLOWS

- Tutorial 3.4: Creating Multi-Currency Contracts
- Tutorial 6.2: Currency Conversion and Revaluation
- Tutorial 9.3: Managing Exchange Rate Risk

---

## Contract Management Workflows

### Tutorial 2.1: Creating Your First Contract

**DIFFICULTY:** Beginner
**TIME:** 15 minutes
**PREREQUISITES:**
- System setup completed
- Valid customer in SAP B1
- At least one service/item master

#### OVERVIEW

Learn how to create a complete contract from start to finish, including header information, contract lines, and saving.

#### STEPS

**Step 1: Navigate to Contracts**

1. In SAP B1, click **Add-Ons** → **Contract Management**
2. Click **Contracts** → **New**
3. Empty contract form opens

📸 *Screenshot Reference: 005_Contract_Form_Header.png*

**Step 2: Fill General Information**

1. **Contract Code:** Leave blank (auto-generated)
2. **Contract Name:** Enter descriptive name
   ```
   Example: Web Development Project - Phase 1
   ```
3. **Customer Code:** Click 🔍 Browse
   - Select customer from list
   - Double-click to select
   - Customer name auto-populates
4. **Contact Person:** Enter name
   ```
   Example: John Smith
   ```
5. **Email:** Enter contact email
   ```
   Example: john.smith@acme.com
   ```
6. **Phone:** Enter contact phone
   ```
   Example: +1-555-0100
   ```

**Step 3: Set Contract Dates**

1. **Start Date:** Click 📅 calendar
   - Select contract start date
   - Example: 01/01/2025
2. **End Date:** Click 📅 calendar
   - Select contract end date
   - Must be >= Start Date
   - Example: 31/12/2025

⚠️ **Validation:** System will warn if End Date < Start Date

**Step 4: Select Contract Type**

Choose one:
- ○ **Fixed Price** - Predetermined total amount
- ○ **Time & Materials** - Billed based on hours/costs
- ○ **Cost Plus** - Costs plus percentage markup

For this example: ● **Fixed Price**

**Step 5: Configure Currency and Financial Terms**

1. **Currency:** Select from dropdown
   ```
   Example: USD - US Dollar
   ```

2. **Exchange Rate:** Auto-populates
   - For system currency (USD): 1.0000
   - For foreign currency: Current rate from exchange rate table
   - Can override if needed (e.g., locked rate)

3. **Payment Terms:** Select from dropdown
   ```
   Example: Net 30
   ```

4. **Tax Group:** Select applicable tax
   ```
   Example: VAT 20%
   ```

**Step 6: Add Contract Lines**

1. Scroll to **Contract Lines** section
2. Click **[+Add]** button
3. Contract Line Details modal opens

📸 *Screenshot Reference: 008_Contract_Line_Details.png*

**For Line 1:**
```
Item Code: SRV-001 (Browse from item master)
Description: Consulting Services - Phase 1
Item Type: ● Service
Quantity: 100.00
Unit: Hours
Unit Price: 500.00
Discount %: 5.00

Auto-calculated:
Gross Total: 50,000.00 (100 × 500)
Discount Amount: 2,500.00 (50,000 × 5%)
Taxable Amount: 47,500.00
Tax Amount: 9,500.00 (47,500 × 20%)
Line Total: 47,500.00 (excluding tax)
```

4. Set Revenue Recognition:
```
Recognition Method: ● Over Time
Start Date: 01/01/2025
End Date: 31/03/2025
Allocation %: 25.00 (of total contract)
```

5. Set Accounting:
```
GL Account: 4000 - Service Revenue
Cost Center: CC-001 - Professional Services
Project: PRJ-2025-001
```

6. Click **[💾 OK]**
7. Line appears in grid

**For Line 2:**
```
Item Code: SRV-002
Description: Implementation Services
Quantity: 50.00
Unit: Hours
Unit Price: 800.00
Discount %: 0.00
Line Total: 40,000.00
Recognition: Over Time, Q2 2025
```

**For Line 3:**
```
Item Code: LIC-001
Description: Software License - Annual
Quantity: 1.00
Unit: Each
Unit Price: 12,500.00
Discount %: 10.00
Line Total: 11,250.00
Recognition: Point in Time, 01/01/2025
```

8. Repeat for all lines
9. Total should calculate automatically

**Step 7: Review Contract Total**

1. Scroll to bottom of lines section
2. Verify **Subtotal** matches sum of lines:
   ```
   Line 1: 47,500.00
   Line 2: 40,000.00
   Line 3: 11,250.00
   ───────────────────
   Subtotal: 98,750.00
   ```

3. Check **Tax Amount:** System calculates
4. Note **Total Value:** 98,750.00 (before tax in this example)

**Step 8: Add Additional Information (Optional)**

1. Scroll to **Additional Information** section
2. **Owner:** Select from dropdown
   ```
   Example: John Doe (Project Manager)
   ```

3. **Department:** Select department
   ```
   Example: Professional Services
   ```

4. **Remarks:** Add notes
   ```
   Example: Initial phase of 3-year project.
   Customer has option to extend.
   ```

5. **Attachments:** Click **[📎 Browse]**
   - Select statement of work PDF
   - Click Open
   - File attaches to contract

**Step 9: Validate Contract**

1. Review all fields for accuracy
2. Check for validation warnings:
   - Red asterisk (*) = Required field
   - Yellow warning = Validation issue
3. Fix any issues

Common validations:
- ✓ Contract Name filled
- ✓ Customer selected
- ✓ Start Date <= End Date
- ✓ At least 1 contract line
- ✓ Line totals match contract total

**Step 10: Save Contract**

1. Click **[💾 Save]** button (bottom left)
2. System validates all fields
3. If valid:
   - Contract Code generates: CNT-2025-00001
   - Success message appears
   - Contract status = Active
4. Contract is now saved

📸 *Screenshot Reference: 011_Contract_Saved_Success.png*

**Step 11: Verify Saved Contract**

1. Note Contract Code from success message
2. Navigate to: **Contracts** → **List**
3. Find your contract in list
4. Double-click to open
5. Verify all data saved correctly
6. Close

📸 *Screenshot Reference: 070_Contract_List.png*

#### VERIFICATION

✓ Contract created with unique code
✓ All header fields populated
✓ Contract lines added (minimum 1)
✓ Totals calculate correctly
✓ Contract saved successfully
✓ Contract appears in list
✓ Status shows "Active"

#### TIPS & BEST PRACTICES

💡 **Contract Naming:**
- Use descriptive names (not just "Contract 1")
- Include customer name or project name
- Consider adding phase or version number
- Keep under 100 characters

💡 **Contract Lines:**
- Group related services together
- Use clear, professional descriptions
- Set realistic quantities and prices
- Align revenue recognition with delivery schedule

💡 **Revenue Recognition:**
- Match recognition method to how work is delivered
- Use "Point in Time" for licenses, delivered goods
- Use "Over Time" for services, subscriptions
- Align periods with expected cash flow

💡 **Attachments:**
- Attach signed statement of work
- Include customer PO if available
- Attach any scope documents
- Keep file sizes reasonable (< 10MB each)

💡 **Common Mistakes to Avoid:**
- ✗ Using unrealistic test data
- ✗ Forgetting to set revenue recognition method
- ✗ End date before start date
- ✗ Missing required fields
- ✗ Not attaching source documents

#### TROUBLESHOOTING

**Problem:** Cannot find customer in browse list
**Solution:**
1. Verify customer exists in SAP B1 Business Partners
2. Check customer is type "Customer" (not just Supplier)
3. Check customer is Active status
4. Use search/filter in browse window
5. Refresh browse list (F5)

**Problem:** Contract Code not auto-generating
**Solution:**
1. Check numbering series setup (Setup → Add-on Settings → Numbering)
2. Verify "Auto-Generate" is enabled
3. Check next number is available (not already used)
4. Try manually entering code (if manual entry allowed)

**Problem:** Line total not calculating
**Solution:**
1. Verify Quantity and Unit Price are filled
2. Click in another field to trigger calculation
3. Check for decimal/formatting issues
4. Refresh form (F5)
5. Re-enter values if needed

**Problem:** Cannot save - validation error
**Solution:**
1. Read error message carefully
2. Look for fields highlighted in red
3. Check all required fields (*) are filled
4. Verify dates are valid (End >= Start)
5. Ensure at least 1 contract line exists

**Problem:** Currency exchange rate not populating
**Solution:**
1. Check exchange rate exists for today's date
2. Navigate: Setup → Exchange Rates
3. Add rate for selected currency
4. Return to contract and reselect currency

**Problem:** Attached file too large
**Solution:**
1. Maximum file size is typically 10-20MB
2. Compress PDF using online tools
3. Or split large files into smaller parts
4. Or store in SharePoint and attach link instead

#### RELATED WORKFLOWS

- Tutorial 2.2: Editing an Existing Contract
- Tutorial 2.3: Copying a Contract
- Tutorial 3.1: Creating Your First IPC
- Tutorial 5.1: Setting Up Performance Obligations
- Tutorial 6.1: Multi-Currency Contracts

---

### Tutorial 2.2: Editing an Existing Contract

**DIFFICULTY:** Beginner
**TIME:** 10 minutes
**PREREQUISITES:**
- At least one saved contract
- Understanding of contract states

#### OVERVIEW

Learn how to safely edit contracts, understand what can be changed, and manage contract versions.

#### STEPS

**Step 1: Find Contract to Edit**

1. Navigate to: **Contracts** → **List**
2. Use filters to find contract:
   ```
   Filter options:
   - Customer
   - Contract Code
   - Date Range
   - Status
   ```

3. Locate target contract
4. Note current status

**Step 2: Understand Edit Restrictions by Status**

```
Contract Status → What Can Be Edited
═══════════════════════════════════════

DRAFT:
✓ Everything can be edited
✓ Can delete contract

ACTIVE (No IPCs Posted):
✓ Header information
✓ Dates (with restrictions)
✓ Can add/edit lines
✓ Cannot change customer
⚠ Changing value may affect revenue recognition

ACTIVE (IPCs Posted):
✓ Header remarks only
✓ Can add new lines (with Change Order)
✓ Cannot edit existing lines
✓ Cannot change dates
✓ Cannot change value without CO
⚠ Most changes require Change Order

COMPLETED:
✓ Remarks only
✗ Cannot edit financial data
✗ Cannot add lines

CANCELLED:
✗ Cannot edit
✗ Read-only mode
```

**Step 3: Open Contract for Editing**

1. From list, double-click contract
2. Contract form opens in view mode
3. Click **[✏ Edit]** button
4. Form switches to edit mode
5. Fields become editable (per status restrictions)

**Step 4: Make Allowed Changes**

**Example: Adding a New Line to Active Contract**

1. Ensure contract status allows (Active, no IPC posted yet)
2. Scroll to **Contract Lines**
3. Click **[+Add]** button
4. Fill new line details:
   ```
   Item Code: SRV-004
   Description: Additional Training
   Quantity: 20.00
   Unit Price: 300.00
   Line Total: 6,000.00
   ```

5. Click **[OK]**
6. New line added to grid
7. **Contract Total** updates automatically
8. Original lines remain unchanged

**Example: Extending Contract End Date**

1. Check no IPCs extend beyond current end date
2. Change **End Date:**
   ```
   Old: 31/12/2025
   New: 31/03/2026 (3-month extension)
   ```

3. System validates:
   - New date must be > current end date
   - All IPCs must fall within new date range
   - All POs must fall within new date range

4. If valid, date updates

⚠️ **Warning:** Extending dates may impact revenue recognition schedule

**Step 5: Handle Changes Requiring Change Order**

If trying to make restricted change:

1. System displays message:
   ```
   "This change requires a Change Order.
   Would you like to create one now?"
   [Yes] [No]
   ```

2. Click **[Yes]**
3. Change Order form opens
4. Change Order pre-filled with:
   - Linked to current contract
   - Current vs. proposed values shown
5. Complete Change Order (see Tutorial 4.1)

6. After CO approved:
   - Return to contract
   - Change is now allowed
   - Make the edit

**Step 6: Update Remarks and Notes**

Documenting changes is critical:

1. Scroll to **Remarks** field
2. Add entry with date and description:
   ```
   [31/01/2025] Added training line (SRV-004) -
   $6,000. Customer request per email dated 30/01.
   See Change Order CO-2025-00015.
   ```

3. Clear documentation aids audits

**Step 7: Review Audit Trail**

1. Click **[History]** button (if available)
2. OR Navigate: **Tools** → **Audit Log**
3. Filter by this contract code
4. View all changes:
   ```
   Date/Time | User | Field | Old Value | New Value
   ──────────────────────────────────────────────────
   31/01 2PM | JDoe | Lines | 3 lines   | 4 lines
   31/01 2PM | JDoe | Total | $98,750   | $104,750
   15/01 10AM| Admin| Status| Draft     | Active
   ```

5. Review for accuracy

📸 *Screenshot Reference: 064_Audit_Log.png*

**Step 8: Validate Changes**

1. Review all modified fields
2. Check calculations:
   - Line totals correct
   - Contract total updated
   - Tax recalculated
3. Verify dates still valid
4. Check no validation errors

**Step 9: Save Changes**

1. Click **[💾 Save]**
2. System validates
3. Success message appears
4. Contract updated

**Alternative: Cancel Changes**

- If you want to discard changes:
  1. Click **[❌ Cancel]**
  2. Confirm: "Discard changes?" → **[Yes]**
  3. Form reverts to saved state
  4. No changes applied

**Step 10: Notify Stakeholders (If Needed)**

For significant changes:

1. Navigate to: **Tools** → **Send Notification**
2. Select recipients:
   ```
   ☑ Project Manager
   ☑ Finance Manager
   ☐ Customer
   ```

3. Email template auto-fills:
   ```
   Subject: Contract CNT-2025-00001 Updated

   The following contract has been modified:
   - Contract: Web Development Project
   - Customer: Acme Corporation
   - Changes: Added line for training services
   - New Total: $104,750 (was $98,750)
   - Modified by: John Doe
   - Date: 31/01/2025
   ```

4. Add custom message if needed
5. Click **[Send]**

#### VERIFICATION

✓ Changes made successfully
✓ Contract total updated correctly
✓ All validations pass
✓ Audit trail shows changes
✓ Stakeholders notified (if needed)
✓ Contract re-opened shows correct data

#### TIPS & BEST PRACTICES

💡 **Before Editing:**
- Always check contract status first
- Review existing IPCs and POs
- Understand impact of changes
- Get necessary approvals before editing

💡 **Change Documentation:**
- Always update Remarks field
- Include date, reason, and who requested
- Reference supporting documents (emails, COs)
- Maintain clear audit trail

💡 **When to Use Change Orders:**
- Any scope change after work starts
- Value changes > 5% of original
- Date extensions > 30 days
- Customer-requested modifications
- When contract requires it

💡 **Testing Changes:**
- Make changes in test environment first (if available)
- Verify calculations manually
- Check impact on revenue recognition
- Test with different user roles

💡 **Coordination:**
- Notify Finance before changing values
- Notify Project Manager before changing dates
- Get customer approval for scope changes
- Update project plans to match contract

#### TROUBLESHOOTING

**Problem:** Edit button is grayed out
**Solution:**
1. Check contract status - may be locked
2. Verify you have edit permission
3. Check if another user has it open
4. Contact administrator if needed

**Problem:** Cannot save - "Contract locked"
**Solution:**
1. Another user may have record open
2. Check who: Navigate to Tools → Active Users
3. Contact that user to close
4. Or administrator can force-unlock (risky)

**Problem:** Total not recalculating after line change
**Solution:**
1. Click **[↻ Refresh Totals]** button
2. Or close and reopen contract
3. Or click in Total field and press F9 (recalculate)

**Problem:** Changes lost after save
**Solution:**
1. Check for error messages during save
2. Verify changes don't violate business rules
3. Check audit log to see if save actually succeeded
4. Re-enter changes if needed

**Problem:** Cannot add line - no Add button
**Solution:**
1. May be in view mode - click Edit
2. Status may not allow (check restrictions by status)
3. May require Change Order first
4. Check user permissions

**Problem:** Date change rejected
**Solution:**
1. Check End Date >= Start Date
2. Verify no IPCs beyond new end date
3. Check no POs beyond new date range
4. May need to adjust IPC/PO dates first

#### RELATED WORKFLOWS

- Tutorial 2.1: Creating Your First Contract
- Tutorial 2.4: Cancelling a Contract
- Tutorial 4.1: Creating a Change Order
- Tutorial 5.2: Adjusting Performance Obligations

---

*[Continuing with more tutorials...]*

---

### Tutorial 3.1: Creating Your First IPC

**DIFFICULTY:** Intermediate
**TIME:** 20 minutes
**PREREQUISITES:**
- Active contract with work in progress
- Understanding of performance obligations
- Work actually performed to claim

#### OVERVIEW

Learn how to create an Interim Payment Certificate to claim payment for work completed under a contract.

#### STEPS

**Step 1: Prepare IPC Information**

Before starting, gather:
```
✓ Contract code
✓ Work completion data
✓ Period being claimed (dates)
✓ Costs incurred (if using cost-to-cost method)
✓ Supporting documentation (work reports, photos)
✓ Approval from project manager
```

**Step 2: Navigate to IPC Creation**

1. Navigate to: **Add-Ons** → **Contract Management**
2. Click **IPC** → **New**
3. Empty IPC form opens

📸 *Screenshot Reference: 015_IPC_Form_Header.png*

**Step 3: Fill IPC Header**

1. **IPC Number:** Leave blank (auto-generates)
2. **Contract Code:** Click 🔍 Browse
   - List of active contracts appears
   - Select your contract
   - Double-click to select
3. Contract details auto-populate:
   ```
   Contract Name: Web Development Project
   Customer: Acme Corporation
   Contract Value: $1,000,000
   ```

**Step 4: Set IPC Dates and Period**

1. **IPC Date:** Select certificate date
   ```
   Example: 31/01/2025 (typically month-end)
   ```

2. **Period From:** Start of work period
   ```
   Example: 01/01/2025
   ```

3. **Period To:** End of work period
   ```
   Example: 31/01/2025
   ```

⚠️ **Validation:**
- Period To >= Period From
- Both dates within contract period
- No overlapping IPCs for same contract

**Step 5: Select IPC Type**

Choose appropriate type:
```
☑ Regular - Standard monthly/quarterly IPC
☐ Final - Final payment, closes contract
☐ Retention Release - Releasing held retention
```

For first IPC: ☑ **Regular**

**Step 6: Review Work Summary**

System auto-calculates based on contract:

```
Work Summary Section:
══════════════════════════════════════

Contract Value: $1,000,000.00 USD
Previous IPCs (Cumulative): $0.00 (0%)
This IPC: [To be calculated]
───────────────────────────────────────
Cumulative to Date: [Auto-calculates]
Balance Remaining: [Auto-calculates]

Progress Bar: [░░░░░░░░░░░░░░░░░░░░] 0%
```

This updates as you enter PO data.

**Step 7: Add Performance Obligations**

1. Scroll to **Performance Obligations** section
2. Click **[+Add]** or **[↻ Refresh]**
3. System loads all POs for this contract
4. Grid shows:

```
┌──────────┬─────────────────┬──────────┬──────────┬────────────┬───────┬─┐
│PO Code   │Description      │PO Value  │Previous  │This Period │Total %│✓│
├──────────┼─────────────────┼──────────┼──────────┼────────────┼───────┼─┤
│PO-001    │Phase 1: Design  │ 300,000  │       0  │            │    0% │ │
│PO-002    │Phase 2: Dev     │ 500,000  │       0  │            │    0% │ │
│PO-003    │Phase 3: Testing │ 200,000  │       0  │            │    0% │ │
└──────────┴─────────────────┴──────────┴──────────┴────────────┴───────┴─┘
```

**Step 8: Enter Work Completed**

For each PO, enter completion:

**PO-001 (Design Phase):**
1. Double-click row or click **[📝 Details]**
2. PO Details window opens
3. Enter completion data:
   ```
   Method: Cost-to-Cost (Input Method)

   Estimated Total Cost: $250,000
   Costs Incurred to Date: $250,000
   Completion %: 100% (auto-calculated)

   Revenue to Recognize: $300,000
   (100% of $300,000 PO value)
   ```

4. Attach evidence:
   ```
   ☑ Work completed report
   ☑ Customer sign-off
   ☑ Photos of deliverables
   ```

5. Click **[OK]**
6. Grid updates:
   ```
   PO-001: This Period = $300,000, Total % = 100% ✓
   ```

**PO-002 (Development Phase):**
1. Open details
2. Enter:
   ```
   Method: Cost-to-Cost

   Estimated Total Cost: $400,000
   Costs Incurred to Date: $100,000
   Completion %: 25% (auto-calculated)

   Revenue to Recognize: $125,000
   (25% of $500,000 PO value)
   ```

3. Attach work reports
4. Click **[OK]**
5. Grid updates:
   ```
   PO-002: This Period = $125,000, Total % = 25%
   ```

**PO-003 (Testing Phase):**
- Not started yet
- Leave at 0%

**Step 9: Review Financial Calculation**

System auto-calculates based on PO completion:

```
Financial Calculation Section:
═══════════════════════════════════════

Gross Amount (This IPC):        $425,000.00
(PO-001: $300k + PO-002: $125k)

Deductions:
Less: Retention (10%):          -$42,500.00
Less: Previous Advances:        $0.00

Subtotal:                       $382,500.00

Taxes:
Add: VAT (20%):                 +$76,500.00

═══════════════════════════════════════
Net Amount Payable:             $459,000.00 USD
```

Verify calculations are correct.

📸 *Screenshot Reference: 017_IPC_Financial_Calculation.png*

**Step 10: Attach Supporting Documents**

1. Scroll to **Supporting Documents** section
2. **Work Reports:**
   - Click **[📎 Browse]**
   - Select "January_2025_Work_Report.pdf"
   - Click Open
   - Shows: ☑ Attached (1 file)

3. **Photos/Evidence:**
   - Click **[📎 Browse]**
   - Select multiple images
   - Shows: ☑ Attached (8 files)

4. **Approval Docs:**
   - If customer pre-approval required
   - Attach signed approval email/letter

📸 *Screenshot Reference: 018_IPC_Attachments.png*

**Step 11: Add Remarks**

Document important information:

```
Remarks:
─────────────────────────────────────
January 2025 IPC for Phase 1 (Design)
and 25% of Phase 2 (Development).

Phase 1 completed on schedule.
Customer sign-off received 28/01/2025.

Phase 2 development ahead of schedule.
Core modules completed: User Management,
Dashboard, and Reports.

No issues or delays this period.

Prepared by: John Doe, PM
Date: 31/01/2025
```

**Step 12: Review Approval Workflow**

Check approval requirements:

```
Approval Workflow Section:
═══════════════════════════════════════

┌────────────┬────────────────┬──────────┬──────┬────────┐
│Approver    │Role            │Status    │Date  │Comments│
├────────────┼────────────────┼──────────┼──────┼────────┤
│John Doe    │Project Manager │Pending   │ -    │ -      │
│Jane Smith  │Finance Manager │Not Start │ -    │ -      │
└────────────┴────────────────┴──────────┴──────┴────────┘

IPC will be submitted for approval upon save.
```

📸 *Screenshot Reference: 019_IPC_Approval_Workflow.png*

**Step 13: Validate IPC**

Final checks:
```
✓ Contract selected and active
✓ Dates valid (no overlaps)
✓ At least one PO has completion > 0
✓ Financial calculations correct
✓ Supporting documents attached
✓ Remarks filled
✓ No validation errors
```

**Step 14: Save as Draft**

1. Click **[💾 Save]**
2. System validates
3. IPC Code generates: IPC-2025-00001
4. Status = Draft
5. Success message:
   ```
   "IPC-2025-00001 saved successfully.
   Status: Draft
   You can edit before submitting for approval."
   ```

**Step 15: Submit for Approval**

1. Review IPC one final time
2. Click **[✔ Submit for Approval]**
3. Confirmation dialog:
   ```
   "Submit IPC-2025-00001 for approval?

   Net Amount: $459,000.00
   Approvers: John Doe, Jane Smith

   Once submitted, you cannot edit until
   approval is complete or IPC is rejected."

   [Submit] [Cancel]
   ```

4. Click **[Submit]**
5. Status changes: Draft → Pending Approval
6. Email sent to first approver (John Doe)
7. Confirmation message:
   ```
   "IPC submitted for approval.
   Current status: Pending Approval
   Next approver: John Doe (Project Manager)"
   ```

📸 *Screenshot Reference: 020_IPC_Status_Draft.png, 021_IPC_Status_Approved.png*

#### VERIFICATION

✓ IPC created with unique code
✓ Contract linked correctly
✓ All POs with work completed claimed
✓ Financial calculations correct
✓ Supporting documents attached
✓ Status = Pending Approval
✓ Approvers notified
✓ IPC appears in list

#### TIPS & BEST PRACTICES

💡 **Timing:**
- Create IPCs on regular schedule (monthly/quarterly)
- Submit before month-end closing
- Allow time for approval process (2-5 days typical)
- Don't delay - claim work promptly

💡 **Completion Percentages:**
- Be conservative - don't over-claim
- Must have evidence for claimed work
- Match completion to costs incurred (if cost-to-cost)
- Get customer confirmation before claiming 100%

💡 **Documentation:**
- Attach evidence for all claimed work
- Include photos of physical work
- Attach time sheets for services
- Include customer emails/approvals
- More documentation = faster approval

💡 **Retention:**
- Standard: 10% held until project completion
- Retention released with final IPC
- Confirm retention % matches contract
- Track cumulative retention

💡 **Common Mistakes:**
- ✗ Claiming work not yet completed
- ✗ Missing supporting documents
- ✗ Overlapping periods with previous IPC
- ✗ Incorrect calculations
- ✗ Submitting before getting PM approval

#### TROUBLESHOOTING

**Problem:** Cannot find contract in browse list
**Solution:**
1. Verify contract status is Active
2. Check contract dates (must be current)
3. Verify you have access to contract
4. Check no IPCs already cover this period

**Problem:** Performance Obligations not loading
**Solution:**
1. Verify contract has POs defined
2. Check POs are Active status
3. Click **[↻ Refresh]** button
4. If still empty, POs may need to be created first (see Tutorial 5.1)

**Problem:** Completion percentage won't calculate
**Solution:**
1. Check "Estimated Total Cost" is > 0
2. Verify "Costs Incurred" is entered
3. Check recognition method is set correctly
4. Try manual calculation: (Costs Incurred / Total Cost) × 100

**Problem:** Retention amount seems wrong
**Solution:**
1. Check contract retention % (Setup → Defaults)
2. Verify: Retention = Gross Amount × Retention %
3. Check if retention cap reached
4. Review retention held from previous IPCs

**Problem:** Cannot attach files
**Solution:**
1. Check file size (max usually 10-20MB)
2. Verify file type is allowed (.pdf, .jpg, .xlsx, etc.)
3. Check disk space on server
4. Try smaller file or compress

**Problem:** Submit button grayed out
**Solution:**
1. IPC must be in Draft status
2. Must have at least one PO with completion > 0
3. All required fields must be filled
4. No validation errors
5. Check you have submit permission

**Problem:** Approver not receiving notification
**Solution:**
1. Verify email address in user master
2. Check SMTP settings (Setup → Email)
3. Check spam/junk folder
4. Check notification is enabled for IPC approval
5. Manually notify approver if needed

#### RELATED WORKFLOWS

- Tutorial 3.2: Approving an IPC
- Tutorial 3.3: Rejecting and Correcting an IPC
- Tutorial 5.1: Setting Up Performance Obligations
- Tutorial 8.2: Posting Revenue from Approved IPC

---

*[Due to length, I'll continue with summaries of remaining key tutorials...]*

---

## Summary of Remaining Tutorials

### Change Order Workflows

**Tutorial 4.1: Creating a Change Order**
- When change orders are required
- Documenting scope/cost/schedule changes
- Approval workflows by change magnitude
- Integrating approved COs back to contracts

**Tutorial 4.2: Approval Process for Change Orders**
- Multi-level approval routing
- Reviewing and commenting
- Approving vs. rejecting
- Requesting revisions

### Revenue Recognition Workflows

**Tutorial 5.1: Setting Up Performance Obligations**
- Breaking contracts into POs
- Allocating contract value
- Choosing recognition methods (Point in Time vs. Over Time)
- Selecting progress measurement (Input/Output/Time-based)

**Tutorial 5.2: Monitoring Revenue Schedules**
- Auto-generating revenue schedules
- Monthly/quarterly recognition
- Variance analysis (planned vs. actual)
- Adjusting schedules

**Tutorial 5.3: Month-End Revenue Recognition**
- Period-end checklist
- Calculating deferred revenue
- Posting revenue journals
- Reconciliation procedures

### Multi-Currency Workflows

**Tutorial 6.1: Creating Multi-Currency Contracts**
- Selecting foreign currency
- Locking vs. floating exchange rates
- Currency conversion calculations
- Managing exchange rate risk

**Tutorial 6.2: Currency Revaluation**
- Month-end revaluation
- Unrealized gains/losses
- Posting revaluation journals
- Reporting in multiple currencies

### Reporting Workflows

**Tutorial 7.1: Running Revenue Recognition Reports**
- Parameter selection
- Filtering and grouping
- Interpreting results
- Exporting to Excel

**Tutorial 7.2: Contract Backlog Analysis**
- Calculating remaining value
- Aging analysis
- Forecast revenue
- Trend analysis

**Tutorial 7.3: Creating Custom Reports**
- Using report builder
- Selecting fields
- Adding calculations
- Saving templates

### Administration Workflows

**Tutorial 8.1: User Management**
- Adding users
- Assigning roles
- Setting permissions
- Deactivating users

**Tutorial 8.2: Backup and Recovery**
- Daily backup procedures
- Exporting data
- Restoring from backup
- Disaster recovery

**Tutorial 8.3: Performance Optimization**
- Database maintenance
- Index optimization
- Query performance tuning
- Archive old contracts

### Troubleshooting Workflows

**Tutorial 9.1: Fixing Common Errors**
- Validation errors
- Calculation mismatches
- Integration issues
- Data inconsistencies

**Tutorial 9.2: Recovery Procedures**
- Reversing posted IPCs
- Correcting revenue entries
- Fixing exchange rates
- Unlocking stuck records

**Tutorial 9.3: Support Escalation**
- When to contact support
- Gathering diagnostic information
- Log file locations
- Escalation procedures

---

## Best Practices Summary

### Planning
✓ Complete system setup before going live
✓ Train all users thoroughly
✓ Start with simple contracts, add complexity gradually
✓ Document your processes

### Execution
✓ Create contracts immediately after customer agreement
✓ Generate IPCs on regular schedule
✓ Attach all supporting documentation
✓ Get approvals before period-end close

### Monitoring
✓ Review revenue recognition monthly
✓ Reconcile to SAP GL regularly
✓ Monitor exchange rate changes
✓ Track approval workflow bottlenecks

### Maintenance
✓ Update exchange rates daily
✓ Review and archive old contracts
✓ Clean up test data periodically
✓ Keep documentation current

---

## Quick Reference: Common Tasks

### Daily Tasks
- [ ] Update exchange rates (if manual)
- [ ] Review pending approvals
- [ ] Respond to workflow notifications

### Weekly Tasks
- [ ] Review new contracts for accuracy
- [ ] Follow up on stuck approvals
- [ ] Check system performance
- [ ] Update project statuses

### Monthly Tasks
- [ ] Generate all IPCs for period
- [ ] Review revenue recognition
- [ ] Post revenue journals
- [ ] Run month-end reports
- [ ] Reconcile to GL
- [ ] Review aging analysis
- [ ] Update forecasts

### Quarterly Tasks
- [ ] Review contract backlog
- [ ] Analyze revenue trends
- [ ] Update revenue schedules
- [ ] Revalue foreign currency contracts
- [ ] Archive completed contracts
- [ ] Review and update user permissions

### Annual Tasks
- [ ] Review numbering series
- [ ] Archive historical data
- [ ] Update exchange rate sources
- [ ] Review approval workflows
- [ ] Refresh user training
- [ ] Update documentation

---

## Support Resources

### Internal Resources
- User Help Guide: `USER_HELP_GUIDE.md`
- Form Wireframes: `FORM_WIREFRAMES.md`
- Screenshot Guide: `SCREENSHOT_CAPTURE_GUIDE.md`
- Testing Guide: `AUTOMATED_TESTING_GUIDE.md`

### External Resources
- SAP B1 Help Center: https://help.sap.com/
- Revenue Recognition Standards: ASC 606 / IFRS 15
- Currency Exchange Rates: European Central Bank, Federal Reserve

### Getting Help
1. Check User Help Guide
2. Review relevant tutorial in this document
3. Search SAP B1 knowledge base
4. Contact your system administrator
5. Open support ticket: support@yourcompany.com

---

## Glossary of Terms

**Contract:** Legal agreement between company and customer for delivery of goods/services
**IPC:** Interim Payment Certificate - claim for payment based on work completed
**Performance Obligation (PO):** Distinct promise to deliver goods/services within a contract
**Revenue Schedule:** Timeline showing when revenue will be recognized over contract period
**Deferred Revenue:** Revenue received but not yet earned (liability)
**Change Order (CO):** Approved modification to original contract scope/cost/schedule
**Retention:** Percentage of payment held until contract completion (typically 10%)
**Exchange Rate:** Conversion rate between two currencies
**Point in Time:** Revenue recognized at a single point (e.g., delivery date)
**Over Time:** Revenue recognized gradually as work progresses
**Input Method:** Progress measured by costs incurred (cost-to-cost)
**Output Method:** Progress measured by units delivered
**Backlog:** Contracted but not yet recognized revenue

---

**END OF WORKFLOW TUTORIALS**

**Document Version:** 1.0.0
**Last Updated:** January 2025
**Next Review:** March 2025

---

*For questions or suggestions, contact the Documentation Team*
