import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AssessmentApiService } from '../services/assessment-api';
import { CourseApiService } from '../../CourseService/services/course-api';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-add-quiz',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
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

  constructor(
    private fb: FormBuilder,
    private assessmentApi: AssessmentApiService,
    private courseApi: CourseApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {

    // PHASE 1: get courseId from route
    this.courseId = Number(this.route.snapshot.paramMap.get('courseId'));

    // PHASE 2: load modules for this course
    this.courseApi.getModules(this.courseId).subscribe({
      next: (res: any[]) => (this.modules = res),
      error: (err) => console.error('Module Load Error:', err)
    });

    // PHASE 3: quiz create form
    this.quizForm = this.fb.group({
      moduleId: [''],
      title: [''],
      timeLimitMinutes: [10]
    });

    // ⭐ PHASE 4: first question form
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

  createQuiz() {
    const dto = this.quizForm.value;

    this.assessmentApi.createQuiz(dto).subscribe({
      next: (res: any) => {
        if (res.quizId) {
          this.quizCreated = true;
          this.createdQuizId = res.quizId;
          alert("Quiz created. Add your first question now.");
        }
      },
      error: (err) => console.error('Quiz Create Error:', err)
    });
  }

  addQuestion() {
    const dto = this.questionForm.value;

    this.assessmentApi.addQuestion(this.createdQuizId, dto).subscribe({
      next: () => {
        alert("Question added!");

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
  this.router.navigate(['/courses']);
}


}
