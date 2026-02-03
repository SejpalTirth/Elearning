import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormArray, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ToastService } from '../../../common-modules/ui/toast/toast-service'; 
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { GatewayCourseService,
         GatewayContractsCourseCourseIdRequest,
         GatewayContractsCourseUpdateCourseRequest,          
         AssessmentGatewayService
       } from 'api';

@Component({
  selector: 'app-edit-course',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule
  ],
  templateUrl: './edit-course.html',
  styleUrls: ['./edit-course.css']
})
export class EditCourseComponent implements OnInit {

  form!: FormGroup;
  categories: any[] = [];
  courseId!: number;
  courseData: any;
  submitted = false;

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly loading = inject(LoadingService);
  private readonly toast = inject(ToastService);
  private readonly courseapi = inject(GatewayCourseService);
  private readonly assessmentapi = inject(AssessmentGatewayService);

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      modules: this.fb.array([])
    });

    this.loading.show();
    this.loadCategories();
  }

  get modules(): FormArray {
    return this.form.get('modules') as FormArray;
  }

  loadCategories(): void {
    this.courseapi.postApiCourseCategories().subscribe({
      next: res => {
        this.categories = res || [];
        this.loadCourse();
      },
      error: () => {
        this.categories = [];
        this.loadCourse();
      }
    });
  }

  loadCourse(): void {
    const request: GatewayContractsCourseCourseIdRequest = {
      courseId : this.courseId
    }
    this.courseapi.postApiCourseById(request).subscribe({
      next: course => {
        if (course.isDeleted) {
          this.loading.hide();
          this.toast.showError('This course was deleted by the admin and cannot be edited.');
          this.router.navigate(['/courses']);
          return;
        }

        this.courseData = course;

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

        this.loading.hide();
      },
      error: () => this.loading.hide()
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
    return this.submitted && !!this.form.get(name)?.invalid;
  }

  save(): void {
    this.submitted = true;

    if (this.form.invalid || this.modules.controls.some(m => m.invalid)) {
      this.form.markAllAsTouched();
      return;
    }

    const requestBody : GatewayContractsCourseUpdateCourseRequest = {
      courseId: this.courseId,
      course: {
        title: this.form.value.title,
        description: this.form.value.description,
        categoryId: this.form.value.categoryId,
        modules: this.form.value.modules.map((m: any) => ({
          id: m.id,
          title: m.title,
          content: m.content
        }))
      }
    };

    this.loading.show();

    this.courseapi.postApiCourseUpdate(requestBody).subscribe({
      next: () => {
        this.assessmentapi.postApiAssessmentCourseQuizStatus({courseId: this.courseId}).subscribe({
          next: status => {
            this.loading.hide();
            if (!status.allQuizzesCreated) {
              this.toast.showError('Course updated! Some quizzes are still missing.');
              this.router.navigate(['/assessment/pending']);
              return;
            }
            this.toast.showSuccess('Course updated successfully!');
            this.router.navigate(['/courses']);
          },
          error: () => {
            this.loading.hide();
            this.toast.showError('Updated, but failed to check quiz status.');
            this.router.navigate(['/courses']);
          }
        });
      },
      error: () => {
        this.loading.hide();
        this.toast.showError('Update failed');
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