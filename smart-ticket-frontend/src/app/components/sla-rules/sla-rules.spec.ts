import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaRules } from './sla-rules';

describe('SlaRules', () => {
  let component: SlaRules;
  let fixture: ComponentFixture<SlaRules>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SlaRules]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SlaRules);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
