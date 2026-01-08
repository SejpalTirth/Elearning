import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { LoadingService } from '@frontend/ui';
import { ToastService } from '@frontend/ui';

@Component({
  selector: 'app-module-content',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './module-content.html',
  styleUrls: ['./module-content.css']
})
export class ModuleContentComponent implements OnInit, OnDestroy {

  module: any = null;
  quizStatus: any = null;
  moduleId!: number;
  userId: string | null = null;

  private authSub?: Subscription;

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CourseFacade);
  private readonly assessmentApi = inject(AssessmentFacade);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  private readonly loading = inject(LoadingService);
  private readonly toast = inject(ToastService);

  ngOnInit(): void {
    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));

    this.authSub = this.authState.user$.subscribe(user => {
      this.userId = user?.userId ?? null;
      if (this.userId) {
        this.loadData();
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  loadData(): void {
    this.loading.show();

    forkJoin({
      module: this.api.getCourseModule({moduleId: this.moduleId})
        .pipe(catchError(() => of(null))),
      quiz: this.assessmentApi.getQuizForModule({moduleId: this.moduleId})
        .pipe(catchError(() => of(null)))
    }).subscribe(({ module, quiz }) => {
      this.module = module;
      this.quizStatus = quiz;
      this.loading.hide();
    });
  }

  takeQuiz(): void {
    if (!this.quizStatus?.quizId) {
      this.toast.showError('No quiz available.');
      return;
    }

    this.router.navigate([`/quiz/${this.moduleId}`]);
  }
}