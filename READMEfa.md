# Insurance System - Frontend Technical Documentation

## 1. Introduction
The **Insurance Frontend** is a modern, responsive Single Page Application (SPA) built with **Angular 17+**. It provides a high-fidelity user interface tailored for four distinct user roles, ensuring a premium experience for employees and customers alike.

---

## 2. Technical Stack & USP

*   **Framework**: Angular 17 (utilizing Signals for reactive state management).
*   **Styling**: Modern CSS3 with Flexbox/Grid. Focus on "Glassmorphism" and high-end visual aesthetics.
*   **Visuals**: Dynamic charts using Chart.js/ngx-charts for management insights.
*   **State**: Reactive programming with RxJS and Angular Signals for optimized change detection.
*   **Communication**: REST Client using Angular's `HttpClient` with Interceptors for JWT handling.

---

## 3. Architecture & Role Modules

The application is structured using **Feature-Based Modularity** to optimize bundle size and enforce security boundaries.

### 3.1 Auth Module (`/features/auth`)
*   **Users**: All
*   **Purpose**: Handle registration for customers and login for all staff roles.
*   **Auth**: LocalStorage management of JWT tokens.

### 3.2 Customer Module (`/features/customer`)
*   **Users**: `Customer`
*   **Features**:
    *   **Product Discovery**: Visual cards displaying coverage details.
    *   **Application Stepper**: Multi-step guide for policy application.
    *   **Payment Integration**: Secure portal for premium payments.
    *   **My Policies**: Dashboard for active and expired coverage.

### 3.3 Agent Module (`/features/agent`)
*   **Users**: `Agent`
*   **Features**:
    *   **Customer CRM**: View and manage assigned clients.
    *   **Application Review**: Interface to vet pending customer requests.
    *   **Earnings Graph**: Visual representation of commissions earned.

### 3.4 Admin Module (`/features/admin`)
*   **Users**: `Admin`
*   **Features**:
    *   **Business Intelligence**: High-level revenue and claim charts.
    *   **User Management**: Interfaces to onboard agents and claims officers.
    *   **Product Factory**: Tool to create and price new insurance offerings.

### 3.5 Claims Officer Module (`/features/claims-officer`)
*   **Users**: `ClaimsOfficer`
*   **Features**:
    *   **Master Queue**: Prioritized list of claims pending review.
    *   **AI Insight Panel**: Visual breakdown of risk assessments fetched from the Python AI service.

---

## 4. Functional & Non-Functional Requirements

### 4.1 Functional
1.  **Role-Based Routing**: Automatic redirection based on JWT claims.
2.  **Document Upload**: Binary file handling for claim supporting evidence.
3.  **Real-time Notifications**: Angular signals-based notification system for system alerts (e.g., Application Approved).

### 4.2 Non-Functional
1.  **Performance**: Lazy-loaded modules for fast initial page load.
2.  **Accessibility**: Semantic HTML and ARIA labels.
3.  **UI/UX USP**: A "Premium Dashboard" feel with subtle micro-animations and smooth transitions between states.
4.  **Security**: Route guards (`canActivate`) prevent unauthorized access to restricted features.

---

## 5. Deployment & Execution
1.  Ensure the Backend is running at the configured `environment.ts` URL.
2.  Install dependencies: `npm install`.
3.  Run Dev Server: `ng serve`.
4.  The application will be available at `http://localhost:4200`.
