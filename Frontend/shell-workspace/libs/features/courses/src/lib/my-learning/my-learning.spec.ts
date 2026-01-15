import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { MyLearningComponent } from './my-learning';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade, ProgressService } from '@frontend/core';
import { AuthStateService, AuthUser } from '@frontend/auth';
import { Router } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { LoadingService } from '@frontend/ui';

describe('MyLearningComponent', () => {
  let component: MyLearningComponent;
  let fixture: ComponentFixture<MyLearningComponent>;

  let courseApiMock: Partial<CourseFacade>;
  let progressServiceMock: Partial<ProgressService>;
  let authStateMock: Partial<AuthStateService>;
  let routerMock: Partial<Router>;
  let loadingMock: Partial<LoadingService>;

  beforeEach(waitForAsync(() => {
    // Fake user
    const fakeUser: AuthUser = {
      userId: 'user-1',
      email: 'test@test.com',
      name: 'Test User',
      role: 'Student'
    };

    authStateMock = {
      user$: of(fakeUser)
    };

    // Mock enrolled courses
    const enrolledCourses = [
      { id: 101, title: 'Course 1', modules: [{ id: 1 }, { id: 2 }] },
      { id: 102, title: 'Course 2', modules: [{ id: 3 }] }
    ];

    courseApiMock = {
      getEnrolledCourses: jasmine.createSpy('getEnrolledCourses').and.returnValue(of(enrolledCourses))
    };

    // Mock progress
    progressServiceMock = {
      loadUserProgress: jasmine.createSpy('loadUserProgress').and.returnValue(of(null)),
      progress$: new BehaviorSubject([
        { courseId: 101, moduleId: 1, isCompleted: true },
        { courseId: 101, moduleId: 2, isCompleted: false },
        { courseId: 102, moduleId: 3, isCompleted: true }
      ])
    };

    routerMock = {
      navigate: jasmine.createSpy('navigate')
    };

    loadingMock = {
      show: jasmine.createSpy('show'),
      hide: jasmine.createSpy('hide')
    };

    TestBed.configureTestingModule({
      imports: [MyLearningComponent],
      providers: [
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: ProgressService, useValue: progressServiceMock },
        { provide: AuthStateService, useValue: authStateMock },
        { provide: Router, useValue: routerMock },
        { provide: LoadingService, useValue: loadingMock },
        ...provideCommonMocks
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyLearningComponent);
    component = fixture.componentInstance;
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load enrolled courses and bind progress', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(progressServiceMock.loadUserProgress).toHaveBeenCalled();
    expect(courseApiMock.getEnrolledCourses).toHaveBeenCalled();
    expect(loadingMock.show).toHaveBeenCalled();
    expect(loadingMock.hide).toHaveBeenCalled();

    expect(component.courses.length).toBe(2);

    const course1 = component.courses.find(c => c.id === 101);
    const course2 = component.courses.find(c => c.id === 102);

    expect(course1?.progressPercent).toBe(50); // 1 completed of 2
    expect(course2?.progressPercent).toBe(100); // 1 completed of 1
    expect(component.loading).toBeFalse();
  });

  it('should navigate to course modules when continueLearning is called', () => {
    component.continueLearning(101);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses/101/modules']);
  });

  it('should handle null user', () => {
    authStateMock.user$ = of(null);
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.courses).toEqual([]);
    expect(component.loading).toBeFalse();
  });
});
