using System;
using System.Collections.Generic;
using System.IO;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Service for managing document attachments using SAP B1 Attachments2
    /// </summary>
    public class AttachmentService
    {
        private readonly Company _company;
        private string _attachmentsPath;

        public AttachmentService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            InitializeAttachmentsPath();
        }

        /// <summary>
        /// Initialize attachments path from SAP B1
        /// </summary>
        private void InitializeAttachmentsPath()
        {
            try
            {
                // Get attachments path from SAP B1 administration
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = "SELECT \"AttachPath\" FROM OADP WHERE \"AbsEntry\" = (SELECT MIN(\"AbsEntry\") FROM OADP)";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF && oRecordset.Fields.Item("AttachPath").Value != null)
                {
                    _attachmentsPath = oRecordset.Fields.Item("AttachPath").Value.ToString();
                    Logger.Info($"Attachments path initialized: {_attachmentsPath}");
                }
                else
                {
                    // Fallback to default path
                    _attachmentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                        "SAP", "Attachments");
                    Logger.Warning($"Using default attachments path: {_attachmentsPath}");
                }

                // Ensure directory exists
                if (!Directory.Exists(_attachmentsPath))
                {
                    Directory.CreateDirectory(_attachmentsPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing attachments path: {ex.Message}", ex);
                _attachmentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                    "ContractManagement", "Attachments");
            }
        }

        #region Add Attachments

        /// <summary>
        /// Add attachment to document
        /// </summary>
        public int AddAttachment(string sourceFilePath, string targetFileName = null)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
                }

                Logger.Info($"Adding attachment: {sourceFilePath}");

                // Use SAP B1 Attachments2 object
                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);

                // Set target file name
                if (string.IsNullOrEmpty(targetFileName))
                {
                    targetFileName = Path.GetFileName(sourceFilePath);
                }

                // Add attachment line
                oAttachments.Lines.SourcePath = Path.GetDirectoryName(sourceFilePath);
                oAttachments.Lines.FileName = Path.GetFileName(sourceFilePath);
                oAttachments.Lines.FileExtension = Path.GetExtension(sourceFilePath).TrimStart('.');

                // Optionally override target file name
                if (targetFileName != Path.GetFileName(sourceFilePath))
                {
                    oAttachments.Lines.Override = BoYesNoEnum.tYES;
                    oAttachments.Lines.FileName = Path.GetFileNameWithoutExtension(targetFileName);
                }

                int result = oAttachments.Add();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Error adding attachment: {error}");
                }

                // Get the AbsEntry of the newly created attachment
                int absEntry = int.Parse(_company.GetNewObjectKey());

                Logger.Info($"Attachment added successfully. AbsEntry: {absEntry}");

                return absEntry;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding attachment: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Add multiple attachments
        /// </summary>
        public int AddMultipleAttachments(List<string> sourceFilePaths)
        {
            try
            {
                if (sourceFilePaths == null || sourceFilePaths.Count == 0)
                {
                    throw new ArgumentException("No files provided");
                }

                Logger.Info($"Adding {sourceFilePaths.Count} attachments");

                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);

                for (int i = 0; i < sourceFilePaths.Count; i++)
                {
                    string filePath = sourceFilePaths[i];

                    if (!File.Exists(filePath))
                    {
                        Logger.Warning($"File not found, skipping: {filePath}");
                        continue;
                    }

                    if (i > 0)
                    {
                        oAttachments.Lines.Add();
                    }

                    oAttachments.Lines.SourcePath = Path.GetDirectoryName(filePath);
                    oAttachments.Lines.FileName = Path.GetFileName(filePath);
                    oAttachments.Lines.FileExtension = Path.GetExtension(filePath).TrimStart('.');
                }

                int result = oAttachments.Add();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Error adding attachments: {error}");
                }

                int absEntry = int.Parse(_company.GetNewObjectKey());

                Logger.Info($"Multiple attachments added successfully. AbsEntry: {absEntry}");

                return absEntry;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding multiple attachments: {ex.Message}", ex);
                throw;
            }
        }

        #endregion

        #region Update Attachments

        /// <summary>
        /// Update existing attachment by adding more files
        /// </summary>
        public bool UpdateAttachment(int absEntry, string sourceFilePath)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
                }

                Logger.Info($"Updating attachment {absEntry} with file: {sourceFilePath}");

                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);

                if (!oAttachments.GetByKey(absEntry))
                {
                    throw new Exception($"Attachment not found: {absEntry}");
                }

                // Add new line
                oAttachments.Lines.Add();
                oAttachments.Lines.SetCurrentLine(oAttachments.Lines.Count - 1);
                oAttachments.Lines.SourcePath = Path.GetDirectoryName(sourceFilePath);
                oAttachments.Lines.FileName = Path.GetFileName(sourceFilePath);
                oAttachments.Lines.FileExtension = Path.GetExtension(sourceFilePath).TrimStart('.');

                int result = oAttachments.Update();

                if (result != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Error updating attachment: {error}");
                }

                Logger.Info($"Attachment updated successfully");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating attachment: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Remove specific file from attachment
        /// </summary>
        public bool RemoveFileFromAttachment(int absEntry, int lineNum)
        {
            try
            {
                Logger.Info($"Removing file from attachment {absEntry}, line {lineNum}");

                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);

                if (!oAttachments.GetByKey(absEntry))
                {
                    throw new Exception($"Attachment not found: {absEntry}");
                }

                // Navigate to the line and delete it
                oAttachments.Lines.SetCurrentLine(lineNum);
                // Note: Attachments2_Lines doesn't have a Delete() method
                // To remove a line, you need to delete the entire attachment and recreate without that line
                // or mark it for removal and call Update()
                // For now, we'll throw an exception to indicate this needs a different approach
                throw new NotImplementedException("Removing individual files from Attachments2 requires recreating the attachment without the unwanted file");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error removing file from attachment: {ex.Message}", ex);
                throw;
            }
        }

        #endregion

        #region Query Attachments

        /// <summary>
        /// Get attachment details
        /// </summary>
        public List<AttachmentInfo> GetAttachmentDetails(int absEntry)
        {
            List<AttachmentInfo> attachments = new List<AttachmentInfo>();

            try
            {
                if (absEntry <= 0)
                {
                    return attachments;
                }

                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);

                if (!oAttachments.GetByKey(absEntry))
                {
                    Logger.Warning($"Attachment not found: {absEntry}");
                    return attachments;
                }

                for (int i = 0; i < oAttachments.Lines.Count; i++)
                {
                    oAttachments.Lines.SetCurrentLine(i);

                    AttachmentInfo info = new AttachmentInfo
                    {
                        AbsEntry = absEntry,
                        LineNum = i,
                        FileName = oAttachments.Lines.FileName,
                        FileExtension = oAttachments.Lines.FileExtension,
                        SourcePath = oAttachments.Lines.SourcePath,
                        FullPath = Path.Combine(oAttachments.Lines.SourcePath,
                            oAttachments.Lines.FileName + "." + oAttachments.Lines.FileExtension),
                        Date = oAttachments.Lines.AttachmentDate // Use AttachmentDate property instead of Date
                    };

                    attachments.Add(info);
                }

                Logger.Debug($"Retrieved {attachments.Count} attachment(s) for AbsEntry {absEntry}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting attachment details: {ex.Message}", ex);
            }

            return attachments;
        }

        /// <summary>
        /// Check if attachment exists
        /// </summary>
        public bool AttachmentExists(int absEntry)
        {
            try
            {
                if (absEntry <= 0)
                    return false;

                Attachments2 oAttachments = (Attachments2)_company.GetBusinessObject(BoObjectTypes.oAttachments2);
                return oAttachments.GetByKey(absEntry);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking attachment existence: {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Link Attachments to Documents

        /// <summary>
        /// Link attachment to contract
        /// </summary>
        public bool LinkAttachmentToContract(string contractCode, int attachmentAbsEntry)
        {
            try
            {
                Logger.Info($"Linking attachment {attachmentAbsEntry} to contract {contractCode}");

                string query = $@"
                    UPDATE ""@CONTRACT_HDR""
                    SET ""AtcEntry"" = {attachmentAbsEntry}
                    WHERE ""Code"" = '{contractCode}'";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info("Attachment linked to contract successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error linking attachment to contract: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Link attachment to IPC
        /// </summary>
        public bool LinkAttachmentToIPC(string ipcCode, int attachmentAbsEntry)
        {
            try
            {
                Logger.Info($"Linking attachment {attachmentAbsEntry} to IPC {ipcCode}");

                string query = $@"
                    UPDATE ""@IPC_HDR""
                    SET ""AtcEntry"" = {attachmentAbsEntry}
                    WHERE ""Code"" = '{ipcCode}'";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info("Attachment linked to IPC successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error linking attachment to IPC: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Link attachment to change order
        /// </summary>
        public bool LinkAttachmentToChangeOrder(string changeOrderCode, int attachmentAbsEntry)
        {
            try
            {
                Logger.Info($"Linking attachment {attachmentAbsEntry} to change order {changeOrderCode}");

                string query = $@"
                    UPDATE ""@CO_HDR""
                    SET ""AtcEntry"" = {attachmentAbsEntry}
                    WHERE ""Code"" = '{changeOrderCode}'";

                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);

                Logger.Info("Attachment linked to change order successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error linking attachment to change order: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get attachment AbsEntry for contract
        /// </summary>
        public int GetContractAttachmentEntry(string contractCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT ""AtcEntry""
                    FROM ""@CONTRACT_HDR""
                    WHERE ""Code"" = '{contractCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF && oRecordset.Fields.Item("AtcEntry").Value != null)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("AtcEntry").Value);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting contract attachment entry: {ex.Message}", ex);
                return -1;
            }
        }

        /// <summary>
        /// Get attachment AbsEntry for IPC
        /// </summary>
        public int GetIPCAttachmentEntry(string ipcCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT ""AtcEntry""
                    FROM ""@IPC_HDR""
                    WHERE ""Code"" = '{ipcCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF && oRecordset.Fields.Item("AtcEntry").Value != null)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("AtcEntry").Value);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPC attachment entry: {ex.Message}", ex);
                return -1;
            }
        }

        /// <summary>
        /// Get attachment AbsEntry for change order
        /// </summary>
        public int GetChangeOrderAttachmentEntry(string changeOrderCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT ""AtcEntry""
                    FROM ""@CO_HDR""
                    WHERE ""Code"" = '{changeOrderCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF && oRecordset.Fields.Item("AtcEntry").Value != null)
                {
                    return Convert.ToInt32(oRecordset.Fields.Item("AtcEntry").Value);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting change order attachment entry: {ex.Message}", ex);
                return -1;
            }
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Copy file to attachments directory
        /// </summary>
        public string CopyFileToAttachmentsDirectory(string sourceFilePath, string targetFileName = null)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
                }

                if (string.IsNullOrEmpty(targetFileName))
                {
                    targetFileName = Path.GetFileName(sourceFilePath);
                }

                string targetPath = Path.Combine(_attachmentsPath, targetFileName);

                // Handle duplicate file names
                int counter = 1;
                while (File.Exists(targetPath))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(targetFileName);
                    string extension = Path.GetExtension(targetFileName);
                    targetFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                    targetPath = Path.Combine(_attachmentsPath, targetFileName);
                    counter++;
                }

                File.Copy(sourceFilePath, targetPath);

                Logger.Info($"File copied to attachments directory: {targetPath}");

                return targetPath;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error copying file to attachments directory: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Open attachment file
        /// </summary>
        public void OpenAttachment(int absEntry, int lineNum)
        {
            try
            {
                List<AttachmentInfo> attachments = GetAttachmentDetails(absEntry);

                if (lineNum < 0 || lineNum >= attachments.Count)
                {
                    throw new ArgumentException("Invalid line number");
                }

                string filePath = attachments[lineNum].FullPath;

                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(filePath);
                    Logger.Info($"Opened attachment: {filePath}");
                }
                else
                {
                    throw new FileNotFoundException($"Attachment file not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error opening attachment: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Save attachment to specified location
        /// </summary>
        public string SaveAttachmentAs(int absEntry, int lineNum, string targetPath)
        {
            try
            {
                List<AttachmentInfo> attachments = GetAttachmentDetails(absEntry);

                if (lineNum < 0 || lineNum >= attachments.Count)
                {
                    throw new ArgumentException("Invalid line number");
                }

                string sourceFilePath = attachments[lineNum].FullPath;

                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Attachment file not found: {sourceFilePath}");
                }

                File.Copy(sourceFilePath, targetPath, true);

                Logger.Info($"Attachment saved to: {targetPath}");

                return targetPath;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving attachment: {ex.Message}", ex);
                throw;
            }
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Attachment information model
    /// </summary>
    public class AttachmentInfo
    {
        public int AbsEntry { get; set; }
        public int LineNum { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string SourcePath { get; set; }
        public string FullPath { get; set; }
        public DateTime Date { get; set; }

        public string DisplayName => $"{FileName}.{FileExtension}";

        public string FileSize
        {
            get
            {
                try
                {
                    if (File.Exists(FullPath))
                    {
                        FileInfo fi = new FileInfo(FullPath);
                        return FormatFileSize(fi.Length);
                    }
                    return "N/A";
                }
                catch
                {
                    return "N/A";
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    #endregion
}
