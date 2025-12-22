import { Component, inject, OnInit, signal } from '@angular/core';
import { CourseApiService } from '../services/course-api';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastService } from 'app/shared/toast.service';
import { LoadingService } from 'app/shared/loading/LoadingService';

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
  private readonly toast = inject(ToastService);
  private readonly loader = inject(LoadingService);

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
    // eslint-disable-next-line no-alert
    if (!confirm('Are you sure you want to archive this course?')) { return; }

    this.api.deleteCourse(id).subscribe({
      next: () => {
        this.toast.showSuccess('Course archived (soft deleted) successfully!');
        this.loader.show();
        this.loadCourses();
        this.loader.hide();
      },
      error: err => console.error(err)
    });
  }

  restoreCourse(id: number): void {
    this.api.restoreCourse(id).subscribe({
      next: () => {
        this.toast.showSuccess('Course restored successfully!');
        this.loader.show();
        this.loadCourses();
        this.loader.hide();
      },
      error: err => console.error(err)
    });
  }

  trackById(index: number, item: any): number {
    return item.id;
  }
}
