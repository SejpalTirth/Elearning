import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  it('should show success toast', () => {
    service.showSuccess('Success message');
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].type).toBe('success');
    expect(service.toasts[0].message).toBe('Success message');
  });

  it('should show error toast', () => {
    service.showError('Error message');
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].type).toBe('error');
    expect(service.toasts[0].message).toBe('Error message');
  });

  it('should show info toast', () => {
    service.showInfo('Info message');
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].type).toBe('info');
    expect(service.toasts[0].message).toBe('Info message');
  });

  it('should automatically remove toast after 3 seconds', (done) => {
    service.showSuccess('Auto remove success');
    expect(service.toasts.length).toBe(1);

    setTimeout(() => {
      expect(service.toasts.length).toBe(0);
      done();
    }, 3000);
  });
});
