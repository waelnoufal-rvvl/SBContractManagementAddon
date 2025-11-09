using System;
using SAPbobsCOM;

namespace ContractManagementAddon.SAP
{
    /// <summary>
    /// Extensions and compatibility fixes for SAP B1 SDK types
    /// Provides missing enum values and type aliases for different SAP B1 versions
    /// </summary>
    public static class SAPTypesExtensions
    {
        // SAP B1 Object Type Constants
        // These numeric values are used when enum values are not available
        public const int OBJ_TYPE_APPROVAL_REQUESTS = 234; // WaitingApprovalRequest object type

        // UDO object types are dynamically assigned starting from 1470000000
        // We don't use a fixed constant but query it dynamically
    }

    /// <summary>
    /// Compatibility wrapper for ApprovalRequests object
    /// Maps to the correct SAP B1 SDK type based on version
    /// </summary>
    public interface IApprovalRequests
    {
        int ApprovalTemplatesID { get; set; }
        BoObjectTypes ObjectType { get; set; }
        int ObjectEntry { get; set; }
        BoYesNoEnum IsDraft { get; set; }
        BoApprovalRequestStatusEnum Status { get; set; }
        string Remarks { get; set; }
        int OriginatorID { get; set; }
        int Add();
        bool GetByKey(int code);
        int Update();
    }

    /// <summary>
    /// Factory for creating approval request objects
    /// Handles version-specific implementations
    /// </summary>
    public static class ApprovalRequestFactory
    {
        public static object CreateApprovalRequest(Company company)
        {
            try
            {
                // Try to get the approval request object using the numeric type
                // This is more compatible across different SAP B1 versions
                return company.GetBusinessObject((BoObjectTypes)SAPTypesExtensions.OBJ_TYPE_APPROVAL_REQUESTS);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(
                    "ApprovalRequests object is not available in this SAP B1 version. " +
                    "Please use SAP B1 9.0 or later for native approval workflow support.", ex);
            }
        }

        public static BoObjectTypes GetApprovalRequestsObjectType()
        {
            // Return the numeric constant cast to BoObjectTypes
            return (BoObjectTypes)SAPTypesExtensions.OBJ_TYPE_APPROVAL_REQUESTS;
        }

        public static BoObjectTypes GetUserDefinedObjectType(string udoCode)
        {
            // UDOs don't have a fixed BoObjectTypes value
            // The object type is assigned dynamically when the UDO is registered
            // We need to query it from the UDO metadata
            // For now, return a placeholder that will be resolved at runtime
            return (BoObjectTypes)(-1); // Placeholder - should be resolved dynamically
        }
    }
}
