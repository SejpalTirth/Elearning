import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AssessmentApiService } from '../services/assessment-api';
import { CourseApiService } from '../../CourseService/services/course-api';
import { ModuleTitlePipe } from './module-title.pipe';

@Component({
  selector: 'app-add-quiz',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModuleTitlePipe],
  templateUrl: './add-quiz.html',
  styleUrls: ['./add-quiz.css']
})
export class AddQuizComponent implements OnInit {

  modules: any[] = [];
  quizForm!: FormGroup;
  questionForm!: FormGroup;

  quizCreated = false;
  createdQuizId = 0;
  courseId = 0;

  currentModuleId: number | null = null;
  currentModuleTitle: string = "";

  constructor(
    private fb: FormBuilder,
    private assessmentApi: AssessmentApiService,
    private courseApi: CourseApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {

    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));

    // Load course modules
    this.courseApi.getModules(this.courseId).subscribe({
      next: (res: any[]) => {
        this.modules = res;
        this.loadNextPendingModule();
      },
      error: (err) => console.error('Module Load Error:', err)
    });

    // Quiz Details form
    this.quizForm = this.fb.group({
      moduleId: [''],
      title: [''],
      timeLimitMinutes: [10]
    });

    // First Question form
    this.questionForm = this.fb.group({
      text: [''],
      marks: [1],
      options: this.fb.array([
        this.fb.control(''),
        this.fb.control(''),
        this.fb.control(''),
        this.fb.control('')
      ]),
      correctAnswerIndex: [0]
    });
  }

  get options(): FormArray {
    return this.questionForm.get('options') as FormArray;
  }

  // AUTO SELECT NEXT MODULE WITHOUT QUIZ
  loadNextPendingModule() {
    this.assessmentApi.getUnquizzedModules(this.courseId).subscribe({
      next: (missing: number[]) => {
        if (!missing || missing.length === 0) {
          this.currentModuleId = null;
          this.currentModuleTitle = "";
          return;
        }

        this.currentModuleId = missing[0];

        const moduleObj = this.modules.find(x => x.id === this.currentModuleId);
        this.currentModuleTitle = moduleObj ? moduleObj.title : "";

        // Prefill form automatically
        this.quizForm.patchValue({
          moduleId: this.currentModuleId,
          title: `${this.currentModuleTitle} Quiz`
        });
      },
      error: err => console.error(err)
    });
  }

  createQuiz() {
    this.assessmentApi.createQuiz(this.quizForm.value).subscribe({
      next: (res: any) => {
        if (res.quizId) {
          this.quizCreated = true;
          this.createdQuizId = res.quizId;
        }
      },
      error: (err) => console.error('Quiz Create Error:', err)
    });
  }

  addQuestion() {
    this.assessmentApi.addQuestion(this.createdQuizId, this.questionForm.value).subscribe({
      next: () => {
        this.questionForm.reset({
          text: '',
          marks: 1,
          correctAnswerIndex: 0,
          options: ['', '', '', '']
        });
      },
      error: (err) => console.error('Add Question Error:', err)
    });
  }

  exit() {
    this.assessmentApi.getUnquizzedModules(this.courseId).subscribe({
      next: (missing) => {
        if (missing && missing.length > 0) {
          alert("You must complete quizzes for all modules before exiting.");
          return;
        }

        // All quizzes done → redirect
        this.router.navigate(['/courses']);
      }
    });
  }
}
