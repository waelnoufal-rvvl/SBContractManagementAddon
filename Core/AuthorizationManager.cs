using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Core
{
    /// <summary>
    /// Manages user authorization and permissions based on SAP B1 user permissions
    /// </summary>
    public class AuthorizationManager
    {
        private readonly Company _company;
        private readonly DatabaseQueryHelper _dbHelper;
        private int _currentUserCode;
        private Dictionary<string, bool> _permissionCache;

        // Permission IDs for Contract Management
        public const string PERM_CONTRACT_VIEW = "CM_CONTRACT_VIEW";
        public const string PERM_CONTRACT_CREATE = "CM_CONTRACT_CREATE";
        public const string PERM_CONTRACT_UPDATE = "CM_CONTRACT_UPDATE";
        public const string PERM_CONTRACT_DELETE = "CM_CONTRACT_DELETE";
        public const string PERM_CONTRACT_APPROVE = "CM_CONTRACT_APPROVE";

        public const string PERM_IPC_VIEW = "CM_IPC_VIEW";
        public const string PERM_IPC_CREATE = "CM_IPC_CREATE";
        public const string PERM_IPC_UPDATE = "CM_IPC_UPDATE";
        public const string PERM_IPC_DELETE = "CM_IPC_DELETE";
        public const string PERM_IPC_SUBMIT = "CM_IPC_SUBMIT";
        public const string PERM_IPC_APPROVE = "CM_IPC_APPROVE";
        public const string PERM_IPC_REJECT = "CM_IPC_REJECT";

        public const string PERM_CO_VIEW = "CM_CO_VIEW";
        public const string PERM_CO_CREATE = "CM_CO_CREATE";
        public const string PERM_CO_UPDATE = "CM_CO_UPDATE";
        public const string PERM_CO_DELETE = "CM_CO_DELETE";
        public const string PERM_CO_APPROVE = "CM_CO_APPROVE";
        public const string PERM_CO_REJECT = "CM_CO_REJECT";

        public const string PERM_REPORT_VIEW = "CM_REPORT_VIEW";
        public const string PERM_REPORT_EXPORT = "CM_REPORT_EXPORT";

        public const string PERM_ADMIN = "CM_ADMIN";

        public AuthorizationManager(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _dbHelper = new DatabaseQueryHelper(company);
            _currentUserCode = _company.UserSignature;
            _permissionCache = new Dictionary<string, bool>();

            Logger.Info($"AuthorizationManager initialized for user: {_currentUserCode}");
        }

        /// <summary>
        /// Get current logged in user code
        /// </summary>
        public string GetCurrentUserCode()
        {
            return _currentUserCode.ToString();
        }

        /// <summary>
        /// Get current user name
        /// </summary>
        public string GetCurrentUserName()
        {
            try
            {
                SAPbobsCOM.Users oUser = (SAPbobsCOM.Users)_company.GetBusinessObject(BoObjectTypes.oUsers);
                if (oUser.GetByKey(_currentUserCode))
                {
                    return oUser.UserName;
                }
                return _company.UserName;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting user name: {ex.Message}", ex);
                return _company.UserName;
            }
        }

        /// <summary>
        /// Check if current user has specific permission
        /// </summary>
        public bool HasPermission(string permissionId)
        {
            try
            {
                // Check cache first
                if (_permissionCache.ContainsKey(permissionId))
                {
                    return _permissionCache[permissionId];
                }

                // Superuser or admin always has permission
                if (IsSuperUser() || IsUserInRole("Admin"))
                {
                    _permissionCache[permissionId] = true;
                    return true;
                }

                // Check admin permission
                if (permissionId == PERM_ADMIN && IsUserInRole("ContractAdmin"))
                {
                    _permissionCache[permissionId] = true;
                    return true;
                }

                // Check user authorization in custom user table or use SAP authorizations
                bool hasPermission = CheckCustomPermission(permissionId);

                _permissionCache[permissionId] = hasPermission;
                return hasPermission;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking permission {permissionId}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if user is super user
        /// </summary>
        public bool IsSuperUser()
        {
            try
            {
                SAPbobsCOM.Users oUser = (SAPbobsCOM.Users)_company.GetBusinessObject(BoObjectTypes.oUsers);
                if (oUser.GetByKey(_currentUserCode))
                {
                    return oUser.Superuser == BoYesNoEnum.tYES;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking superuser status: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if user belongs to a specific role/group
        /// SQL injection protected with safe query building
        /// </summary>
        public bool IsUserInRole(string roleName)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(*) as CNT
                    FROM OUSR U
                    INNER JOIN USR1 UG ON U.USERID = UG.USERID
                    INNER JOIN OUGE G ON UG.GROUPID = G.ID
                    WHERE U.USER_CODE = {_dbHelper.QuoteString(_currentUserCode.ToString())}
                    AND G.NAME = {_dbHelper.QuoteString(roleName)}";

                int count = _dbHelper.ExecuteCount(query);
                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking user role {roleName}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check custom permission from user-defined table
        /// HANA & SQL Server compatible with SQL injection protection
        /// </summary>
        private bool CheckCustomPermission(string permissionId)
        {
            try
            {
                // Check if user has permission in custom authorization table
                // First check if authorization table exists (HANA & SQL Server compatible)
                string checkTableQuery = _dbHelper.GetTableExistsQuery("@CM_USER_PERM");

                int tableExists = _dbHelper.ExecuteCount(checkTableQuery);

                if (tableExists > 0)
                {
                    // Table exists, check permission with safe query
                    string query = $@"
                        SELECT ""U_HasPermission""
                        FROM ""@CM_USER_PERM""
                        WHERE ""U_UserCode"" = {_dbHelper.QuoteString(_currentUserCode.ToString())}
                        AND ""U_PermissionID"" = {_dbHelper.QuoteString(permissionId)}";

                    Recordset oRecordset = _dbHelper.ExecuteQuery(query);

                    if (!oRecordset.EoF)
                    {
                        string hasPermission = oRecordset.Fields.Item("U_HasPermission").Value?.ToString();
                        return hasPermission == "Y";
                    }
                }

                // Default permissions based on permission type
                return GetDefaultPermission(permissionId);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking custom permission {permissionId}: {ex.Message}", ex);
                return GetDefaultPermission(permissionId);
            }
        }

        /// <summary>
        /// Get default permission when no custom permission is defined
        /// </summary>
        private bool GetDefaultPermission(string permissionId)
        {
            // Default: all users can view, but only certain roles can modify
            if (permissionId.Contains("VIEW"))
            {
                return true; // All users can view
            }
            else if (permissionId.Contains("CREATE") || permissionId.Contains("UPDATE") ||
                     permissionId.Contains("DELETE") || permissionId.Contains("SUBMIT"))
            {
                return IsUserInRole("ContractManager") || IsUserInRole("ProjectManager");
            }
            else if (permissionId.Contains("APPROVE") || permissionId.Contains("REJECT"))
            {
                return IsUserInRole("ContractApprover") || IsUserInRole("FinanceManager");
            }
            else if (permissionId == PERM_ADMIN)
            {
                return false; // Admin permission must be explicitly granted
            }

            return false; // Default deny
        }

        /// <summary>
        /// Check if user can approve based on amount threshold
        /// </summary>
        public bool CanApproveAmount(double amount)
        {
            try
            {
                if (IsSuperUser())
                    return true;

                // Get user's approval limit
                double approvalLimit = GetUserApprovalLimit();

                return amount <= approvalLimit;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking approval amount: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get user's approval limit
        /// SQL injection protected with safe query building
        /// </summary>
        public double GetUserApprovalLimit()
        {
            try
            {
                string query = $@"
                    SELECT ""U_ApprovalLimit""
                    FROM ""@CM_USER_PERM""
                    WHERE ""U_UserCode"" = {_dbHelper.QuoteString(_currentUserCode.ToString())}";

                Recordset oRecordset = _dbHelper.ExecuteQuery(query);

                if (!oRecordset.EoF)
                {
                    object limitValue = oRecordset.Fields.Item("U_ApprovalLimit").Value;
                    return SafeConversion.SafeToDouble(limitValue, 0);
                }

                // Default limits based on role
                if (IsUserInRole("CEO") || IsUserInRole("CFO"))
                    return double.MaxValue;
                else if (IsUserInRole("FinanceManager"))
                    return 1000000;
                else if (IsUserInRole("ContractManager"))
                    return 500000;
                else if (IsUserInRole("ProjectManager"))
                    return 100000;

                return 0; // No approval authority
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approval limit: {ex.Message}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Check if user can modify document based on status
        /// </summary>
        public bool CanModifyDocument(string status, string createdBy)
        {
            // Admins can always modify
            if (HasPermission(PERM_ADMIN))
                return true;

            // User can modify their own drafts
            if (status == "Draft" && createdBy == _currentUserCode.ToString())
                return true;

            // Submitted/Approved documents cannot be modified by regular users
            if (status == "Submitted" || status == "Approved")
                return false;

            return true;
        }

        /// <summary>
        /// Get list of users who can approve for a given amount
        /// SQL injection protected with safe query building
        /// </summary>
        public List<string> GetApproversForAmount(double amount)
        {
            List<string> approvers = new List<string>();

            try
            {
                string query = $@"
                    SELECT DISTINCT U.USER_CODE, U.U_NAME
                    FROM OUSR U
                    LEFT JOIN ""@CM_USER_PERM"" P ON U.USER_CODE = P.""U_UserCode""
                    WHERE (U.SUPERUSER = 'Y' OR P.""U_ApprovalLimit"" >= {_dbHelper.SanitizeDouble(amount)})
                    AND U.LOCKED = 'N'
                    ORDER BY U.U_NAME";

                Recordset oRecordset = _dbHelper.ExecuteQuery(query);

                while (!oRecordset.EoF)
                {
                    string userCode = SafeConversion.SafeToString(oRecordset.Fields.Item("USER_CODE").Value);
                    if (!string.IsNullOrEmpty(userCode))
                    {
                        approvers.Add(userCode);
                    }
                    oRecordset.MoveNext();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approvers: {ex.Message}", ex);
            }

            return approvers;
        }

        /// <summary>
        /// Clear permission cache
        /// </summary>
        public void ClearCache()
        {
            _permissionCache.Clear();
            Logger.Debug("Permission cache cleared");
        }

        /// <summary>
        /// Log authorization attempt
        /// </summary>
        public void LogAuthorizationAttempt(string action, string resourceType, string resourceId, bool granted)
        {
            try
            {
                string message = $"User {_currentUserCode} attempted {action} on {resourceType} [{resourceId}] - {(granted ? "GRANTED" : "DENIED")}";

                if (granted)
                {
                    Logger.Debug(message);
                }
                else
                {
                    Logger.Warning(message);
                }

                // Optionally write to audit table
                WriteAuditLog(action, resourceType, resourceId, granted);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error logging authorization attempt: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Write to audit log table
        /// SQL injection protected with safe query building
        /// </summary>
        private void WriteAuditLog(string action, string resourceType, string resourceId, bool granted)
        {
            try
            {
                Dictionary<string, object> auditValues = new Dictionary<string, object>
                {
                    { "Code", Guid.NewGuid().ToString() },
                    { "U_UserCode", _currentUserCode.ToString() },
                    { "U_Action", action },
                    { "U_ResourceType", resourceType },
                    { "U_ResourceID", resourceId },
                    { "U_Granted", granted ? "Y" : "N" },
                    { "U_LogDate", DateTime.Now }
                };

                string query = _dbHelper.BuildInsertQuery("@CM_AUDIT_LOG", auditValues);
                Recordset oRecordset = _dbHelper.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                // Don't throw - audit logging should not break functionality
                Logger.Debug($"Could not write to audit log: {ex.Message}");
            }
        }
    }
}
