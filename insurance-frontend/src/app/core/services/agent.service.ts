import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Commission, CustomerReport, Policy, PolicyApplication } from '../models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class AgentService {
  private apiUrl = `${environment.apiUrl}/Agent`;

  constructor(private http: HttpClient) {}

  getMyCustomers(): Observable<CustomerReport[]> {
    return this.http.get<CustomerReport[]>(`${this.apiUrl}/my-customers`);
  }

  getPolicies(status?: string): Observable<Policy[]> {
    let url = `${this.apiUrl}/policies`;
    if (status) url += `?status=${status}`;
    return this.http.get<Policy[]>(url);
  }

  getAssignedApplications(): Observable<PolicyApplication[]> {
    return this.http.get<PolicyApplication[]>(`${this.apiUrl}/assigned-applications`);
  }

  approveApplication(applicationId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/approve-application/${applicationId}`, {});
  }

  rejectApplication(applicationId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reject-application/${applicationId}`, {});
  }

  getDashboardSummary(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/dashboard-summary`);
  }

  getCustomerPolicies(customerId: string): Observable<Policy[]> {
    return this.http.get<Policy[]>(`${this.apiUrl}/customer/${customerId}/policies`);
  }

  getCommissions(): Observable<Commission[]> {
    return this.http.get<Commission[]>(`${this.apiUrl}/commissions`);
  }
}

