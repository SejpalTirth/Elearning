import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { Router } from '@angular/router';
import { AuthStateService, AuthUser } from '@frontend/auth';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { of, BehaviorSubject } from 'rxjs';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  let authStateMock: Partial<AuthStateService>;
  let courseApiMock: Partial<CourseFacade>;
  let assessmentApiMock: Partial<AssessmentFacade>;
  let routerMock: Partial<Router>;

  beforeEach(waitForAsync(() => {
    // Mock router
    routerMock = {
      navigate: jasmine.createSpy('navigate')
    };

    // Mock auth state
    const fakeUser: AuthUser = {
      userId: 'user-1',
      email: 'test@test.com',
      name: 'Test User',
      role: 'Instructor' // we will test Instructor path
    };
    const status$ = new BehaviorSubject<'idle' | 'authenticated'>('authenticated');
    authStateMock = {
      status$: status$.asObservable(),
      user: fakeUser
    };

    // Mock CourseFacade & AssessmentFacade
    courseApiMock = {
      getUnfinishedCourses: jasmine.createSpy('getUnfinishedCourses').and.returnValue(
        of({ id: 101, title: 'Course 1' })
      )
    };
    assessmentApiMock = {
      getUnquizzedModules: jasmine.createSpy('getUnquizzedModules').and.returnValue(of([]))
    };

    TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        { provide: AuthStateService, useValue: authStateMock },
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AssessmentFacade, useValue: assessmentApiMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should populate user info when authenticated', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.userId).toBe('user-1');
    expect(component.userName).toBe('Test User');
    expect(component.role).toBe('Instructor');
  });

  it('should navigate to root if status is idle', () => {
    (authStateMock.status$ as BehaviorSubject<any>).next('idle');
    component.ngOnInit();
    fixture.detectChanges();

    expect(routerMock.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should call getUnfinishedCourses and getUnquizzedModules for Instructor', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(courseApiMock.getUnfinishedCourses).toHaveBeenCalled();
    expect(assessmentApiMock.getUnquizzedModules).toHaveBeenCalledWith(101);
  });

  it('goToCourses should navigate to /courses', () => {
    component.goToCourses();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses']);
  });

  it('should unsubscribe on destroy', () => {
    component.ngOnInit();
    const spy = spyOn(component['authSub']!, 'unsubscribe');
    component.ngOnDestroy();
    expect(spy).toHaveBeenCalled();
  });
});
