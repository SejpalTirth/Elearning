import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthCallback } from './auth-callback';

describe('AuthCallback', () => {
  let component: AuthCallback;
  let fixture: ComponentFixture<AuthCallback>;
  let router: Router;

  function mockParams(map: Record<string, string | null>) {
    spyOn(window as any, 'URLSearchParams').and.returnValue({
      get: (key: string) => map[key] ?? null
    });
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthCallback, RouterTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AuthCallback);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);

    localStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // NEW USER -----------------------------------
  it('should redirect new users to complete-profile', () => {
    mockParams({
      isNewUser: 'true',
      userId: '123',
      token: null,
      refresh: null
    });

    const nav = spyOn(router, 'navigate');

    component.ngOnInit();

    expect(nav).toHaveBeenCalledWith(
      ['/complete-profile'],
      { queryParams: { userId: '123' } }
    );
  });

  // EXISTING USER -----------------------------------
  it('should store tokens and redirect to home', fakeAsync(() => {
    mockParams({
      isNewUser: null,
      userId: null,
      token: 'abc123',
      refresh: 'xyz456'
    });

    const nav = spyOn(router, 'navigate');

    component.ngOnInit();

    expect(localStorage.getItem('accessToken')).toBe('abc123');
    expect(localStorage.getItem('refreshToken')).toBe('xyz456');

    tick(300);
    expect(nav).toHaveBeenCalledWith(['/home']);
  }));

  it('should convert spaces to + in refresh token', fakeAsync(() => {
    mockParams({
      token: 'aaa',
      refresh: 'bbb ccc' // raw value
    });

    const nav = spyOn(router, 'navigate');

    component.ngOnInit();

    expect(localStorage.getItem('refreshToken')).toBe('bbb+ccc');

    tick(300);
    expect(nav).toHaveBeenCalledWith(['/home']);
  }));

  // FALLBACK -----------------------------------
  it('should redirect to login when params missing', () => {
    mockParams({
      isNewUser: null,
      userId: null,
      token: null,
      refresh: null
    });

    const nav = spyOn(router, 'navigate');

    component.ngOnInit();

    expect(nav).toHaveBeenCalledWith(['/login']);
  });
});
