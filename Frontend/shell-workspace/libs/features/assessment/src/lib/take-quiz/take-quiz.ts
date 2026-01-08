import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { AssessmentFacade, ProgressService } from '@frontend/core'
import { AuthStateService } from '@frontend/auth'
import { ToastService } from '@frontend/ui'

@Component({
  selector: 'app-take-quiz',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './take-quiz.html',
  styleUrls: ['./take-quiz.css']
})
export class TakeQuizComponent implements OnInit, OnDestroy {

  moduleId!: number;
  quiz: any = null;
  answers: {
    questionId: number;
    selectedAnswerId: number | undefined;
  }[] = [];

  loading = true;
  submitting = false;
  userId: string | null = null;

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AssessmentFacade);
  private readonly progressService = inject(ProgressService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly toast = inject(ToastService);

  private authSub?: Subscription;

  ngOnInit(): void {

    this.authSub = this.authState.user$.subscribe(user => {
      this.userId = user?.userId ?? null;
    });

    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));
    if (!this.moduleId) {
      console.error('No moduleId found');
      return;
    }

    this.loadQuiz();
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  // ---------------- LOAD QUIZ ----------------

  loadQuiz(): void {
    this.api.getQuizForModule({moduleId: this.moduleId}).subscribe({
      next: (res: any) => {
        this.quiz = res;

        this.answers = res.questions.map((q: any) => ({
          questionId: q.id,
          selectedAnswerId: undefined
        }));


        this.loading = false;
      },
      error: () => {
        this.toast.showError('Failed to load quiz');
        this.loading = false;
      }
    });
  }

  // ---------------- SUBMIT ----------------

  submit(): void {
    if (!this.quiz?.quizId || !this.userId) {
      return;
    }

    if (!this.allQuestionsAnswered()) {
      this.toast.showError('Please answer all questions.')
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
          this.progressService
            .completeModule({moduleId: this.moduleId})
            .subscribe();
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
        this.toast.showError('Submission failed.');
        this.submitting = false;
      }
    });
  }

  // ---------------- HELPERS ----------------

  allQuestionsAnswered(): boolean {
    return this.answers.every(
      a => typeof a.selectedAnswerId === 'number'
    );
  }

}