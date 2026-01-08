import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AddCourseComponent } from './add-course';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { Router } from '@angular/router';
import { CourseFacade } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { ToastService } from '@frontend/ui';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { FormArray } from '@angular/forms';
import { CdkDragDrop } from '@angular/cdk/drag-drop';

describe('AddCourseComponent', () => {
  let component: AddCourseComponent;
  let fixture: ComponentFixture<AddCourseComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate') };
  let courseApiMock: Partial<CourseFacade>;
  let authMock: Partial<AuthStateService>;
  let toastMock: Partial<ToastService>;

  const userSubject = new BehaviorSubject<any>({ userId: 'user123' });

  beforeEach(async () => {
    courseApiMock = {
      getCategories: jasmine.createSpy('getCategories').and.returnValue(of([{ id: 1, name: 'Cat1' }])),
      createCourse: jasmine.createSpy('createCourse').and.returnValue(of({ id: 42 }))
    };

    authMock = { user$: userSubject.asObservable() };
    toastMock = {
      showError: jasmine.createSpy('showError'),
      showSuccess: jasmine.createSpy('showSuccess')
    };

    await TestBed.configureTestingModule({
      imports: [AddCourseComponent],
      providers: [
        provideCommonMocks,
        { provide: Router, useValue: routerMock },
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AuthStateService, useValue: authMock },
        { provide: ToastService, useValue: toastMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize categories and userId on ngOnInit', fakeAsync(() => {
    component.ngOnInit();
    tick();
    expect(component.categories.length).toBe(1);
    expect(component.userId).toBe('user123');
  }));

  it('should add and remove modules', () => {
    const initialLength = component.modules.length;
    component.addModule();
    expect(component.modules.length).toBe(initialLength + 1);

    component.removeModule(0);
    expect(component.modules.length).toBe(initialLength);
  });

  it('should reorder modules with drop', () => {
    component.addModule();
    const controlsBefore = [...component.modules.controls];
    const event = { previousIndex: 0, currentIndex: 1 } as CdkDragDrop<any[]>;
    component.drop(event);
    expect(component.modules.controls).not.toEqual(controlsBefore);
  });

  it('should validate fieldInvalid correctly', () => {
    component.submitted = true;
    expect(component.fieldInvalid('title')).toBe(true); // initially invalid
    component.form.get('title')?.setValue('Test Course');
    expect(component.fieldInvalid('title')).toBe(false);
  });

  it('submit should show error if form invalid', () => {
    component.form.get('title')?.setValue(''); // invalid
    component.submit();
    expect(toastMock.showError).toHaveBeenCalled();
  });

  it('submit should show error if userId missing', () => {
    component.userId = '';
    component.form.get('title')?.setValue('Test Course');
    component.submit();
    expect(toastMock.showError).toHaveBeenCalledWith('User session not found.');
  });

  it('submit should call createCourse and navigate on success', fakeAsync(() => {
    component.form.get('title')?.setValue('Test Course');
    component.form.get('categoryId')?.setValue(1);
    component.modules.at(0).get('title')?.setValue('Module 1');
    component.modules.at(0).get('content')?.setValue('Content 1');

    component.submit();
    tick();

    expect(courseApiMock.createCourse).toHaveBeenCalled();
    expect(toastMock.showSuccess).toHaveBeenCalled();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 42]);
  }));

  it('submit should handle API error', fakeAsync(() => {
    (courseApiMock.createCourse as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    component.form.get('title')?.setValue('Test Course');
    component.form.get('categoryId')?.setValue(1);
    component.modules.at(0).get('title')?.setValue('Module 1');
    component.modules.at(0).get('content')?.setValue('Content 1');

    component.submit();
    tick();

    expect(toastMock.showError).toHaveBeenCalledWith('Error creating course');
  }));

  it('cancel should navigate to /courses', () => {
    component.cancel();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses']);
  });
});
