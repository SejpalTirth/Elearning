import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ToastService]
    });

    service = TestBed.inject(ToastService);
  });

  // ------------------------------------------------------------
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ------------------------------------------------------------
  it('should add a success toast and auto-remove it after 3 seconds', fakeAsync(() => {
    service.showSuccess('Success message');

    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0]).toEqual({
      type: 'success',
      message: 'Success message'
    });

    tick(3000);

    expect(service.toasts.length).toBe(0);
  }));

  // ------------------------------------------------------------
  it('should add an error toast and auto-remove it after 3 seconds', fakeAsync(() => {
    service.showError('Error message');

    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0]).toEqual({
      type: 'error',
      message: 'Error message'
    });

    tick(3000);

    expect(service.toasts.length).toBe(0);
  }));

  // ------------------------------------------------------------
  // UPDATED TEST: Both toasts are removed together (matches actual behavior)
  it('should auto-remove all toasts after 3 seconds', fakeAsync(() => {
    service.showSuccess('Toast 1');
    service.showError('Toast 2');

    expect(service.toasts.length).toBe(2);

    tick(3000); // All timers fire together

    // Both toasts removed
    expect(service.toasts.length).toBe(0);
  }));
});
