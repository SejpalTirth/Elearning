import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddCourseComponent } from './add-course';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { GatewayCourseService } from 'api';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormArray } from '@angular/forms';

describe('AddCourseComponent', () => {
  let component: AddCourseComponent;
  let fixture: ComponentFixture<AddCourseComponent>;
  
  // Mocks
  let mockRouter: any;
  let mockAuthState: any;
  let mockToast: any;
  let mockApi: any;
  
  // A Subject to simulate user login state changes
  const userSubject = new BehaviorSubject<any>(null);

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { 
      showError: jasmine.createSpy('showError'), 
      showSuccess: jasmine.createSpy('showSuccess') 
    };
    mockAuthState = { user$: userSubject.asObservable() };
    mockApi = {
      postApiCourseCategories: jasmine.createSpy('postApiCourseCategories').and.returnValue(of([{ id: 1, name: 'Tech' }])),
      postApiCourseCreate: jasmine.createSpy('postApiCourseCreate').and.returnValue(of({ id: 'course_99' }))
    };

    await TestBed.configureTestingModule({
      imports: [
        AddCourseComponent, 
        ReactiveFormsModule, 
        NoopAnimationsModule // Required for DragDrop tests to not lag
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayCourseService, useValue: mockApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should initialize with categories and user session', () => {
    userSubject.next({ userId: 'instructor_1' });
    fixture.detectChanges();

    expect(mockApi.postApiCourseCategories).toHaveBeenCalled();
    expect(component.categories.length).toBe(1);
    expect(component.userId).toBe('instructor_1');
  });

  describe('Form Management', () => {
    it('should add a module to the form array', () => {
      const initialCount = component.modules.length;
      component.addModule();
      expect(component.modules.length).toBe(initialCount + 1);
    });

    it('should remove a module from the form array', () => {
      component.addModule(); // Now 2 modules
      component.removeModule(0);
      expect(component.modules.length).toBe(1);
    });
  });

  describe('Submit Logic', () => {
    it('should show error toast if form is invalid', () => {
      component.submit(); // Empty form
      expect(mockToast.showError).toHaveBeenCalledWith(jasmine.stringMatching(/complete all the necessary fields/));
      expect(mockApi.postApiCourseCreate).not.toHaveBeenCalled();
    });

    it('should show error if user session is missing', () => {
  // 1. Make the form valid so it passes the first check
  component.form.patchValue({ 
    title: 'Valid Title', 
    categoryId: '1' 
  });
  // Ensure the default module is valid
  component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });

  // 2. Set the condition we want to test
  component.userId = ''; 
  
  component.submit();

  expect(mockToast.showError).toHaveBeenCalledWith('User session not found.');
});

    it('should handle API error gracefully', () => {
      // 1. Setup API to fail
      mockApi.postApiCourseCreate.and.returnValue(throwError(() => new Error('Server Down')));
      
      // 2. Make the form valid so it passes the initial validation check
      component.userId = 'valid_user_id';
      component.form.patchValue({ 
        title: 'Valid Title', 
        categoryId: '1' 
      });
      component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });

      // 3. Trigger submit
      component.submit();

      // Now it should reach the API error block
      expect(mockToast.showError).toHaveBeenCalledWith('Error creating course');
    });

    it('should navigate to assessment on successful creation', () => {
      userSubject.next({ userId: 'instr_1' });
      component.form.patchValue({
        title: 'New Course',
        description: 'Desc',
        categoryId: '1'
      });
      // modules array already has 1 valid entry by default
      (component.form.get('modules') as FormArray).at(0).patchValue({ title: 'M1', content: 'C1' });
      component.submit();

      expect(mockApi.postApiCourseCreate).toHaveBeenCalled();
      expect(mockToast.showSuccess).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 'course_99']);
    });

    it('should handle API error gracefully', () => {
      // 1. Setup API to fail
      mockApi.postApiCourseCreate.and.returnValue(throwError(() => new Error('Server Down')));
      
      // 2. Make the form valid so it passes the initial validation check
      component.userId = 'valid_user_id';
      component.form.patchValue({ 
        title: 'Valid Title', 
        categoryId: '1' 
      });
      component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });

      // 3. Trigger submit
      component.submit();

      // Now it should reach the API error block
      expect(mockToast.showError).toHaveBeenCalledWith('Error creating course');
    });
  });
});