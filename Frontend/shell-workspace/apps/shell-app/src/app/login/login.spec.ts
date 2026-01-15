import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login';
import { AuthService } from '@frontend/auth';
import { ActivatedRoute } from '@angular/router';
import { provideCommonMocks } from '../../../../../test-utils/mocks';
import { of } from 'rxjs';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authSpy = jasmine.createSpyObj('AuthService', [
      'loginWithGoogle',
      'loginWithMicrosoft'
    ]);

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        ...provideCommonMocks,
        { provide: AuthService, useValue: authSpy },
        {
          provide: ActivatedRoute,
          useValue: { queryParams: of({ success: 'true' }) }
        }
      ]
    });

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show success message', () => {
    fixture.detectChanges();
    expect(component.loginMessage).toBe('Successfully Logged In!');
  });

  it('should login with Google', () => {
    component.loginWithGoogle();
    expect(authSpy.loginWithGoogle).toHaveBeenCalled();
  });

  it('should login with Microsoft', () => {
    component.loginWithMicrosoft();
    expect(authSpy.loginWithMicrosoft).toHaveBeenCalled();
  });
});
