import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupportAgents } from './support-agents';

describe('SupportAgents', () => {
  let component: SupportAgents;
  let fixture: ComponentFixture<SupportAgents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupportAgents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupportAgents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
