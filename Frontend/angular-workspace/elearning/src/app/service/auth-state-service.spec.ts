import { TestBed } from '@angular/core/testing';
import { AuthStateService } from './auth-state-service';
import { AuthService } from './auth-service';
import { of, throwError, Subject } from 'rxjs';
import { AuthUser } from '../modules/auth/auth-user.model';

describe('AuthStateService', () => {
  let service: AuthStateService;
  let mockAuth: any;

  const mockUser: AuthUser = {
    userId: 'user_123',
    email: 'test@test.com',
    name: 'Test User',
    role: 'Admin'
  };

  beforeEach(() => {
    mockAuth = {
      getMe: jasmine.createSpy('getMe').and.returnValue(of(mockUser))
    };

    TestBed.configureTestingModule({
      providers: [
        AuthStateService,
        { provide: AuthService, useValue: mockAuth }
      ]
    });

    service = TestBed.inject(AuthStateService);
  });

  it('should have initial idle state', () => {
    expect(service.user).toBeNull();
    expect(service.isLoggedIn).toBeFalse();
    expect(service.role).toBeNull();
  });

  it('should load user and update state on initialize', () => {
    service.initialize();

    expect(mockAuth.getMe).toHaveBeenCalled();
    expect(service.user).toEqual(mockUser);
    expect(service.isLoggedIn).toBeTrue();
    expect(service.role).toBe('Admin');
    expect(service.userId).toBe('user_123');
  });

  it('should load user on login success', () => {
    service.onLoginSuccess();
    expect(mockAuth.getMe).toHaveBeenCalled();
    expect(service.isLoggedIn).toBeTrue();
  });

  it('should handle error by setting status to idle and keeping user null', (done) => {
    mockAuth.getMe.and.returnValue(throwError(() => new Error('Unauthorized')));
    
    service.initialize();

    expect(service.user).toBeNull();
    expect(service.isLoggedIn).toBeFalse();
    
    service.status$.subscribe(status => {
      if (status === 'idle') {
        expect(status).toBe('idle');
        done();
      }
    });
  });

  it('should clear state when clear is called', () => {
    service.initialize(); 
    expect(service.isLoggedIn).toBeTrue();

    service.clear();

    expect(service.user).toBeNull();
    expect(service.isLoggedIn).toBeFalse();
    expect(service.role).toBeNull();
  });

  it('should ignore older session responses (race condition check)', () => {
    const firstCallSubject = new Subject<AuthUser>();
    const secondCallSubject = new Subject<AuthUser>();

    mockAuth.getMe.and.returnValues(
      firstCallSubject.asObservable(), 
      secondCallSubject.asObservable()
    );

    service.initialize(); 
    
    service.onLoginSuccess(); 

    firstCallSubject.next(mockUser);
    firstCallSubject.complete();

    expect(service.isLoggedIn).toBeFalse();
    expect(service.user).toBeNull();

    const updatedUser = { ...mockUser, name: 'Updated Name' };
    secondCallSubject.next(updatedUser);
    secondCallSubject.complete();

    expect(service.isLoggedIn).toBeTrue();
    expect(service.user?.name).toBe('Updated Name');
  });
});