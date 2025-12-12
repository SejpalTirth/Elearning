import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CourseApiService } from '../services/course-api';
import { CommonModule } from '@angular/common';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

@Component({
  selector: 'app-manage-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage.html',
  styleUrls: ['./manage.css'],
})
export class ManageCoursesComponent implements OnInit {

  courses: any[] = [];
  userId: string | null = null;
  loading = true;

  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);
  private readonly tokenService = inject(SecureTokenService);

  ngOnInit(): void {

    this.userId = this.tokenService.getUserId();

    if (!this.userId) {
      this.loading = false;
      return;
    }

    this.loadCourses();
  }

  loadCourses():void {
    this.courseApi.getCoursesByInstructor(this.userId!).subscribe({
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
    if (isDeleted) {return;} 
    this.router.navigate(['/courses/edit', id]);
  }

  viewCourse(id: number): void {
    this.router.navigate(['/courses', id]);
  }

  isDeleted(course: any): boolean {
    return course.isDeleted === true;
  }

}
