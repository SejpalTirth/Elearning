import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../services/progress.service';
import { Router } from '@angular/router';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

@Component({
  selector: 'app-my-learning',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-learning.html',
  styleUrls: ['./my-learning.css']
})
export class MyLearningComponent implements OnInit {

  courses: any[] = [];
  progress: any[] = [];
  loading = true;
  userId: string | null = null;

  private readonly api = inject(CourseApiService);
  private readonly progressService = inject(ProgressService);
  private readonly router = inject(Router);
  private readonly tokenService = inject(SecureTokenService);

  ngOnInit(): void {
    this.extractUserId();
    this.loadMyCourses();
  }

  extractUserId(): void {
    this.userId = this.tokenService.getUserId();
  }

  loadMyCourses(): void {
    if (!this.userId) {
      this.loading = false;
      return;
    }

    this.progressService.getUserProgress(this.userId).subscribe((p: any) => {
      this.progress = p;

      this.api.getEnrolledCourses(this.userId!).subscribe({
        next: (res) => {
          this.courses = res.map((course: any) => {
            const courseProgressItems = this.progress.filter(pr => pr.courseId === course.id);

            const completedModules = courseProgressItems.length;
            const totalModules = course.modules?.length || 0;

            const percent = totalModules > 0
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
      },
        error: () => (this.loading = false)
      });

    });
  }



  getProgress(courseId: number): number {
    const entries = this.progress.filter(p => p.courseId === courseId);
    if (!entries.length) {return 0;}

    return Math.round(entries.reduce((sum, p) => sum + p.progressPercent, 0) / entries.length);
  }

  continueLearning(courseId: number): void {
    localStorage.setItem('currentCourseId', courseId.toString());
    this.router.navigate([`/courses/${courseId}/modules`]);
  }
}
