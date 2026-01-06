import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketsView } from './tickets-view';

describe('TicketsView', () => {
  let component: TicketsView;
  let fixture: ComponentFixture<TicketsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketsView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
