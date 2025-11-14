import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Redirectpage } from './redirectpage';

describe('Redirectpage', () => {
  let component: Redirectpage;
  let fixture: ComponentFixture<Redirectpage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Redirectpage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Redirectpage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
