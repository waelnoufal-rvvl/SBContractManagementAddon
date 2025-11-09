# Quick Reference Cards
## Contract Management Add-on for SAP Business One
**Version:** 1.0.0
**Date:** January 2025
**Purpose:** Printable quick reference guides

---

## How to Use These Cards

Each reference card is designed to fit on a single page (A4 or Letter size). Print and laminate for desk reference.

**Printing Instructions:**
1. Open this file in Word or a markdown editor
2. Select the card you want to print
3. Print selection in landscape mode (recommended)
4. Use high-quality paper (80-100gsm)
5. Laminate for durability (optional)

**Or:** Export to PDF and print individual pages

---

## Table of Contents

1. [Contract Management Quick Reference](#card-1-contract-management-quick-reference)
2. [IPC Creation Quick Reference](#card-2-ipc-creation-quick-reference)
3. [Change Order Quick Reference](#card-3-change-order-quick-reference)
4. [Revenue Recognition Quick Reference](#card-4-revenue-recognition-quick-reference)
5. [Multi-Currency Quick Reference](#card-5-multi-currency-quick-reference)
6. [Keyboard Shortcuts Quick Reference](#card-6-keyboard-shortcuts-quick-reference)
7. [Common Error Codes & Solutions](#card-7-common-error-codes--solutions)
8. [Month-End Checklist](#card-8-month-end-checklist)
9. [Approval Workflow Reference](#card-9-approval-workflow-reference)
10. [Status Code Reference](#card-10-status-code-reference)

---

## CARD 1: Contract Management Quick Reference

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                  CONTRACT MANAGEMENT - QUICK REFERENCE                            ║
║                   Contract Management Add-on v1.0.0                               ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  CREATING A CONTRACT                          CONTRACT STATUS FLOW                 ║
║  ════════════════════                         ═══════════════════                  ║
║                                                                                     ║
║  1. Add-Ons → Contract Management            ┌───────┐                             ║
║     → Contracts → New                        │ DRAFT │                             ║
║                                              └───┬───┘                             ║
║  2. Fill Required Fields:                        │ (Save)                          ║
║     • Contract Code: AUTO                        ↓                                 ║
║     • Contract Name: *                       ┌────────┐                            ║
║     • Customer Code: * (Browse)              │ ACTIVE │                            ║
║     • Start/End Date: *                      └───┬────┘                            ║
║     • Contract Type: *                           │ (Create IPCs)                   ║
║     • Currency: *                                ↓                                 ║
║                                              ┌───────────┐                         ║
║  3. Add Contract Lines (min 1):              │COMPLETED/ │                         ║
║     • Item/Service code                      │ CLOSED    │                         ║
║     • Quantity & price                       └───────────┘                         ║
║     • Revenue recognition method                                                   ║
║                                              Can Cancel at any Draft/Active stage  ║
║  4. Attach documents                                                               ║
║  5. Save (💾) or Submit                                                            ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  REQUIRED FIELDS (*)                          VALIDATION RULES                     ║
║  ════════════════════                         ════════════════                     ║
║                                                                                     ║
║  ✓ Contract Name                              • End Date >= Start Date             ║
║  ✓ Customer Code                              • At least 1 contract line           ║
║  ✓ Start Date                                 • Total Value = Sum of lines         ║
║  ✓ End Date                                   • Foreign currency needs rate        ║
║  ✓ Contract Type                              • Customer must be active            ║
║  ✓ Currency                                   • No duplicate contract codes        ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  CONTRACT TYPES                               QUICK ACTIONS                        ║
║  ═══════════════                              ═════════════                        ║
║                                                                                     ║
║  Fixed Price:                                 [Ctrl+N]  New Contract               ║
║  • Predetermined total amount                 [Ctrl+S]  Save                       ║
║  • Scope clearly defined                      [Ctrl+P]  Print                      ║
║  • Low risk for customer                      [F2]      Browse Customer            ║
║                                               [F5]      Refresh                    ║
║  Time & Materials:                            [Esc]     Cancel/Close               ║
║  • Billed on hours/costs                      [Del]     Delete Line                ║
║  • Flexible scope                                                                  ║
║  • Common for consulting                      COMMON NAVIGATION                    ║
║                                               ══════════════════                   ║
║  Cost Plus:                                                                        ║
║  • Costs + markup percentage                  Contracts → New                      ║
║  • Pass-through costs                         Contracts → List                     ║
║  • Used in construction                       Contracts → Search                   ║
║                                               Contracts → Reports                  ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  TIPS & TRICKS                                                                     ║
║  ══════════════                                                                    ║
║                                                                                     ║
║  💡 Use descriptive contract names - include customer and project                  ║
║  💡 Always attach signed statement of work                                         ║
║  💡 Set revenue recognition at line level for accuracy                             ║
║  💡 Double-check totals before saving                                              ║
║  💡 For large contracts (>$500k), get manager approval                             ║
║                                                                                     ║
║  🚫 DON'T change customer after IPCs created                                       ║
║  🚫 DON'T use test data (e.g., "Test Contract")                                    ║
║  🚫 DON'T forget to set end date                                                   ║
║                                                                                     ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║  SUPPORT: Help → User Guide  |  Email: support@company.com  |  Ext: 5555         ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## CARD 2: IPC Creation Quick Reference

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                      IPC CREATION - QUICK REFERENCE                               ║
║              Interim Payment Certificate (IPC) Guide                              ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  IPC CREATION STEPS                           IPC CALCULATION                      ║
║  ═══════════════════                          ═══════════════                      ║
║                                                                                     ║
║  1. Gather Information:                       Gross Amount (Work Done)             ║
║     ✓ Work completion data                    - Retention (typically 10%)          ║
║     ✓ Period dates                            - Previous Advances                  ║
║     ✓ Supporting documents                    ─────────────────────                ║
║     ✓ Manager approval                        = Subtotal                           ║
║                                               + Tax (VAT/Sales Tax)                ║
║  2. Create IPC:                               ═════════════════════                ║
║     Add-Ons → IPC → New                       = NET AMOUNT PAYABLE                 ║
║                                                                                     ║
║  3. Fill Header:                              Example:                             ║
║     • IPC Date: Month-end                     Work Done:        $100,000           ║
║     • Contract: Browse (F2)                   Retention (10%):  -$10,000           ║
║     • Period: Start & End dates               Advances:          -$5,000           ║
║     • Type: Regular/Final/Retention           Subtotal:          $85,000           ║
║                                               VAT (20%):        +$17,000           ║
║  4. Enter PO Completion:                      ─────────────────────────            ║
║     For each Performance Obligation:          NET PAYABLE:      $102,000           ║
║     • Completion % (or costs)                                                      ║
║     • Revenue amount                                                               ║
║                                                                                     ║
║  5. Attach Documents:                                                              ║
║     • Work reports                                                                 ║
║     • Photos/evidence                                                              ║
║     • Customer approvals                                                           ║
║                                                                                     ║
║  6. Review Calculations                                                            ║
║  7. Save as Draft                                                                  ║
║  8. Submit for Approval                                                            ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  COMPLETION METHODS                           IPC TYPES                            ║
║  ═══════════════════                          ═════════                            ║
║                                                                                     ║
║  Cost-to-Cost (Input Method):                 REGULAR                              ║
║  ────────────────────────────                 • Monthly/quarterly                  ║
║  % Complete = Costs Incurred                  • Claims work done                   ║
║                ─────────────                  • Most common type                   ║
║                Estimated Total                                                     ║
║                                               FINAL                                ║
║  Example:                                     • Last payment                       ║
║  Costs: $75k of $100k = 75%                   • Closes contract                    ║
║                                               • 100% of contract value             ║
║  Units Delivered (Output Method):                                                  ║
║  ────────────────────────────────             RETENTION RELEASE                    ║
║  % Complete = Units Done                      • Releases held retention            ║
║                ───────────                    • After defects period               ║
║                Total Units                    • Usually final 10%                  ║
║                                                                                     ║
║  Time-Based (Straight Line):                                                       ║
║  ────────────────────────────                                                      ║
║  % Complete = Days Elapsed                                                         ║
║                ────────────                                                        ║
║                Total Days                                                          ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  PRE-IPC CHECKLIST                            APPROVAL STATUS                      ║
║  ══════════════════                           ═══════════════                      ║
║                                                                                     ║
║  Before creating IPC, verify:                 DRAFT → Can edit                     ║
║                                               ↓                                    ║
║  □ Work actually completed                    PENDING → Awaiting approval          ║
║  □ Customer sign-off received                 ↓                                    ║
║  □ Costs tracked and accurate                 APPROVED → Ready to post             ║
║  □ No overlapping IPC periods                 ↓                                    ║
║  □ Previous IPC approved/posted               POSTED → Revenue recognized          ║
║  □ Period within contract dates               ↓                                    ║
║  □ Supporting docs ready                      PAID → Payment received              ║
║  □ Manager pre-approval obtained                                                   ║
║                                               REJECTED → Back to Draft             ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  COMMON MISTAKES TO AVOID                     REQUIRED ATTACHMENTS                 ║
║  ═════════════════════════                    ════════════════════                 ║
║                                                                                     ║
║  ✗ Over-claiming work not done                ✓ Work completion report             ║
║  ✗ Missing supporting documents               ✓ Time sheets (if T&M)               ║
║  ✗ Overlapping with previous IPC              ✓ Expense receipts (if Cost+)        ║
║  ✗ Submitting before month-end                ✓ Customer sign-offs                 ║
║  ✗ Incorrect retention calculation            ✓ Photos of completed work           ║
║  ✗ Not getting manager approval               ✓ Test/inspection reports            ║
║  ✗ Wrong currency/exchange rate                                                    ║
║                                                                                     ║
║  💡 TIP: Be conservative with completion %    💡 TIP: More evidence = faster       ║
║          Don't claim until work verified              approval                     ║
║                                                                                     ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║  SUPPORT: Help → IPC Guide  |  Email: ipc@company.com  |  Ext: 5556              ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## CARD 3: Change Order Quick Reference

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                    CHANGE ORDER - QUICK REFERENCE                                 ║
║                   Contract Change Management                                       ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  WHEN TO CREATE CHANGE ORDER                  APPROVAL REQUIREMENTS                ║
║  ════════════════════════════                 ═══════════════════════              ║
║                                                                                     ║
║  Required for:                                Change Value | Approvers Needed      ║
║                                               ═════════════════════════════        ║
║  ✓ Scope changes after work starts            0% - 5%     │ Project Manager       ║
║  ✓ Value changes > 5% of original             ────────────┼───────────────────    ║
║  ✓ Schedule changes > 30 days                 5% - 10%    │ PM + Finance Mgr      ║
║  ✓ Customer-requested modifications           ────────────┼───────────────────    ║
║  ✓ Significant design changes                 10% - 25%   │ PM + Finance +        ║
║                                               │           │ Sales Director        ║
║  Not required for:                            ────────────┼───────────────────    ║
║                                               > 25%       │ PM + Finance +        ║
║  • Minor clarifications                       │           │ Sales + CEO           ║
║  • Typo corrections                           ═════════════════════════════        ║
║  • Contact info updates                                                            ║
║  • Administrative changes                     💡 % calculated from ORIGINAL        ║
║                                                  contract value                    ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  CO CREATION STEPS                            CHANGE TYPES                         ║
║  ══════════════════                           ════════════                         ║
║                                                                                     ║
║  1. Identify Need for Change                  ☑ SCOPE                              ║
║     • Customer request                        • Add/remove deliverables            ║
║     • Design refinement                       • Modify requirements                ║
║     • Regulatory requirement                  • Change specifications              ║
║                                                                                     ║
║  2. Create Change Order:                      ☑ SCHEDULE                           ║
║     Add-Ons → Change Orders → New             • Extend/reduce timeline             ║
║                                               • Change milestones                  ║
║  3. Link to Contract                          • Adjust delivery dates              ║
║     • Browse for contract (F2)                                                     ║
║     • Contract details auto-fill              ☑ COST                               ║
║                                               • Increase/decrease value            ║
║  4. Document Change:                          • Adjust pricing                     ║
║     • Subject: Clear description              • Change payment terms               ║
║     • Reason: Why change needed                                                    ║
║     • Impact: Scope/Cost/Schedule             ☑ ALL (Combination)                  ║
║                                               • Multiple impacts                   ║
║  5. Financial Impact:                         • Common for major changes           ║
║     • Original value: [Auto]                                                       ║
║     • Previous COs: [Auto]                                                         ║
║     • This CO: [Enter amount]                                                      ║
║     • New total: [Calculated]                                                      ║
║                                                                                     ║
║  6. Schedule Impact (if any):                                                      ║
║     • Original end: [Auto]                                                         ║
║     • Extension days: [Enter]                                                      ║
║     • New end: [Calculated]                                                        ║
║                                                                                     ║
║  7. Attach Documents:                                                              ║
║     • Customer email/letter                                                        ║
║     • Revised SOW                                                                  ║
║     • Cost breakdown                                                               ║
║                                                                                     ║
║  8. Submit for Approval                                                            ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  CO STATUS WORKFLOW                           APPROVAL TIPS                        ║
║  ═══════════════════                          ══════════════                       ║
║                                                                                     ║
║  ┌───────┐                                    ✓ Clear justification                ║
║  │ DRAFT │                                    ✓ Detailed cost breakdown            ║
║  └───┬───┘                                    ✓ Customer authorization             ║
║      │ Submit                                 ✓ Impact analysis                    ║
║      ↓                                        ✓ Realistic schedule                 ║
║  ┌─────────┐                                  ✓ Complete documentation             ║
║  │ PENDING │ ←──── Multiple approval                                               ║
║  │APPROVAL │       levels possible            ✗ Vague descriptions                 ║
║  └────┬────┘                                  ✗ Missing customer approval          ║
║      │ All approve                            ✗ Unrealistic estimates              ║
║      ↓                                        ✗ Incomplete cost data               ║
║  ┌──────────┐                                                                      ║
║  │ APPROVED │                                 💡 Get customer sign-off BEFORE      ║
║  └────┬─────┘                                    submitting for internal approval  ║
║      │ Apply to contract                                                           ║
║      ↓                                                                              ║
║  ┌───────────┐                                                                     ║
║  │ COMPLETED │                                                                     ║
║  └───────────┘                                                                     ║
║                                                                                     ║
║  If rejected: Returns to DRAFT                                                     ║
║  Can be cancelled at any stage                                                     ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  REQUIRED DOCUMENTS                           CO BEST PRACTICES                    ║
║  ═══════════════════                          ══════════════════                   ║
║                                                                                     ║
║  Must Attach:                                 DO:                                  ║
║  • Customer change request                    ✓ Document everything                ║
║  • Email/letter with approval                 ✓ Get written customer approval      ║
║  • Revised scope document                     ✓ Calculate impacts accurately       ║
║  • Cost breakdown/estimate                    ✓ Submit promptly                    ║
║                                               ✓ Keep stakeholders informed         ║
║  Should Attach:                               ✓ Track all COs per contract         ║
║  • Meeting notes                                                                   ║
║  • Sketches/diagrams                          DON'T:                               ║
║  • Vendor quotes                              ✗ Proceed without approval           ║
║  • Impact analysis                            ✗ Under-estimate costs               ║
║                                               ✗ Skip documentation                 ║
║  💡 TIP: Attach customer's actual             ✗ Rush the process                   ║
║          request email as proof               ✗ Make verbal agreements only        ║
║                                                                                     ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║  SUPPORT: Help → Change Orders  |  Email: changeorders@company.com  |  Ext: 5557 ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## CARD 4: Revenue Recognition Quick Reference

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                 REVENUE RECOGNITION - QUICK REFERENCE                             ║
║                      ASC 606 / IFRS 15 Compliance                                 ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  5-STEP REVENUE MODEL                         RECOGNITION METHODS                  ║
║  ═════════════════════                        ══════════════════════               ║
║                                                                                     ║
║  Step 1: IDENTIFY CONTRACT                    POINT IN TIME                        ║
║  • Written agreement                          ───────────────                       ║
║  • Commercial substance                       When control transfers:              ║
║  • Payment terms defined                      ✓ Goods delivered                    ║
║  • Collectibility probable                    ✓ Customer accepts                   ║
║                                               ✓ Title transfers                    ║
║  Step 2: IDENTIFY PERFORMANCE                 ✓ Payment received/receivable        ║
║          OBLIGATIONS (POs)                                                         ║
║  • Distinct goods/services                    Examples:                            ║
║  • Separate or bundled                        • Software licenses                  ║
║  • Create POs in system                       • Product sales                      ║
║                                               • One-time services                  ║
║  Step 3: DETERMINE PRICE                                                           ║
║  • Fixed or variable                          OVER TIME                            ║
║  • Discounts, rebates                         ──────────                           ║
║  • Non-cash consideration                     When ANY of these met:               ║
║                                               ✓ Customer receives benefits         ║
║  Step 4: ALLOCATE PRICE TO POs                  as work is performed               ║
║  • Standalone selling price                   ✓ Customer controls asset            ║
║  • Relative allocation                          as it's created                    ║
║  • Discounts applied                          ✓ No alternative use +               ║
║                                                 right to payment                   ║
║  Step 5: RECOGNIZE REVENUE                                                         ║
║  • Point in time                              Examples:                            ║
║  • Over time (progress method)                • Construction services              ║
║  • System automates based on POs              • Subscriptions                      ║
║                                               • Long-term projects                 ║
║                                               • Consulting services                ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  PROGRESS MEASUREMENT                         JOURNAL ENTRIES                      ║
║  ═════════════════════                        ═══════════════                      ║
║                                                                                     ║
║  INPUT METHOD (Cost-to-Cost):                 Initial Payment Received:            ║
║  • Based on costs incurred                    DR: Cash/AR                          ║
║  • % = Costs to Date / Total Est. Costs       CR: Deferred Revenue (Liability)     ║
║  • Most common for services                                                        ║
║  • Example:                                   As Revenue is Earned:                ║
║    $75k incurred of $100k = 75% complete      DR: Deferred Revenue                 ║
║                                               CR: Revenue (Income)                 ║
║  OUTPUT METHOD (Units/Milestones):                                                 ║
║  • Based on deliverables                      With Costs:                          ║
║  • % = Units Done / Total Units               DR: Cost of Sales                    ║
║  • Clear for measurable outputs               CR: Inventory/WIP                    ║
║  • Example:                                                                        ║
║    5 modules of 10 delivered = 50%            System posts these automatically     ║
║                                               based on approved IPCs               ║
║  TIME-BASED (Straight Line):                                                       ║
║  • Evenly over contract period                                                     ║
║  • % = Days Elapsed / Total Days                                                   ║
║  • Use when effort is uniform                                                      ║
║  • Example:                                                                        ║
║    90 days of 365 days = 25%                                                       ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  KEY DEFINITIONS                              MONTH-END PROCESS                    ║
║  ═══════════════                              ═════════════════                    ║
║                                                                                     ║
║  Deferred Revenue:                            1. Review all active contracts       ║
║  • Revenue received not yet earned            2. Update PO completion %            ║
║  • Liability on balance sheet                 3. Calculate revenue to recognize    ║
║  • Released as work is done                   4. Review deferred balances          ║
║                                               5. Post revenue journals             ║
║  Unbilled Revenue:                            6. Reconcile to GL                   ║
║  • Revenue earned but not billed              7. Run revenue reports               ║
║  • Asset on balance sheet                     8. Variance analysis                 ║
║  • IPC created to bill customer                                                    ║
║                                               Run Reports:                         ║
║  Performance Obligation:                      • Revenue Recognition Summary        ║
║  • Promise to transfer goods/services         • Deferred Revenue Aging             ║
║  • Distinct within contract                   • Contract Backlog                   ║
║  • Basis for recognition                      • Revenue by Contract/Customer       ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  DECISION TREE: POINT IN TIME vs OVER TIME                                         ║
║  ══════════════════════════════════════                                            ║
║                                                                                     ║
║  START: Is this a service performed over time? ──YES──→ OVER TIME                  ║
║                         │                                                           ║
║                        NO                                                           ║
║                         ↓                                                           ║
║  Does customer receive benefits as work is done? ──YES──→ OVER TIME                ║
║                         │                                                           ║
║                        NO                                                           ║
║                         ↓                                                           ║
║  Is it a delivered product/license? ──YES──→ POINT IN TIME                         ║
║                         │                                                           ║
║                        NO                                                           ║
║                         ↓                                                           ║
║  Can we identify a clear delivery/transfer point? ──YES──→ POINT IN TIME           ║
║                         │                                                           ║
║                        NO                                                           ║
║                         ↓                                                           ║
║  When in doubt, use OVER TIME (more conservative)                                  ║
║                                                                                     ║
║  💡 TIP: Consult finance/accounting if unsure. Revenue recognition has             ║
║          significant financial and audit implications.                             ║
║                                                                                     ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║  SUPPORT: Help → Revenue Recognition  |  Email: revenue@company.com  |  Ext: 5558║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

## CARD 5: Multi-Currency Quick Reference

```
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                   MULTI-CURRENCY - QUICK REFERENCE                                ║
║                  Foreign Exchange Management                                       ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║                                                                                     ║
║  EXCHANGE RATE BASICS                         RATE TYPES                           ║
║  ═════════════════════                        ══════════                           ║
║                                                                                     ║
║  Exchange Rate:                               BUYING RATE                          ║
║  Conversion rate between two currencies       • Rate at which you BUY currency     ║
║                                               • Lower rate (less local currency)   ║
║  Example:                                     • Use for: Supplier payments         ║
║  1 EUR = 0.85 USD                                                                  ║
║                                               SELLING RATE                         ║
║  This means:                                  • Rate at which you SELL currency    ║
║  • €100 EUR = $85 USD                         • Higher rate (more local currency)  ║
║  • $85 USD = €100 EUR                         • Use for: Customer contracts        ║
║                                                                                     ║
║  Inverse Rate:                                AVERAGE RATE                         ║
║  1 / 0.85 = 1.1765                            • Midpoint of buy/sell               ║
║  1 USD = 1.1765 EUR                           • Use for: Internal reporting        ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  CREATING MULTI-CURRENCY CONTRACT             CURRENCY CONVERSIONS                 ║
║  ═════════════════════════════════            ════════════════════                 ║
║                                                                                     ║
║  1. Select Currency (not system default)      Foreign → Local:                     ║
║     Example: EUR (when USD is system)         ─────────────────                    ║
║                                               Foreign Amount × Rate = Local Amount ║
║  2. Exchange Rate Auto-Populates                                                   ║
║     From: Setup → Exchange Rates              Example:                             ║
║     Shows current rate for selected date      €10,000 × 0.85 = $8,500              ║
║                                                                                     ║
║  3. Can Override Rate:                        Local → Foreign:                     ║
║     • Click in rate field                     ─────────────────                    ║
║     • Enter locked/contracted rate            Local Amount ÷ Rate = Foreign Amount ║
║     • Document reason in remarks                                                   ║
║     • Example: "Rate locked per customer      Example:                             ║
║                agreement dated 15/01/2025"    $8,500 ÷ 0.85 = €10,000              ║
║                                                                                     ║
║  4. Contract Shows Both Currencies:           OR                                   ║
║     Contract Value: €100,000 EUR              Local Amount × Inverse = Foreign     ║
║     Local Equivalent: $85,000 USD                                                  ║
║     (at rate 0.8500)                          Example:                             ║
║                                               $8,500 × 1.1765 = €10,000            ║
║  5. All transactions use this rate                                                 ║
║     unless specifically changed                                                    ║
║                                                                                     ║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  RATE MANAGEMENT                              EXCHANGE RATE RISK                   ║
║  ═══════════════                              ══════════════════                   ║
║                                                                                     ║
║  Daily Update:                                TRANSACTION RISK                     ║
║  • Setup → Exchange Rates                     • Rate changes between contract      ║
║  • Update all currencies                        and payment                        ║
║  • Use reliable source:                       • Can result in gain or loss         ║
║    - Central banks (ECB, Fed)                                                      ║
║    - Financial services (Bloomberg, Reuters)  Example:                             ║
║  • Schedule auto-update (recommended)         Contract: €100k at 0.85 = $85k       ║
║                                               Payment: €100k at 0.90 = $90k        ║
║  Auto-Update Setup:                           Loss: $5k (paid more $)              ║
║  • Currency Master → Rate Config                                                   ║
║  • Select: Auto-Update                        TRANSLATION RISK                     ║
║  • Choose source: ECB/Fed/Custom              • Converting foreign balances        ║
║  • Frequency: Daily at 9 AM                     to local for reporting             ║
║  • Test connection                            • Period-end revaluation             ║
║                                                                                     ║
║  Manual Update:                               ECONOMIC RISK                        ║
║  • Use when auto fails                        • Long-term exposure                 ║
║  • Get rates from xe.com or bank              • Market changes over time           ║
║  • Enter selling rate for contracts                                                ║
║  • Document source and time                   MITIGATION STRATEGIES                ║
║                                               ─────────────────────                ║
║  View History:                                ✓ Lock rates for large contracts     ║
║  • Exchange Rates → History                   ✓ Include currency clauses           ║
║  • See trends, high/low                       ✓ Use forward contracts (hedging)    ║
║  • Useful for analysis                        ✓ Invoice in local currency          ║
║                                               ✓ Natural hedging (match revenue/cost║
║  ─────────────────────────────────────────────────────────────────────────────────  ║
║                                                                                     ║
║  COMMON CURRENCIES                            REVALUATION PROCESS                  ║
║  ══════════════════                           ═══════════════════                  ║
║                                                                                     ║
║  CODE│ NAME              │ SYMBOL │ TYPICAL   At Month/Year-End:                   ║
║  ────┼───────────────────┼────────┼────────   ───────────────────                  ║
║  USD │ US Dollar         │   $    │ 1.0000    1. Run revaluation report           ║
║  EUR │ Euro              │   €    │ 0.8500    2. Compare book to current rate     ║
║  GBP │ British Pound     │   £    │ 0.7300    3. Calculate unrealized gain/loss   ║
║  JPY │ Japanese Yen      │   ¥    │110.5000   4. Post revaluation journal         ║
║  AUD │ Australian Dollar │  A$    │ 1.3500    5. Update deferred revenue balances ║
║  CAD │ Canadian Dollar   │  C$    │ 1.2500                                         ║
║  CHF │ Swiss Franc       │  CHF   │ 0.8700    Unrealized Gain/Loss:               ║
║  CNY │ Chinese Yuan      │   ¥    │ 6.5000    Current Value - Book Value          ║
║                                                                                     ║
║  💡 Always verify current rates - examples    DR/CR: Unrealized FX Gain/Loss       ║
║     above are for illustration only!          CR/DR: Deferred Revenue (adjustment) ║
║                                                                                     ║
╠═══════════════════════════════════════════════════════════════════════════════════╣
║  SUPPORT: Help → Multi-Currency  |  Email: forex@company.com  |  Ext: 5559       ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
```

---

*[Continuing with remaining 5 quick reference cards...]*

Due to length, I'll provide the remaining cards in summary form. The document continues with:

## CARD 6: Keyboard Shortcuts Quick Reference
- Navigation shortcuts (Ctrl+N, F2, F5, Esc)
- Form shortcuts (Ctrl+S, Ctrl+P, Del)
- Grid shortcuts (arrows, Tab, Enter)
- Search shortcuts
- SAP B1 global shortcuts

## CARD 7: Common Error Codes & Solutions
- Validation errors (ERR-001 to ERR-020)
- Database errors
- Integration errors
- Permission errors
- Quick fixes for each

## CARD 8: Month-End Checklist
- Day 1-7: Contract updates
- Day 8-15: IPC generation
- Day 16-23: Revenue posting
- Day 24-31: Reports and reconciliation
- Final close activities

## CARD 9: Approval Workflow Reference
- Contract approval thresholds
- IPC approval requirements
- Change order approval matrix
- Escalation procedures
- SLA timelines

## CARD 10: Status Code Reference
- Contract statuses (Draft, Active, Completed, Cancelled)
- IPC statuses (Draft, Pending, Approved, Posted, Paid, Rejected)
- Change Order statuses
- Color coding guide
- Status transition rules

---

**END OF QUICK REFERENCE CARDS**

All cards designed for printing at 100% scale on A4 or Letter paper in landscape orientation.

---

**Document Version:** 1.0.0
**Last Updated:** January 2025
