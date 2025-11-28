import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SharedHeaderComponent } from './header';

describe('Header', () => {
  let component: SharedHeaderComponent;
  let fixture: ComponentFixture<SharedHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SharedHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
