import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthStateService } from '@frontend/auth';
import { GatewayCourseService } from '@frontend/api';

@Component({
  selector: 'lib-instructor-course-list',
  imports: [],
  templateUrl: './instructor-course-list.html',
  styleUrl: './instructor-course-list.css',
})
export class InstructorCourseList implements OnInit, OnDestroy{

  courses: any[] = [];
  userId: string | null = null;
  loading = true;

  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly courseapi = inject(GatewayCourseService);

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

    this.courseapi.postApiCourseInstructor().subscribe({
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
