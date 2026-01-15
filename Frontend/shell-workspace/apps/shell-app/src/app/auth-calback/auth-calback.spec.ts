import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AuthCallback } from './auth-calback';
import { AuthStateService } from '@frontend/auth';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { provideCommonMocks } from '../../../../../test-utils/mocks';

describe('AuthCallback Component', () => {
  let fixture: ComponentFixture<AuthCallback>;
  let authStateSpy: jasmine.SpyObj<AuthStateService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authStateSpy = jasmine.createSpyObj(
      'AuthStateService',
      ['initialize'],
      {
        user$: of({
          userId: '123',
          name: 'Temp',
          email: 'test@gmail.com',
          role: 'Instructor'
        })
      }
    );

    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [AuthCallback],
      providers: [
        ...provideCommonMocks,
        { provide: AuthStateService, useValue: authStateSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    fixture = TestBed.createComponent(AuthCallback);

    //  CRITICAL: stop real browser redirects
    spyOn<any>(fixture.componentInstance, 'redirectByRole').and.stub();
  });

  it('should initialize auth state on init', () => {
    fixture.detectChanges();
    expect(authStateSpy.initialize).toHaveBeenCalled();
  });
});
