import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserDashboardNav } from './user-dashboard-nav';

describe('UserDashboardNav', () => {
  let component: UserDashboardNav;
  let fixture: ComponentFixture<UserDashboardNav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserDashboardNav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserDashboardNav);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
