# Insurance System - Database & Data Layer Documentation

## 1. Overview
The data layer of the Insurance System is powered by **MS SQL Server** and managed through **Entity Framework Core (EF Core)**. It uses a relational schema designed for high integrity, supporting complex insurance workflows and financial auditing.

---

## 2. Relational Schema Architecture

### 2.1 Identity & User Management
*   **Users Table**: Centralized storage for all system actors. Stores Username, Hashed Password (BCrypt), and Role.
*   **Agents Table**: One-to-one relationship with Users. Stores sales-specific metrics.
*   **Customers Table**: One-to-one relationship with Users. Stores policy ownership data.
*   **ClaimsOfficers Table**: Specialized staff for risk management.

### 2.2 Insurance Core Entities
*   **PolicyProducts**: The blueprint for insurance offerings. Contains `MonthlyPremium`, `MaximumCoverage`, and `Description`.
*   **Property**: Detailed attributes for insured assets (Market value, risk zones, security systems).
*   **Policies**: The binding contract linking a Customer to a PolicyProduct and a Property. Stores `EffectiveDate`, `ExpiryDate`, and `Status`.

### 2.3 Operations & Finance
*   **PolicyApplications**: The "Pending" state before a policy is active. Linked to an Agent for review.
*   **Claims**: Linked to a Policy. Stores `ClaimAmount`, `Reason`, and `Status` (Pending, UnderReview, Settled).
*   **Payments**: Audit trail for premiums paid by customers.
*   **Commissions**: Financial records linking an Agent to a Policy payout.

### 2.4 Support & Auditing
*   **AuditLogs**: Tracking of every state change in the system for regulatory compliance.
*   **Notifications**: Storage for user alerts and system messages.

---

## 3. Data Flow & Role Authorization Logic

| Role | Permissions | Primary Tables |
| :--- | :--- | :--- |
| **Admin** | Read/Write All | All Tables (esp. Products, Users) |
| **Agent** | Review/Read | Applications, My Customers, Commissions |
| **Customer**| Write (Claims/App) | Policies, Payments, Claims |
| **ClaimsOfficer**| Review/Settle | Claims, AI Reports |

---

## 4. Integration with AI Service (USP)
The database structure is designed to support **AI-driven decisions**.
1.  When a **Claim** is submitted, the system retrieves data from:
    *   `Claims` (Reason, Amount)
    *   `Policies` (Coverage)
    *   `Properties` (Value, Risk Zone, Security System)
2.  This dataset is exported to the **Python Flask Service**.
3.  The AI's verdict is then surfaced back through the API and can be saved in the `AuditLogs` for justification of settlement.

---

## 5. Security & Maintenance

### 5.1 Data Integrity
*   **Cascading Rules**: Carefully defined to prevent accidental deletion of financial history.
*   **Soft Deletes**: Standard implementation for critical entities like Users.

### 5.2 Performance
*   **Indexing**: Critical paths indexed on `PolicyId`, `UserId`, and `ClaimId` for fast retrieval during dashboard population.
*   **Migrations**: Managed via EF Core. To refresh the schema, use:
    ```bash
    dotnet ef database update
    ```

---

## 6. Functional & Non-Functional Requirements (Data)

*   **Functional**: Must accurately reflect current policy status and financial balances.
*   **Non-Functional**:
    *   **Durability**: ACID compliance via SQL Server.
    *   **Security**: Data-at-rest encryption (optional at DB level) and strictly hashed credentials.
    *   **Referential Integrity**: Enforcement of relationships between Agents, Customers, and Policies.
