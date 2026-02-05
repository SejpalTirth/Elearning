import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InstructorCourseList } from './instructor-course-list';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { GatewayCourseService } from 'api';
import { of, BehaviorSubject, throwError } from 'rxjs';

describe('InstructorCourseList', () => {
  let component: InstructorCourseList;
  let fixture: ComponentFixture<InstructorCourseList>;
  let mockRouter: any;
  let mockAuthState: any;
  let mockCourseApi: any;
  const userSubject = new BehaviorSubject<any>(null);

  const mockCourses = [
    { id: 101, title: 'Course 1', isDeleted: false },
    { id: 102, title: 'Course 2', isDeleted: true }
  ];

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockAuthState = { user$: userSubject.asObservable() };
    mockCourseApi = {
      postApiCourseInstructor: jasmine.createSpy('postApiCourseInstructor').and.returnValue(of(mockCourses))
    };

    await TestBed.configureTestingModule({
      imports: [InstructorCourseList],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: GatewayCourseService, useValue: mockCourseApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InstructorCourseList);
    component = fixture.componentInstance;
  });

  it('should not load courses if user is not logged in', () => {
    userSubject.next(null);
    fixture.detectChanges();

    expect(component.userId).toBeNull();
    expect(component.courses).toEqual([]);
    expect(mockCourseApi.postApiCourseInstructor).not.toHaveBeenCalled();
  });

  it('should load courses when user logs in', () => {
    userSubject.next({ userId: 'instructor_7' });
    fixture.detectChanges();

    expect(component.userId).toBe('instructor_7');
    expect(mockCourseApi.postApiCourseInstructor).toHaveBeenCalled();
    expect(component.courses).toEqual(mockCourses);
    expect(component.loading).toBeFalse();
  });

  describe('Actions', () => {
    beforeEach(() => {
      userSubject.next({ userId: 'instructor_7' });
      fixture.detectChanges();
    });

    it('should navigate to edit page if course is not deleted', () => {
      component.editCourse(101, false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/edit', 101]);
    });

    it('should NOT navigate to edit page if course is deleted', () => {
      component.editCourse(102, true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should navigate to view course details', () => {
      component.viewCourse(101);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses', 101]);
    });
  });

  it('should handle API errors gracefully', () => {
    mockCourseApi.postApiCourseInstructor.and.returnValue(throwError(() => new Error('API Fail')));
    userSubject.next({ userId: 'instructor_7' });
    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(component.courses).toEqual([]);
  });

  it('should unsubscribe on destroy', () => {
    userSubject.next({ userId: 'instructor_7' });
    fixture.detectChanges();
    const unsubscribeSpy = spyOn((component as any).authSub, 'unsubscribe');
    component.ngOnDestroy();
    expect(unsubscribeSpy).toHaveBeenCalled();
  });
});