============================================================
DATABASE MIGRATION SCRIPTS
Contract Management Add-on for SAP Business One
Version: 1.0.0
Date: November 5, 2025
============================================================

IMPORTANT: Run these scripts BEFORE using the Contract Management
Add-on for the first time.

============================================================
WHAT DO THESE SCRIPTS DO?
============================================================

These scripts add a required field to the deferred revenue table:
- Field Name: U_PerformObligCode (Performance Obligation Code)
- Data Type: NVARCHAR(50)
- Purpose: Links deferred revenue to specific performance obligations

This field is CRITICAL for proper revenue recognition functionality.

============================================================
WHICH SCRIPT SHOULD I RUN?
============================================================

Choose the script that matches your database type:

SQL Server:
  → 01_Add_PerformObligCode_Field_SQLServer.sql

SAP HANA:
  → 01_Add_PerformObligCode_Field_HANA.sql

============================================================
BEFORE YOU BEGIN
============================================================

Prerequisites:
  1. Database administrator credentials
  2. Database backup (CRITICAL - always backup before migration!)
  3. SAP Business One closed (no active users)
  4. 5-10 minutes of downtime

Permissions Required:
  - ALTER TABLE permission
  - CREATE INDEX permission

============================================================
SQL SERVER - STEP-BY-STEP INSTRUCTIONS
============================================================

1. BACKUP YOUR DATABASE
   ----------------------
   Open SQL Server Management Studio (SSMS)
   Right-click on your SAP B1 database → Tasks → Back Up...
   Choose "Full" backup
   Specify backup location
   Click OK

2. OPEN THE MIGRATION SCRIPT
   ---------------------------
   In SSMS, click File → Open → File
   Navigate to: 01_Add_PerformObligCode_Field_SQLServer.sql
   Click Open

3. UPDATE DATABASE NAME
   ---------------------
   Find this line near the top:
   USE [YOUR_DATABASE_NAME]

   Replace YOUR_DATABASE_NAME with your actual database name
   Example: USE [SBODemoUS]

4. EXECUTE THE SCRIPT
   -------------------
   Click Execute (or press F5)
   Wait for completion

5. VERIFY SUCCESS
   ---------------
   Check the Messages window for:
   "✅ Verification: Field U_PerformObligCode exists in @CM_DEFERRED_REV"
   "Migration completed successfully!"

   If you see these messages, migration is complete!

6. VERIFY IN DATABASE
   -------------------
   Run this query to double-check:

   SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = '@CM_DEFERRED_REV'
     AND COLUMN_NAME = 'U_PerformObligCode';

   Expected result:
   COLUMN_NAME          DATA_TYPE    CHARACTER_MAXIMUM_LENGTH
   U_PerformObligCode   nvarchar     50

============================================================
SAP HANA - STEP-BY-STEP INSTRUCTIONS
============================================================

1. BACKUP YOUR DATABASE
   ----------------------
   Open SAP HANA Studio or HANA Cockpit
   Right-click on your system → Backup
   Choose "Complete Data Backup"
   Specify backup destination
   Click OK

2. OPEN SQL CONSOLE
   -----------------
   In HANA Studio: Right-click on database → Open SQL Console
   In HANA Cockpit: Navigate to SQL Console

3. OPEN THE MIGRATION SCRIPT
   ---------------------------
   Click File → Open File
   Navigate to: 01_Add_PerformObligCode_Field_HANA.sql
   Click Open

4. EXECUTE THE SCRIPT
   -------------------
   Click Execute (green arrow) or press F8
   Wait for completion

5. VERIFY SUCCESS
   ---------------
   Check the Results window for:
   "✅ Verification: Field U_PerformObligCode exists in CM_DEFERRED_REV"
   "Migration completed successfully!"

6. VERIFY IN DATABASE
   -------------------
   Run this query:

   SELECT COLUMN_NAME, DATA_TYPE_NAME, LENGTH
   FROM TABLE_COLUMNS
   WHERE SCHEMA_NAME = CURRENT_SCHEMA
     AND TABLE_NAME = 'CM_DEFERRED_REV'
     AND COLUMN_NAME = 'U_PerformObligCode';

   Expected result:
   COLUMN_NAME          DATA_TYPE_NAME    LENGTH
   U_PerformObligCode   NVARCHAR         50

