import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { AuthStateService } from '@frontend/auth';
import { AssessmentFacade, CourseFacade } from '@frontend/core';
import { ToastService } from '@frontend/ui';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit, OnDestroy {

  userName: string | null = null;
  role: string | null = null;
  userId: string | null = null;

  private readonly authState = inject(AuthStateService);
  private readonly toastService = inject(ToastService);
  private readonly assessmentApi = inject(AssessmentFacade);
  private readonly courseApi = inject(CourseFacade);
  private readonly router = inject(Router);

  private authSub?: Subscription;

  ngOnInit(): void {
    this.authSub = this.authState.status$.subscribe(status => {

  if (status === 'idle') {
    this.router.navigate(['/']);
    return;
  }

  if (status === 'authenticated') {
    const user = this.authState.user;
    if (!user) return;

    this.userId = user.userId;
    this.userName = user.name;
    this.role = user.role;

    if (this.role === 'Instructor') {
      this.checkForPendingTasks();
    }
  }
});

  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  // ------------------ Check If Instructor Has Unfinished Course ------------------
  checkForPendingTasks(): void {
    if (!this.userId)
    {
      return;
    }

    this.courseApi.getUnfinishedCourses().subscribe({
      next: (course: any): void => {
        if (!course?.id)
        {
          return;
        }

        this.assessmentApi.getUnquizzedModules({courseId: course.id}).subscribe({
          next: (modules: number[]): void => {
            if (modules?.length > 0) {
              this.toastService.showError(
                'You have pending course tasks — quizzes need to be completed.'
              );
            }
          }
        });
      }
    });
  }

  goToCourses(): void {
    this.router.navigate(['/courses']);
  }
}