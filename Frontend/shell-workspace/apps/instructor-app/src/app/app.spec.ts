import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthStateService, AuthService } from '@frontend/auth';
import { of } from 'rxjs';
import { provideCommonMocks } from './../../../../test-utils/mocks'
import { ActivatedRoute } from '@angular/router';
import { API_BASE_URL } from '@frontend/core';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        // HttpClient safety (still good practice)
        provideHttpClientTesting(),
        ...provideCommonMocks,

        // API token
        { provide: API_BASE_URL, useValue:  API_BASE_URL },
        { provide: ActivatedRoute, useValue: ActivatedRoute},

        // Stub AuthStateService
        {
          provide: AuthStateService,
          useValue: {
            initialize: jasmine.createSpy('initialize'),
            user$: of(null),
            status$: of('idle'),
            user: null
          }
        },

        // Stub AuthService as well
        {
          provide: AuthService,
          useValue: {
            loginWithGoogle: jasmine.createSpy('loginWithGoogle'),
            loginWithMicrosoft: jasmine.createSpy('loginWithMicrosoft'),
            logout: jasmine.createSpy('logout')
          }
        }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
