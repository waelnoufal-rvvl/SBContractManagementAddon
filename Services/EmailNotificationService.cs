using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Email notification service for Contract Management
    /// </summary>
    public class EmailNotificationService
    {
        private readonly Company _company;
        private string _smtpServer;
        private int _smtpPort;
        private string _smtpUsername;
        private string _smtpPassword;
        private bool _enableSSL;
        private string _fromEmail;
        private string _fromName;

        public EmailNotificationService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            LoadConfiguration();
        }

        /// <summary>
        /// Load email configuration from App.config
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                _smtpServer = ConfigurationManager.AppSettings["SMTP_Server"] ?? "smtp.office365.com";
                _smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_Port"] ?? "587");
                _smtpUsername = ConfigurationManager.AppSettings["SMTP_Username"] ?? "";
                _smtpPassword = ConfigurationManager.AppSettings["SMTP_Password"] ?? "";
                _enableSSL = bool.Parse(ConfigurationManager.AppSettings["SMTP_EnableSSL"] ?? "true");
                _fromEmail = ConfigurationManager.AppSettings["Email_From"] ?? "noreply@company.com";
                _fromName = ConfigurationManager.AppSettings["Email_FromName"] ?? "Contract Management System";

                Logger.Info("Email configuration loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error loading email configuration: {ex.Message}");
            }
        }

        #region Approval Notifications

        /// <summary>
        /// Send approval request notification
        /// </summary>
        public bool SendApprovalRequestNotification(int approvalRequestId, string documentType, string documentCode,
            double amount, List<string> approvers, string remarks)
        {
            try
            {
                Logger.Info($"Sending approval request notification for {documentType} [{documentCode}]");

                List<string> recipientEmails = GetUserEmails(approvers);

                if (recipientEmails.Count == 0)
                {
                    Logger.Warning("No recipient emails found for approval notification");
                    return false;
                }

                string subject = $"Approval Request: {documentType} {documentCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2>New Approval Request</h2>");
                body.AppendLine($"<p>A new {documentType} has been submitted for your approval.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Document Type:</strong></td><td>{documentType}</td></tr>");
                body.AppendLine($"<tr><td><strong>Document Code:</strong></td><td>{documentCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Amount:</strong></td><td>{amount:N2}</td></tr>");
                body.AppendLine($"<tr><td><strong>Request ID:</strong></td><td>{approvalRequestId}</td></tr>");
                body.AppendLine($"<tr><td><strong>Remarks:</strong></td><td>{remarks}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please log in to SAP Business One to review and approve/reject this request.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(recipientEmails, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending approval request notification: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Send approval granted notification
        /// </summary>
        public bool SendApprovalGrantedNotification(string documentType, string documentCode, string approverName,
            string requesterEmail, string comments)
        {
            try
            {
                Logger.Info($"Sending approval granted notification for {documentType} [{documentCode}]");

                if (string.IsNullOrEmpty(requesterEmail))
                {
                    Logger.Warning("No requester email found");
                    return false;
                }

                string subject = $"Approved: {documentType} {documentCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2 style='color:green;'>Approval Granted</h2>");
                body.AppendLine($"<p>Your {documentType} has been approved.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Document Type:</strong></td><td>{documentType}</td></tr>");
                body.AppendLine($"<tr><td><strong>Document Code:</strong></td><td>{documentCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Approved By:</strong></td><td>{approverName}</td></tr>");
                body.AppendLine($"<tr><td><strong>Date:</strong></td><td>{DateTime.Now:yyyy-MM-dd HH:mm}</td></tr>");
                if (!string.IsNullOrEmpty(comments))
                {
                    body.AppendLine($"<tr><td><strong>Comments:</strong></td><td>{comments}</td></tr>");
                }
                body.AppendLine($"</table>");
                body.AppendLine($"<p>You can now proceed with the next steps.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(new List<string> { requesterEmail }, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending approval granted notification: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Send approval rejected notification
        /// </summary>
        public bool SendApprovalRejectedNotification(string documentType, string documentCode, string approverName,
            string requesterEmail, string reason)
        {
            try
            {
                Logger.Info($"Sending approval rejected notification for {documentType} [{documentCode}]");

                if (string.IsNullOrEmpty(requesterEmail))
                {
                    Logger.Warning("No requester email found");
                    return false;
                }

                string subject = $"Rejected: {documentType} {documentCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2 style='color:red;'>Approval Rejected</h2>");
                body.AppendLine($"<p>Your {documentType} has been rejected.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Document Type:</strong></td><td>{documentType}</td></tr>");
                body.AppendLine($"<tr><td><strong>Document Code:</strong></td><td>{documentCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Rejected By:</strong></td><td>{approverName}</td></tr>");
                body.AppendLine($"<tr><td><strong>Date:</strong></td><td>{DateTime.Now:yyyy-MM-dd HH:mm}</td></tr>");
                body.AppendLine($"<tr><td><strong>Reason:</strong></td><td>{reason}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please review the reason and make necessary corrections before resubmitting.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(new List<string> { requesterEmail }, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending approval rejected notification: {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Contract Notifications

        /// <summary>
        /// Send contract expiry warning
        /// </summary>
        public bool SendContractExpiryWarning(string contractCode, string contractDescription, DateTime endDate,
            List<string> recipients)
        {
            try
            {
                Logger.Info($"Sending contract expiry warning for: {contractCode}");

                List<string> recipientEmails = GetUserEmails(recipients);

                if (recipientEmails.Count == 0)
                {
                    Logger.Warning("No recipient emails found");
                    return false;
                }

                int daysRemaining = (endDate - DateTime.Today).Days;

                string subject = $"Contract Expiry Warning: {contractCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2 style='color:orange;'>Contract Expiring Soon</h2>");
                body.AppendLine($"<p>The following contract is expiring in {daysRemaining} days.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Contract Code:</strong></td><td>{contractCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Description:</strong></td><td>{contractDescription}</td></tr>");
                body.AppendLine($"<tr><td><strong>End Date:</strong></td><td>{endDate:yyyy-MM-dd}</td></tr>");
                body.AppendLine($"<tr><td><strong>Days Remaining:</strong></td><td>{daysRemaining}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please take necessary action to renew or close the contract.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(recipientEmails, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending contract expiry warning: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Send IPC created notification
        /// </summary>
        public bool SendIPCCreatedNotification(string ipcCode, int ipcNumber, string contractCode,
            double amount, List<string> recipients)
        {
            try
            {
                Logger.Info($"Sending IPC created notification: {ipcCode}");

                List<string> recipientEmails = GetUserEmails(recipients);

                if (recipientEmails.Count == 0)
                {
                    Logger.Warning("No recipient emails found");
                    return false;
                }

                string subject = $"New IPC Created: {ipcCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2>New Interim Payment Certificate</h2>");
                body.AppendLine($"<p>A new IPC has been created.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>IPC Code:</strong></td><td>{ipcCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>IPC Number:</strong></td><td>{ipcNumber}</td></tr>");
                body.AppendLine($"<tr><td><strong>Contract:</strong></td><td>{contractCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Amount:</strong></td><td>{amount:N2}</td></tr>");
                body.AppendLine($"<tr><td><strong>Date:</strong></td><td>{DateTime.Now:yyyy-MM-dd}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please review in SAP Business One.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(recipientEmails, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending IPC created notification: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Send change order notification
        /// </summary>
        public bool SendChangeOrderNotification(string changeOrderCode, string contractCode, string type,
            double amount, List<string> recipients)
        {
            try
            {
                Logger.Info($"Sending change order notification: {changeOrderCode}");

                List<string> recipientEmails = GetUserEmails(recipients);

                if (recipientEmails.Count == 0)
                {
                    Logger.Warning("No recipient emails found");
                    return false;
                }

                string subject = $"Change Order {type}: {changeOrderCode}";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2>Change Order Created</h2>");
                body.AppendLine($"<p>A new change order has been created.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Change Order:</strong></td><td>{changeOrderCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Contract:</strong></td><td>{contractCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Type:</strong></td><td>{type}</td></tr>");
                body.AppendLine($"<tr><td><strong>Amount:</strong></td><td>{amount:N2}</td></tr>");
                body.AppendLine($"<tr><td><strong>Date:</strong></td><td>{DateTime.Now:yyyy-MM-dd}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please review in SAP Business One.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(recipientEmails, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending change order notification: {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Alert Notifications

        /// <summary>
        /// Send budget threshold alert
        /// </summary>
        public bool SendBudgetThresholdAlert(string contractCode, double budgetUsedPercentage,
            double thresholdPercentage, List<string> recipients)
        {
            try
            {
                Logger.Info($"Sending budget threshold alert for contract: {contractCode}");

                List<string> recipientEmails = GetUserEmails(recipients);

                if (recipientEmails.Count == 0)
                {
                    Logger.Warning("No recipient emails found");
                    return false;
                }

                string subject = $"Budget Alert: {contractCode} - {budgetUsedPercentage:N1}% Used";

                StringBuilder body = new StringBuilder();
                body.AppendLine($"<html><body>");
                body.AppendLine($"<h2 style='color:orange;'>Budget Threshold Alert</h2>");
                body.AppendLine($"<p>Contract {contractCode} has exceeded the budget threshold.</p>");
                body.AppendLine($"<table border='1' cellpadding='5' style='border-collapse:collapse;'>");
                body.AppendLine($"<tr><td><strong>Contract:</strong></td><td>{contractCode}</td></tr>");
                body.AppendLine($"<tr><td><strong>Budget Used:</strong></td><td>{budgetUsedPercentage:N1}%</td></tr>");
                body.AppendLine($"<tr><td><strong>Threshold:</strong></td><td>{thresholdPercentage:N1}%</td></tr>");
                body.AppendLine($"<tr><td><strong>Date:</strong></td><td>{DateTime.Now:yyyy-MM-dd}</td></tr>");
                body.AppendLine($"</table>");
                body.AppendLine($"<p>Please review the contract budget and take necessary action.</p>");
                body.AppendLine($"<p><small>This is an automated message from Contract Management System.</small></p>");
                body.AppendLine($"</body></html>");

                return SendEmail(recipientEmails, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending budget threshold alert: {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Send email
        /// </summary>
        private bool SendEmail(List<string> recipients, string subject, string body, bool isHtml)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername))
                {
                    Logger.Warning("SMTP configuration is not complete. Email not sent.");
                    return false;
                }

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);

                    foreach (string recipient in recipients)
                    {
                        if (!string.IsNullOrEmpty(recipient))
                        {
                            mail.To.Add(recipient);
                        }
                    }

                    if (mail.To.Count == 0)
                    {
                        Logger.Warning("No valid recipients found");
                        return false;
                    }

                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = isHtml;

                    using (SmtpClient smtp = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        smtp.EnableSsl = _enableSSL;

                        smtp.Send(mail);
                    }
                }

                Logger.Info($"Email sent successfully to {recipients.Count} recipients: {subject}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending email: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get user emails from user codes
        /// </summary>
        private List<string> GetUserEmails(List<string> userCodes)
        {
            List<string> emails = new List<string>();

            try
            {
                foreach (string userCode in userCodes)
                {
                    string email = GetUserEmail(userCode);
                    if (!string.IsNullOrEmpty(email))
                    {
                        emails.Add(email);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting user emails: {ex.Message}", ex);
            }

            return emails;
        }

        /// <summary>
        /// Get email for specific user
        /// </summary>
        private string GetUserEmail(string userCode)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT ""E_Mail""
                    FROM OUSR
                    WHERE ""USER_CODE"" = '{userCode}'";

                oRecordset.DoQuery(query);

                if (!oRecordset.EoF)
                {
                    return oRecordset.Fields.Item("E_Mail").Value?.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting user email for {userCode}: {ex.Message}", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        public bool TestEmailConfiguration(string testRecipient)
        {
            try
            {
                Logger.Info($"Testing email configuration, sending to: {testRecipient}");

                string subject = "Test Email from Contract Management System";
                string body = @"<html><body>
                    <h2>Email Configuration Test</h2>
                    <p>This is a test email from the Contract Management Add-on.</p>
                    <p>If you receive this email, your email configuration is working correctly.</p>
                    <p><small>Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</small></p>
                    </body></html>";

                return SendEmail(new List<string> { testRecipient }, subject, body, true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error testing email configuration: {ex.Message}", ex);
                return false;
            }
        }

        #endregion
    }
}
