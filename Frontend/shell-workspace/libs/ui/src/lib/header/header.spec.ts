import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SharedHeaderComponent } from './header';
import { AuthStateService, AuthService } from '@frontend/auth';
import { ActivatedRoute, convertToParamMap, UrlSegment } from '@angular/router';
import { CommonModule } from '@angular/common';
import { provideCommonMocks } from './../../../../../test-utils/mocks';

describe('SharedHeaderComponent', () => {
  let component: SharedHeaderComponent;
  let fixture: ComponentFixture<SharedHeaderComponent>;

  let authStateMock: Partial<AuthStateService>;
  let authMock: Partial<AuthService>;
  let activatedRouteMock: Partial<ActivatedRoute>;

  beforeEach(async () => {
    //  PREVENT REAL PAGE NAVIGATION (correct way)
    spyOnProperty(Location.prototype, 'href', 'set').and.stub();

    authStateMock = {
      user: {
        name: 'Test User',
        email: 'test@test.com',
        userId: 'u1',
        role: 'Admin'
      },
      role: 'Admin',
      isLoggedIn: true,
      clear: jasmine.createSpy('clear')
    };

    authMock = {
      logout: jasmine.createSpy('logout')
    };

    activatedRouteMock = {
      snapshot: {
        url: [] as UrlSegment[],
        params: {},
        queryParams: {},
        fragment: '',
        data: {},
        outlet: 'primary',
        component: null,
        routeConfig: null,
        root: null as any,
        parent: null as any,
        firstChild: null as any,
        children: [],
        pathFromRoot: [],
        paramMap: convertToParamMap({}),
        queryParamMap: convertToParamMap({}),
        title: undefined
      }
    };

    await TestBed.configureTestingModule({
      imports: [CommonModule, SharedHeaderComponent],
      providers: [
        { provide: AuthStateService, useValue: authStateMock },
        { provide: AuthService, useValue: authMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
        ...provideCommonMocks
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SharedHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return the userName from authState', () => {
    expect(component.userName).toBe('Test User');
    authStateMock.user!.name = '';
    expect(component.userName).toBe('test@test.com');
  });

  it('should return the role from authState', () => {
    expect(component.role).toBe('Admin');
  });

  it('should return loggedIn from authState', () => {
    expect(component.loggedIn).toBe(true);
  });

  it('should toggle dropdownOpen', () => {
    expect(component.dropdownOpen).toBe(false);
    component.toggleDropdown();
    expect(component.dropdownOpen).toBe(true);
    component.toggleDropdown();
    expect(component.dropdownOpen).toBe(false);
  });

  it('should call logout and clear authState', () => {
    component.dropdownOpen = true;

    component.logout();

    expect(component.dropdownOpen).toBe(false);
    expect(authMock.logout).toHaveBeenCalled();
    expect(authStateMock.clear).toHaveBeenCalled();
  });
});
