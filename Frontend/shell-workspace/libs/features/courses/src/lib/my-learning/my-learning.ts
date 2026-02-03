import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthStateService } from '@frontend/auth';
import { combineLatest, Subscription } from 'rxjs';
import { LoadingService } from '@frontend/ui';
import { GatewayCourseService, ProgressGatewayService } from '@frontend/api';

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

  private readonly courseapi = inject(GatewayCourseService);
  private readonly progressService = inject(ProgressGatewayService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly load = inject(LoadingService);

  private sub = new Subscription();

  ngOnInit(): void {
    this.sub.add(
      this.authState.user$.subscribe(user => {
        if (!user) {
          this.courses = [];
          this.loading = false;
          return;
        }

        this.loading = true;
        this.load.show();

        this.bindCoursesWithProgress();
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  // ===============================
  // BIND COURSES + PROGRESS
  // ===============================
  private bindCoursesWithProgress(): void {

    this.loading = true;

    this.sub.add(
      combineLatest([
        this.courseapi.postApiCourseEnrolled(),
        this.progressService.postApiProgressUser()
      ]).subscribe({
        next: ([courses, progress]) => {

          this.courses = courses.map((course: any) => {

            const completedModules = progress.filter(
              p => p.courseId === course.id && p.isCompleted
            ).length;

            const totalModules = course.modules?.length ?? 0;

            const rawpercent =
              totalModules > 0
                ? Math.round((completedModules / totalModules) * 100)
                : 0;

            const percent = Math.min(100, rawpercent);

            return {
              ...course,
              progressPercent: percent
            };
          });

          this.loading = false;
          this.load.hide();
        },
        error: () => {
          this.loading = false;
          this.load.hide();
        }
      })
    );
  }

  continueLearning(courseId: number): void {
    this.router.navigate([`/courses/${courseId}/modules`]);
  }
}
