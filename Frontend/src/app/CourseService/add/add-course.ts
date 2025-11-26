import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-add-course',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule
  ],
  templateUrl: './add-course.html',
  styleUrls: ['./add-course.css']
})
export class AddCourseComponent implements OnInit {
  form: FormGroup;
  categories: any[] = [];

  constructor(
    private fb: FormBuilder,
    private courseApi: CourseApiService,
    private router: Router
  ) {
    this.form = this.fb.group({
      title: [''],
      description: [''],
      categoryId: [''],
      modules: this.fb.array([
        this.fb.group({
          title: [''],
          content: ['']
        })
      ])
    });
  }

  ngOnInit(): void {
    this.courseApi.getCategories().subscribe({
      next: (res: any) => (this.categories = res),
      error: err => console.error(err)
    });
  }

  get modules(): FormArray {
    return this.form.get('modules') as FormArray;
  }

  addModule() {
    this.modules.push(
      this.fb.group({
        title: [''],
        content: ['']
      })
    );
  }

  removeModule(index: number) {
    this.modules.removeAt(index);
  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.modules.controls, event.previousIndex, event.currentIndex);
  }

  submit() {
    const instructorUserId = localStorage.getItem('userId');

    const dto = {
      ...this.form.value,
      instructorUserId
    };

    this.courseApi.addCourse(dto).subscribe({
      next: () => {
        alert('Course created 🎉');
        this.router.navigate(['/courses']);
      },
      error: err => {
        console.error(err);
        alert('Error creating course');
      }
    });
  }

  cancel() {
    this.router.navigate(['/courses']);
  }
}
