import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth-service';
import { GatewayAuthService } from 'api';
import { DOCUMENT } from '@angular/common';
import { of, throwError } from 'rxjs';
import { environment } from '../../../environment/environment';

describe('AuthService', () => {
  let service: AuthService;
  let mockAuthClient: any;
  let mockDocument: any;

  beforeEach(() => {
    mockAuthClient = {
      getApiGatewayAuthMe: jasmine.createSpy('getApiGatewayAuthMe'),
      postApiGatewayAuthRefresh: jasmine.createSpy('postApiGatewayAuthRefresh'),
      postApiGatewayAuthLocalRegister: jasmine.createSpy('postApiGatewayAuthLocalRegister'),
      postApiGatewayAuthLocalLogin: jasmine.createSpy('postApiGatewayAuthLocalLogin')
    };

    mockDocument = {
      defaultView: {
        location: {
          href: ''
        }
      }
    };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: GatewayAuthService, useValue: mockAuthClient },
        { provide: DOCUMENT, useValue: mockDocument }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should fetch current user data', (done) => {
    const mockUser = { userId: '1', email: 'test@test.com' };
    mockAuthClient.getApiGatewayAuthMe.and.returnValue(of(mockUser));

    service.getMe().subscribe(user => {
      expect(user).toEqual(mockUser as any);
      expect(mockAuthClient.getApiGatewayAuthMe).toHaveBeenCalledWith({ withCredentials: true });
      done();
    });
  });

  describe('Token Refresh', () => {
    it('should return true on successful refresh', (done) => {
      mockAuthClient.postApiGatewayAuthRefresh.and.returnValue(of({}));
      service.refreshTokens().subscribe(result => {
        expect(result).toBeTrue();
        done();
      });
    });

    it('should return false on refresh error', (done) => {
      mockAuthClient.postApiGatewayAuthRefresh.and.returnValue(throwError(() => new Error('Error')));
      service.refreshTokens().subscribe(result => {
        expect(result).toBeFalse();
        done();
      });
    });
  });

  describe('External Redirects', () => {
    it('should redirect to logout URL', () => {
      service.logout();
      const expected = `${environment.apiBaseUrl}/api/GatewayAuth/logout?redirectUrl=http%3A%2F%2Flocalhost%3A4200`;
      expect(mockDocument.defaultView.location.href).toBe(expected);
    });

    it('should redirect to Google login', () => {
      service.loginWithGoogle();
      expect(mockDocument.defaultView.location.href).toBe(`${environment.apiBaseUrl}/api/GatewayAuth/google-login`);
    });

    it('should redirect to Microsoft login', () => {
      service.loginWithMicrosoft();
      expect(mockDocument.defaultView.location.href).toBe(`${environment.apiBaseUrl}/api/GatewayAuth/microsoft-login`);
    });
  });

  describe('Local Auth', () => {
    it('should call register with encrypted password', () => {
      mockAuthClient.postApiGatewayAuthLocalRegister.and.returnValue(of({}));
      service.localRegister('test@test.com', 'secret123');
      const payload = mockAuthClient.postApiGatewayAuthLocalRegister.calls.mostRecent().args[0];
      expect(payload.password).not.toBe('secret123');
    });

    it('should call login with encrypted password', () => {
      mockAuthClient.postApiGatewayAuthLocalLogin.and.returnValue(of({}));
      service.localLogin('test@test.com', 'secret123');
      const payload = mockAuthClient.postApiGatewayAuthLocalLogin.calls.mostRecent().args[0];
      expect(payload.password).not.toBe('secret123');
    });
  });
});