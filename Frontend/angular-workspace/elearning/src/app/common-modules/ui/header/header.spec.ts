import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HeaderComponent } from './header';
import { AuthService } from '../../../service/auth-service';
import { AuthStateService } from '../../../service/auth-state-service';
import { provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { AuthUser } from '../../../modules/auth/auth-user.model';

describe('HeaderComponent', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;
  let mockAuth: any;
  let mockAuthState: any;

  const userSubject = new BehaviorSubject<AuthUser | null>(null);

  const mockUser: AuthUser = {
    userId: 'u1',
    email: 'test@example.com',
    name: 'John Doe',
    role: 'Student'
  };

  beforeEach(async () => {
    mockAuth = {
      logout: jasmine.createSpy('logout')
    };

    mockAuthState = {
      user$: userSubject.asObservable(),
      clear: jasmine.createSpy('clear'),
      get isLoggedIn() { return !!userSubject.value; },
      get user() { return userSubject.value; },
      get role() { return userSubject.value?.role ?? null; }
    };

    await TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuth },
        { provide: AuthStateService, useValue: mockAuthState }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should show "User" when no user is logged in', () => {
    userSubject.next(null);
    expect(component.loggedIn).toBeFalse();
    expect(component.userName).toBe('User');
  });

  it('should show user name when logged in', () => {
    userSubject.next(mockUser);
    expect(component.loggedIn).toBeTrue();
    expect(component.userName).toBe('John Doe');
  });

  it('should fallback to email if name is missing', () => {
    userSubject.next({ ...mockUser, name: '' });
    expect(component.userName).toBe('test@example.com');
  });

  it('should toggle dropdown state', () => {
    expect(component.dropdownOpen).toBeFalse();
    component.toggleDropdown();
    expect(component.dropdownOpen).toBeTrue();
    component.toggleDropdown();
    expect(component.dropdownOpen).toBeFalse();
  });

  it('should perform logout actions', () => {
    component.dropdownOpen = true;
    component.logout();

    expect(component.dropdownOpen).toBeFalse();
    expect(mockAuth.logout).toHaveBeenCalled();
    expect(mockAuthState.clear).toHaveBeenCalled();
  });

  it('should expose the user role', () => {
    userSubject.next(mockUser);
    expect(component.role).toBe('Student');
  });
});