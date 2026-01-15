import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CourseDetailsComponent } from './course-details';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade } from '@frontend/core';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('CourseDetailsComponent', () => {
  let component: CourseDetailsComponent;
  let fixture: ComponentFixture<CourseDetailsComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)) };
  let courseApiMock: Partial<CourseFacade>;

  beforeEach(async () => {
    courseApiMock = {
      getCourseById: jasmine.createSpy('getCourseById').and.returnValue(of({ id: 1, title: 'Test Course' })),
      getEnrolledCourses: jasmine.createSpy('getEnrolledCourses').and.returnValue(of([{ id: 2 }])),
      enroll: jasmine.createSpy('enroll').and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [CourseDetailsComponent],
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should call loadCourse and checkEnrollment', () => {
    spyOn(component, 'loadCourse');
    spyOn(component, 'checkEnrollment');

    component.ngOnInit();

    expect(component.loadCourse).toHaveBeenCalled();
    expect(component.checkEnrollment).toHaveBeenCalled();
  });

  it('loadCourse should set course', fakeAsync(() => {
    component.loadCourse();
    tick();
    expect(courseApiMock.getCourseById).toHaveBeenCalledWith({ courseId: component.courseId });
    expect(component.course).toEqual({ id: 1, title: 'Test Course' });
  }));

  it('checkEnrollment should set isEnrolled false', fakeAsync(() => {
    component.checkEnrollment();
    tick();
    expect(courseApiMock.getEnrolledCourses).toHaveBeenCalled();
    expect(component.isEnrolled).toBeFalse();
  }));

  it('enrollOrContinue navigates if already enrolled', () => {
    component.isEnrolled = true;
    component.enrollOrContinue();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses', component.courseId, 'modules']);
  });

  it('enrollOrContinue calls enroll if not enrolled', fakeAsync(() => {
    component.isEnrolled = false;
    component.courseId = 1;

    component.enrollOrContinue();
    tick();

    expect(courseApiMock.enroll).toHaveBeenCalledWith({ courseId: 1 });
    expect(component.isEnrolled).toBeTrue();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses', 1, 'modules']);
    expect(component.isLoading).toBeFalse();
    expect(component.btnLoading).toBeFalse();
  }));

  it('enrollOrContinue handles 400 error', fakeAsync(() => {
    component.isEnrolled = false;
    component.courseId = 1;

    (courseApiMock.enroll as jasmine.Spy).and.returnValue(throwError(() => ({ status: 400 })));

    component.enrollOrContinue();
    tick();

    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses', 1, 'modules']);
    expect(component.isLoading).toBeFalse();
    expect(component.btnLoading).toBeFalse();
  }));

  it('enrollOrContinue handles other errors', fakeAsync(() => {
    component.isEnrolled = false;
    component.courseId = 1;

    (courseApiMock.enroll as jasmine.Spy).and.returnValue(throwError(() => ({ status: 500 })));

    component.enrollOrContinue();
    tick();

    expect(component.isEnrolled).toBeFalse();
    expect(component.isLoading).toBeFalse();
    expect(component.btnLoading).toBeFalse();
  }));
});
