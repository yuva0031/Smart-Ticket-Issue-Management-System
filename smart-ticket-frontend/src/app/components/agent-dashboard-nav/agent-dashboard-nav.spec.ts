import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AgentDashboardNav } from './agent-dashboard-nav';

describe('AgentDashboardNav', () => {
  let component: AgentDashboardNav;
  let fixture: ComponentFixture<AgentDashboardNav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgentDashboardNav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AgentDashboardNav);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
