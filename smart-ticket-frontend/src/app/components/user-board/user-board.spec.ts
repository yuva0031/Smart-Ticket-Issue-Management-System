import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserBoard } from './user-board';

describe('UserBoard', () => {
  let component: UserBoard;
  let fixture: ComponentFixture<UserBoard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserBoard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserBoard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
