import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Policy, Claim, PolicyApplication, PolicyProduct } from '../models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerApiService {
  private apiUrl = `${environment.apiUrl}/Customer`;

  constructor(private http: HttpClient) {}

  getProducts(): Observable<PolicyProduct[]> {
    return this.http.get<PolicyProduct[]>(`${this.apiUrl}/products`);
  }

  getApplications(): Observable<PolicyApplication[]> {
    return this.http.get<PolicyApplication[]>(`${this.apiUrl}/applications`);
  }

  getPolicies(): Observable<Policy[]> {
    return this.http.get<Policy[]>(`${this.apiUrl}/policies`);
  }

  getClaims(): Observable<Claim[]> {
    return this.http.get<Claim[]>(`${this.apiUrl}/claims`);
  }

  applyProduct(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/apply-product`, request);
  }

  payPremium(policyId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/pay-premium/${policyId}`, {});
  }

  submitClaim(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit-claim`, request);
  }
}
