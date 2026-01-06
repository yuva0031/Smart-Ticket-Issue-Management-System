import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminDashboardNav } from './admin-dashboard-nav';

describe('AdminDashboardNav', () => {
  let component: AdminDashboardNav;
  let fixture: ComponentFixture<AdminDashboardNav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminDashboardNav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminDashboardNav);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
