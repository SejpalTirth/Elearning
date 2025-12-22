import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ToastService } from '../../shared/toast.service';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';
import { CourseApiService } from '../../CourseService/services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';
import { LoadingService } from 'app/shared/loading/LoadingService';

@Component({
  selector: 'app-pending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pending.html',
  styleUrls: ['./pending.css']
})
export class Pending implements OnInit, OnDestroy {

  userId: string | null = null;
  pendingCourses: any[] = [];

  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly authState = inject(AuthStateService);
  private readonly courseApi = inject(CourseApiService);
  private readonly assessmentApi = inject(AssessmentApiService);
  private readonly loading = inject(LoadingService);

  private authSub?: Subscription;

  ngOnInit(): void {
    this.authSub = this.authState.user$.subscribe(user => {
      if (!user) {
        this.pendingCourses = [];
        return;
      }

      this.userId = user.userId;
      this.loading.show();
      this.loadUnfinishedCourses();
    });
  }


  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  // ------------------------------------------
  // Load ALL unfinished (draft) courses
  // ------------------------------------------
  loadUnfinishedCourses(): void {
  if (!this.userId) {
    this.loading.hide();
    return;
  }

  this.courseApi.getInstructorUnfinishedCourses().subscribe({
    next: (courses: any[]) => {
        this.pendingCourses = (courses || []).filter(c => !c.isDeleted);

        if (this.pendingCourses.length === 0) {
          this.loading.hide();
          return;
        }
        Promise
          .all(this.pendingCourses.map(c => this.loadPendingModules(c)))
          .finally(() => this.loading.hide());
        },
        error: () => {
          this.pendingCourses = [];
          this.loading.hide();
        }
    });
  }


  // ------------------------------------------
  // Load quiz status per course (NEW)
  // ------------------------------------------
  loadPendingModules(course: any): void {
    this.assessmentApi.getQuizStatus(course.id).subscribe({
      next: (status: any) => {
        const pending = status.modules.filter(
          (m: any) => !m.quizExists
        );

        course.pendingModules = pending;          // ← module objects
        course.allQuizzesDone = pending.length === 0;
        course.nextPendingModuleId = status.nextPendingModuleId;
        this.loading.hide();
      },
      error: () => {
        course.pendingModules = [];
        course.allQuizzesDone = false;
        this.loading.hide();
      }
    });
  }

  // ------------------------------------------
  // Navigate to quiz creation
  // ------------------------------------------
  continueQuiz(courseId: number): void {
    this.router.navigate(['/assessment/add-quiz', courseId]);
  }

  // ------------------------------------------
  // Publish course
  // ------------------------------------------
  publishCourse(courseId: number): void {
    this.courseApi.publishCourse(courseId).subscribe({
      next: () => {
        this.toastService.showSuccess('Course published successfully!');
        this.loadUnfinishedCourses();
      },
      error: () => {
        this.toastService.showError('Failed to publish the course.');
      }
    });
  }
}
