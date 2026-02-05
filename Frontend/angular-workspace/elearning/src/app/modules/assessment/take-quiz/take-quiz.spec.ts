import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TakeQuizComponent } from './take-quiz';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { AuthStateService } from '../../../service/auth-state-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { AssessmentGatewayService, ProgressGatewayService } from 'api';
import { FormsModule } from '@angular/forms';

describe('TakeQuizComponent', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;
  let mockRouter: any;
  let mockActivatedRoute: any;
  let mockAuthState: any;
  let mockToast: any;
  let mockProgressApi: any;
  let mockAssessmentApi: any;

  const userSubject = new BehaviorSubject<any>({ userId: 'user-1' });

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showError: jasmine.createSpy('showError') };
    mockProgressApi = { postApiProgressCompleteModule: jasmine.createSpy('postApiProgressCompleteModule').and.returnValue(of({})) };
    
    mockActivatedRoute = {
      snapshot: { paramMap: { get: () => '10' } }
    };

    mockAuthState = { user$: userSubject.asObservable() };

    mockAssessmentApi = {
      postApiAssessmentQuizModule: jasmine.createSpy('postApiAssessmentQuizModule').and.returnValue(of({
        quizId: 55,
        questions: [{ id: 1 }, { id: 2 }]
      })),
      postApiAssessmentQuizSubmit: jasmine.createSpy('postApiAssessmentQuizSubmit').and.returnValue(of({
        passed: true,
        submissionId: 'sub-abc'
      }))
    };

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent, FormsModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: ToastService, useValue: mockToast },
        { provide: ProgressGatewayService, useValue: mockProgressApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;
  });

  it('should initialize and load quiz questions', () => {
    fixture.detectChanges();

    expect(component.moduleId).toBe(10);
    expect(component.userId).toBe('user-1');
    expect(mockAssessmentApi.postApiAssessmentQuizModule).toHaveBeenCalledWith({ moduleId: 10 });
    expect(component.answers.length).toBe(2);
    expect(component.answers[0].questionId).toBe(1);
    expect(component.loading).toBeFalse();
  });

  describe('submit()', () => {
    it('should show error if not all questions are answered', () => {
      fixture.detectChanges();
      component.answers[0].selectedAnswerId = 1;
      // answers[1] remains undefined
      
      component.submit();

      expect(mockToast.showError).toHaveBeenCalledWith('Please answer all questions.');
      expect(mockAssessmentApi.postApiAssessmentQuizSubmit).not.toHaveBeenCalled();
    });

    it('should submit quiz and navigate to results if all answered', () => {
      fixture.detectChanges();
      component.answers[0].selectedAnswerId = 1;
      component.answers[1].selectedAnswerId = 3;

      component.submit();

      expect(mockAssessmentApi.postApiAssessmentQuizSubmit).toHaveBeenCalled();
      expect(mockProgressApi.postApiProgressCompleteModule).toHaveBeenCalledWith({ moduleId: 10 });
      expect(mockRouter.navigate).toHaveBeenCalledWith(
        ['/assessment/result/sub-abc'],
        { queryParams: { moduleId: 10 }, replaceUrl: true }
      );
    });

    it('should not call progress service if quiz is failed', () => {
      mockAssessmentApi.postApiAssessmentQuizSubmit.and.returnValue(of({
        passed: false,
        submissionId: 'sub-failed'
      }));
      fixture.detectChanges();
      component.answers.forEach(a => a.selectedAnswerId = 1);

      component.submit();

      expect(mockProgressApi.postApiProgressCompleteModule).not.toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalled();
    });

    it('should handle submission error', () => {
      mockAssessmentApi.postApiAssessmentQuizSubmit.and.returnValue(throwError(() => new Error('Fail')));
      fixture.detectChanges();
      component.answers.forEach(a => a.selectedAnswerId = 1);

      component.submit();

      expect(mockToast.showError).toHaveBeenCalledWith('Submission failed.');
      expect(component.submitting).toBeFalse();
    });
  });

  it('should unsubscribe on destroy', () => {
    fixture.detectChanges();
    const sub = component['authSub'];
    const spy = spyOn(sub!, 'unsubscribe');
    component.ngOnDestroy();
    expect(spy).toHaveBeenCalled();
  });
});