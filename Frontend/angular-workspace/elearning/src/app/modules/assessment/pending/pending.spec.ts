import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Pending } from './pending';
import { Router } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { AuthStateService } from '../../../service/auth-state-service';
import { AssessmentGatewayService, GatewayCourseService } from 'api';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';

describe('Pending Component', () => {
  let component: Pending;
  let fixture: ComponentFixture<Pending>;

  // Mocks
  let mockRouter: any;
  let mockToast: any;
  let mockLoading: any;
  let mockAuthState: any;
  let mockCourseApi: any;
  let mockAssessmentApi: any;

  const userSubject = new BehaviorSubject<any>(null);

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showSuccess: jasmine.createSpy('showSuccess'), showError: jasmine.createSpy('showError') };
    mockLoading = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
    
    mockAuthState = {
      user$: userSubject.asObservable()
    };

    mockCourseApi = {
      postApiCourseUnfinished: jasmine.createSpy('postApiCourseUnfinished').and.returnValue(of([
        { id: 1, title: 'Course 1', isDeleted: false },
        { id: 2, title: 'Course 2', isDeleted: true }
      ])),
      postApiCoursePublish: jasmine.createSpy('postApiCoursePublish').and.returnValue(of({}))
    };

    mockAssessmentApi = {
      postApiAssessmentCourseUnquizzedModules: jasmine.createSpy('postApiAssessmentCourseUnquizzedModules').and.returnValue(of({
        modules: [{ id: 10, quizExists: false }],
        nextPendingModuleId: 10
      }))
    };

    await TestBed.configureTestingModule({
      imports: [Pending],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ToastService, useValue: mockToast },
        { provide: LoadingService, useValue: mockLoading },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Pending);
    component = fixture.componentInstance;
  });

  it('should not load courses if user is not logged in', () => {
    userSubject.next(null);
    fixture.detectChanges();

    expect(component.pendingCourses.length).toBe(0);
    expect(mockCourseApi.postApiCourseUnfinished).not.toHaveBeenCalled();
  });

  it('should load unfinished courses and filter out deleted ones', () => {
    userSubject.next({ userId: 'user123' });
    fixture.detectChanges();

    expect(component.userId).toBe('user123');
    expect(mockCourseApi.postApiCourseUnfinished).toHaveBeenCalled();
    expect(component.pendingCourses.length).toBe(1);
    expect(component.pendingCourses[0].id).toBe(1);
  });

  it('should call loadPendingModules for each unfinished course', () => {
    userSubject.next({ userId: 'user123' });
    fixture.detectChanges();

    expect(mockAssessmentApi.postApiAssessmentCourseUnquizzedModules).toHaveBeenCalledWith({ courseId: 1 });
    expect(component.pendingCourses[0].pendingModules.length).toBe(1);
    expect(component.pendingCourses[0].allQuizzesDone).toBeFalse();
  });

  it('should navigate to add-quiz when continueQuiz is called', () => {
    component.continueQuiz(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 1]);
  });

  describe('publishCourse()', () => {
    it('should show success toast and refresh list on success', () => {
      userSubject.next({ userId: 'user123' });
      fixture.detectChanges();
      
      mockCourseApi.postApiCourseUnfinished.calls.reset();

      component.publishCourse(1);

      expect(mockCourseApi.postApiCoursePublish).toHaveBeenCalledWith({ courseId: 1 });
      expect(mockToast.showSuccess).toHaveBeenCalled();
      expect(mockCourseApi.postApiCourseUnfinished).toHaveBeenCalled();
    });

    it('should show error toast on API failure', () => {
      mockCourseApi.postApiCoursePublish.and.returnValue(throwError(() => new Error('Error')));
      
      component.publishCourse(1);

      expect(mockToast.showError).toHaveBeenCalledWith('Failed to publish the course.');
    });
  });

  it('should unsubscribe from auth changes on destroy', () => {
    userSubject.next({ userId: '123' });
    fixture.detectChanges();

    if (component['authSub']) {
      const unsubscribeSpy = spyOn(component['authSub'], 'unsubscribe');
      
      component.ngOnDestroy();
      
      expect(unsubscribeSpy).toHaveBeenCalled();
    } else {
      fail('authSub was not initialized');
    }
  });
});