import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerReport, ClaimsReport, RevenueReport, AgentPerformance, AiAnalytics, AgentPerformanceAnalytics } from '../models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private apiUrl = `${environment.apiUrl}/Admin`;

  constructor(private http: HttpClient) {}

  getCustomers(): Observable<CustomerReport[]> {
    return this.http.get<CustomerReport[]>(`${this.apiUrl}/customers`);
  }

  getAgents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/agents`);
  }

  getClaimsOfficers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/claimsofficers`);
  }

  getProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/products`);
  }

  getApplications(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/applications`);
  }

  getPolicies(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/policies`);
  }

  addAgent(agent: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-agent`, agent);
  }

  addClaimsOfficer(officer: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add-claimsofficer`, officer);
  }

  assignCustomer(customerId: string, agentId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign-customer`, {}, {
      params: { customerId, agentId }
    });
  }

  getClaimsReport(): Observable<ClaimsReport[]> {
    return this.http.get<ClaimsReport[]>(`${this.apiUrl}/claims-report`);
  }

  getRevenueReport(): Observable<RevenueReport[]> {
    return this.http.get<RevenueReport[]>(`${this.apiUrl}/revenue-report`);
  }

  getAgentPerformance(): Observable<AgentPerformance[]> {
    return this.http.get<AgentPerformance[]>(`${this.apiUrl}/agent-performance`);
  }

  createProduct(product: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create-product`, product);
  }

  assignAgentToApplication(applicationId: string, agentId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign-agent-to-application/${applicationId}`, { agentId });
  }

  getAiAnalytics(): Observable<AiAnalytics> {
    return this.http.get<AiAnalytics>(`${this.apiUrl}/ai-analytics`);
  }

  getAgentPerformanceAnalytics(): Observable<AgentPerformanceAnalytics[]> {
    return this.http.get<AgentPerformanceAnalytics[]>(`${this.apiUrl}/agent-performance-analytics`);
  }

  getAllClaims(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/claims-report`);
  }

  getAdminClaims(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/claims`);
  }

  assignOfficerToClaim(claimId: string, officerUserId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign-officer-to-claim/${claimId}`, { officerUserId });
  }
}
