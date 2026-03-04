import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyCustomers } from './my-customers';

describe('MyCustomers', () => {
  let component: MyCustomers;
  let fixture: ComponentFixture<MyCustomers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyCustomers]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyCustomers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
