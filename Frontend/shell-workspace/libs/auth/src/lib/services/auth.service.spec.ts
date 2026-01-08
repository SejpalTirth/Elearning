import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { AuthService } from './auth.service';
import { GatewayAuthService } from '@frontend/api';
import { API_BASE_URL } from '@frontend/core';
import { AuthUser } from '../models/auth-user.model';

describe('AuthService', () => {
  let service: AuthService;
  let gatewaySpy: jasmine.SpyObj<GatewayAuthService>;
  const mockApiBaseUrl = 'http://localhost:3000';

  beforeEach(() => {
    
    gatewaySpy = jasmine.createSpyObj('GatewayAuthService', [
      'getApiGatewayAuthMe',
      'postApiGatewayAuthRefresh',
      'postApiGatewayAuthLogout'
    ]);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: GatewayAuthService, useValue: gatewaySpy },
        { provide: API_BASE_URL, useValue: mockApiBaseUrl }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // =========================
  // GET ME
  // =========================
  it('should return current user from getMe', (done) => {
  const mockUser: AuthUser = { 
    userId: '123', 
    name: 'Alice', 
    email: 'alice@example.com',
    role: 'Admin' 
  };

  gatewaySpy.getApiGatewayAuthMe.and.returnValue(
    of(mockUser) as any
  );

  service.getMe().subscribe(result => {
    expect(result).toEqual(mockUser);
    done();
  });

  expect(gatewaySpy.getApiGatewayAuthMe).toHaveBeenCalledWith(
    jasmine.objectContaining({ withCredentials: true })
  );
});



  // =========================
  // REFRESH TOKENS
  // =========================
  it('should return true on successful refreshTokens', (done) => {
  // The body does not matter – success maps to true
  gatewaySpy.postApiGatewayAuthRefresh.and.returnValue(
    of({}) as any
  );

  service.refreshTokens().subscribe(result => {
    expect(result).toBeTrue();
    done();
  });

  expect(gatewaySpy.postApiGatewayAuthRefresh).toHaveBeenCalledWith(
  jasmine.anything()
);

});


  it('should return false on failed refreshTokens', (done) => {
    gatewaySpy.postApiGatewayAuthRefresh.and.returnValue(
      throwError(() => new Error('Network error'))
    );

    service.refreshTokens().subscribe(result => {
      expect(result).toBeFalse();
      done();
    });
  });

  // =========================
  // LOGOUT
  // =========================
  it('should call logout and redirect', () => {
  // Mock the logout API call
  gatewaySpy.postApiGatewayAuthLogout.and.returnValue(of({}) as any);

  // Spy on the protected redirect method
  const redirectSpy = spyOn<any>(service, 'redirectToHome');

  // Call logout
  service.logout();

  // Assert that the logout API was called
  expect(gatewaySpy.postApiGatewayAuthLogout).toHaveBeenCalledWith(jasmine.anything());

  // Assert that the redirect method was called
  expect(redirectSpy).toHaveBeenCalled();
});



});
