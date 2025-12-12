import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { AssessmentApiService } from './assessment-api';

describe('AssessmentApiService', () => {

  let service: AssessmentApiService;
  let httpMock: HttpTestingController;

  const baseUrl = 'https://localhost:7249/api/AssessmentGateway';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AssessmentApiService]
    });

    service = TestBed.inject(AssessmentApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Ensures no unexpected HTTP calls
  });

  // ----------------------------------------------------------
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ----------------------------------------------------------
  it('should call getQuizForModule', () => {
    service.getQuizForModule(10).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/quiz/module/10`);
    expect(req.request.method).toBe('GET');

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call submitQuiz', () => {
    const mockPayload = { quizId: 1, answers: [] };

    service.submitQuiz(mockPayload).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/submit`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockPayload);

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call getResult', () => {
    service.getResult('SUB123').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/result/SUB123`);
    expect(req.request.method).toBe('GET');

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call createQuiz', () => {
    const quiz = { title: 'Test Quiz' };

    service.createQuiz(quiz).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/quiz`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(quiz);

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call addQuestion', () => {
    const question = { text: 'Hello?', correctAnswer: 1 };

    service.addQuestion(7, question).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/quiz/7/questions`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(question);

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call getQuizStatus', () => {
    service.getQuizStatus(5).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/course/5/quiz-status`);
    expect(req.request.method).toBe('GET');

    req.flush({});
  });

  // ----------------------------------------------------------
  it('should call getUnquizzedModules', () => {
    service.getUnquizzedModules(99).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/unquizzed-modules/99`);
    expect(req.request.method).toBe('GET');

    req.flush([]);
  });

});
