import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { ToastService } from '../../shared/toast.service';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

import { CourseApiService } from '../../CourseService/services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';

@Component({
  selector: 'app-pending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pending.html',
  styleUrls: ['./pending.css']
})
export class Pending implements OnInit {

  userId: string | null = null;
  pendingCourses: any[] = [];

  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly tokenService = inject(SecureTokenService);
  private readonly courseApi = inject(CourseApiService);
  private readonly assessmentApi = inject(AssessmentApiService);

  ngOnInit(): void {
    this.userId = this.tokenService.getUserId();
    this.loadUnfinishedCourses();
  }

  // ------------------------------------------
  // Load ALL unfinished (draft) courses
  // ------------------------------------------
  loadUnfinishedCourses(): void {
    if (!this.userId) {return;}

    this.courseApi.getInstructorUnfinishedCourses(this.userId).subscribe({
      next: (courses: any[]) => {
        if (!courses || courses.length === 0) {
          this.pendingCourses = [];
          return;
        }

        // Safety: Filter out admin-deleted courses
        this.pendingCourses = courses.filter(c => !c.isDeleted);

        // Load module quiz status
        this.pendingCourses.forEach(course => this.loadPendingModules(course));
      },
      error: () => {
        this.pendingCourses = [];
      }
    });
  }

  // ------------------------------------------
  // Load missing quizzes for each course
  // ------------------------------------------
  loadPendingModules(course: any): void {
    this.assessmentApi.getUnquizzedModules(course.id).subscribe({
      next: (moduleIds: number[]) => {
        course.pendingModules = moduleIds || [];
        course.allQuizzesDone = course.pendingModules.length === 0;
      },
      error: () => {
        course.pendingModules = [];
        course.allQuizzesDone = false;
      }
    });
  }

  // ------------------------------------------
  // Navigate to add quiz
  // ------------------------------------------
  continueQuiz(courseId: number): void {
    this.router.navigate(['/assessment/add-quiz', courseId]);
  }

  // ------------------------------------------
  // Publish this specific course
  // ------------------------------------------
  publishCourse(courseId: number): void {
    this.courseApi.publishCourse(courseId).subscribe({
      next: () => {
        this.toastService.showSuccess('Course published successfully! 🚀');
        this.loadUnfinishedCourses();
      },
      error: () => {
        this.toastService.showError('Failed to publish the course.');
      }
    });
  }
}
