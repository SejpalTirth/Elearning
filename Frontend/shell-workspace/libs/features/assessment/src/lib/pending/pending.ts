import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ToastService, LoadingService } from '@frontend/ui';
import { AuthStateService } from '@frontend/auth';
import { AssessmentGatewayService, GatewayCourseService } from '@frontend/api';

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
  private readonly loading = inject(LoadingService);
  private readonly courseapi = inject(GatewayCourseService);
  private readonly assessmentapi = inject(AssessmentGatewayService);

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

  this.courseapi.postApiCourseUnfinished().subscribe({
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
    this.assessmentapi.postApiAssessmentCourseUnquizzedModules({courseId: course.id}).subscribe({
      next: (status: any) => {
        const pending = status.modules.filter(
          (m: any) => !m.quizExists
        );

        course.pendingModules = pending;
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
    this.courseapi.postApiCoursePublish({courseId}).subscribe({
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