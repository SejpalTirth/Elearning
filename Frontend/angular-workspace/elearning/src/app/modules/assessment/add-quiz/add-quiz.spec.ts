import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddQuizComponent } from './add-quiz';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { AssessmentGatewayService, GatewayCourseService } from 'api';
import { ModuleTitlePipe } from '../../../common-modules/pipes/module-title.pipe';

describe('AddQuizComponent', () => {
  let component: AddQuizComponent;
  let fixture: ComponentFixture<AddQuizComponent>;

  // Mocks
  let mockRouter: any;
  let mockActivatedRoute: any;
  let mockToast: any;
  let mockCourseApi: any;
  let mockAssessmentApi: any;

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showInfo: jasmine.createSpy('showInfo'), showError: jasmine.createSpy('showError') };
    
    mockActivatedRoute = {
      snapshot: { paramMap: { get: () => '123' } }
    };

    mockCourseApi = {
      postApiCourseModules: jasmine.createSpy('postApiCourseModules').and.returnValue(of([{ id: 1, title: 'Intro' }]))
    };

    mockAssessmentApi = {
      postApiAssessmentCourseUnquizzedModules: jasmine.createSpy('postApiAssessmentCourseUnquizzedModules').and.returnValue(of([1])),
      postApiAssessmentQuiz: jasmine.createSpy('postApiAssessmentQuiz').and.returnValue(of({ quizId: 500 })),
      postApiAssessmentQuizQuestions: jasmine.createSpy('postApiAssessmentQuizQuestions').and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [AddQuizComponent, ReactiveFormsModule, ModuleTitlePipe],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddQuizComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // Triggers ngOnInit
  });

  it('should initialize with courseId from route and load modules', () => {
    expect(component.courseId).toBe(123);
    expect(mockCourseApi.postApiCourseModules).toHaveBeenCalledWith({ courseId: 123 });
    expect(component.quizForm.value.moduleId).toBe(1);
    expect(component.quizForm.value.title).toBe('Intro Quiz');
  });

  describe('createQuiz()', () => {
    it('should not call API if form is invalid', () => {
      component.quizForm.patchValue({ title: '' });
      component.createQuiz();
      expect(mockAssessmentApi.postApiAssessmentQuiz).not.toHaveBeenCalled();
    });

    it('should lock the form and set quizCreated on success', () => {
      component.quizForm.patchValue({ title: 'New Quiz', moduleId: 1, timeLimitMinutes: 15 });
      component.createQuiz();

      expect(mockAssessmentApi.postApiAssessmentQuiz).toHaveBeenCalled();
      expect(component.quizCreated).toBeTrue();
      expect(component.createdQuizId).toBe(500);
      expect(component.quizForm.disabled).toBeTrue();
      expect(mockToast.showInfo).toHaveBeenCalled();
    });

    it('should show error toast if API fails', () => {
      mockAssessmentApi.postApiAssessmentQuiz.and.returnValue(throwError(() => new Error('API Error')));
      component.quizForm.patchValue({ title: 'Fail Quiz', moduleId: 1 });
      component.createQuiz();

      expect(mockToast.showError).toHaveBeenCalledWith('Failed to create quiz.');
    });
  });

  describe('addQuestion()', () => {
    it('should reset question form and increment count on success', () => {
      component.createdQuizId = 500;
      component.questionForm.patchValue({
        text: 'What is Angular?',
        marks: 5,
        options: ['A', 'B', 'C', 'D'],
        correctAnswerIndex: 1
      });

      component.addQuestion();

      expect(mockAssessmentApi.postApiAssessmentQuizQuestions).toHaveBeenCalled();
      expect(component.questionCount).toBe(1);
      // Verify form reset
      expect(component.questionForm.value.text).toBe('');
      expect(component.submittedQuestion).toBeFalse();
    });
  });

  describe('completeModule()', () => {
    it('should show error if quiz was never created', () => {
      component.quizCreated = false;
      component.completeModule();
      expect(mockToast.showError).toHaveBeenCalledWith('Please create a quiz first.');
    });

    it('should navigate to courses if no more modules are unquizzed', () => {
      component.quizCreated = true;
      component.questionCount = 1;
      mockAssessmentApi.postApiAssessmentCourseUnquizzedModules.and.returnValue(of([]));

      component.completeModule();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
    });
  });
});