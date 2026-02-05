import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login';
import { ActivatedRoute } from '@angular/router';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { AuthService } from '../../../service/auth-service';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { FormsModule } from '@angular/forms';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuth: any;
  let mockLoader: any;
  let queryParamsSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    queryParamsSubject = new BehaviorSubject({});
    
    mockAuth = {
      loginWithGoogle: jasmine.createSpy('loginWithGoogle'),
      loginWithMicrosoft: jasmine.createSpy('loginWithMicrosoft'),
      localLogin: jasmine.createSpy('localLogin').and.returnValue(of({ success: true }))
    };

    mockLoader = {
      show: jasmine.createSpy('show'),
      hide: jasmine.createSpy('hide')
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent, FormsModule],
      providers: [
        { provide: AuthService, useValue: mockAuth },
        { provide: LoadingService, useValue: mockLoader },
        {
          provide: ActivatedRoute,
          useValue: { queryParams: queryParamsSubject.asObservable() }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should show success message when success query param is true', () => {
    queryParamsSubject.next({ success: 'true' });
    fixture.detectChanges();
    expect(component.loginMessage).toBe('Successfully Logged In!');
  });

  it('should call loginWithGoogle and show loader', () => {
    component.loginWithGoogle();
    expect(mockLoader.show).toHaveBeenCalled();
    expect(mockAuth.loginWithGoogle).toHaveBeenCalled();
  });

  it('should call loginWithMicrosoft and show loader', () => {
    component.loginWithMicrosoft();
    expect(mockLoader.show).toHaveBeenCalled();
    expect(mockAuth.loginWithMicrosoft).toHaveBeenCalled();
  });

  describe('loginWithEmail()', () => {
    it('should set error if fields are empty', () => {
      component.email = '';
      component.password = '';
      component.loginWithEmail();
      expect(component.error).toBe('Please enter both email and password');
      expect(mockAuth.localLogin).not.toHaveBeenCalled();
    });

    it('should show error if response success is false', () => {
      mockAuth.localLogin.and.returnValue(of({ success: false, message: 'Invalid credentials' }));
      component.email = 'test@test.com';
      component.password = '123';
      
      component.loginWithEmail();

      expect(component.error).toBe('Invalid credentials');
      expect(mockLoader.hide).toHaveBeenCalled();
    });

    it('should handle API error stream', () => {
      mockAuth.localLogin.and.returnValue(throwError(() => ({ error: { message: 'Server Error' } })));
      component.email = 'test@test.com';
      component.password = '123';

      component.loginWithEmail();

      expect(component.error).toBe('Server Error');
      expect(mockLoader.hide).toHaveBeenCalled();
    });
  });
});