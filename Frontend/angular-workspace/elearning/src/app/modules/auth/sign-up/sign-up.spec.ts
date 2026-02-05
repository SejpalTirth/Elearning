import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SignUpComponent } from './sign-up';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../service/auth-service';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { FormsModule } from '@angular/forms';
import { LOCATION_TOKEN } from '../../../common-modules/tokens/location.token';

describe('SignUpComponent', () => {
  let component: SignUpComponent;
  let fixture: ComponentFixture<SignUpComponent>;
  let mockAuth: any;
  let mockRouter: any;
  let mockLoader: any;
  let mockLocation: { href: string };

  beforeEach(async () => {
    mockLocation = { href: '' };

    mockAuth = {
      localRegister: jasmine.createSpy('localRegister').and.returnValue(
        of({ isNewUser: true, userId: 'user_123' })
      )
    };

    mockRouter = { navigate: jasmine.createSpy('navigate') };
    
    mockLoader = {
      show: jasmine.createSpy('show'),
      hide: jasmine.createSpy('hide')
    };

    await TestBed.configureTestingModule({
      imports: [SignUpComponent, FormsModule],
      providers: [
        { provide: AuthService, useValue: mockAuth },
        { provide: Router, useValue: mockRouter },
        { provide: LoadingService, useValue: mockLoader },
        { provide: LOCATION_TOKEN, useValue: mockLocation }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Validation', () => {
    it('should set error if passwords mismatch', () => {
      component.password = 'Pass123!';
      component.confirmPassword = 'Different123!';
      component.register();
      expect(component.error).toBe('Passwords do not match');
      expect(mockAuth.localRegister).not.toHaveBeenCalled();
    });

    it('should set error if password fails regex', () => {
      component.password = 'weak';
      component.confirmPassword = 'weak';
      component.register();
      expect(component.error).toContain('Password must be at least 8 characters');
      expect(mockAuth.localRegister).not.toHaveBeenCalled();
    });
  });

  describe('Registration Flow', () => {
    beforeEach(() => {
      component.email = 'test@example.com';
      component.password = 'StrongPass123!';
      component.confirmPassword = 'StrongPass123!';
    });

    it('should redirect to callback on successful registration', () => {
      component.register();

      expect(mockAuth.localRegister).toHaveBeenCalledWith('test@example.com', 'StrongPass123!');
      expect(mockLocation.href).toBe(
        'http://localhost:4200/gateway/auth/callback?isNewUser=true&userId=user_123'
      );
      expect(mockLoader.hide).toHaveBeenCalled();
    });

    it('should handle registration error', () => {
      mockAuth.localRegister.and.returnValue(
        throwError(() => ({ error: { message: 'User already exists' } }))
      );

      component.register();

      expect(component.error).toBe('User already exists');
      expect(mockLoader.hide).toHaveBeenCalled();
    });
  });

  it('should navigate to home on goToLogin', () => {
    component.goToLogin();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });
});