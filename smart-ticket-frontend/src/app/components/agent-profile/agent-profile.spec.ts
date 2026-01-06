import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AgentSkillProfile } from './agent-profile';

describe('AgentProfile', () => {
  let component: AgentSkillProfile;
  let fixture: ComponentFixture<AgentSkillProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgentSkillProfile]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AgentSkillProfile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
