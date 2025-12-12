import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { SecureTokenService } from '../../GatewayService/Security/secure-token.service';

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
  userId = '';
  submitted = false;

  private readonly fb = inject(FormBuilder);
  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);
  private readonly tokenService = inject(SecureTokenService);

  constructor()
  {
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

  addModule(): void {
    this.modules.push(
      this.fb.group({
        title: ['', Validators.required],
        content: ['', Validators.required]
      })
    );
  }

  removeModule(index: number): void {
    this.modules.removeAt(index);
  }

  drop(event: CdkDragDrop<any[]>): void {
    moveItemInArray(this.modules.controls, event.previousIndex, event.currentIndex);
  }

  fieldInvalid(name: string): boolean | undefined {
    const control = this.form.get(name);
    return this.submitted && control?.invalid;
  }

  fieldValid(name: string): boolean | undefined {
    const control = this.form.get(name);
    return control?.valid && control?.touched;
  }

  submit():void {
    this.submitted = true;

    // Course must have at least one module
    if (this.modules.length === 0 || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const userId = this.tokenService.getUserId();

    if (!userId) {
      return;
    }

    this.userId = userId;


    const dto = {
      ...this.form.value,
      instructorUserId: this.userId
    };

    this.courseApi.addCourse(dto).subscribe({
      next: (res: any) => {
        const courseId = res?.id;
        if (!courseId) {
          return;
        }
        this.router.navigate(['/assessment/add-quiz', courseId]);
      },
      error: err => {
        console.error(err);
        // eslint-disable-next-line no-alert
        alert('Error creating course');
      }
    });
  }

  cancel():void {
    this.router.navigate(['/courses']);
  }
  autoResize(event: any):void {
    const textarea = event.target;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight  }px`;
  }

}
