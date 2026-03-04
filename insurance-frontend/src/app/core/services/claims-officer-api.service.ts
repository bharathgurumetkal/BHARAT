import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Claim } from '../models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class ClaimsOfficerApiService {
  private apiUrl = `${environment.apiUrl}/ClaimsOfficer`;

  constructor(private http: HttpClient) {}
  
  getClaims(): Observable<Claim[]> {
    return this.http.get<Claim[]>(`${this.apiUrl}/claims`);
  }

  getPolicies(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/policies`);
  }

  startReview(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/start-review?claimId=${claimId}`, {});
  }

  reviewClaim(claimId: string, approve: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/review?claimId=${claimId}&approve=${approve}`, {});
  }

  settleClaim(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/settle?claimId=${claimId}`, {});
  }
}
