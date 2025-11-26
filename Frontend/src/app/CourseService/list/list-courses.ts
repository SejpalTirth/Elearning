  import { Component, OnInit } from '@angular/core';
  import { CommonModule } from '@angular/common';
  import { Router } from '@angular/router';
  import { CourseApiService } from '../services/course-api';

  @Component({
    selector: 'app-list-courses',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './list-courses.html',
    styleUrls: ['./list-courses.css']
  })
  export class ListCoursesComponent implements OnInit {

    courses: any[] = [];
    loading = true;

    constructor(
      private courseApi: CourseApiService,
      private router: Router
    ) {}

    ngOnInit(): void {
      this.loadCourses();
    }

    loadCourses() {
      this.courseApi.getAll().subscribe({
        next: (res) => {
          this.courses = res || [];
          this.loading = false;
        },
        error: (err) => {
          console.error(err);
          this.loading = false;
        }
      });
    }

    openCourse(id: number) {
      this.router.navigate(['/courses', id]);
    }
  }
