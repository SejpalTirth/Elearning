import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AssessmentApiService } from '../services/assessment-api';
import { CourseApiService } from '../../CourseService/services/course-api';
import { ModuleTitlePipe } from './module-title.pipe';

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

  constructor(
    private fb: FormBuilder,
    private assessmentApi: AssessmentApiService,
    private courseApi: CourseApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {

    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));

    this.courseApi.getModules(this.courseId).subscribe(res => {
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

  loadNextPendingModule() {
    this.assessmentApi.getUnquizzedModules(this.courseId).subscribe(missing => {
      this.unquizzedModules = missing;

      if (!missing || missing.length === 0) {
        this.currentModuleId = null;
        return;
      }

      this.currentModuleId = missing[0];
      const found = this.modules.find(m => m.id === this.currentModuleId);

      this.quizForm.patchValue({
        moduleId: this.currentModuleId,
        title: found ? `${found.title} Quiz` : ''
      });
    });
  }

  createQuiz() {
    this.submittedQuiz = true;

    if (this.quizForm.invalid) return;

    this.assessmentApi.createQuiz(this.quizForm.value).subscribe({
      next: (res: any) => {
        if (res.quizId) {
          this.quizCreated = true;
          this.createdQuizId = res.quizId;

          // Lock form immediately
          this.quizForm.disable();
        }
      },
      error: err => console.error(err)
    });
  }

  addQuestion() {
    this.submittedQuestion = true;

    if (this.questionForm.invalid) return;

    this.assessmentApi.addQuestion(this.createdQuizId, this.questionForm.value).subscribe(() => {

      this.questionForm.reset({
        text: '',
        marks: 1,
        correctAnswerIndex: 0,
        options: ['', '', '', '']
      });

      this.submittedQuestion = false;
    });
  }

  completeModule() {
    this.assessmentApi.getUnquizzedModules(this.courseId).subscribe(missing => {

      if (missing && missing.length > 0) {
        alert("Please complete all quizzes for this module before continuing.");
        return;
      }

      this.reloadPage();
    });
  }

  reloadPage() {
    window.location.reload();
  }
}
