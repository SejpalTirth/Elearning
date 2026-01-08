import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { TakeQuizComponent } from './take-quiz';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { Router, ActivatedRoute } from '@angular/router';
import { AssessmentFacade, ProgressService } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { ToastService } from '@frontend/ui';
import { of, throwError, BehaviorSubject } from 'rxjs';

describe('TakeQuizComponent', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate') };
  let apiMock: Partial<AssessmentFacade>;
  let progressMock: Partial<ProgressService>;
  let authMock: Partial<AuthStateService>;
  let toastMock: Partial<ToastService>;

  const userSubject = new BehaviorSubject<any>({ userId: 'user123' });

  beforeEach(async () => {
    apiMock = {
      getQuizForModule: jasmine.createSpy('getQuizForModule').and.returnValue(of({
        quizId: 1,
        questions: [{ id: 101 }, { id: 102 }]
      })),
      submitQuiz: jasmine.createSpy('submitQuiz').and.returnValue(of({
        submissionId: 'sub1',
        passed: true
      }))
    };

    progressMock = {
      completeModule: jasmine.createSpy('completeModule').and.returnValue(of(void 0))
    };

    authMock = {
      user$: userSubject.asObservable()
    };

    toastMock = {
      showError: jasmine.createSpy('showError')
    };

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent],
      providers: [
        provideCommonMocks,
        { provide: Router, useValue: routerMock },
        { provide: AssessmentFacade, useValue: apiMock },
        { provide: ProgressService, useValue: progressMock },
        { provide: AuthStateService, useValue: authMock },
        { provide: ToastService, useValue: toastMock },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: { get: (key: string) => key === 'moduleId' ? '42' : null }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set moduleId and subscribe to user on init', () => {
    component.ngOnInit();
    expect(component.moduleId).toBe(42);
    expect(component.userId).toBe('user123');
  });

  it('should load quiz successfully', fakeAsync(() => {
    component.loadQuiz();
    tick();
    expect(component.quiz.quizId).toBe(1);
    expect(component.answers.length).toBe(2);
    expect(component.loading).toBe(false);
    expect(apiMock.getQuizForModule).toHaveBeenCalledWith({ moduleId: 42 });
  }));

  it('should handle load quiz error', fakeAsync(() => {
    (apiMock.getQuizForModule as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    component.loadQuiz();
    tick();
    expect(toastMock.showError).toHaveBeenCalledWith('Failed to load quiz');
    expect(component.loading).toBe(false);
  }));

  it('allQuestionsAnswered should return true if all answered', () => {
    component.answers = [
      { questionId: 1, selectedAnswerId: 0 },
      { questionId: 2, selectedAnswerId: 1 }
    ];
    expect(component.allQuestionsAnswered()).toBe(true);
  });

  it('allQuestionsAnswered should return false if any unanswered', () => {
    component.answers = [
      { questionId: 1, selectedAnswerId: 0 },
      { questionId: 2, selectedAnswerId: undefined }
    ];
    expect(component.allQuestionsAnswered()).toBe(false);
  });

  it('submit should show error if questions not answered', () => {
    component.answers = [
      { questionId: 1, selectedAnswerId: 0 },
      { questionId: 2, selectedAnswerId: undefined }
    ];
    component.quiz = { quizId: 1 };
    component.userId = 'user123';
    component.submit();
    expect(toastMock.showError).toHaveBeenCalledWith('Please answer all questions.');
  });

  it('submit should call API and navigate on success', fakeAsync(() => {
    component.answers = [
      { questionId: 101, selectedAnswerId: 1 },
      { questionId: 102, selectedAnswerId: 2 }
    ];
    component.quiz = { quizId: 1 };
    component.userId = 'user123';

    component.submit();
    tick();

    expect(apiMock.submitQuiz).toHaveBeenCalled();
    expect(progressMock.completeModule).toHaveBeenCalledWith({ moduleId: 42 });
    expect(routerMock.navigate).toHaveBeenCalledWith(
      ['/assessment/result/sub1'],
      { queryParams: { moduleId: 42 }, replaceUrl: true }
    );
  }));

  it('submit should handle API error', fakeAsync(() => {
    (apiMock.submitQuiz as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    component.answers = [
      { questionId: 101, selectedAnswerId: 1 },
      { questionId: 102, selectedAnswerId: 2 }
    ];
    component.quiz = { quizId: 1 };
    component.userId = 'user123';

    component.submit();
    tick();

    expect(toastMock.showError).toHaveBeenCalledWith('Submission failed.');
    expect(component.submitting).toBe(false);
  }));
});
