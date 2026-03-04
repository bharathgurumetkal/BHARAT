import { TestBed } from '@angular/core/testing';

import { ClaimsOfficerApi } from './claims-officer-api';

describe('ClaimsOfficerApi', () => {
  let service: ClaimsOfficerApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ClaimsOfficerApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
