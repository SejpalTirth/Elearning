import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AuthStateService } from './auth-state.service';
import { AuthService } from './auth.service';
import { AuthUser } from '../models/auth-user.model';
import { provideCommonMocks } from './../../../../../test-utils/mocks'

describe('AuthStateService', () => {
  let service: AuthStateService;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getMe']);

    TestBed.configureTestingModule({
      providers: [
        AuthStateService,
        { provide: AuthService, useValue: authServiceSpy },
        ...provideCommonMocks
      ]
    });

    service = TestBed.inject(AuthStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ================================
  // Test for initialize & loading user
  // ================================
  it('should set user and status to authenticated on successful getMe', (done) => {
    const mockUser: AuthUser = { 
      userId: '123', 
      name: 'Alice', 
      email: 'alice@example.com', 
      role: 'Admin' 
    };

    authServiceSpy.getMe.and.returnValue(of(mockUser));

    service.initialize();

    service.user$.subscribe(user => {
      expect(user).toEqual(mockUser);
      done();
    });

    service.status$.subscribe(status => {
      expect(status).toBe('authenticated');
    });
  });

  it('should set status to idle if getMe fails', (done) => {
    authServiceSpy.getMe.and.returnValue(throwError(() => new Error('error')));

    service.initialize();

    service.user$.subscribe(user => {
      expect(user).toBeNull();
      done();
    });

    service.status$.subscribe(status => {
      expect(status).toBe('idle');
    });
  });

  // ================================
  // Test for user helpers
  // ================================
  it('should return the correct user and status helpers', () => {
    const mockUser: AuthUser = { 
      userId: '123', 
      name: 'Alice', 
      email: 'alice@example.com', 
      role: 'Admin' 
    };

    authServiceSpy.getMe.and.returnValue(of(mockUser));
    service.initialize();

    expect(service.isLoggedIn).toBeTrue();
    expect(service.role).toBe('Admin');
    expect(service.userId).toBe('123');
    expect(service.user).toEqual(mockUser);
  });

  // ================================
  // Test for clear method
  // ================================
  it('should clear user and reset status to idle', () => {
  const mockUser: AuthUser = { 
    userId: '123', 
    name: 'Alice', 
    email: 'alice@example.com', 
    role: 'Admin'
  };

  authServiceSpy.getMe.and.returnValue(of(mockUser));

  service.initialize();

  service.clear();

  service.status$.subscribe(status => {
    expect(status).toBe('idle');
  });
});

});
