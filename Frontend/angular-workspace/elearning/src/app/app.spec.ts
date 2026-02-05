import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { provideRouter } from '@angular/router';
import { AuthService } from './service/auth-service';
import { AuthStateService } from './service/auth-state-service';
import { of } from 'rxjs';

describe('App', () => {
  let mockAuth: any;
  let mockAuthState: any;

  beforeEach(async () => {
    mockAuth = {
      logout: jasmine.createSpy('logout')
    };

    mockAuthState = {
      user$: of(null),
      isLoggedIn: false,
      role: null,
      user: null
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuth },
        { provide: AuthStateService, useValue: mockAuthState }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const app = fixture.componentInstance;
    expect(app['title']()).toContain('elearning');
  });
});