import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageClaims } from './manage-claims';

describe('ManageClaims', () => {
  let component: ManageClaims;
  let fixture: ComponentFixture<ManageClaims>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageClaims]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageClaims);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
