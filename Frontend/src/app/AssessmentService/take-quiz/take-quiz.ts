import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AssessmentApiService } from '../services/assessment-api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProgressService } from '../../CourseService/services/progress.service';

@Component({
  selector: 'app-take-quiz',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './take-quiz.html',
  styleUrls: ['./take-quiz.css']
})
export class TakeQuizComponent implements OnInit {

  moduleId!: number;
  courseId!: number;
  quiz: any = null;
  answers: any[] = [];
  loading = true;
  submitting = false;
  userId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private api: AssessmentApiService,
    private progressService: ProgressService,
    private router: Router
  ) {}

  ngOnInit() {
    this.extractUserId();
    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));

    if (!this.courseId) {
      console.error("No courseId found in storage!");
    }

    this.loadQuiz();
  }

  extractUserId() {
    const token = localStorage.getItem('accessToken');

    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload.sub;
    } catch {}
  }

  loadQuiz() {
    this.api.getQuizForModule(this.moduleId).subscribe({
      next: (res: any) => {
        this.quiz = res;
        this.answers = res.questions.map((q: any) => ({
          questionId: q.questionId,
          selectedAnswerId: null
        }));

        this.loading = false;
      },
      error: () => alert("Failed to load quiz.")
    });
  }

  submit() {
  if (!this.quiz?.quizId || !this.userId) {
    alert("Error — missing quiz or user.");
    return;
  }

  this.submitting = true;

  const payload = {
    quizId: this.quiz.quizId,
    userId: this.userId,
    answers: this.answers
  };

  this.api.submitQuiz(payload).subscribe({
    next: (result: any) => {

      if (result.passed) {

        this.progressService.markModuleCompleted({
          userId: this.userId!,
          moduleId: this.moduleId
        }).subscribe({
          next: () => console.log("Progress updated!"),
          error: (err) => console.error("Progress update failed:", err)
        });

      }

      // Navigate to result page regardless of pass/fail
      this.router.navigate([`/assessment/result/${result.submissionId}`], {
        queryParams: { moduleId: this.moduleId }
      });

    },
    error: () => {
      alert("Submission failed.");
      this.submitting = false;
    }
  });
}



}
