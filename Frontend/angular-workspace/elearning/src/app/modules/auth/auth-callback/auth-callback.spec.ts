import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AuthCallback } from './auth-callback';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { BehaviorSubject, throwError } from 'rxjs';

describe('AuthCallback', () => {
  let component: AuthCallback;
  let fixture: ComponentFixture<AuthCallback>;
  let mockRouter: any;
  let mockAuthState: any;
  let userSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    userSubject = new BehaviorSubject<any>(null);

    mockAuthState = {
      initialize: jasmine.createSpy('initialize'),
      user$: userSubject.asObservable(),
      user: null
    };

    await TestBed.configureTestingModule({
      imports: [AuthCallback],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState }
      ]
    }).compileComponents();
  });

  const setupMockUrl = (paramsObj: { [key: string]: string | null }) => {
    spyOn(URLSearchParams.prototype, 'get').and.callFake((key: string) => {
      return paramsObj[key] ?? null;
    });
  };

  it('should navigate to complete-profile for new users', () => {
    setupMockUrl({ isNewUser: 'true', userId: 'user_123' });

    fixture = TestBed.createComponent(AuthCallback);
    fixture.detectChanges();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/complete-profile'], {
      queryParams: { userId: 'user_123' },
      replaceUrl: true
    });
  });

  it('should navigate to home for existing users when auth state initializes', () => {
    setupMockUrl({ isNewUser: 'false' });

    fixture = TestBed.createComponent(AuthCallback);
    fixture.detectChanges();

    userSubject.next({ userId: 'user_456' });

    expect(mockAuthState.initialize).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('should navigate to root if timeout reaches 4 seconds without user', fakeAsync(() => {
    setupMockUrl({});

    fixture = TestBed.createComponent(AuthCallback);
    fixture.detectChanges();

    tick(4000);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  }));

  it('should not navigate to root after 4 seconds if user is present', fakeAsync(() => {
    setupMockUrl({});

    fixture = TestBed.createComponent(AuthCallback);
    fixture.detectChanges();

    mockAuthState.user = { userId: 'user_123' };
    userSubject.next(mockAuthState.user);

    tick(4000);

    expect(mockRouter.navigate).not.toHaveBeenCalledWith(['/']);
  }));
});