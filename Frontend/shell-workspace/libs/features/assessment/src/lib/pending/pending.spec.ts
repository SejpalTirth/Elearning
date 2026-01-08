import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Pending } from './pending';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { Router } from '@angular/router';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { ToastService, LoadingService } from '@frontend/ui';
import { AuthStateService } from '@frontend/auth';

describe('Pending Component', () => {
  let component: Pending;
  let fixture: ComponentFixture<Pending>;

  // Mocks
  let routerMock = { navigate: jasmine.createSpy('navigate') };
  let toastMock = { showSuccess: jasmine.createSpy('showSuccess'), showError: jasmine.createSpy('showError') };
  let loadingMock = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
  let authStateMock: Partial<AuthStateService>;
  let courseApiMock: Partial<CourseFacade>;
  let assessmentApiMock: Partial<AssessmentFacade>;

  const userSubject = new BehaviorSubject<any>({ userId: 'user123' });

  beforeEach(async () => {
    authStateMock = { user$: userSubject.asObservable() };
    courseApiMock = { 
      getUnfinishedCourses: jasmine.createSpy('getUnfinishedCourses').and.returnValue(of([
        { id: 1, title: 'Course 1', isDeleted: false },
        { id: 2, title: 'Course 2', isDeleted: false }
      ])),
      publishCourse: jasmine.createSpy('publishCourse').and.returnValue(of(void 0))
    };
    assessmentApiMock = {
      getQuizStatusForCourse: jasmine.createSpy('getQuizStatusForCourse').and.returnValue(of({
        modules: [{ id: 101, quizExists: false }],
        nextPendingModuleId: 101
      }))
    };

    await TestBed.configureTestingModule({
      imports: [Pending],
      providers: [
        provideCommonMocks,
        { provide: Router, useValue: routerMock },
        { provide: ToastService, useValue: toastMock },
        { provide: LoadingService, useValue: loadingMock },
        { provide: AuthStateService, useValue: authStateMock },
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AssessmentFacade, useValue: assessmentApiMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Pending);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load unfinished courses on init', fakeAsync(() => {
    component.ngOnInit();
    tick();

    expect(loadingMock.show).toHaveBeenCalled();
    expect(component.pendingCourses.length).toBe(2);
    expect(component.pendingCourses[0].pendingModules.length).toBe(1);
    expect(component.pendingCourses[0].allQuizzesDone).toBe(false);
  }));

  it('should navigate to add-quiz', () => {
    component.continueQuiz(1);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 1]);
  });

  it('should publish course successfully', fakeAsync(() => {
    component.publishCourse(1);
    tick();
    expect(toastMock.showSuccess).toHaveBeenCalledWith('Course published successfully!');
    expect(courseApiMock.publishCourse).toHaveBeenCalledWith({ courseId: 1 });
  }));

  it('should show error on publish course failure', fakeAsync(() => {
    (courseApiMock.publishCourse as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    component.publishCourse(1);
    tick();
    expect(toastMock.showError).toHaveBeenCalledWith('Failed to publish the course.');
  }));

  it('should handle getUnfinishedCourses error', fakeAsync(() => {
    (courseApiMock.getUnfinishedCourses as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    component.loadUnfinishedCourses();
    tick();
    expect(component.pendingCourses).toEqual([]);
    expect(loadingMock.hide).toHaveBeenCalled();
  }));
});
