import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CourseApiService } from '../services/course-api';
import { CommonModule } from '@angular/common';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-manage-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage.html',
  styleUrls: ['./manage.css'],
})
export class ManageCoursesComponent implements OnInit, OnDestroy {

  courses: any[] = [];
  userId: string | null = null;
  loading = true;

  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);

  private authSub?: Subscription;

  ngOnInit(): void {

    this.authSub = this.authState.user$.subscribe(user => {

      if (!user) {
        this.userId = null;
        this.courses = [];
        this.loading = false;
        return;
      }

      this.userId = user.userId;
      this.loadCourses();
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  loadCourses(): void {
    if (!this.userId) 
    {
      return;
    }
    this.loading = true;

    this.courseApi.getCoursesByInstructor().subscribe({
      next: (res: any[]) => {
        this.courses = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('API Error:', err);
        this.loading = false;
      }
    });
  }

  editCourse(id: number, isDeleted: boolean): void {
    if (isDeleted)
    {
      return;
    }
    this.router.navigate(['/courses/edit', id]);
  }

  viewCourse(id: number): void {
    this.router.navigate(['/courses', id]);
  }

  isDeleted(course: any): boolean {
    return course.isDeleted === true;
  }
}
