import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AssessmentApiService } from '../services/assessment-api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProgressService } from '../../CourseService/services/progress.service';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

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

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AssessmentApiService);
  private readonly progressService = inject(ProgressService);
  private readonly router = inject(Router);
  private readonly tokenService = inject(SecureTokenService);

  ngOnInit(): void {
    this.extractUserId();
    history.replaceState(null, '');
    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));

    if (!this.courseId) {
      console.error('No course Id found for the quiz.');
    }

    this.loadQuiz();
  }

  extractUserId(): void {
    this.userId = this.tokenService.getUserId();
  }

  loadQuiz(): void {
    this.api.getQuizForModule(this.moduleId).subscribe({
      next: (res: any) => {
        this.quiz = res;
        this.answers = res.questions.map((q: any) => ({
          questionId: q.questionId,
          selectedAnswerId: null
        }));
        this.loading = false;
      },
      // eslint-disable-next-line no-alert
      error: () => window.alert('Failed to load quiz.')
    });
  }

  submit(): void {
    if (!this.quiz?.quizId || !this.userId) {
      return;
    }

    const unanswered = this.answers.filter(a => a.selectedAnswerId === null);
    if (unanswered.length > 0) {
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
            // eslint-disable-next-line no-console
            next: () => console.log('Module marked completed'),
            error: (err) => console.error('Failed to mark module completed', err)
          });
        }

        this.router.navigate(
          [`/assessment/result/${result.submissionId}`],
          {
            queryParams: { moduleId: this.moduleId },
            replaceUrl: true
          }
        );
      },
      error: () => {
        // eslint-disable-next-line no-alert
        window.alert('Submission failed.');
        this.submitting = false;
      }
    });
  }

  allQuestionsAnswered(): boolean {
    return this.answers.every(a => a.selectedAnswerId !== null && a.selectedAnswerId !== undefined);
  }
}
