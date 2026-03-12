# Insurance System - Backend Technical Documentation

## 1. Overview
The **Insurance System Backend** is a high-performance, scalable web API built using **ASP.NET Core 8.0**. It follows the **Clean Architecture** (Onion Architecture) pattern to ensure a strict separation of concerns, testability, and maintainability.

### Architecture Layers:
1.  **Domain**: Core entities, value objects, and domain logic (Business heart).
2.  **Application**: Application logic, DTOs, Interfaces, and Services (Use Case layer).
3.  **Infrastructure**: Data access (EF Core), External integrations (HTTP Clients for AI Service), and security implementations (BCrypt).
4.  **API**: RESTful controllers, Middleware, and JWT Authentication (Delivery layer).

---

## 2. Core Modules & Controllers

The system leverages **Role-Based Access Control (RBAC)** to ensure users only access relevant endpoints.

### 2.1 Auth Controller (`/api/Auth`)
*   **Role**: Anonymous/General
*   **Purpose**: User onboarding and security token issuance.
*   **Endpoints**: `Login`, `Register`.
*   **Auth**: Public. Uses **BCrypt** for hashing and **JWT** for token generation.

### 2.2 Admin Controller (`/api/Admin`)
*   **Role**: `Admin`
*   **Purpose**: Global management and business intelligence.
*   **Key Controls**:
    *   Agent & Claims Officer onboarding.
    *   Policy Product creation (Terms, rates, coverage).
    *   Global Revenue and Claims reporting.
    *   Assigning Customers to Agents.
*   **USP**: AI Analytics dashboard integration.

### 2.3 Agent Controller (`/api/Agent`)
*   **Role**: `Agent`
*   **Purpose**: Sales management and customer relationship maintenance.
*   **Key Controls**:
    *   `Approve/Reject Application`: Vetting initial policy requests.
    *   `My Customers`: Viewing assigned customer portfolios.
    *   `Commissions`: Tracking earnings per successful sale.

### 2.4 Customer Controller (`/api/Customer`)
*   **Role**: `Customer`
*   **Purpose**: Self-service portal for policyholders.
*   **Key Controls**:
    *   `Apply Product`: Submitting new applications.
    *   `Pay Premium`: Payment gateway integration.
    *   `Submit Claim`: Filing for payouts with document upload.

### 2.5 Claims Officer Controller (`/api/ClaimsOfficer`)
*   **Role**: `ClaimsOfficer`
*   **Purpose**: Professional verification and settlement.
*   **Key Controls**:
    *   `Start Review`: Assigning a claim to themselves.
    *   `Settle/Reject`: Final financial disposition of claims.
*   **USP**: Reviewing AI Risk Scores provided by the Python Service.

---

## 3. Technology USP: Python Flask & Gemini AI Integration

A unique feature of this system is the integration with a separate **AI Microservice** (`ai-service`).

*   **Technology**: Python 3.10, Flask, Google Gemini Pro API.
*   **Purpose**: To provide "Intelligent Risk Assessment" during the claim process.
*   **How it works**:
    1.  The .NET Infrastructure layer sends a JSON payload of claim/policy data to the Flask service.
    2.  The Flask service prompts Gemini Pro with professional actuarial guidelines.
    3.  Gemini analyzes risk factors (e.g., Claim vs Coverage, Risk Zones, Security Systems).
    4.  The Backend receives a "Risk Assessment Report" which is displayed to the Claims Officer.
*   **Benefit**: Reduces fraud and speeds up legitimate payouts by providing decision support.

---

## 4. Requirement Specifications

### 4.1 Functional Requirements
*   **Identity Management**: Secure sign-up/sign-in.
*   **Product Lifecycle**: Create, update, and deactivate insurance products.
*   **Workflow Automation**: Transitions from Application -> Active Policy -> Claim -> Settlement.
*   **Financial Tracking**: Automated commission calculation and premium payment logs.

### 4.2 Non-Functional Requirements
*   **Security**: RBAC, JWT, BCrypt, SQL Injection protection via EF Core.
*   **Scalability**: Stateless controllers allow for horizontal scaling in containerized environments (Docker/K8s).
*   **Performance**: Middleware-based exception handling and optimized SQL queries.
*   **Auditing**: Every critical action is recorded in the `AuditLogs` table.

---

## 5. Development Setup
1.  Configure `appsettings.json` with your SQL Connection String.
2.  Enable the AI Service in the `ai-service` folder (`python app.py`).
3.  Run Migrations: `dotnet ef database update`.
4.  Launch API: `dotnet run --project Insurance.API`.
