import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AgentAnalytics } from './agent-analytics';

describe('AgentAnalytics', () => {
  let component: AgentAnalytics;
  let fixture: ComponentFixture<AgentAnalytics>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgentAnalytics]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AgentAnalytics);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
