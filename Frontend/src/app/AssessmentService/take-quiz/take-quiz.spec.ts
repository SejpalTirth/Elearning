import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { TakeQuizComponent } from './take-quiz';
import { AssessmentApiService } from '../services/assessment-api';
import { ProgressService } from '../../CourseService/services/progress.service';

// =======================================================
// PART 1: Initialization & User / Quiz Setup
// =======================================================
describe('TakeQuizComponent - Initialization', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;
  let mockApi: any;
  let mockProgress: any;
  let mockRouter: any;

  function fakeJwt(payload: any): string {
    return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
  }

  beforeEach(async () => {
    mockApi = {
      getQuizForModule: jasmine.createSpy().and.returnValue(
        of({
          quizId: 123,
          questions: [{ questionId: 1 }, { questionId: 2 }]
        })
      ),
      submitQuiz: jasmine.createSpy()
    };

    mockProgress = { markModuleCompleted: jasmine.createSpy().and.returnValue(of({})) };
    mockRouter = { navigate: jasmine.createSpy('navigate') };

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent],
      providers: [
        { provide: AssessmentApiService, useValue: mockApi },
        { provide: ProgressService, useValue: mockProgress },
        { provide: Router, useValue: mockRouter },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: { get: (): string => '55' } }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;

    spyOn(window, 'alert');
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should extract userId from token', () => {
    localStorage.setItem('accessToken', fakeJwt({ sub: 'USER123' }));
    component.extractUserId();
    expect(component.userId).toBe('USER123');
  });

  it('should load quiz and initialize answers array', () => {
    expect(mockApi.getQuizForModule).toHaveBeenCalledWith(55);
    expect(component.quiz.quizId).toBe(123);
    expect(component.answers.length).toBe(2);
    expect(component.answers[0]).toEqual({ questionId: 1, selectedAnswerId: null });
  });
});

describe('TakeQuizComponent - Validation Checks', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;
  let mockApi: any;
  let mockProgress: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockApi = {
      getQuizForModule: jasmine.createSpy().and.returnValue(
        of({ quizId: 123, questions: [{ questionId: 1 }, { questionId: 2 }] })
      ),
      submitQuiz: jasmine.createSpy()
    };
    mockProgress = { markModuleCompleted: jasmine.createSpy().and.returnValue(of({})) };
    mockRouter = { navigate: jasmine.createSpy('navigate') };

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent],
      providers: [
        { provide: AssessmentApiService, useValue: mockApi },
        { provide: ProgressService, useValue: mockProgress },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: (): string => '55' } } } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;
    spyOn(window, 'alert');
    fixture.detectChanges();
  });

  it('should stop submit when quizId or userId missing', () => {
    component.quiz = null;
    component.userId = null;
    component.submit();
    expect(window.alert).toHaveBeenCalled();
  });

  it('should prevent submitting if some answers are missing', () => {
    component.quiz = { quizId: 123 };
    component.userId = 'U1';
    component.answers = [
      { questionId: 1, selectedAnswerId: 10 },
      { questionId: 2, selectedAnswerId: null }
    ];
    component.submit();
    expect(window.alert).toHaveBeenCalledWith(
      'Please answer all questions before submitting.'
    );
  });

  it('should validate allQuestionsAnswered', () => {
    component.answers = [
      { questionId: 1, selectedAnswerId: 10 },
      { questionId: 2, selectedAnswerId: null }
    ];
    expect(component.allQuestionsAnswered()).toBeFalse();
    component.answers[1].selectedAnswerId = 20;
    expect(component.allQuestionsAnswered()).toBeTrue();
  });
});

describe('TakeQuizComponent - Submission & Navigation', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;
  let mockApi: any;
  let mockProgress: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockApi = {
      getQuizForModule: jasmine.createSpy().and.returnValue(
        of({ quizId: 123, questions: [{ questionId: 1 }, { questionId: 2 }] })
      ),
      submitQuiz: jasmine.createSpy()
    };
    mockProgress = { markModuleCompleted: jasmine.createSpy().and.returnValue(of({})) };
    mockRouter = { navigate: jasmine.createSpy('navigate') };

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent],
      providers: [
        { provide: AssessmentApiService, useValue: mockApi },
        { provide: ProgressService, useValue: mockProgress },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: (): string => '55' } } } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;
    spyOn(window, 'alert');
    fixture.detectChanges();
  });

  it('should submit quiz and mark module completed when passed', () => {
    component.quiz = { quizId: 123 };
    component.userId = 'U1';
    component.moduleId = 55;
    component.answers = [
      { questionId: 1, selectedAnswerId: 10 },
      { questionId: 2, selectedAnswerId: 20 }
    ];
    mockApi.submitQuiz.and.returnValue(of({ passed: true, submissionId: 'S999' }));
    component.submit();
    expect(mockApi.submitQuiz).toHaveBeenCalled();
    expect(mockProgress.markModuleCompleted).toHaveBeenCalledWith({ userId: 'U1', moduleId: 55 });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/result/S999'], { queryParams: { moduleId: 55 } });
  });

  it('should navigate to result even if not passed', () => {
    component.quiz = { quizId: 123 };
    component.userId = 'U1';
    component.moduleId = 55;
    component.answers = [
      { questionId: 1, selectedAnswerId: 10 },
      { questionId: 2, selectedAnswerId: 20 }
    ];
    mockApi.submitQuiz.and.returnValue(of({ passed: false, submissionId: 'S100' }));
    component.submit();
    expect(mockProgress.markModuleCompleted).not.toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/result/S100'], { queryParams: { moduleId: 55 } });
  });

  it('should handle submitQuiz error', () => {
    component.quiz = { quizId: 123 };
    component.userId = 'U1';
    component.answers = [
      { questionId: 1, selectedAnswerId: 10 },
      { questionId: 2, selectedAnswerId: 20 }
    ];
    mockApi.submitQuiz.and.returnValue(throwError(() => 'error'));
    component.submit();
    expect(window.alert).toHaveBeenCalledWith('Submission failed.');
    expect(component.submitting).toBeFalse();
  });
});

