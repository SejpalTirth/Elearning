import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';

import { AuthStateService } from '@frontend/auth';
import { AssessmentGatewayService, GatewayCourseService } from '@frontend/api';

@Component({
  selector: 'feature-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {

  userName: string | null = null;
  role: string | null = null;
  userId: string | null = null;

  private readonly authState = inject(AuthStateService);
  private readonly courseapi = inject(GatewayCourseService);
  private readonly router = inject(Router);
  private readonly assessmentapi = inject(AssessmentGatewayService);

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

  private checkForPendingTasks(): void {
    if (!this.userId) return;

    this.courseapi.postApiCourseUnfinished().subscribe({
      next: (course: any) => {
        if (!course?.id) return;

        this.assessmentapi.postApiAssessmentCourseUnquizzedModules(course.id).subscribe();
      }
    });
  }

  goToCourses(): void {
    this.router.navigate(['/courses']);
  }
}
