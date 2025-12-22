import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../services/progress.service';
import { Router } from '@angular/router';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';
import { combineLatest, Subscription } from 'rxjs';
import { LoadingService } from 'app/shared/loading/LoadingService';

@Component({
  selector: 'app-my-learning',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-learning.html',
  styleUrls: ['./my-learning.css']
})
export class MyLearningComponent implements OnInit, OnDestroy {

  courses: any[] = [];
  loading = true;

  private readonly api = inject(CourseApiService);
  private readonly progressService = inject(ProgressService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly loadingservice = inject(LoadingService);

  private sub?: Subscription;

  ngOnInit(): void {

    this.sub = this.authState.user$.subscribe(user => {

      if (!user) {
        this.courses = [];
        this.loading = false;
        return;
      }
      this.loadingservice.show();
      this.progressService.loadUserProgress().subscribe();

      this.bindCoursesWithProgress();
      this.loadingservice.hide();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  // =====================================================
  //  BIND COURSES + PROGRESS (REACTIVE)
  // =====================================================
  private bindCoursesWithProgress(): void {

    this.loading = true;
    this.loadingservice.show();

    this.sub = combineLatest([
      this.api.getEnrolledCourses(),
      this.progressService.progress$
    ]).subscribe({
      next: ([courses, progress]) => {

        this.courses = courses.map((course: any) => {

          const courseProgress = progress.filter(
            p => p.courseId === course.id && p.isCompleted
          );

          const completedModules = courseProgress.length;
          const totalModules = course.modules?.length ?? 0;

          const percent =
            totalModules > 0
              ? Math.min(100, Math.round((completedModules / totalModules) * 100))
              : 0;

          return {
            ...course,
            progressPercent: percent,
            completedModules,
            totalModules
          };
        });

        this.loading = false;
        this.loadingservice.hide();
      },
      error: () => (this.loading = false, this.loadingservice.hide())
    });
  }

  // =====================================================
  //  NAVIGATION
  // =====================================================
  continueLearning(courseId: number): void {
    this.router.navigate([`/courses/${courseId}/modules`]);
  }
}
