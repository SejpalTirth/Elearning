import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';
import { AuthStateService } from '@frontend/auth';
import { Router } from '@angular/router';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { provideCommonMocks } from '../../../../../test-utils/mocks';
import { of } from 'rxjs';

describe('Home Component', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let authStateSpy: jasmine.SpyObj<AuthStateService>;
  let courseSpy: jasmine.SpyObj<CourseFacade>;
  let assessmentSpy: jasmine.SpyObj<AssessmentFacade>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authStateSpy = jasmine.createSpyObj(
      'AuthStateService',
      [],
      {
        status$: of('authenticated'),
        user: { userId: '123', role: 'Instructor' }
      }
    );

    courseSpy = jasmine.createSpyObj('CourseFacade', ['getUnfinishedCourses']);
    assessmentSpy = jasmine.createSpyObj('AssessmentFacade', ['getUnquizzedModules']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    courseSpy.getUnfinishedCourses.and.returnValue(of({ id: 123 }));
    assessmentSpy.getUnquizzedModules.and.returnValue(of([]));

    TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        ...provideCommonMocks,
        { provide: AuthStateService, useValue: authStateSpy },
        { provide: CourseFacade, useValue: courseSpy },
        { provide: AssessmentFacade, useValue: assessmentSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call checkForPendingTasks for instructor', () => {
    spyOn(component, 'checkForPendingTasks');
    fixture.detectChanges();
    expect(component.checkForPendingTasks).toHaveBeenCalled();
  });

  it('should fetch unfinished courses and unquizzed modules', () => {
    fixture.detectChanges();
    expect(courseSpy.getUnfinishedCourses).toHaveBeenCalled();
    expect(assessmentSpy.getUnquizzedModules)
  .toHaveBeenCalledWith({ courseId: 123 });

  });

  it('should navigate to courses', () => {
    component.goToCourses();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/courses']);
  });

  it('should unsubscribe on destroy', () => {
    const sub = jasmine.createSpyObj('Subscription', ['unsubscribe']);
    (component as any).authSub = sub;

    component.ngOnDestroy();

    expect(sub.unsubscribe).toHaveBeenCalled();
  });
});
