import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignedApplications } from './assigned-applications';

describe('AssignedApplications', () => {
  let component: AssignedApplications;
  let fixture: ComponentFixture<AssignedApplications>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignedApplications]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssignedApplications);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
