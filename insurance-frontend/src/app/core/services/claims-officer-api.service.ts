import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Claim } from '../models/insurance.models';

@Injectable({
  providedIn: 'root'
})
export class ClaimsOfficerApiService {
  private apiUrl = `${environment.apiUrl}/ClaimsOfficer`;

  // Cached observable — null means cache is busted and needs a fresh fetch
  private claimsCache$: Observable<Claim[]> | null = null;

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
    return this.http.get<any[]>(`${this.apiUrl}/policies`);
  }

  /** Bust the cache after any write operation */
  invalidateClaimsCache(): void {
    this.claimsCache$ = null;
  }

  startReview(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/start-review?claimId=${claimId}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }

  reviewClaim(claimId: string, approve: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/review?claimId=${claimId}&approve=${approve}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }

  settleClaim(claimId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/settle?claimId=${claimId}`, {}).pipe(
      tap(() => this.invalidateClaimsCache())
    );
  }
}
