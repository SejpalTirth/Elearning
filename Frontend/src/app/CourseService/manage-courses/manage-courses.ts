import { Component, OnInit } from '@angular/core';
import { CourseApiService } from '../services/course-api';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-courses',
  imports:[CommonModule],
  templateUrl: './manage-courses.html',
  styleUrls: ['./manage-courses.css']
})
export class ManageCoursesComponent implements OnInit {

  courses: any[] = [];
  loading = true;

  constructor(private api: CourseApiService, private router: Router) {}

  ngOnInit() {
    this.loadCourses();
  }

  loadCourses() {
    this.api.getAll().subscribe({
      next: res => {
        this.courses = res;
        this.loading = false;
      },
      error: err => console.error(err)
    });
  }

  editCourse(id: number) {
    this.router.navigate([`/courses/edit/${id}`]);
  }

  deleteCourse(id: number) {
  if (!confirm("Are you sure you want to archive this course?")) return;

  this.api.deleteCourse(id).subscribe({
    next: () => {
      alert("Course archived successfully!");

      //  Remove from UI instantly
      this.courses = this.courses.filter(c => c.id !== id);
    },
    error: err => console.error(err)
  });
}



}
