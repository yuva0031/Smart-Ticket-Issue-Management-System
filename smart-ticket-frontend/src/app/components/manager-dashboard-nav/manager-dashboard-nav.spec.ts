import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagerDashboardNav } from './manager-dashboard-nav';

describe('ManagerDashboardNav', () => {
  let component: ManagerDashboardNav;
  let fixture: ComponentFixture<ManagerDashboardNav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManagerDashboardNav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManagerDashboardNav);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
