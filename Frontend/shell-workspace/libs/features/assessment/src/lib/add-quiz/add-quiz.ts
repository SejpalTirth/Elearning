import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { ModuleTitlePipe } from '@frontend/core';
import { ToastService } from '@frontend/ui';

@Component({
  selector: 'app-add-quiz',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ModuleTitlePipe
  ],
  templateUrl: './add-quiz.html',
  styleUrls: ['./add-quiz.css']
})
export class AddQuizComponent implements OnInit {

  modules: any[] = [];
  unquizzedModules: number[] = [];

  quizForm!: FormGroup;
  questionForm!: FormGroup;

  submittedQuiz = false;
  submittedQuestion = false;

  quizCreated = false;
  createdQuizId = 0;
  courseId = 0;

  currentModuleId: number | null = null;
  questionCount = 0;

  private readonly fb = inject(FormBuilder);
  private readonly assessmentApi = inject(AssessmentFacade);
  private readonly courseApi = inject(CourseFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  ngOnInit(): void {

    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));

    this.courseApi.getCourseModules({courseId: this.courseId}).subscribe(res => {
      this.modules = res;
      this.loadNextPendingModule();
    });

    this.quizForm = this.fb.group({
      moduleId: ['', Validators.required],
      title: ['', Validators.required],
      timeLimitMinutes: [10, [Validators.required, Validators.min(1)]]
    });

    this.questionForm = this.fb.group({
      text: ['', Validators.required],
      marks: [1, [Validators.required, Validators.min(1)]],
      options: this.fb.array([
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required)
      ]),
      correctAnswerIndex: [0, Validators.required]
    });
  }

  get options(): FormArray {
    return this.questionForm.get('options') as FormArray;
  }

  loadNextPendingModule(): void {
    this.assessmentApi.getUnquizzedModules({courseId: this.courseId}).subscribe(missing => {
      this.unquizzedModules = missing;

      if (!missing || missing.length === 0) {
        this.currentModuleId = null;
        this.router.navigate(['/courses']);
        return;
      }

      this.currentModuleId = missing[0];
      const found = this.modules.find(m => m.id === this.currentModuleId);

      this.quizForm.patchValue({
        moduleId: this.currentModuleId,
        title: found ? `${found.title} Quiz` : ''
      });

      this.quizCreated = false;
      this.createdQuizId = 0;
      this.questionCount = 0;

      this.quizForm.enable();
    });
  }

  createQuiz(): void {
  this.submittedQuiz = true;

  if (this.quizForm.invalid) {
    return;
  }

  this.assessmentApi.createQuiz(this.quizForm.value).subscribe({
    next: (res: any) => {

      const quizId = res?.quizId ?? res?.id;

      if (!quizId) {
        this.toast.showError('Failed to create quiz.');
        return;
      }

      this.toast.showInfo(
        'Quiz details are now locked. You cannot change quiz information after creation.'
      );

      this.quizCreated = true;
      this.createdQuizId = quizId;

      this.quizForm.disable();
    },
    error: () => {
      this.toast.showError('Failed to create quiz.');
    }
  });
}


  /** Add question */
  addQuestion(): void {
    this.submittedQuestion = true;

    if (this.questionForm.invalid) {
      return;
    }

    const payload = {
      quizId: this.createdQuizId,
      question: {
        question: this.questionForm.value.text,
        marks: this.questionForm.value.marks,
        options: this.questionForm.value.options,
        correctAnswerIndex: this.questionForm.value.correctAnswerIndex
      }
    };

    this.assessmentApi.addQuestion(payload).subscribe(() => {
      this.questionCount++;

      this.questionForm.reset({
        text: '',
        marks: 1,
        correctAnswerIndex: 0,
        options: ['', '', '', '']
      });

      this.submittedQuestion = false;
    });
  }

  /** Complete module */
  completeModule(): void {

    if (!this.quizCreated) {
      this.toast.showError('Please create a quiz first.');
      return;
    }

    if (this.questionCount < 1) {
      this.toast.showError('Please add at least one question to the quiz.');
      return;
    }

    this.assessmentApi.getUnquizzedModules({courseId: this.courseId}).subscribe(missing => {
      if (missing.includes(this.currentModuleId!)) {
        this.toast.showError('Please finish quiz creation for this module.');
        return;
      }

      this.loadNextPendingModule();
    });
  }
}