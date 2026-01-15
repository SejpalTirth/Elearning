import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { API_BASE_URL } from '@frontend/core';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthStateService } from '@frontend/auth';
import { of } from 'rxjs';

describe('App', () => {

  beforeAll(() => {
    // Global alert stub (Angular 20 compatible)
    spyOn(window, 'alert').and.callFake(() => {});
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'http://localhost:3000' },

        // CRITICAL: mock AuthStateService
        {
          provide: AuthStateService,
          useValue: {
            initialize: jasmine.createSpy('initialize'),
            user$: of(null),
            status$: of('idle')
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
