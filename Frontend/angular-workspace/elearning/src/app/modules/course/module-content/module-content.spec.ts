import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModuleContentComponent } from './module-content';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { GatewayCourseService, AssessmentGatewayService } from 'api';
import { of, BehaviorSubject, throwError } from 'rxjs';

describe('ModuleContentComponent', () => {
  let component: ModuleContentComponent;
  let fixture: ComponentFixture<ModuleContentComponent>;
  let mockRouter: any;
  let mockAuthState: any;
  let mockLoading: any;
  let mockToast: any;
  let mockCourseApi: any;
  let mockAssessmentApi: any;
  
  const userSubject = new BehaviorSubject<any>(null);

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockLoading = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
    mockToast = { showError: jasmine.createSpy('showError') };
    mockAuthState = { user$: userSubject.asObservable() };
    
    mockCourseApi = {
      postApiCourseModule: jasmine.createSpy('postApiCourseModule').and.returnValue(of({ id: 10, title: 'Module 1' }))
    };
    
    mockAssessmentApi = {
      postApiAssessmentQuizModule: jasmine.createSpy('postApiAssessmentQuizModule').and.returnValue(of({ quizId: 500 }))
    };

    await TestBed.configureTestingModule({
      imports: [ModuleContentComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: LoadingService, useValue: mockLoading },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: { get: () => '10' } }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModuleContentComponent);
    component = fixture.componentInstance;
  });

  it('should not load data if user is not logged in', () => {
    userSubject.next(null);
    fixture.detectChanges();

    expect(mockLoading.show).not.toHaveBeenCalled();
    expect(mockCourseApi.postApiCourseModule).not.toHaveBeenCalled();
  });

  it('should load module and quiz data when user is logged in', () => {
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();

    expect(component.moduleId).toBe(10);
    expect(mockLoading.show).toHaveBeenCalled();
    expect(mockCourseApi.postApiCourseModule).toHaveBeenCalledWith({ moduleId: 10 });
    expect(mockAssessmentApi.postApiAssessmentQuizModule).toHaveBeenCalledWith({ moduleId: 10 });
    expect(component.module).not.toBeNull();
    expect(component.quizStatus).not.toBeNull();
    expect(mockLoading.hide).toHaveBeenCalled();
  });

  it('should handle API errors using catchError in forkJoin', () => {
    mockCourseApi.postApiCourseModule.and.returnValue(throwError(() => new Error('Fail')));
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();

    expect(component.module).toBeNull();
    expect(component.quizStatus).toBeDefined();
    expect(mockLoading.hide).toHaveBeenCalled();
  });

  describe('takeQuiz', () => {
    it('should navigate to quiz if quizId exists', () => {
      component.moduleId = 10;
      component.quizStatus = { quizId: 500 };
      
      component.takeQuiz();
      
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/quiz/10']);
    });

    it('should show error toast if no quizId exists', () => {
      component.quizStatus = null;
      
      component.takeQuiz();
      
      expect(mockToast.showError).toHaveBeenCalledWith('No quiz available.');
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  it('should unsubscribe on destroy', () => {
    userSubject.next({ userId: 'user_1' });
    fixture.detectChanges();
    const unsubscribeSpy = spyOn((component as any).authSub, 'unsubscribe');
    component.ngOnDestroy();
    expect(unsubscribeSpy).toHaveBeenCalled();
  });
});