import { TestBed } from '@angular/core/testing';

import { CustomerApi } from './customer-api';

describe('CustomerApi', () => {
  let service: CustomerApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