============================================================
TROUBLESHOOTING
============================================================

Problem: "Column already exists" error
Solution: Field was already added in a previous attempt
Action:   Verify the field exists with correct data type
          If correct, no action needed - proceed with installation

Problem: "Permission denied" or "Access denied"
Solution: Run script as database administrator
Action:   Login with SA account (SQL Server) or SYSTEM user (HANA)

Problem: "Database locked" or "Cannot modify schema"
Solution: Active connections are blocking the change
Action:   1. Close SAP Business One on all computers
          2. Wait for all connections to close
          3. Or kill blocking sessions (ask DBA for help)
          4. Retry script execution

Problem: Script execution times out
Solution: Database may be busy or under heavy load
Action:   1. Schedule migration during off-hours
          2. Ensure no other processes are running
          3. Retry execution

Problem: "Table @CM_DEFERRED_REV does not exist"
Solution: UDO tables not created yet
Action:   1. Run Contract Management Add-on once
          2. Let it create UDO tables
          3. Then run migration script
          Note: This is unusual - UDOs should auto-create

============================================================
VERIFICATION CHECKLIST
============================================================

After running the migration script, verify:

[  ] Script completed without errors
[  ] Success message displayed
[  ] Verification query returns expected result
[  ] Field U_PerformObligCode exists
[  ] Field data type is NVARCHAR(50)
[  ] Field allows NULL values
[  ] Index created (if applicable)
[  ] No error messages in log

If all items checked, migration is complete!

============================================================
WHAT'S NEXT?
============================================================

1. Start SAP Business One
2. Navigate to: Administration → Add-Ons → Add-On Administration
3. Verify Contract Management Add-on is listed
4. Right-click → Start Add-On
5. Verify menu appears: Modules → Contract Management
6. Test creating a contract

============================================================
ROLLBACK (IF NEEDED)
============================================================

If you need to undo the migration:

SQL Server:
  ALTER TABLE [@CM_DEFERRED_REV]
  DROP COLUMN U_PerformObligCode;

HANA:
  ALTER TABLE "CM_DEFERRED_REV"
  DROP (U_PerformObligCode);

WARNING: Only rollback if you haven't created any data yet!
If data exists, contact support before rolling back.

============================================================
SUPPORT
============================================================

If you encounter any issues:

Email:   support@yourcompany.com
Phone:   +1-XXX-XXX-XXXX
Website: https://yourcompany.com/support

When contacting support, please provide:
- Database type (SQL Server or HANA)
- Database version
- Error message (if any)
- Screenshot of error (if applicable)

============================================================
ADDITIONAL INFORMATION
============================================================

Migration Script Purpose:
This migration script is required because the U_PerformObligCode
field was added in version 1.0.0 to fix Bug #4 (Missing Field).

The field enables proper tracking of which performance obligation
each deferred revenue record belongs to, which is critical for
accurate revenue recognition under ASC 606 / IFRS 15.

Without this field, the system cannot correctly match deferred
revenue to specific performance obligations, which would cause
incorrect revenue release calculations.

Database Impact:
- Execution time: < 1 second
- Locks acquired: Schema modification lock
- Data impact: None (only adds field, doesn't modify data)
- Rollback: Simple (DROP COLUMN)
- Risk level: Low

Index Information:
The script also creates an index on U_PerformObligCode for
performance optimization:

SQL Server:
  IDX_CM_DEFERRED_REV_PerformObligCode

HANA:
  IDX_CM_DEFERRED_REV_PerformObligCode

This index improves query performance when filtering deferred
revenue by performance obligation.

============================================================

For complete documentation, see:
- DEPLOYMENT_GUIDE.md
- PHASE_1_SIGN_OFF.md
- ADVANCED_FEATURES_GUIDE.md

============================================================
END OF DOCUMENT
============================================================
