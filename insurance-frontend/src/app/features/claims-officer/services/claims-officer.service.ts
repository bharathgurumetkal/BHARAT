import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Claim } from '../../../core/models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class ClaimsOfficerService {
  private apiUrl = `${environment.apiUrl}/ClaimsOfficer`;

  // Cached observables — null means cache is busted and needs a fresh fetch
  private claimsCache$: Observable<Claim[]> | null = null;
  private policiesCache$: Observable<any[]> | null = null;

  constructor(private http: HttpClient) {}

  getClaims(): Observable<Claim[]> {
    if (!this.claimsCache$) {
      this.claimsCache$ = this.http.get<Claim[]>(`${this.apiUrl}/claims`).pipe(
        shareReplay(1)
      );
    }
    return this.claimsCache$;
  }

  getPolicies(): Observable<any[]> {
    if (!this.policiesCache$) {
      this.policiesCache$ = this.http.get<any[]>(`${this.apiUrl}/policies`).pipe(
        shareReplay(1)
      );
    }
    return this.policiesCache$;
  }

  /** Call this after any action that mutates claim data so next subscriber gets fresh data */
  invalidateClaimsCache(): void {
    this.claimsCache$ = null;
  }

  startReview(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/start-review?claimId=${claimId}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }

  reviewClaim(claimId: string, approve: boolean, remarks?: string): Observable<any> {
    const params: any = { approve };
    if (remarks) params.remarks = remarks;
    return this.http.post(`${this.apiUrl}/review?claimId=${claimId}&approve=${approve}${remarks ? '&remarks=' + encodeURIComponent(remarks) : ''}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }

  settleClaim(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/settle?claimId=${claimId}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }
}
