import { TestBed } from '@angular/core/testing';
import { AuthGuard } from './auth-guard';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let router: Router;
  let mockStorage: Record<string, string> = {};

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [AuthGuard]
    });

    guard = TestBed.inject(AuthGuard);
    router = TestBed.inject(Router);

    // localStorage mock
    spyOn(localStorage, 'getItem').and.callFake((key: string) => {
      return mockStorage[key] || null;
    });

    // spy navigate
    spyOn(router, 'navigate');
    mockStorage = {};
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should block and redirect when no token', () => {
    mockStorage = {};

    const result = guard.canActivate();

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should allow activation when token exists', () => {
    mockStorage = { token: 'abc123' };

    const result = guard.canActivate();

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
