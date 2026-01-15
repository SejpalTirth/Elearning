import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CourseFacade } from '@frontend/core';
import { ToastService, LoadingService } from '@frontend/ui';

@Component({
  selector: 'lib-admin-manage-courses',
  imports: [],
  templateUrl: './admin-manage-courses.html',
  styleUrl: './admin-manage-courses.css',
})
export class AdminManageCoursesComponent implements OnInit {

  courses = signal<any[]>([]);
  loading = signal(true);

  private readonly api = inject(CourseFacade);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly loader = inject(LoadingService);

  ngOnInit():void {
    this.loadCourses();
  }

  loadCourses():void {
    this.api.getAllCourses().subscribe({
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

    this.api.deleteCourse({courseId: id}).subscribe({
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
    this.api.restoreCourse({courseId: id}).subscribe({
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
