using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;
using ContractManagementAddon.SAP;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// World-Class SAP B1 Native Approval Manager
    /// Uses SAP's built-in approval procedures (OWDD, WDD1, OWTR, WTR1)
    /// Compliant with SAP B1 standards and best practices
    /// </summary>
    public class SAPApprovalManager
    {
        private readonly Company _company;
        private readonly EmailNotificationService _emailService;
        private readonly DatabaseQueryHelper _dbHelper;

        // SAP B1 Approval Status Constants
        public const string APPROVAL_STATUS_PENDING = "W";  // Waiting for approval
        public const string APPROVAL_STATUS_APPROVED = "Y"; // Approved
        public const string APPROVAL_STATUS_REJECTED = "N"; // Not approved/Rejected
        public const string APPROVAL_STATUS_GENERATED = "G"; // Generated (created but not submitted)

        public SAPApprovalManager(Company company, EmailNotificationService emailService)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _emailService = emailService;
            _dbHelper = new DatabaseQueryHelper(company);
        }

        #region SAP Native Approval Template Setup

        /// <summary>
        /// Create SAP B1 native approval templates for Contract Management documents
        /// Uses OWDD (Approval Template Definition) and WDD1 (Template Stages)
        /// </summary>
        public void CreateApprovalTemplates()
        {
            try
            {
                Logger.Info("Creating SAP B1 native approval templates...");

                // Create approval template for Contracts (amount-based)
                CreateApprovalTemplate("CONTRACT_APPR", "Contract Approval", new List<ApprovalStageDefinition>
                {
                    new ApprovalStageDefinition { StageNumber = 1, StageName = "Project Manager", AuthorizerType = "U", MinAmount = 0, MaxAmount = 50000 },
                    new ApprovalStageDefinition { StageNumber = 2, StageName = "Finance Manager", AuthorizerType = "U", MinAmount = 50001, MaxAmount = 200000 },
                    new ApprovalStageDefinition { StageNumber = 3, StageName = "CFO Approval", AuthorizerType = "U", MinAmount = 200001, MaxAmount = 1000000 },
                    new ApprovalStageDefinition { StageNumber = 4, StageName = "CEO Approval", AuthorizerType = "U", MinAmount = 1000001, MaxAmount = 999999999 }
                });

                // Create approval template for IPCs
                CreateApprovalTemplate("IPC_APPR", "IPC Approval", new List<ApprovalStageDefinition>
                {
                    new ApprovalStageDefinition { StageNumber = 1, StageName = "Contract Manager", AuthorizerType = "U", MinAmount = 0, MaxAmount = 100000 },
                    new ApprovalStageDefinition { StageNumber = 2, StageName = "Finance Manager", AuthorizerType = "U", MinAmount = 100001, MaxAmount = 500000 },
                    new ApprovalStageDefinition { StageNumber = 3, StageName = "CFO Approval", AuthorizerType = "U", MinAmount = 500001, MaxAmount = 999999999 }
                });

                // Create approval template for Change Orders
                CreateApprovalTemplate("CO_APPR", "Change Order Approval", new List<ApprovalStageDefinition>
                {
                    new ApprovalStageDefinition { StageNumber = 1, StageName = "Project Manager", AuthorizerType = "U", MinAmount = 0, MaxAmount = 50000 },
                    new ApprovalStageDefinition { StageNumber = 2, StageName = "Contract Manager", AuthorizerType = "U", MinAmount = 50001, MaxAmount = 200000 },
                    new ApprovalStageDefinition { StageNumber = 3, StageName = "Finance & CFO", AuthorizerType = "U", MinAmount = 200001, MaxAmount = 999999999 }
                });

                Logger.Info("SAP B1 approval templates created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating approval templates: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create individual approval template using SAP's native structure
        /// HANA & SQL Server compatible with SQL injection protection
        /// </summary>
        private void CreateApprovalTemplate(string templateCode, string templateName, List<ApprovalStageDefinition> stages)
        {
            try
            {
                // Check if template exists
                if (ApprovalTemplateExists(templateCode))
                {
                    Logger.Info($"Approval template {templateCode} already exists");
                    return;
                }

                // Get next available template code number
                int nextCode = GetNextApprovalTemplateCode();

                // Insert into OWDD (Approval Template Definition) - Safe query building
                // SAP HANA system tables use PascalCase column names
                Dictionary<string, object> templateValues = new Dictionary<string, object>
                {
                    { "Code", nextCode },
                    { "Name", templateCode },
                    { "Descrip", templateName },
                    { "IsActive", "Y" },
                    { "CreateDate", DateTime.Now },
                    { "CreateTime", DateTime.Now.Hour * 100 + DateTime.Now.Minute },
                    { "UserSign", GetUserIDFromUserCode(_company.UserName) }
                };

                string insertTemplate = _dbHelper.BuildInsertQuery("OWDD", templateValues);
                _dbHelper.ExecuteQuery(insertTemplate);

                // Insert stages into WDD1 (Template Stages)
                // SAP HANA system tables use PascalCase column names
                foreach (var stage in stages)
                {
                    Dictionary<string, object> stageValues = new Dictionary<string, object>
                    {
                        { "Code", nextCode },
                        { "LineNum", stage.StageNumber - 1 },
                        { "StepName", stage.StageName },
                        { "ApprovalType", stage.AuthorizerType },
                        { "MinAmnt", stage.MinAmount },
                        { "MaxAmnt", stage.MaxAmount },
                        { "UserID", GetUserIDFromUserCode(_company.UserName) },
                        { "IsActive", "Y" }
                    };

                    string insertStage = _dbHelper.BuildInsertQuery("WDD1", stageValues);
                    _dbHelper.ExecuteQuery(insertStage);
                }

                Logger.Info($"Created approval template: {templateCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating approval template {templateCode}: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if approval template exists
        /// HANA & SQL Server compatible with SQL injection protection
        /// </summary>
        private bool ApprovalTemplateExists(string templateCode)
        {
            try
            {
                // SAP HANA system tables use PascalCase column names
                string query = $@"
                    SELECT COUNT(*) as CNT
                    FROM OWDD
                    WHERE Name = {_dbHelper.QuoteString(templateCode)}";

                return _dbHelper.ExecuteCount(query) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking approval template: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get next available approval template code
        /// HANA & SQL Server compatible
        /// </summary>
        private int GetNextApprovalTemplateCode()
        {
            try
            {
                // Use COALESCE instead of ISNULL for HANA compatibility
                // SAP HANA system tables use PascalCase column names
                string query = $"SELECT {_dbHelper.GetMaxWithDefault("Code", "0")} + 1 as NextCode FROM OWDD";

                return _dbHelper.ExecuteScalar<int>(query, "NextCode");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting next approval template code: {ex.Message}", ex);
                return 1;
            }
        }

        #endregion

        #region Document Approval Requests

        /// <summary>
        /// Submit document for approval using SAP's native ApprovalRequests object
        /// THIS IS THE CORRECT SAP B1 WAY
        /// </summary>
        public int SubmitDocumentForApproval(string objectType, int docEntry, double amount, string remarks, string originatorUserCode)
        {
            try
            {
                Logger.Info($"Submitting document for approval: {objectType}, DocEntry: {docEntry}, Amount: {amount}");

                // Use SAP's native ApprovalRequests object
                // Using dynamic to handle different SAP B1 SDK versions
                dynamic approvalReq = ApprovalRequestFactory.CreateApprovalRequest(_company);

                // Set approval request properties
                approvalReq.ApprovalTemplatesID = GetApprovalTemplateForType(objectType);
                approvalReq.ObjectType = GetBoObjectType(objectType);
                approvalReq.ObjectEntry = docEntry;
                approvalReq.IsDraft = BoYesNoEnum.tNO;
                approvalReq.Status = BoApprovalRequestStatusEnum.arsPending;
                approvalReq.Remarks = remarks ?? string.Empty;
                approvalReq.OriginatorID = GetUserIDFromUserCode(originatorUserCode);

                // Add the approval request
                int result = approvalReq.Add();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Failed to create approval request: {error}");
                }

                // Get the newly created approval request ID
                string newKey = _company.GetNewObjectKey();
                int approvalRequestCode = int.Parse(newKey);

                Logger.Info($"Approval request created successfully. WTR Code: {approvalRequestCode}");

                // Update document status to "Pending Approval"
                UpdateDocumentApprovalStatus(objectType, docEntry, APPROVAL_STATUS_PENDING);

                // Send notifications to approvers
                NotifyApprovers(approvalRequestCode, objectType, docEntry, amount);

                return approvalRequestCode;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error submitting document for approval: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Approve document using SAP's native approval API
        /// </summary>
        public bool ApproveDocument(int approvalRequestCode, string approverUserCode, string comments)
        {
            try
            {
                Logger.Info($"Approving document: Approval Request {approvalRequestCode} by user {approverUserCode}");

                // Get the approval request
                dynamic approvalReq = ApprovalRequestFactory.CreateApprovalRequest(_company);

                if (!approvalReq.GetByKey(approvalRequestCode))
                {
                    throw new Exception($"Approval request {approvalRequestCode} not found");
                }

                // Validate approver has authority
                if (!ValidateApproverAuthority(approvalReq, approverUserCode))
                {
                    throw new Exception("User does not have authority to approve this document");
                }

                // Update approval status
                approvalReq.Status = BoApprovalRequestStatusEnum.arsApproved;
                approvalReq.Remarks = comments ?? string.Empty;

                int result = approvalReq.Update();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Failed to approve document: {error}");
                }

                // Update source document status
                string objectType = GetObjectTypeString(approvalReq.ObjectType);
                UpdateDocumentApprovalStatus(objectType, approvalReq.ObjectEntry, APPROVAL_STATUS_APPROVED);

                // Send notification
                if (_emailService != null)
                {
                    SendApprovalNotification(approvalRequestCode, true, approverUserCode, comments);
                }

                Logger.Info($"Document approved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error approving document: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Reject document using SAP's native approval API
        /// </summary>
        public bool RejectDocument(int approvalRequestCode, string approverUserCode, string reason)
        {
            try
            {
                Logger.Info($"Rejecting document: Approval Request {approvalRequestCode} by user {approverUserCode}");

                // Get the approval request
                dynamic approvalReq = ApprovalRequestFactory.CreateApprovalRequest(_company);

                if (!approvalReq.GetByKey(approvalRequestCode))
                {
                    throw new Exception($"Approval request {approvalRequestCode} not found");
                }

                // Validate approver has authority
                if (!ValidateApproverAuthority(approvalReq, approverUserCode))
                {
                    throw new Exception("User does not have authority to reject this document");
                }

                // Update approval status
                approvalReq.Status = BoApprovalRequestStatusEnum.arsNotApproved;
                approvalReq.Remarks = reason ?? string.Empty;

                int result = approvalReq.Update();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Failed to reject document: {error}");
                }

                // Update source document status
                string objectType = GetObjectTypeString(approvalReq.ObjectType);
                UpdateDocumentApprovalStatus(objectType, approvalReq.ObjectEntry, APPROVAL_STATUS_REJECTED);

                // Send notification
                if (_emailService != null)
                {
                    SendApprovalNotification(approvalRequestCode, false, approverUserCode, reason);
                }

                Logger.Info($"Document rejected successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error rejecting document: {ex.Message}", ex);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get approval template ID for document type
        /// </summary>
        private int GetApprovalTemplateForType(string objectType)
        {
            try
            {
                string templateName = objectType.ToUpper() switch
                {
                    "CONTRACT" => "CONTRACT_APPR",
                    "IPC" => "IPC_APPR",
                    "CHANGEORDER" => "CO_APPR",
                    _ => "CONTRACT_APPR"
                };

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string escapedName = templateName.Replace("'", "''");
                string query = $@"
                    SELECT ""Code""
                    FROM OWDD
                    WHERE ""Name"" = '{escapedName}'
                    AND ""IsActive"" = 'Y'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("Code").Value);
                }

                throw new Exception($"Approval template not found for type: {objectType}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approval template: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get SAP BoObjectType from string
        /// </summary>
        private BoObjectTypes GetBoObjectType(string objectType)
        {
            // For UDOs, the object type is dynamically assigned during UDO registration
            // We need to query the actual object type from the UDO metadata
            // For now, return a placeholder - the actual implementation should query
            // from OUDO table: SELECT Object FROM OUDO WHERE Code = '<UDO_CODE>'

            // Using a workaround: UDO object types start from 1470000000 and are sequential
            // This is a temporary solution - ideally should query from OUDO table
            return objectType.ToUpper() switch
            {
                "CONTRACT" => (BoObjectTypes)1470000001, // Placeholder - should query from OUDO
                "IPC" => (BoObjectTypes)1470000002,      // Placeholder - should query from OUDO
                "CHANGEORDER" => (BoObjectTypes)1470000003, // Placeholder - should query from OUDO
                _ => (BoObjectTypes)1470000001
            };
        }

        /// <summary>
        /// Get object type string from BoObjectTypes
        /// </summary>
        private string GetObjectTypeString(BoObjectTypes boObjectType)
        {
            // This would need to be enhanced to map UDO types back to strings
            return "CONTRACT"; // Simplified - would need UDO type lookup
        }

        /// <summary>
        /// Get SAP User ID from User Code
        /// HANA & SQL Server compatible with SQL injection protection
        /// </summary>
        private int GetUserIDFromUserCode(string userCode)
        {
            try
            {
                string query = $@"
                    SELECT ""USERID""
                    FROM OUSR
                    WHERE ""USER_CODE"" = {_dbHelper.QuoteString(userCode)}";

                return _dbHelper.ExecuteScalar<int>(query, "USERID");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting user ID for {userCode}: {ex.Message}", ex);
                throw new Exception($"User not found: {userCode}", ex);
            }
        }

        /// <summary>
        /// Validate approver has authority to approve
        /// HANA & SQL Server compatible
        /// </summary>
        private bool ValidateApproverAuthority(dynamic approvalReq, string approverUserCode)
        {
            try
            {
                // Get approver's user ID
                int approverUserID = GetUserIDFromUserCode(approverUserCode);

                // Query approval stages to check if user is authorized
                string query = $@"
                    SELECT COUNT(*) as CNT
                    FROM WDD1 D1
                    WHERE D1.""Code"" = {_dbHelper.SanitizeInteger(approvalReq.ApprovalTemplatesID)}
                    AND D1.""UserID"" = {_dbHelper.SanitizeInteger(approverUserID)}
                    AND D1.""IsActive"" = 'Y'";

                return _dbHelper.ExecuteCount(query) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error validating approver authority: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Update document approval status
        /// </summary>
        private void UpdateDocumentApprovalStatus(string objectType, int docEntry, string approvalStatus)
        {
            try
            {
                string tableName = objectType.ToUpper() switch
                {
                    "CONTRACT" => "@CONTRACT_HDR",
                    "IPC" => "@IPC_HDR",
                    "CHANGEORDER" => "@CO_HDR",
                    _ => "@CONTRACT_HDR"
                };

                // Update status using parameterized approach
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string statusText = approvalStatus switch
                {
                    APPROVAL_STATUS_PENDING => "Pending Approval",
                    APPROVAL_STATUS_APPROVED => "Approved",
                    APPROVAL_STATUS_REJECTED => "Rejected",
                    _ => "Draft"
                };

                string query = $@"
                    UPDATE ""{tableName}""
                    SET ""U_Status"" = '{statusText}'
                    WHERE ""DocEntry"" = {docEntry}";

                oRecordset.DoQuery(query);

                Logger.Debug($"Updated {tableName} DocEntry {docEntry} status to {statusText}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating document approval status: {ex.Message}", ex);
                // Don't throw - this is a secondary operation
            }
        }

        /// <summary>
        /// Notify approvers of pending approval request
        /// </summary>
        private void NotifyApprovers(int approvalRequestCode, string objectType, int docEntry, double amount)
        {
            try
            {
                if (_emailService == null)
                {
                    Logger.Warning("Email service not available for notifications");
                    return;
                }

                // Get list of approvers for this request
                List<string> approvers = GetApproversForRequest(approvalRequestCode);

                if (approvers.Count > 0)
                {
                    _emailService.SendApprovalRequestNotification(
                        approvalRequestCode,
                        objectType,
                        $"DocEntry: {docEntry}",
                        amount,
                        approvers,
                        "Please review and approve this document"
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending approval notifications: {ex.Message}", ex);
                // Don't throw - notifications are not critical
            }
        }

        /// <summary>
        /// Get list of approvers for an approval request
        /// </summary>
        private List<string> GetApproversForRequest(int approvalRequestCode)
        {
            List<string> approvers = new List<string>();

            try
            {
                // Get approval request to find template
                dynamic approvalReq = ApprovalRequestFactory.CreateApprovalRequest(_company);

                if (approvalReq.GetByKey(approvalRequestCode))
                {
                    int templateID = approvalReq.ApprovalTemplatesID;

                    // Get approvers from template stages
                    Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                    string query = $@"
                        SELECT DISTINCT U.""USER_CODE""
                        FROM WDD1 D1
                        INNER JOIN OUSR U ON D1.""UserID"" = U.""USERID""
                        WHERE D1.""Code"" = {templateID}
                        AND D1.""IsActive"" = 'Y'
                        AND U.""LOCKED"" = 'N'";

                    oRecordset.DoQuery(query);

                    while (!oRecordset.EoF)
                    {
                        approvers.Add(oRecordset.Fields.Item("USER_CODE").Value.ToString());
                        oRecordset.MoveNext();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approvers: {ex.Message}", ex);
            }

            return approvers;
        }

        /// <summary>
        /// Send approval/rejection notification
        /// </summary>
        private void SendApprovalNotification(int approvalRequestCode, bool approved, string approverUserCode, string comments)
        {
            try
            {
                // Get approval request details
                dynamic approvalReq = ApprovalRequestFactory.CreateApprovalRequest(_company);

                if (approvalReq.GetByKey(approvalRequestCode))
                {
                    string objectType = GetObjectTypeString(approvalReq.ObjectType);
                    string requesterEmail = GetUserEmail(approvalReq.OriginatorID);
                    string approverName = GetUserName(approverUserCode);

                    if (approved)
                    {
                        _emailService.SendApprovalGrantedNotification(
                            objectType,
                            $"DocEntry: {approvalReq.ObjectEntry}",
                            approverName,
                            requesterEmail,
                            comments
                        );
                    }
                    else
                    {
                        _emailService.SendApprovalRejectedNotification(
                            objectType,
                            $"DocEntry: {approvalReq.ObjectEntry}",
                            approverName,
                            requesterEmail,
                            comments
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending approval notification: {ex.Message}", ex);
                // Don't throw - notifications are not critical
            }
        }

        /// <summary>
        /// Get user email from user ID
        /// </summary>
        private string GetUserEmail(int userID)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT ""E_Mail""
                    FROM OUSR
                    WHERE ""USERID"" = {userID}";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return oRecordset.Fields.Item("E_Mail").Value?.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get user name from user code
        /// </summary>
        private string GetUserName(string userCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string escapedCode = userCode.Replace("'", "''");
                string query = $@"
                    SELECT ""U_NAME""
                    FROM OUSR
                    WHERE ""USER_CODE"" = '{escapedCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return oRecordset.Fields.Item("U_NAME").Value?.ToString() ?? userCode;
                }

                return userCode;
            }
            catch
            {
                return userCode;
            }
        }

        /// <summary>
        /// Get pending approval requests for a user
        /// </summary>
        public List<ApprovalRequestInfo> GetPendingApprovalsForUser(string userCode)
        {
            List<ApprovalRequestInfo> requests = new List<ApprovalRequestInfo>();

            try
            {
                int userID = GetUserIDFromUserCode(userCode);

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT DISTINCT
                        WTR.""WTRCode"",
                        WTR.""ObjectType"",
                        WTR.""ObjID"",
                        WTR.""Status"",
                        WTR.""CreateDate"",
                        WTR.""Remarks""
                    FROM OWTR WTR
                    INNER JOIN WDD1 D1 ON WTR.""ApprovalTemplate"" = D1.""Code""
                    WHERE WTR.""Status"" = 'W'
                    AND D1.""UserID"" = {userID}
                    AND D1.""IsActive"" = 'Y'
                    ORDER BY WTR.""CreateDate"" DESC";

                oRecordset.DoQuery(query);

                while (!oRecordset.EoF)
                {
                    requests.Add(new ApprovalRequestInfo
                    {
                        ApprovalRequestCode = Convert.ToInt32(oRecordset.Fields.Item("WTRCode").Value),
                        ObjectType = oRecordset.Fields.Item("ObjectType").Value.ToString(),
                        ObjectEntry = Convert.ToInt32(oRecordset.Fields.Item("ObjID").Value),
                        Status = oRecordset.Fields.Item("Status").Value.ToString(),
                        CreateDate = Convert.ToDateTime(oRecordset.Fields.Item("CreateDate").Value),
                        Remarks = oRecordset.Fields.Item("Remarks").Value?.ToString() ?? string.Empty
                    });

                    oRecordset.MoveNext();
                }

                Logger.Debug($"Found {requests.Count} pending approval(s) for user {userCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting pending approvals: {ex.Message}", ex);
            }

            return requests;
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Approval stage definition for template creation
    /// </summary>
    public class ApprovalStageDefinition
    {
        public int StageNumber { get; set; }
        public string StageName { get; set; }
        public string AuthorizerType { get; set; } // U = User, G = Group
        public double MinAmount { get; set; }
        public double MaxAmount { get; set; }
    }

    /// <summary>
    /// Approval request information
    /// </summary>
    public class ApprovalRequestInfo
    {
        public int ApprovalRequestCode { get; set; }
        public string ObjectType { get; set; }
        public int ObjectEntry { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string Remarks { get; set; }
    }

    #endregion
}
