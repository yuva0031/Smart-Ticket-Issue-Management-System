import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAnalytics } from './user-analytics';

describe('UserAnalytics', () => {
  let component: UserAnalytics;
  let fixture: ComponentFixture<UserAnalytics>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserAnalytics]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserAnalytics);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
