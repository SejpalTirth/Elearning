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
  userId = "";

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

    const token = localStorage.getItem('token');
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

    console.log("DTO sent to gateway:", dto);

    this.courseApi.addCourse(dto).subscribe({
      next: (res: any) => {

        console.log("Course created response:", res);

        const courseId = res?.id;

        if (!courseId) {
          alert("Course created, but no course ID returned.");
          return;
        }

        // 🎯 PHASE 1 MAIN LOGIC: Redirect to Add-Quiz page with course ID
        this.router.navigate([ '/assessment/add-quiz', courseId ]);
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

  getCourseId(): number | undefined {
  return undefined;
  }

}
