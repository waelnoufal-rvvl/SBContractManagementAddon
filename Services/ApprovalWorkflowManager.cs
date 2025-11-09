using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Manages approval workflows using SAP B1 approval procedures
    /// </summary>
    public class ApprovalWorkflowManager
    {
        private readonly Company _company;
        private const string TEMPLATE_CONTRACT = "CONTRACT_APPROVAL";
        private const string TEMPLATE_IPC = "IPC_APPROVAL";
        private const string TEMPLATE_CHANGEORDER = "CO_APPROVAL";

        public ApprovalWorkflowManager(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        #region Approval Template Management

        /// <summary>
        /// Create approval templates for Contract Management documents
        /// </summary>
        public void CreateApprovalTemplates()
        {
            try
            {
                Logger.Info("Creating approval templates...");

                CreateContractApprovalTemplate();
                CreateIPCApprovalTemplate();
                CreateChangeOrderApprovalTemplate();

                Logger.Info("Approval templates created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating approval templates: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create contract approval template
        /// </summary>
        private void CreateContractApprovalTemplate()
        {
            try
            {
                // Check if template already exists
                if (ApprovalTemplateExists(TEMPLATE_CONTRACT))
                {
                    Logger.Info($"Approval template {TEMPLATE_CONTRACT} already exists");
                    return;
                }

                // Create approval template using SAP B1 Approval Templates
                // Note: This requires OWTM (Approval Templates) and WTM1 (Approval Template Stages)
                string query = $@"
                    INSERT INTO OWTM (""WtmCode"", ""Descript"", ""IsActive"")
                    VALUES ('{TEMPLATE_CONTRACT}', 'Contract Approval Workflow', 'Y')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info($"Created approval template: {TEMPLATE_CONTRACT}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not create contract approval template: {ex.Message}");
            }
        }

        /// <summary>
        /// Create IPC approval template
        /// </summary>
        private void CreateIPCApprovalTemplate()
        {
            try
            {
                if (ApprovalTemplateExists(TEMPLATE_IPC))
                {
                    Logger.Info($"Approval template {TEMPLATE_IPC} already exists");
                    return;
                }

                string query = $@"
                    INSERT INTO OWTM (""WtmCode"", ""Descript"", ""IsActive"")
                    VALUES ('{TEMPLATE_IPC}', 'IPC Approval Workflow', 'Y')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info($"Created approval template: {TEMPLATE_IPC}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not create IPC approval template: {ex.Message}");
            }
        }

        /// <summary>
        /// Create change order approval template
        /// </summary>
        private void CreateChangeOrderApprovalTemplate()
        {
            try
            {
                if (ApprovalTemplateExists(TEMPLATE_CHANGEORDER))
                {
                    Logger.Info($"Approval template {TEMPLATE_CHANGEORDER} already exists");
                    return;
                }

                string query = $@"
                    INSERT INTO OWTM (""WtmCode"", ""Descript"", ""IsActive"")
                    VALUES ('{TEMPLATE_CHANGEORDER}', 'Change Order Approval Workflow', 'Y')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info($"Created approval template: {TEMPLATE_CHANGEORDER}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not create change order approval template: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if approval template exists
        /// </summary>
        private bool ApprovalTemplateExists(string templateCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"
                    SELECT COUNT(*) as CNT
                    FROM OWTM
                    WHERE ""WtmCode"" = '{templateCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("CNT").Value) > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Approval Request Management

        /// <summary>
        /// Submit document for approval
        /// </summary>
        public int SubmitForApproval(string documentType, string documentCode, double amount, string remarks)
        {
            try
            {
                Logger.Info($"Submitting {documentType} [{documentCode}] for approval, amount: {amount}");

                // Create approval request
                int approvalRequestId = CreateApprovalRequest(documentType, documentCode, amount, remarks);

                if (approvalRequestId > 0)
                {
                    // Update document status
                    UpdateDocumentStatus(documentType, documentCode, "Pending Approval");

                    Logger.Info($"Approval request created with ID: {approvalRequestId}");
                    return approvalRequestId;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error submitting for approval: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create approval request in custom table
        /// </summary>
        private int CreateApprovalRequest(string documentType, string documentCode, double amount, string remarks)
        {
            try
            {
                int requestId = GetNextApprovalRequestId();
                string templateCode = GetTemplateForDocumentType(documentType);
                List<ApprovalStage> stages = GetApprovalStages(templateCode, amount);

                // Insert approval request header
                string query = $@"
                    INSERT INTO ""@CM_APPR_REQ""
                    (""Code"", ""DocType"", ""DocCode"", ""Amount"", ""Status"", ""RequestDate"",
                     ""RequestedBy"", ""CurrentStage"", ""Remarks"")
                    VALUES
                    ({requestId}, '{documentType}', '{documentCode}', {amount}, 'Pending',
                     '{DateTime.Now:yyyyMMdd}', '{_company.UserName}', 1, '{remarks}')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                // Insert approval stages
                foreach (var stage in stages)
                {
                    InsertApprovalStage(requestId, stage);
                }

                // Notify first stage approvers
                NotifyApprovers(requestId, 1);

                return requestId;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating approval request: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get next approval request ID
        /// </summary>
        private int GetNextApprovalRequestId()
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = @"
                    SELECT ISNULL(MAX(CAST(""Code"" AS INT)), 0) + 1 as NextID
                    FROM ""@CM_APPR_REQ""";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("NextID").Value);
                }

                return 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Get template code for document type
        /// </summary>
        private string GetTemplateForDocumentType(string documentType)
        {
            switch (documentType.ToUpper())
            {
                case "CONTRACT":
                    return TEMPLATE_CONTRACT;
                case "IPC":
                    return TEMPLATE_IPC;
                case "CHANGEORDER":
                    return TEMPLATE_CHANGEORDER;
                default:
                    return TEMPLATE_CONTRACT;
            }
        }

        /// <summary>
        /// Get approval stages based on amount
        /// </summary>
        private List<ApprovalStage> GetApprovalStages(string templateCode, double amount)
        {
            List<ApprovalStage> stages = new List<ApprovalStage>();

            try
            {
                // Define approval stages based on amount thresholds
                if (amount <= 50000)
                {
                    // Single approval: Project Manager
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 1,
                        StageName = "Project Manager Approval",
                        ApproverRole = "ProjectManager",
                        RequiredApprovals = 1
                    });
                }
                else if (amount <= 200000)
                {
                    // Two-stage approval: Project Manager -> Finance Manager
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 1,
                        StageName = "Project Manager Approval",
                        ApproverRole = "ProjectManager",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 2,
                        StageName = "Finance Manager Approval",
                        ApproverRole = "FinanceManager",
                        RequiredApprovals = 1
                    });
                }
                else if (amount <= 1000000)
                {
                    // Three-stage approval: Project Manager -> Finance Manager -> CFO
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 1,
                        StageName = "Project Manager Approval",
                        ApproverRole = "ProjectManager",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 2,
                        StageName = "Finance Manager Approval",
                        ApproverRole = "FinanceManager",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 3,
                        StageName = "CFO Approval",
                        ApproverRole = "CFO",
                        RequiredApprovals = 1
                    });
                }
                else
                {
                    // Four-stage approval: All above + CEO
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 1,
                        StageName = "Project Manager Approval",
                        ApproverRole = "ProjectManager",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 2,
                        StageName = "Finance Manager Approval",
                        ApproverRole = "FinanceManager",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 3,
                        StageName = "CFO Approval",
                        ApproverRole = "CFO",
                        RequiredApprovals = 1
                    });
                    stages.Add(new ApprovalStage
                    {
                        StageNumber = 4,
                        StageName = "CEO Approval",
                        ApproverRole = "CEO",
                        RequiredApprovals = 1
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approval stages: {ex.Message}", ex);
            }

            return stages;
        }

        /// <summary>
        /// Insert approval stage
        /// </summary>
        private void InsertApprovalStage(int requestId, ApprovalStage stage)
        {
            try
            {
                string query = $@"
                    INSERT INTO ""@CM_APPR_STAG""
                    (""Code"", ""LineId"", ""U_RequestID"", ""U_StageNum"", ""U_StageName"",
                     ""U_ApprRole"", ""U_ReqApprov"", ""U_Status"")
                    VALUES
                    ('{requestId}_{stage.StageNumber}', {stage.StageNumber}, {requestId}, {stage.StageNumber},
                     '{stage.StageName}', '{stage.ApproverRole}', {stage.RequiredApprovals}, 'Pending')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error inserting approval stage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Approve approval request
        /// </summary>
        public bool ApproveRequest(int requestId, string approverCode, string comments)
        {
            try
            {
                Logger.Info($"Processing approval for request {requestId} by user {approverCode}");

                // Get current stage
                int currentStage = GetCurrentStage(requestId);

                // Record approval
                RecordApproval(requestId, currentStage, approverCode, "Approved", comments);

                // Check if stage is complete
                if (IsStageComplete(requestId, currentStage))
                {
                    // Check if there are more stages
                    int totalStages = GetTotalStages(requestId);

                    if (currentStage < totalStages)
                    {
                        // Move to next stage
                        UpdateRequestStage(requestId, currentStage + 1);
                        NotifyApprovers(requestId, currentStage + 1);

                        Logger.Info($"Moved to stage {currentStage + 1}");
                    }
                    else
                    {
                        // All stages approved - finalize
                        FinalizeApproval(requestId);
                        Logger.Info($"Approval request {requestId} fully approved");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error approving request: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Reject approval request
        /// </summary>
        public bool RejectRequest(int requestId, string approverCode, string reason)
        {
            try
            {
                Logger.Info($"Rejecting request {requestId} by user {approverCode}");

                int currentStage = GetCurrentStage(requestId);

                // Record rejection
                RecordApproval(requestId, currentStage, approverCode, "Rejected", reason);

                // Update request status
                UpdateRequestStatus(requestId, "Rejected");

                // Update document status
                var (documentType, documentCode) = GetDocumentInfo(requestId);
                UpdateDocumentStatus(documentType, documentCode, "Rejected");

                // Notify requester
                NotifyRejection(requestId, approverCode, reason);

                Logger.Info($"Approval request {requestId} rejected");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error rejecting request: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get current stage of approval request
        /// </summary>
        private int GetCurrentStage(int requestId)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"
                    SELECT ""CurrentStage""
                    FROM ""@CM_APPR_REQ""
                    WHERE CAST(""Code"" AS INT) = {requestId}";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("CurrentStage").Value);
                }

                return 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Record approval/rejection
        /// </summary>
        private void RecordApproval(int requestId, int stageNumber, string approverCode, string decision, string comments)
        {
            try
            {
                string query = $@"
                    INSERT INTO ""@CM_APPR_DTLS""
                    (""Code"", ""U_RequestID"", ""U_StageNum"", ""U_Approver"", ""U_Decision"",
                     ""U_Comments"", ""U_ApprDate"")
                    VALUES
                    ('{Guid.NewGuid()}', {requestId}, {stageNumber}, '{approverCode}',
                     '{decision}', '{comments}', '{DateTime.Now:yyyyMMdd}')";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error recording approval: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if stage is complete
        /// </summary>
        private bool IsStageComplete(int requestId, int stageNumber)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                // Get required approvals for stage
                string query1 = $@"
                    SELECT ""U_ReqApprov""
                    FROM ""@CM_APPR_STAG""
                    WHERE CAST(""U_RequestID"" AS INT) = {requestId}
                    AND CAST(""U_StageNum"" AS INT) = {stageNumber}";

                oRecordset.DoQuery(query1);
                int requiredApprovals = !oRecordset.EoF ? Convert.ToInt32(oRecordset.Fields.Item("U_ReqApprov").Value) : 1;

                // Get actual approvals received
                string query2 = $@"
                    SELECT COUNT(*) as CNT
                    FROM ""@CM_APPR_DTLS""
                    WHERE CAST(""U_RequestID"" AS INT) = {requestId}
                    AND CAST(""U_StageNum"" AS INT) = {stageNumber}
                    AND ""U_Decision"" = 'Approved'";

                oRecordset.DoQuery(query2);
                int actualApprovals = !oRecordset.EoF ? Convert.ToInt32(oRecordset.Fields.Item("CNT").Value) : 0;

                return actualApprovals >= requiredApprovals;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking stage completion: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get total stages for request
        /// </summary>
        private int GetTotalStages(int requestId)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"
                    SELECT MAX(CAST(""U_StageNum"" AS INT)) as MaxStage
                    FROM ""@CM_APPR_STAG""
                    WHERE CAST(""U_RequestID"" AS INT) = {requestId}";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("MaxStage").Value);
                }

                return 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Update request stage
        /// </summary>
        private void UpdateRequestStage(int requestId, int newStage)
        {
            try
            {
                string query = $@"
                    UPDATE ""@CM_APPR_REQ""
                    SET ""CurrentStage"" = {newStage}
                    WHERE CAST(""Code"" AS INT) = {requestId}";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating request stage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update request status
        /// </summary>
        private void UpdateRequestStatus(int requestId, string status)
        {
            try
            {
                string query = $@"
                    UPDATE ""@CM_APPR_REQ""
                    SET ""Status"" = '{status}'
                    WHERE CAST(""Code"" AS INT) = {requestId}";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating request status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update document status
        /// </summary>
        private void UpdateDocumentStatus(string documentType, string documentCode, string status)
        {
            try
            {
                string tableName = GetTableNameForDocumentType(documentType);

                string query = $@"
                    UPDATE ""{tableName}""
                    SET ""Status"" = '{status}'
                    WHERE ""Code"" = '{documentCode}'";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating document status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get table name for document type
        /// </summary>
        private string GetTableNameForDocumentType(string documentType)
        {
            switch (documentType.ToUpper())
            {
                case "CONTRACT":
                    return "@CONTRACT_HDR";
                case "IPC":
                    return "@IPC_HDR";
                case "CHANGEORDER":
                    return "@CO_HDR";
                default:
                    return "@CONTRACT_HDR";
            }
        }

        /// <summary>
        /// Finalize approval
        /// </summary>
        private void FinalizeApproval(int requestId)
        {
            try
            {
                UpdateRequestStatus(requestId, "Approved");

                var (documentType, documentCode) = GetDocumentInfo(requestId);
                UpdateDocumentStatus(documentType, documentCode, "Approved");

                // Notify requester of approval
                NotifyApprovalComplete(requestId);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error finalizing approval: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get document info from request
        /// </summary>
        private (string documentType, string documentCode) GetDocumentInfo(int requestId)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"
                    SELECT ""DocType"", ""DocCode""
                    FROM ""@CM_APPR_REQ""
                    WHERE CAST(""Code"" AS INT) = {requestId}";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    string docType = oRecordset.Fields.Item("DocType").Value.ToString();
                    string docCode = oRecordset.Fields.Item("DocCode").Value.ToString();
                    return (docType, docCode);
                }

                return (string.Empty, string.Empty);
            }
            catch
            {
                return (string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Notify approvers
        /// </summary>
        private void NotifyApprovers(int requestId, int stageNumber)
        {
            try
            {
                // Get approvers for this stage
                List<string> approvers = GetApproversForStage(requestId, stageNumber);

                foreach (string approver in approvers)
                {
                    // Send notification (could be email, SAP message, etc.)
                    Logger.Info($"Notifying approver {approver} for request {requestId}, stage {stageNumber}");
                    // TODO: Integrate with email notification service
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error notifying approvers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get approvers for stage
        /// </summary>
        private List<string> GetApproversForStage(int requestId, int stageNumber)
        {
            List<string> approvers = new List<string>();

            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                // Get role for this stage
                string query = $@"
                    SELECT ""U_ApprRole""
                    FROM ""@CM_APPR_STAG""
                    WHERE CAST(""U_RequestID"" AS INT) = {requestId}
                    AND CAST(""U_StageNum"" AS INT) = {stageNumber}";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    string role = oRecordset.Fields.Item("U_ApprRole").Value.ToString();

                    // Get users in this role
                    string query2 = $@"
                        SELECT DISTINCT U.USER_CODE
                        FROM OUSR U
                        INNER JOIN USR1 UG ON U.USERID = UG.USERID
                        INNER JOIN OUGE G ON UG.GROUPID = G.ID
                        WHERE G.NAME = '{role}'
                        AND U.LOCKED = 'N'";

                    oRecordset.DoQuery(query2);

                    while (!oRecordset.EoF)
                    {
                        approvers.Add(oRecordset.Fields.Item("USER_CODE").Value.ToString());
                        oRecordset.MoveNext();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting approvers for stage: {ex.Message}", ex);
            }

            return approvers;
        }

        /// <summary>
        /// Notify rejection
        /// </summary>
        private void NotifyRejection(int requestId, string approverCode, string reason)
        {
            try
            {
                Logger.Info($"Notifying rejection for request {requestId}");
                // TODO: Integrate with email notification service
            }
            catch (Exception ex)
            {
                Logger.Error($"Error notifying rejection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Notify approval complete
        /// </summary>
        private void NotifyApprovalComplete(int requestId)
        {
            try
            {
                Logger.Info($"Notifying approval complete for request {requestId}");
                // TODO: Integrate with email notification service
            }
            catch (Exception ex)
            {
                Logger.Error($"Error notifying approval complete: {ex.Message}", ex);
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Get pending approval requests for user
        /// </summary>
        public List<ApprovalRequest> GetPendingApprovalsForUser(string userCode)
        {
            List<ApprovalRequest> requests = new List<ApprovalRequest>();

            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT DISTINCT R.""Code"", R.""DocType"", R.""DocCode"", R.""Amount"",
                           R.""RequestDate"", R.""RequestedBy"", R.""CurrentStage"", R.""Remarks""
                    FROM ""@CM_APPR_REQ"" R
                    INNER JOIN ""@CM_APPR_STAG"" S ON CAST(R.""Code"" AS INT) = CAST(S.""U_RequestID"" AS INT)
                    INNER JOIN USR1 UG ON UG.USERID = (SELECT USERID FROM OUSR WHERE USER_CODE = '{userCode}')
                    INNER JOIN OUGE G ON UG.GROUPID = G.ID
                    WHERE R.""Status"" = 'Pending'
                    AND CAST(R.""CurrentStage"" AS INT) = CAST(S.""U_StageNum"" AS INT)
                    AND G.NAME = S.""U_ApprRole""";

                oRecordset.DoQuery(query);

                while (!oRecordset.EoF)
                {
                    requests.Add(new ApprovalRequest
                    {
                        RequestId = Convert.ToInt32(oRecordset.Fields.Item("Code").Value),
                        DocumentType = oRecordset.Fields.Item("DocType").Value.ToString(),
                        DocumentCode = oRecordset.Fields.Item("DocCode").Value.ToString(),
                        Amount = Convert.ToDouble(oRecordset.Fields.Item("Amount").Value),
                        RequestDate = Convert.ToDateTime(oRecordset.Fields.Item("RequestDate").Value),
                        RequestedBy = oRecordset.Fields.Item("RequestedBy").Value.ToString(),
                        CurrentStage = Convert.ToInt32(oRecordset.Fields.Item("CurrentStage").Value),
                        Remarks = oRecordset.Fields.Item("Remarks").Value.ToString()
                    });

                    oRecordset.MoveNext();
                }
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
    /// Approval stage model
    /// </summary>
    public class ApprovalStage
    {
        public int StageNumber { get; set; }
        public string StageName { get; set; }
        public string ApproverRole { get; set; }
        public int RequiredApprovals { get; set; }
    }

    /// <summary>
    /// Approval request model
    /// </summary>
    public class ApprovalRequest
    {
        public int RequestId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentCode { get; set; }
        public double Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; }
        public int CurrentStage { get; set; }
        public string Remarks { get; set; }
    }

    #endregion
}
