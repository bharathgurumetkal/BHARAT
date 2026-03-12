import { Routes } from '@angular/router';
import { LandingComponent } from './features/landing/pages/landing/landing';
import { LoginComponent } from './features/auth/pages/login/login';
import { RegisterComponent } from './features/auth/pages/register/register';
import { MainLayoutComponent } from './shared/layout/main-layout/main-layout';
import { DashboardComponent } from './features/dashboard/pages/dashboard/dashboard';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { ProductListComponent } from './features/customer/pages/product-list/product-list';
import { PolicyListComponent } from './features/customer/pages/policy-list/policy-list';
import { ClaimListComponent } from './features/customer/pages/claim-list/claim-list';
import { ApplicationListComponent } from './features/customer/pages/application-list/application-list';
import { MyCustomersComponent } from './features/agent/pages/my-customers/my-customers';
import { AssignedApplicationsComponent } from './features/agent/pages/assigned-applications/assigned-applications';
import { AgentDashboardComponent } from './features/agent/pages/dashboard/dashboard';
import { AgentPoliciesComponent } from './features/agent/pages/policies/policies';
import { ProductManagementComponent } from './features/admin/pages/product-management/product-management';
import { ClaimsOfficerDashboardComponent } from './features/claims-officer/pages/dashboard/dashboard';
import { ClaimsListComponent } from './features/claims-officer/pages/claims-list/claims-list';
import { ClaimsOfficerLayoutComponent } from './features/claims-officer/layout/claims-officer-layout/claims-officer-layout';
import { OfficerPoliciesListComponent } from './features/claims-officer/pages/policies-list/policies-list';
import { AdminLayoutComponent } from './features/admin/layout/admin-layout/admin-layout';
import { AdminDashboardComponent } from './features/admin/pages/admin-dashboard/admin-dashboard';
import { AgentsListComponent } from './features/admin/pages/agents-list/agents-list';
import { AgentAnalyticsComponent } from './features/admin/pages/agent-analytics/agent-analytics';
import { ClaimsOfficersListComponent } from './features/admin/pages/claims-officers-list/claims-officers-list';
import { ApplicationsListComponent } from './features/admin/pages/applications-list/applications-list';
import { PoliciesListComponent } from './features/admin/pages/policies-list/policies-list';
import { AgentLayoutComponent } from './features/agent/layout/agent-layout/agent-layout';
import { NotificationsPageComponent } from './features/notifications/notifications.page';
import { AgentCommissionsComponent } from './features/agent/pages/commissions/commissions';
import { AdminClaimsComponent } from './features/admin/pages/admin-claims/admin-claims';
import { SystemLogsComponent } from './features/admin/pages/system-logs/system-logs';

// Error pages
import { NotFoundComponent } from './shared/pages/errors/not-found/not-found.component';
import { ServerErrorComponent } from './shared/pages/errors/server-error/server-error.component';
import { UnauthorizedComponent } from './shared/pages/errors/unauthorized/unauthorized.component';
import { ForbiddenComponent } from './shared/pages/errors/forbidden/forbidden.component';
import { NetworkErrorComponent } from './shared/pages/errors/network-error/network-error.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  // Authenticated Dashboard Layout
  { 
    path: 'dashboard', 
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'admin', redirectTo: '/admin/dashboard', pathMatch: 'full' },
      { path: 'customer', component: DashboardComponent, canActivate: [roleGuard], data: { role: 'Customer' } },
      { path: '', redirectTo: 'customer', pathMatch: 'full' }
    ]
  },
  
  // Feature Specific Routes
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', component: AdminDashboardComponent },
        { path: 'agents', component: AgentsListComponent },
        { path: 'agent-analytics', component: AgentAnalyticsComponent },
        { path: 'claims-officers', component: ClaimsOfficersListComponent },
        { path: 'products', component: ProductManagementComponent },
        { path: 'applications', component: ApplicationsListComponent },
        { path: 'policies', component: PoliciesListComponent },
        { path: 'claims', component: AdminClaimsComponent },
        { path: 'system-logs', component: SystemLogsComponent },
        { path: 'notifications', component: NotificationsPageComponent }
    ]
  },
  {
    path: 'agent',
    component: AgentLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { role: 'Agent' },
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', component: AgentDashboardComponent },
        { path: 'customers', component: MyCustomersComponent },
        { path: 'applications', component: AssignedApplicationsComponent },
        { path: 'policies', component: AgentPoliciesComponent },
        { path: 'commissions', component: AgentCommissionsComponent },
        { path: 'notifications', component: NotificationsPageComponent }
    ]
  },
  {
    path: 'customer',
    component: MainLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { role: 'Customer' },
    children: [
        { path: 'products', component: ProductListComponent },
        { path: 'applications', component: ApplicationListComponent },
        { path: 'policies', component: PolicyListComponent },
        { path: 'claims', component: ClaimListComponent },
        { path: 'notifications', component: NotificationsPageComponent }
    ]
  },
  {
    path: 'claims-officer',
    component: ClaimsOfficerLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { role: 'ClaimsOfficer' },
    children: [
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
        { path: 'dashboard', component: ClaimsOfficerDashboardComponent },
        { path: 'claims', component: ClaimsListComponent },
        { path: 'policies', component: OfficerPoliciesListComponent },
        { path: 'notifications', component: NotificationsPageComponent }
    ]
  },
  
  // ─── Error Pages (no guards — always accessible) ──────────
  { path: 'not-found',     component: NotFoundComponent },
  { path: 'server-error',  component: ServerErrorComponent },
  { path: 'unauthorized',  component: UnauthorizedComponent },
  { path: 'forbidden',     component: ForbiddenComponent },
  { path: 'network-error', component: NetworkErrorComponent },

  // Wildcard — must be last
  { path: '**', redirectTo: 'not-found' }
];
