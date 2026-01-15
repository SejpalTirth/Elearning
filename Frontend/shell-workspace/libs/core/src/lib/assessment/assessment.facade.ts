import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  AssessmentGatewayService,
  GatewayContractsAssessmentAddQuestionRequest,
  GatewayContractsAssessmentCreateQuiz,
  GatewayContractsAssessmentGetQuizForModuleRequest,
  GatewayContractsAssessmentGetUnquizzedRequest,
  GatewayContractsAssessmentSubmitQuiz,
  GatewayContractsAssessmentResultRequest,
  GatewayContractsAssessmentCourseQuizStatusRequest
} from '@frontend/api';

@Injectable({ providedIn: 'root' })
export class AssessmentFacade {

  private readonly api = inject(AssessmentGatewayService);

  // --------------------------------------------------
  // QUIZ
  // --------------------------------------------------

  getQuizForModule(
    payload: GatewayContractsAssessmentGetQuizForModuleRequest
  ): Observable<any> {
    return this.api.postApiAssessmentQuizModule(payload);
  }

  createQuiz(
    payload: GatewayContractsAssessmentCreateQuiz
  ): Observable<void> {
    return this.api.postApiAssessmentQuiz(payload);
  }

  addQuestion(
    payload: GatewayContractsAssessmentAddQuestionRequest
  ): Observable<void> {
    return this.api.postApiAssessmentQuizQuestions(payload);
  }

  submitQuiz(
    payload: GatewayContractsAssessmentSubmitQuiz
  ): Observable<void> {
    return this.api.postApiAssessmentQuizSubmit(payload);
  }

  getQuizResult(
    payload: GatewayContractsAssessmentResultRequest
  ): Observable<any> {
    return this.api.postApiAssessmentQuizResult(payload);
  }

  // --------------------------------------------------
  // COURSE ↔ QUIZ RELATION
  // --------------------------------------------------

  getQuizStatusForCourse(
    payload: GatewayContractsAssessmentCourseQuizStatusRequest
  ): Observable<any> {
    return this.api.postApiAssessmentCourseQuizStatus(payload);
  }

  getUnquizzedModules(
    payload: GatewayContractsAssessmentGetUnquizzedRequest
  ): Observable<number[]> {
    return this.api.postApiAssessmentCourseUnquizzedModules(payload);
  }
}
