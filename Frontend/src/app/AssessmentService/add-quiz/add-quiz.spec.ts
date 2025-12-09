import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { Router } from '@angular/router';

import { AddQuizComponent } from './add-quiz.component';
import { AssessmentApiService } from '../services/assessment-api';
import { CourseApiService } from '../../CourseService/services/course-api';
import { ActivatedRoute } from '@angular/router';

// ----------------------
// Mock Services
// ----------------------
class MockCourseApi {
  getModules = jasmine.createSpy().and.returnValue(
    of([
      { id: 1, title: "Intro" },
      { id: 2, title: "Basics" }
    ])
  );
}

class MockAssessmentApi {
  getUnquizzedModules = jasmine.createSpy().and.returnValue(of([2]));
  createQuiz = jasmine.createSpy().and.returnValue(of({ quizId: 50 }));
  addQuestion = jasmine.createSpy().and.returnValue(of({}));
}

class MockRouter {
  navigate = jasmine.createSpy("navigate");
}

const mockRoute = {
  snapshot: { paramMap: { get: () => '10' } }
};

// ----------------------

describe("AddQuizComponent", () => {

  let component: AddQuizComponent;
  let fixture: ComponentFixture<AddQuizComponent>;
  let assessmentApi: MockAssessmentApi;
  let courseApi: MockCourseApi;
  let router: MockRouter;

  beforeEach(async () => {

    assessmentApi = new MockAssessmentApi();
    courseApi = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [AddQuizComponent, ReactiveFormsModule],
      providers: [
        { provide: CourseApiService, useValue: courseApi },
        { provide: AssessmentApiService, useValue: assessmentApi },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: mockRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddQuizComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should load modules on init", fakeAsync(() => {
    tick();
    expect(courseApi.getModules).toHaveBeenCalledWith(10);
    expect(component.modules.length).toBe(2);
  }));

//   it("should load unquizzed modules and patch quiz form", fakeAsync(() => {
//     tick();
//     tick();

//     expect(assessmentApi.getUnquizzedModules).toHaveBeenCalledWith(10);
//     expect(component.currentModuleId).toBe(2);
//     expect(component.quizForm.get("moduleId")?.value).toBe(2);
//     expect(component.quizForm.get("title")?.value).toBe("Basics Quiz");
//   }));

  it("should create quiz and disable form", fakeAsync(() => {
    tick(); tick();

    component.quizForm.setValue({
      moduleId: 2,
      title: "Basics Quiz",
      timeLimitMinutes: 10
    });

    component.createQuiz();
    tick();

    expect(assessmentApi.createQuiz).toHaveBeenCalled();
    expect(component.quizCreated).toBeTrue();
    expect(component.createdQuizId).toBe(50);
    expect(component.quizForm.disabled).toBeTrue();
  }));

  it("should add question and reset", fakeAsync(() => {
    tick(); tick();

    component.createdQuizId = 50;

    component.questionForm.setValue({
      text: "Q1",
      marks: 1,
      correctAnswerIndex: 0,
      options: ["A", "B", "C", "D"]
    });

    component.addQuestion();
    tick();

    expect(assessmentApi.addQuestion).toHaveBeenCalled();
    expect(component.questionForm.get("text")?.value).toBe("");
    expect(component.submittedQuestion).toBeFalse();
  }));

  it("completeModule should alert when modules exist", fakeAsync(() => {
    spyOn(window, "alert");

    assessmentApi.getUnquizzedModules.and.returnValue(of([1]));

    component.completeModule();
    tick();

    expect(window.alert).toHaveBeenCalled();
  }));

  it("completeModule should call reloadPage when none missing", fakeAsync(() => {
    spyOn(component, "reloadPage");

    assessmentApi.getUnquizzedModules.and.returnValue(of([]));

    component.completeModule();
    tick();

    expect(component.reloadPage).toHaveBeenCalled();
  }));
});
