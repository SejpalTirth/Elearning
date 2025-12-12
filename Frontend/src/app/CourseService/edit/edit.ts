import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { LoadingOverlay } from '../../shared/loading/loading-overlay';
import { AssessmentApiService } from 'app/AssessmentService/services/assessment-api';

@Component({
  selector: 'app-edit-course',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule,
    LoadingOverlay
  ],
  templateUrl: './edit.html',
  styleUrls: ['./edit.css']
})
export class EditCourseComponent implements OnInit {

  @ViewChild(LoadingOverlay) loader!: LoadingOverlay;

  form!: FormGroup;
  categories: any[] = [];
  courseId!: number;
  courseData: any;
  submitted = false;

  private readonly fb = inject(FormBuilder);
  private readonly api = inject(CourseApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly assessmentApi = inject(AssessmentApiService);

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      modules: this.fb.array([])
    });

    setTimeout(() => this.loader?.show(), 0);

    this.loadCategories(() => this.loadCourse());
  }

  get modules(): FormArray {
    return this.form.get('modules') as FormArray;
  }

  loadCategories(onComplete: () => void): void {
    this.api.getCategories().subscribe({
      next: (res) => {
        this.categories = res || [];
        onComplete();
      },
      error: (err) => {
        console.error('Failed to load categories', err);
        this.categories = [];
        onComplete();
      }
    });
  }

  loadCourse(): void {
    this.api.getById(this.courseId).subscribe({
      next: (course) => {
        this.courseData = course;

        if (course.isDeleted) {
          this.loader.hide();
          // eslint-disable-next-line no-alert
          window.alert('This course was deleted by the admin and cannot be edited.');
          this.router.navigate(['/courses']);
          return;
        }

        this.form.patchValue({
          title: course.title,
          description: course.description,
          categoryId: course.categoryId
        });

        this.modules.clear();
        course.modules.forEach((m: any) => {
          this.modules.push(this.fb.group({
            id: [m.id],
            title: [m.title, Validators.required],
            content: [m.content, Validators.required]
          }));
        });

        this.loader.hide();
      },
      error: (err) => {
        console.error('Failed to load course', err);
        this.loader.hide();
      }
    });
  }

  addModule(): void {
    this.modules.push(this.fb.group({
      id: [0],
      title: ['', Validators.required],
      content: ['', Validators.required]
    }));
  }

  removeModule(index: number): void {
    this.modules.removeAt(index);
  }

  drop(event: CdkDragDrop<any[]>): void {
    moveItemInArray(this.modules.controls, event.previousIndex, event.currentIndex);
  }

  fieldInvalid(name: string): boolean {
    const control = this.form.get(name);
    return this.submitted && !!control?.invalid;
  }

  autoResize(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  save(): void {
    this.submitted = true;

    if (this.form.invalid || this.modules.controls.some(m => m.invalid)) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      ...this.form.value,
      modules: this.form.value.modules.filter((m: any) =>
        m.title?.trim() !== '' && m.content?.trim() !== ''
      )
    };

    this.loader.show();

    this.api.updateCourse(this.courseId, payload).subscribe({
      next: () => {
        this.assessmentApi.getQuizStatus(this.courseId).subscribe({
          next: (status: any) => {
            this.loader.hide();
            if (!status.allQuizzesCreated) {
              // eslint-disable-next-line no-alert
              window.alert('Course updated! Some quizzes are still missing. You can complete them from Pending Tasks.');
              this.router.navigate(['/assessment/pending']);
              return;
            }
            // eslint-disable-next-line no-alert
            window.alert('Course updated successfully!');
            this.router.navigate(['/courses']);
          },
          error: () => {
            this.loader.hide();
            // eslint-disable-next-line no-alert
            window.alert('Updated, but failed to check quiz status.');
            this.router.navigate(['/courses']);
          }
        });
      },
      error: () => {
        this.loader.hide();
        // eslint-disable-next-line no-alert
        window.alert('Update failed');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/courses']);
  }
}
