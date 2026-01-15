import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AddQuizComponent } from './add-quiz';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { ToastService } from '@frontend/ui';
import { Router } from '@angular/router';

describe('AddQuizComponent', () => {
  let component: AddQuizComponent;
  let fixture: ComponentFixture<AddQuizComponent>;
  let courseApi: jasmine.SpyObj<CourseFacade>;
  let assessmentApi: jasmine.SpyObj<AssessmentFacade>;
  let toast: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const courseSpy = jasmine.createSpyObj('CourseFacade', ['getCourseModules']);
    const assessmentSpy = jasmine.createSpyObj('AssessmentFacade', ['getUnquizzedModules', 'createQuiz', 'addQuestion']);
    const toastSpy = jasmine.createSpyObj('ToastService', ['showError', 'showInfo']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      providers: [
        ...provideCommonMocks,
        { provide: CourseFacade, useValue: courseSpy },
        { provide: AssessmentFacade, useValue: assessmentSpy },
        { provide: ToastService, useValue: toastSpy },
        { provide: Router, useValue: routerSpy }
      ],
      declarations: [AddQuizComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AddQuizComponent);
    component = fixture.componentInstance;

    courseApi = TestBed.inject(CourseFacade) as jasmine.SpyObj<CourseFacade>;
    assessmentApi = TestBed.inject(AssessmentFacade) as jasmine.SpyObj<AssessmentFacade>;
    toast = TestBed.inject(ToastService) as jasmine.SpyObj<ToastService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Default mock responses
    courseApi.getCourseModules.and.returnValue(of([
      { id: 1, title: 'Module 1' },
      { id: 2, title: 'Module 2' }
    ]));
    assessmentApi.getUnquizzedModules.and.returnValue(of([1, 2]));
    assessmentApi.createQuiz.and.returnValue(of(void 0));
    assessmentApi.addQuestion.and.returnValue(of(void 0));

    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize quizForm and questionForm', () => {
    expect(component.quizForm).toBeDefined();
    expect(component.questionForm).toBeDefined();
  });

  it('should load next pending module on init', fakeAsync(() => {
    component.loadNextPendingModule();
    tick();
    expect(component.currentModuleId).toBe(1);
    expect(component.quizForm.value.moduleId).toBe(1);
    expect(component.quizCreated).toBeFalse();
  }));

  it('should create a quiz successfully', fakeAsync(() => {
    component.quizForm.patchValue({
      moduleId: 1,
      title: 'Test Quiz',
      timeLimitMinutes: 10
    });
    component.createQuiz();
    tick();
    expect(assessmentApi.createQuiz).toHaveBeenCalledWith(component.quizForm.value);
    expect(component.quizCreated).toBeTrue();
    expect(component.createdQuizId).toBe(123);
  }));

  it('should show error if createQuiz fails', fakeAsync(() => {
    assessmentApi.createQuiz.and.returnValue(of(void 0));
    component.quizForm.patchValue({
      moduleId: 1,
      title: 'Test Quiz',
      timeLimitMinutes: 10
    });
    component.createQuiz();
    tick();
    expect(toast.showError).toHaveBeenCalledWith('Failed to create quiz.');
  }));

  it('should add a question correctly', fakeAsync(() => {
    component.createdQuizId = 123;
    component.questionForm.patchValue({
      text: 'Question 1',
      marks: 1,
      options: ['a', 'b', 'c', 'd'],
      correctAnswerIndex: 0
    });
    component.addQuestion();
    tick();
    expect(assessmentApi.addQuestion).toHaveBeenCalled();
    expect(component.questionCount).toBe(1);
  }));

  it('should show error if quiz is not created before completing module', fakeAsync(() => {
    component.quizCreated = false;
    component.completeModule();
    tick();
    expect(toast.showError).toHaveBeenCalledWith('Please create a quiz first.');
  }));

  it('should show error if no questions are added before completing module', fakeAsync(() => {
    component.quizCreated = true;
    component.questionCount = 0;
    component.completeModule();
    tick();
    expect(toast.showError).toHaveBeenCalledWith('Please add at least one question to the quiz.');
  }));

  it('should navigate to next module if quiz completed', fakeAsync(() => {
    component.quizCreated = true;
    component.questionCount = 1;
    component.currentModuleId = 1;
    assessmentApi.getUnquizzedModules.and.returnValue(of([2])); // next module is 2

    component.completeModule();
    tick();
    expect(component.currentModuleId).toBe(2);
    expect(router.navigate).not.toHaveBeenCalled(); // navigation only if no modules left
  }));

  it('should navigate to /courses if no modules left', fakeAsync(() => {
    component.quizCreated = true;
    component.questionCount = 1;
    component.currentModuleId = 1;
    assessmentApi.getUnquizzedModules.and.returnValue(of([])); // no modules left

    component.completeModule();
    tick();
    expect(router.navigate).toHaveBeenCalledWith(['/courses']);
  }));
});
