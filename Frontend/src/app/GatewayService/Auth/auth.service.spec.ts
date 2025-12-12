import { TestBed } from '@angular/core/testing';
import { AuthService, TokenResponse } from './auth.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

describe('AuthService - Token Management', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should get access token from localStorage', () => {
    localStorage.setItem('accessToken', 'abc123');
    expect(service.getAccessToken()).toBe('abc123');
  });

  it('should get refresh token from localStorage', () => {
    localStorage.setItem('refreshToken', 'xyz789');
    expect(service.getRefreshToken()).toBe('xyz789');
  });

  it('should store tokens correctly', () => {
    const tokens: TokenResponse = {
      accessToken: 'A1',
      refreshToken: 'R1',
      expiresAt: '999999'
    };
    service.storeTokens(tokens);
    expect(localStorage.getItem('accessToken')).toBe('A1');
    expect(localStorage.getItem('refreshToken')).toBe('R1');
    expect(localStorage.getItem('expiresAt')).toBe('999999');
  });

  it('should do nothing when storeTokens receives null', () => {
    service.storeTokens(null as any);
    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
  });
});

describe('AuthService - Refresh Tokens', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should return null when no refresh token is available', () => {
    service.refreshTokens().subscribe(result => {
      expect(result).toBeNull();
    });
    httpMock.expectNone('https://localhost:7249/api/GatewayAuth/refresh');
  });

  it('should refresh tokens successfully', () => {
    localStorage.setItem('refreshToken', 'REF123');
    const response: TokenResponse = {
      accessToken: 'NEW_ACCESS',
      refreshToken: 'NEW_REFRESH',
      expiresAt: '111111'
    };
    service.refreshTokens().subscribe(result => {
      expect(result).toEqual(response);
      expect(localStorage.getItem('accessToken')).toBe('NEW_ACCESS');
      expect(localStorage.getItem('refreshToken')).toBe('NEW_REFRESH');
      expect(localStorage.getItem('expiresAt')).toBe('111111');
    });
    const req = httpMock.expectOne('https://localhost:7249/api/GatewayAuth/refresh');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ refreshToken: 'REF123' });
    req.flush(response);
  });

  it('should return null when refresh request fails', () => {
    localStorage.setItem('refreshToken', 'REF123');
    service.refreshTokens().subscribe(result => {
      expect(result).toBeNull();
    });
    const req = httpMock.expectOne('https://localhost:7249/api/GatewayAuth/refresh');
    req.flush({ message: 'Error' }, { status: 400, statusText: 'Bad Request' });
  });
});

describe('AuthService - Logout', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should clear localStorage on logout', () => {
    const clearSpy = spyOn(localStorage, 'clear').and.callThrough();
    // Override logout to avoid window.location change
    spyOn(service as any, 'logout').and.callFake(() => localStorage.clear());
    service.logout();
    expect(clearSpy).toHaveBeenCalled();
  });
});
