import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyLearningComponent } from './my-learning';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { GatewayCourseService, ProgressGatewayService } from 'api';
import { of, BehaviorSubject, throwError } from 'rxjs';

describe('MyLearningComponent', () => {
  let component: MyLearningComponent;
  let fixture: ComponentFixture<MyLearningComponent>;
  let mockRouter: any;
  let mockAuthState: any;
  let mockLoading: any;
  let mockCourseApi: any;
  let mockProgressApi: any;
  const userSubject = new BehaviorSubject<any>(null);

  const mockCourses = [
    { id: 1, title: 'Angular' },
    { id: 2, title: 'TypeScript' }
  ];

  const mockProgress = [
    { courseId: 1, moduleId: 101, isCompleted: true },
    { courseId: 1, moduleId: 102, isCompleted: false },
    { courseId: 2, moduleId: 201, isCompleted: true }
  ];

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockLoading = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
    mockAuthState = { user$: userSubject.asObservable() };
    
    mockCourseApi = {
      postApiCourseEnrolled: jasmine.createSpy('postApiCourseEnrolled').and.returnValue(of(mockCourses))
    };
    
    mockProgressApi = {
      postApiProgressUser: jasmine.createSpy('postApiProgressUser').and.returnValue(of(mockProgress))
    };

    await TestBed.configureTestingModule({
      imports: [MyLearningComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: LoadingService, useValue: mockLoading },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: ProgressGatewayService, useValue: mockProgressApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyLearningComponent);
    component = fixture.componentInstance;
  });

  it('should not load courses if user is not logged in', () => {
    userSubject.next(null);
    fixture.detectChanges();

    expect(component.courses).toEqual([]);
    expect(component.loading).toBeFalse();
    expect(mockCourseApi.postApiCourseEnrolled).not.toHaveBeenCalled();
  });

  it('should calculate progress percentage correctly for enrolled courses', () => {
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();

    expect(mockLoading.show).toHaveBeenCalled();
    expect(component.courses.length).toBe(2);
    
    const angularCourse = component.courses.find(c => c.id === 1);
    expect(angularCourse.progressPercent).toBe(50);

    const tsCourse = component.courses.find(c => c.id === 2);
    expect(tsCourse.progressPercent).toBe(100);

    expect(component.loading).toBeFalse();
    expect(mockLoading.hide).toHaveBeenCalled();
  });

  it('should set progress to 0 if total modules is 0', () => {
    mockProgressApi.postApiProgressUser.and.returnValue(of([]));
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();

    expect(component.courses[0].progressPercent).toBe(0);
  });

  it('should navigate when continuing learning', () => {
    component.continueLearning(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/1/modules']);
  });

  it('should handle API errors and hide loader', () => {
    mockCourseApi.postApiCourseEnrolled.and.returnValue(throwError(() => new Error('Fail')));
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(mockLoading.hide).toHaveBeenCalled();
  });

  it('should unsubscribe on destroy', () => {
    fixture.detectChanges();
    const unsubscribeSpy = spyOn((component as any).sub, 'unsubscribe');
    component.ngOnDestroy();
    expect(unsubscribeSpy).toHaveBeenCalled();
  });
});