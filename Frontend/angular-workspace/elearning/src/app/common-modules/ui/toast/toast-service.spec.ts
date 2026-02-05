import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ToastService } from './toast-service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ToastService]
    });
    service = TestBed.inject(ToastService);
  });

  it('should add a success toast and auto-remove it after 3000ms', fakeAsync(() => {
    service.showSuccess('Success Message');
    
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0]).toEqual({ type: 'success', message: 'Success Message' });

    tick(3000);
    expect(service.toasts.length).toBe(0);
  }));

  it('should add an error toast', () => {
    service.showError('Error Message');
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].type).toBe('error');
  });

  it('should add an info toast', () => {
    service.showInfo('Info Message');
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].type).toBe('info');
  });

  it('should handle multiple toasts in sequence', fakeAsync(() => {
    service.showSuccess('First');
    tick(1000);
    service.showError('Second');
    
    expect(service.toasts.length).toBe(2);

    tick(2000); 
    expect(service.toasts.length).toBe(1);
    expect(service.toasts[0].message).toBe('Second');

    tick(1000);
    expect(service.toasts.length).toBe(0);
  }));
});