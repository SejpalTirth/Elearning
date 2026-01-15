import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseFacade } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';
import { ToastService } from '@frontend/ui';

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
  private readonly courseApi = inject(CourseFacade);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly toast = inject(ToastService);

  private authSub?: Subscription;

  constructor() {
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
      next: res => this.categories = res || [],
      error: () => this.categories = []
    });

    this.authSub = this.authState.user$.subscribe(user => {
      if (!user) { return; }
      this.userId = user.userId;
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

  fieldInvalid(name: string): boolean {
    return this.submitted && !!this.form.get(name)?.invalid;
  }

  submit(): void {
    this.submitted = true;

    // Must have at least one module + valid form
    if (this.form.invalid || this.modules.length === 0) {
      this.toast.showError('Please complete all the necessary fields to complete the course creation.');
      this.form.markAllAsTouched();
      return;
    }

    if (!this.userId) {
      this.toast.showError('User session not found.');
      return;
    }

    // EXPLICIT PAYLOAD (backend-safe)
    const requestBody = {
      title: this.form.value.title,
      description: this.form.value.description,
      categoryId: Number(this.form.value.categoryId),
      instructorUserId: this.userId,
      modules: this.form.value.modules.map((m: any) => ({
        title: m.title,
        content: m.content
      }))
    };

    this.courseApi.createCourse(requestBody).subscribe({
      next: (res: any) => {
        const courseId = res?.id;
        if (!courseId) {
          this.toast.showError('Course created but ID not returned.');
          return;
        }
        this.toast.showSuccess('Course created successfully. Now redirecting you to create quizzes for the same.');
        this.router.navigate(['/assessment/add-quiz', courseId]);
      },
      error: err => {
        console.error(err);
        this.toast.showError('Error creating course');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/courses']);
  }

  autoResize(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }
}