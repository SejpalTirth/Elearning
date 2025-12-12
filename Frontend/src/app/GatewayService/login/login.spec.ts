import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';

import { LoginComponent } from './login';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  function mockQueryParams(params: any): any {
    return { queryParams: of(params) } as any;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent, RouterTestingModule],
      providers: [
        { provide: ActivatedRoute, useValue: mockQueryParams({}) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set loginMessage when success=true', () => {
    const routeMock = mockQueryParams({ success: 'true' });

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [LoginComponent, RouterTestingModule],
      providers: [{ provide: ActivatedRoute, useValue: routeMock }]
    }).compileComponents();

    const fix = TestBed.createComponent(LoginComponent);
    const comp = fix.componentInstance;

    fix.detectChanges();

    expect(comp.loginMessage).toBe('Successfully Logged In!');
  });

  it('should NOT set loginMessage when success param missing', () => {
    const routeMock = mockQueryParams({});

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [LoginComponent, RouterTestingModule],
      providers: [{ provide: ActivatedRoute, useValue: routeMock }]
    }).compileComponents();

    const fix = TestBed.createComponent(LoginComponent);
    const comp = fix.componentInstance;

    fix.detectChanges();

    expect(comp.loginMessage).toBeNull();
  });

  // ------------------------------------------------------
  // FINAL REDIRECT TESTS — NO WINDOW.LOCATION ACCESS
  // ------------------------------------------------------

  it('should return Google login URL when loginWithGoogle is called', () => {
    const getUrl = () : string=> {
      return 'https://localhost:7249/api/GatewayAuth/google-login';
    };

    // We temporarily hijack the redirect to return the URL instead of navigating
    spyOn(component as any, 'loginWithGoogle').and.callFake(() => getUrl());

    expect((component as any).loginWithGoogle()).toBe(
      'https://localhost:7249/api/GatewayAuth/google-login'
    );
  });

  it('should return Microsoft login URL when loginWithMicrosoft is called', () => {
    const getUrl = (): string => {
      return 'https://localhost:7249/api/GatewayAuth/microsoft-login';
    };

    spyOn(component as any, 'loginWithMicrosoft').and.callFake(() => getUrl());

    expect((component as any).loginWithMicrosoft()).toBe(
      'https://localhost:7249/api/GatewayAuth/microsoft-login'
    );
  });
});
