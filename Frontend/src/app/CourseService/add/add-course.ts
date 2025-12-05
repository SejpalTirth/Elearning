import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
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
  userId = "";
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private courseApi: CourseApiService,
    private router: Router
  ) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      modules: this.fb.array([
        this.fb.group({
          title: ['', Validators.required],
          content: ['', Validators.required]
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
        title: ['', Validators.required],
        content: ['', Validators.required]
      })
    );
  }

  removeModule(index: number) {
    this.modules.removeAt(index);
  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.modules.controls, event.previousIndex, event.currentIndex);
  }

  fieldInvalid(name: string) {
    const control = this.form.get(name);
    return this.submitted && control?.invalid;
  }

  fieldValid(name: string) {
    const control = this.form.get(name);
    return control?.valid && control?.touched;
  }

  submit() {
    this.submitted = true;

    // Course must have at least one module
    if (this.modules.length === 0 || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const token = localStorage.getItem('accessToken');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload["sub"];
    } catch {
      console.warn("Invalid token");
    }

    const dto = {
      ...this.form.value,
      instructorUserId: this.userId
    };

    this.courseApi.addCourse(dto).subscribe({
      next: (res: any) => {
        const courseId = res?.id;
        if (!courseId) {
          alert("Course created, but no course ID returned.");
          return;
        }
        this.router.navigate(['/assessment/add-quiz', courseId]);
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
  autoResize(event: any) {
  const textarea = event.target;
  textarea.style.height = 'auto';
  textarea.style.height = textarea.scrollHeight + 'px';
}

}
