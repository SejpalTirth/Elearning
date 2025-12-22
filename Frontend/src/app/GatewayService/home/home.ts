import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastService } from 'app/shared/toast.service';
import { CourseApiService } from '../../CourseService/services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';
import { AuthStateService } from '../Auth/auth-state.service';
import { Subscription } from 'rxjs';

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
  private readonly assessmentApi = inject(AssessmentApiService);
  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);

  private authSub?: Subscription;

  ngOnInit(): void {
    this.authSub = this.authState.user$.subscribe(user => {

      if (!user) {
        this.userId = null;
        this.userName = null;
        this.role = null;
        this.router.navigate(['/']);
        return;
      }

      this.userId = user.userId;
      this.userName = user.name;
      this.role = user.role;

      if (this.role === 'Instructor') {
        this.checkForPendingTasks();
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

    this.courseApi.getInstructorUnfinishedCourses().subscribe({
      next: (course: any): void => {
        if (!course?.id)
        {
          return;
        }

        this.assessmentApi.getUnquizzedModules(course.id).subscribe({
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
