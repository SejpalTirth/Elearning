/* eslint-disable no-alert */
import { Component, inject, OnInit, signal } from '@angular/core';
import { CourseApiService } from '../services/course-api';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-courses.html',
  styleUrls: ['./manage-courses.css']
})
export class ManageCoursesComponent implements OnInit {

  courses = signal<any[]>([]);
  loading = signal(true);

  private readonly api = inject(CourseApiService);
  private readonly router = inject(Router);

  ngOnInit():void {
    this.loadCourses();
  }

  loadCourses():void {
    this.api.getAll().subscribe({
      next: res => {
        this.courses.set(res);
        this.loading.set(false);
      },
      error: err => console.error(err)
    });
  }

  editCourse(id: number, isDeleted: boolean):void {
    if (isDeleted) {return;}
    this.router.navigate([`/courses/edit/${id}`]);
  }

  deleteCourse(id: number):void {
    if (!confirm('Are you sure you want to archive this course?')) { return; }

    this.api.deleteCourse(id).subscribe({
      next: () => {
        alert('Course archived (soft deleted) successfully!');
        this.loadCourses();
      },
      error: err => console.error(err)
    });
  }

  restoreCourse(id: number): void {
    this.api.restoreCourse(id).subscribe({
      next: () => {
         
        alert('Course restored successfully!');
        this.loadCourses();
      },
      error: err => console.error(err)
    });
  }

  trackById(index: number, item: any): number {
    return item.id;
  }
}
