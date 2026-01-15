import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { InstructorCourseList } from './instructor-course-list';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { Router } from '@angular/router';
import { of, Subject } from 'rxjs';

describe('InstructorCourseList', () => {
  let component: InstructorCourseList;
  let fixture: ComponentFixture<InstructorCourseList>;

  let routerMock = { navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)) };
  let courseApiMock: Partial<CourseFacade>;
  let authStateMock: Partial<AuthStateService>;

  const user$ = new Subject<any>();

  beforeEach(async () => {
    courseApiMock = {
      getInstructorCourses: jasmine.createSpy('getInstructorCourses').and.returnValue(of([
        { id: 1, title: 'Course 1', isDeleted: false },
        { id: 2, title: 'Course 2', isDeleted: true }
      ]))
    };

    authStateMock = {
      user$: user$
    };

    await TestBed.configureTestingModule({
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AuthStateService, useValue: authStateMock },
        { provide: Router, useValue: routerMock }
      ],
      declarations: [InstructorCourseList]
    }).compileComponents();

    fixture = TestBed.createComponent(InstructorCourseList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    user$.complete();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load courses when user is present', fakeAsync(() => {
    user$.next({ userId: '123' });
    tick();
    expect(component.userId).toBe('123');
    expect(component.courses.length).toBe(2);
    expect(component.loading).toBeFalse();
    expect(courseApiMock.getInstructorCourses).toHaveBeenCalled();
  }));

  it('should clear courses when user is null', fakeAsync(() => {
    user$.next(null);
    tick();
    expect(component.userId).toBeNull();
    expect(component.courses.length).toBe(0);
    expect(component.loading).toBeFalse();
  }));

  it('editCourse should navigate if not deleted', () => {
    component.editCourse(1, false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses/edit', 1]);
  });

  it('editCourse should not navigate if deleted', () => {
    component.editCourse(1, true);
    expect(routerMock.navigate).not.toHaveBeenCalledWith(['/courses/edit', 1]);
  });

  it('viewCourse should navigate', () => {
    component.viewCourse(1);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses', 1]);
  });

  it('isDeleted should return true for deleted course', () => {
    const course = { isDeleted: true };
    expect(component.isDeleted(course)).toBeTrue();
  });

  it('isDeleted should return false for active course', () => {
    const course = { isDeleted: false };
    expect(component.isDeleted(course)).toBeFalse();
  });

  it('should unsubscribe on destroy', () => {
    spyOn(component['authSub']!, 'unsubscribe');
    component.ngOnDestroy();
    expect(component['authSub']!.unsubscribe).toHaveBeenCalled();
  });
});
