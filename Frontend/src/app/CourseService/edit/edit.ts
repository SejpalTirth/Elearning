import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { CdkDropList, CdkDrag, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-edit-course',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule
  ],
  templateUrl: './edit.html',
  styleUrls: ['./edit.css']
})
export class EditCourseComponent implements OnInit {

  form!: FormGroup;
  categories: any[] = [];
  courseId!: number;

  constructor(
    private fb: FormBuilder,
    private api: CourseApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.form = this.fb.group({
      title: [''],
      description: [''],
      categoryId: [''],
      modules: this.fb.array([])
    });

    this.loadCategories();
    this.loadCourse();
  }

  get modules(): FormArray {
    return this.form.get('modules') as FormArray;
  }

  loadCategories() {
    this.api.getCategories().subscribe(res => this.categories = res);
  }

  loadCourse() {
  this.api.getById(this.courseId).subscribe(course => {
    this.form.patchValue({
      title: course.title,
      description: course.description,
      categoryId: course.categoryId
    });

    course.modules.forEach((m: any) => {
      this.modules.push(
        this.fb.group({
          id: [m.id],              
          title: [m.title],
          content: [m.content]
          })
        );
      });
    });
  }


  addModule() {
  this.modules.push(
    this.fb.group({
      id: [0],
      title: [''],
      content: ['']
      })
    );
  }


  removeModule(i: number) {
    this.modules.removeAt(i);
  }

  drop(event: any) {
    moveItemInArray(this.modules.controls, event.previousIndex, event.currentIndex);
  }

  save() {

  const payload = {
    ...this.form.value,
    modules: this.form.value.modules.filter((m: any) =>
      (m.title?.trim() !== '' && m.content?.trim() !== '')
    )
  };

  console.log("📤 Sending update:", payload);

  this.api.updateCourse(this.courseId, payload).subscribe({
    next: () => {
      alert('Course updated successfully!');
      this.router.navigate(['/courses']);
      },  
      error: (err) => console.error("❌ Update failed:", err)
    });
  }



  cancel() {
    this.router.navigate(['/courses']);
  }

  getCourseId(): number | undefined {
  return this.courseId;
  }

}
