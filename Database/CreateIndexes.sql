-- Performance Indexes
-- Created automatically by add-on

CREATE NONCLUSTERED INDEX IX_CONTRACT_Status 
ON [@CONTRACT_HDR](U_Status);

CREATE NONCLUSTERED INDEX IX_CONTRACT_Customer 
ON [@CONTRACT_HDR](U_CustomerCode);

CREATE NONCLUSTERED INDEX IX_IPC_Contract 
ON [@IPC_HDR](U_ContractCode);
