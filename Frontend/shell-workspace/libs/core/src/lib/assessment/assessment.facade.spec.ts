import { TestBed } from '@angular/core/testing';
import { Observable, of } from 'rxjs';

import { AssessmentFacade } from './assessment.facade';
import { AssessmentGatewayService } from '@frontend/api';
import { HttpResponse } from '@angular/common/http';

describe('AssessmentFacade', () => {
  let facade: AssessmentFacade;
  let gatewaySpy: any;

  beforeEach(() => {
    gatewaySpy = jasmine.createSpyObj('AssessmentGatewayService', [
      'postApiAssessmentQuizModule',
      'postApiAssessmentQuiz',
      'postApiAssessmentQuizQuestions',
      'postApiAssessmentQuizSubmit',
      'postApiAssessmentQuizResult',
      'postApiAssessmentCourseQuizStatus',
      'postApiAssessmentCourseUnquizzedModules'
    ]);

    TestBed.configureTestingModule({
      providers: [
        AssessmentFacade,
        { provide: AssessmentGatewayService, useValue: gatewaySpy }
      ]
    });

    facade = TestBed.inject(AssessmentFacade);
  });

  it('should be created', () => {
    expect(facade).toBeTruthy();
  });

  // --------------------------------------------------
  // QUIZ
  // --------------------------------------------------

  it('should get quiz for module', () => {
  const payload = { moduleId: 1 } as any;
  const body = { quizId: 10 };

  gatewaySpy.postApiAssessmentQuizModule.and.returnValue(
    of(new HttpResponse({ body }))
  );

  facade.getQuizForModule(payload).subscribe(res => {
    expect(res.body).toEqual(body);
  });

  expect(gatewaySpy.postApiAssessmentQuizModule).toHaveBeenCalledWith(payload);
});


  it('should create quiz', () => {
  const payload = { moduleId: 1 } as any;

  gatewaySpy.postApiAssessmentQuiz.and.returnValue(
    of(new HttpResponse({ body: null }))
  );

  facade.createQuiz(payload).subscribe();

  expect(gatewaySpy.postApiAssessmentQuiz).toHaveBeenCalledWith(payload);
});


  it('should add question to quiz', () => {
    const payload = { quizId: 1, question: 'Q1' } as any;

    gatewaySpy.postApiAssessmentQuizQuestions.and.returnValue(of(
        new HttpResponse({ body: null })
    ));

    facade.addQuestion(payload).subscribe();

    expect(gatewaySpy.postApiAssessmentQuizQuestions).toHaveBeenCalledWith(payload);
  });

  it('should submit quiz', () => {
    const payload = { quizId: 1 } as any;

    gatewaySpy.postApiAssessmentQuizSubmit.and.returnValue(of(
        new HttpResponse({ body: null })
    ));

    facade.submitQuiz(payload).subscribe();

    expect(gatewaySpy.postApiAssessmentQuizSubmit).toHaveBeenCalledWith(payload);
  });

  it('should get quiz result', () => {
  const payload = { quizId: 1 } as any;
  const body = { score: 80 };

  gatewaySpy.postApiAssessmentQuizResult.and.returnValue(
    of(new HttpResponse({ body }))
  );

  facade.getQuizResult(payload).subscribe(res => {
    expect(res.body).toEqual(body);
  });

  expect(gatewaySpy.postApiAssessmentQuizResult).toHaveBeenCalledWith(payload);
});


  // --------------------------------------------------
  // COURSE ↔ QUIZ RELATION
  // --------------------------------------------------

  it('should get quiz status for course', () => {
  const payload = { courseId: 1 } as any;
  const body = { completed: true };

  gatewaySpy.postApiAssessmentCourseQuizStatus.and.returnValue(
    of(new HttpResponse({ body }))
  );

  facade.getQuizStatusForCourse(payload).subscribe(res => {
    expect(res.body).toEqual(body);
  });

  expect(gatewaySpy.postApiAssessmentCourseQuizStatus).toHaveBeenCalledWith(payload);
});


  it('should get unquizzed modules', () => {
  const payload = { courseId: 1 };
  const body = [1, 2, 3];

  gatewaySpy.postApiAssessmentCourseUnquizzedModules.and.returnValue(
    of(body)
  );

  facade.getUnquizzedModules(payload).subscribe(res => {
    expect(res).toEqual(body);
  });

  expect(gatewaySpy.postApiAssessmentCourseUnquizzedModules)
    .toHaveBeenCalledWith(payload);
});
});
