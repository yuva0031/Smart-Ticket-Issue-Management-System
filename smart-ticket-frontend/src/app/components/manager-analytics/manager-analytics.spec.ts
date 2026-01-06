import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagerAnalytics } from './manager-analytics';

describe('ManagerAnalytics', () => {
  let component: ManagerAnalytics;
  let fixture: ComponentFixture<ManagerAnalytics>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManagerAnalytics]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManagerAnalytics);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
