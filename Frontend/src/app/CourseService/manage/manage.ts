import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CourseApiService } from '../services/course-api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage.html',
  styleUrls: ['./manage.css'],
})
export class ManageCoursesComponent implements OnInit {

  courses: any[] = [];
  token: string | null = null;
  userId: string | null = null;

  loading = true;

  constructor(private courseApi: CourseApiService, private router: Router) {}

  ngOnInit(): void {
    console.log('Manage component loaded');

    this.token = localStorage.getItem('token');

    if (!this.token) {
      console.warn('No token found, user may not be logged in.');
      this.loading = false;
      return;
    }

    // 🔥 decode JWT to extract "sub"
    this.userId = this.getUserIdFromToken(this.token);
    console.log("Extracted userId:", this.userId);

    if (!this.userId) {
      console.error("⚠ Could not extract userId from token");
      this.loading = false;
      return;
    }

    this.loadCourses();
  }

  getUserIdFromToken(token: string): string | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub ?? null;
    } catch (e) {
      console.error("JWT Parse Error:", e);
      return null;
    }
  }

  loadCourses() {
    this.courseApi.getCoursesByInstructor(this.userId!).subscribe({
      next: (res: any[]) => {
        console.log("Loaded courses:", res);
        this.courses = res;
        this.loading = false;
      },
      error: (err) => {
        console.error("API Error:", err);
        this.loading = false;
      }
    });
  }

  editCourse(id: number) {
    this.router.navigate(['/courses/edit', id]);
  }

  viewCourse(id: number) {
    this.router.navigate(['/courses', id]);
  }
}
