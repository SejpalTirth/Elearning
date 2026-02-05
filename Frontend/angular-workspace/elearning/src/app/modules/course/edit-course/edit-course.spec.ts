import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { EditCourseComponent } from './edit-course';
import { ReactiveFormsModule, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { GatewayCourseService, AssessmentGatewayService } from 'api';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('EditCourseComponent', () => {
  let component: EditCourseComponent;
  let fixture: ComponentFixture<EditCourseComponent>;
  
  let mockRouter: any;
  let mockLoading: any;
  let mockToast: any;
  let mockCourseApi: any;
  let mockAssessmentApi: any;

  const mockCourse = {
    id: 50,
    title: 'Original Title',
    description: 'Original Desc',
    categoryId: 1,
    isDeleted: false,
    modules: [
      { id: 10, title: 'Mod 1', content: 'Cont 1' }
    ]
  };

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockLoading = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
    mockToast = { showSuccess: jasmine.createSpy('showSuccess'), showError: jasmine.createSpy('showError') };
    
    mockCourseApi = {
      postApiCourseCategories: jasmine.createSpy('postApiCourseCategories').and.returnValue(of([{ id: 1, name: 'Tech' }])),
      postApiCourseById: jasmine.createSpy('postApiCourseById').and.returnValue(of(mockCourse)),
      postApiCourseUpdate: jasmine.createSpy('postApiCourseUpdate').and.returnValue(of({}))
    };

    mockAssessmentApi = {
      postApiAssessmentCourseQuizStatus: jasmine.createSpy('postApiAssessmentCourseQuizStatus').and.returnValue(of({ allQuizzesCreated: true }))
    };

    await TestBed.configureTestingModule({
      imports: [EditCourseComponent, ReactiveFormsModule, NoopAnimationsModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: LoadingService, useValue: mockLoading },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => '50' } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // Triggers ngOnInit, loadCategories, and loadCourse
  });

  it('should load course data and populate form on init', () => {
    expect(mockCourseApi.postApiCourseById).toHaveBeenCalledWith({ courseId: 50 });
    expect(component.form.value.title).toBe('Original Title');
    expect(component.modules.length).toBe(1);
  });

  it('should prevent editing if course is deleted', () => {
    mockCourseApi.postApiCourseById.and.returnValue(of({ ...mockCourse, isDeleted: true }));
    component.loadCourse(); // Re-trigger manually
    
    expect(mockToast.showError).toHaveBeenCalledWith(jasmine.stringMatching(/cannot be edited/));
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
  });

  describe('Save Logic', () => {
    it('should navigate to /courses on successful update with all quizzes', () => {
      component.save();
      
      expect(mockCourseApi.postApiCourseUpdate).toHaveBeenCalled();
      expect(mockAssessmentApi.postApiAssessmentCourseQuizStatus).toHaveBeenCalled();
      expect(mockToast.showSuccess).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
    });

    it('should redirect to pending assessments if quizzes are missing', () => {
      mockAssessmentApi.postApiAssessmentCourseQuizStatus.and.returnValue(of({ allQuizzesCreated: false }));
      
      component.save();
      
      expect(mockToast.showError).toHaveBeenCalledWith(jasmine.stringMatching(/quizzes are still missing/));
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/pending']);
    });

    it('should handle update failure', () => {
      mockCourseApi.postApiCourseUpdate.and.returnValue(throwError(() => new Error('Fail')));
      
      component.save();
      
      expect(mockToast.showError).toHaveBeenCalledWith('Update failed');
      expect(mockLoading.hide).toHaveBeenCalled();
    });
  });

  it('should add and remove modules correctly', () => {
    component.addModule();
    expect(component.modules.length).toBe(2);
    component.removeModule(0);
    expect(component.modules.length).toBe(1);
  });
});