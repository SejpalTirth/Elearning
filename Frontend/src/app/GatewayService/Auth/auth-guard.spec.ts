import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

import { AuthGuard } from './auth-guard';
import { AuthService } from './auth.service';

class MockAuthService {
  access: string | null = null;
  refresh: string | null = null;

  getAccessToken(): any {
    return this.access;
  }

  getRefreshToken(): any {
    return this.refresh;
  }
}

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let router: Router;
  let auth: MockAuthService;

  beforeEach(() => {
    auth = new MockAuthService();

    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: auth }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    router = TestBed.inject(Router);

    spyOn(router, 'navigate');
  });

  // -------------------------------------------
  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  // -------------------------------------------
  it('should block and redirect to /login when BOTH tokens missing', () => {
    auth.access = null;
    auth.refresh = null;

    const result = guard.canActivate();

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  // -------------------------------------------
  it('should allow activation when accessToken exists', () => {
    auth.access = 'abc123';
    auth.refresh = null;

    const result = guard.canActivate();

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  // -------------------------------------------
  it('should allow activation when refreshToken exists', () => {
    auth.access = null;
    auth.refresh = 'refresh123';

    const result = guard.canActivate();

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
