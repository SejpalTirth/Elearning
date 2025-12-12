import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoadingOverlay } from './loading-overlay';

describe('LoadingOverlay', () => {
  let component: LoadingOverlay;
  let fixture: ComponentFixture<LoadingOverlay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadingOverlay]
    }).compileComponents();

    fixture = TestBed.createComponent(LoadingOverlay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ------------------------------------------------------------
  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // ------------------------------------------------------------
  it('should have visible = false by default', () => {
    expect(component.visible).toBeFalse();
  });

  // ------------------------------------------------------------
  it('should set visible = true when show() is called', () => {
    component.show();
    expect(component.visible).toBeTrue();
  });

  // ------------------------------------------------------------
  it('should set visible = false when hide() is called', () => {
    component.show();
    expect(component.visible).toBeTrue();  // Sanity check

    component.hide();
    expect(component.visible).toBeFalse();
  });
});
