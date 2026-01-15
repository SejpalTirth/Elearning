import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CompleteProfileComponent } from './complete-profile';
import { ActivatedRoute, Router } from '@angular/router';
import { UserFacade } from '@frontend/core';
import { ToastService } from '@frontend/ui';
import { of, throwError } from 'rxjs';

describe('CompleteProfileComponent', () => {
  let fixture: ComponentFixture<CompleteProfileComponent>;
  let component: CompleteProfileComponent;

  let routerSpy: jasmine.SpyObj<Router>;
  let userFacadeSpy: jasmine.SpyObj<UserFacade>;
  let toastSpy: jasmine.SpyObj<ToastService>;

  const routeWithUserId = {
    snapshot: {
      queryParamMap: {
        get: () => '123'
      }
    }
  };

  const routeWithoutUserId = {
    snapshot: {
      queryParamMap: {
        get: () => null
      }
    }
  };

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    userFacadeSpy = jasmine.createSpyObj('UserFacade', ['completeProfile']);
    toastSpy = jasmine.createSpyObj('ToastService', ['showError', 'showInfo']);

    TestBed.configureTestingModule({
      imports: [CompleteProfileComponent],
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: UserFacade, useValue: userFacadeSpy },
        { provide: ToastService, useValue: toastSpy },
        { provide: ActivatedRoute, useValue: routeWithUserId }
      ]
    });

    fixture = TestBed.createComponent(CompleteProfileComponent);
    component = fixture.componentInstance;
  });

  // -------------------------
  // INIT
  // -------------------------

  it('should redirect to login if userId is missing', () => {
    TestBed.resetTestingModule();

    TestBed.configureTestingModule({
      imports: [CompleteProfileComponent],
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: UserFacade, useValue: userFacadeSpy },
        { provide: ToastService, useValue: toastSpy },
        { provide: ActivatedRoute, useValue: routeWithoutUserId }
      ]
    });

    const localFixture = TestBed.createComponent(CompleteProfileComponent);
    localFixture.detectChanges();

    expect(toastSpy.showError).toHaveBeenCalledWith(
      'User ID missing. Please login again.'
    );
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  // -------------------------
  // VALIDATION
  // -------------------------

  it('should show error if name is empty', () => {
    fixture.detectChanges();

    component.name = '';
    component.saveProfile();

    expect(toastSpy.showError).toHaveBeenCalledWith('Please enter your name');
    expect(userFacadeSpy.completeProfile).not.toHaveBeenCalled();
  });

  it('should show error for invalid role', () => {
    fixture.detectChanges();

    component.name = 'John';
    component.roleId = 999;

    component.saveProfile();

    expect(toastSpy.showError).toHaveBeenCalledWith('Invalid role selected');
    expect(userFacadeSpy.completeProfile).not.toHaveBeenCalled();
  });

  // -------------------------
  // SUCCESS FLOW
  // -------------------------

  it('should submit profile and redirect on success', () => {
    userFacadeSpy.completeProfile.and.returnValue(of(void 0));

    fixture.detectChanges();

    component.name = 'John';
    component.roleId = 2; // Instructor

    component.saveProfile();

    expect(userFacadeSpy.completeProfile).toHaveBeenCalledWith({
      userId: '123',
      name: 'John',
      role: 'Instructor'
    });

    expect(toastSpy.showInfo).toHaveBeenCalledWith(
      'Profile completed successfully! Please login again.'
    );

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  // -------------------------
  // ERROR FLOW
  // -------------------------

  it('should show error toast if API fails', () => {
    userFacadeSpy.completeProfile.and.returnValue(
      throwError(() => new Error('API failed'))
    );

    fixture.detectChanges();

    component.name = 'John';
    component.roleId = 2;

    component.saveProfile();

    expect(component.loading).toBeFalse();
    expect(toastSpy.showError).toHaveBeenCalledWith('Something went wrong.');
  });
});
