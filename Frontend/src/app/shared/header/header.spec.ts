import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { SharedHeaderComponent } from './header';
import { RouterTestingModule } from '@angular/router/testing';

describe('SharedHeaderComponent', () => {
  let component: SharedHeaderComponent;
  let fixture: ComponentFixture<SharedHeaderComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedHeaderComponent, RouterTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(SharedHeaderComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  // ------------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ------------------------------------------------------------
  it('should set userName, role, and loggedIn when token is valid', () => {
    const payload = { name: 'Kira', role: 'Admin' };
    const base64 = btoa(JSON.stringify(payload));
    const token = `abc.${base64}.xyz`;
    localStorage.setItem('accessToken', token);

    component.ngOnInit();

    expect(component.userName).toBe('Kira');
    expect(component.role).toBe('Admin');
    expect(component.loggedIn).toBeTrue();
  });

  // ------------------------------------------------------------
  it('should fall back to email if name is missing in token', () => {
    const payload = { email: 'kira@example.com', role: 'User' };
    const base64 = btoa(JSON.stringify(payload));
    const token = `abc.${base64}.xyz`;
    localStorage.setItem('accessToken', token);

    component.ngOnInit();

    expect(component.userName).toBe('kira@example.com');
    expect(component.role).toBe('User');
    expect(component.loggedIn).toBeTrue();
  });

  // ------------------------------------------------------------
  it('should default userName to "User" if both name and email are missing', () => {
    const payload = { role: 'Guest' };
    const base64 = btoa(JSON.stringify(payload));
    const token = `abc.${base64}.xyz`;
    localStorage.setItem('accessToken', token);

    component.ngOnInit();

    expect(component.userName).toBe('User');
    expect(component.role).toBe('Guest');
    expect(component.loggedIn).toBeTrue();
  });

  // ------------------------------------------------------------
  it('should set loggedIn = false when token is invalid', () => {
    localStorage.setItem('accessToken', 'invalid.token');

    component.ngOnInit();

    expect(component.loggedIn).toBeFalse();
  });

  // ------------------------------------------------------------
  it('should toggle dropdownOpen', () => {
    expect(component.dropdownOpen).toBeFalse();

    component.toggleDropdown();
    expect(component.dropdownOpen).toBeTrue();

    component.toggleDropdown();
    expect(component.dropdownOpen).toBeFalse();
  });

  // ------------------------------------------------------------
  it('should remove token and navigate on logout', () => {
    localStorage.setItem('accessToken', 'test.token');

    const navigateSpy = spyOn(router, 'navigate');

    component.logout();

    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  });
});
