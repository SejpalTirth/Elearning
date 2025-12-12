import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { FormBuilder } from '@angular/forms';
import { AddCourseComponent } from './add-course';
import { CourseApiService } from '../services/course-api';

// Mock CourseApiService
class MockCourseApi {
  getCategories = jasmine.createSpy().and.returnValue(of([{ id: 1, name: 'Test' }]));
  addCourse = jasmine.createSpy().and.returnValue(of({ id: 123 }));
}

// Mock Router
class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// Fake JWT generator
function fakeJWT(payload: any): string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

// =======================================================
// PART 1: Creation, Form, Modules, Category Loading
// =======================================================
describe('AddCourseComponent - Initialization & Modules', () => {
  let component: AddCourseComponent;
  let fixture: ComponentFixture<AddCourseComponent>;
  let api: MockCourseApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [AddCourseComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router },
        FormBuilder
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddCourseComponent);
    component = fixture.componentInstance;

    spyOn(window, 'alert'); // Prevent real alerts
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load categories on init', () => {
    expect(api.getCategories).toHaveBeenCalled();
    expect(component.categories.length).toBe(1);
  });

  it('addModule should increase modules length', () => {
    const initial = component.modules.length;
    component.addModule();
    expect(component.modules.length).toBe(initial + 1);
  });

  it('removeModule should remove a module', () => {
    component.addModule();
    const initial = component.modules.length;
    component.removeModule(0);
    expect(component.modules.length).toBe(initial - 1);
  });

  it('drop should reorder modules', () => {
    component.addModule();
    component.addModule();
    component.modules.controls[0].patchValue({ title: 'A', content: '1' });
    component.modules.controls[1].patchValue({ title: 'B', content: '2' });
    component.drop({ previousIndex: 0, currentIndex: 1 } as any);
    expect(component.modules.controls[0].value.title).toBe('B');
    expect(component.modules.controls[1].value.title).toBe('A');
  });
});

// =======================================================
// PART 2: Submission, API interactions, Navigation, Alerts
// =======================================================
// =======================================================
// PART 2A: Submission & DTO / User Extraction
// =======================================================
describe('AddCourseComponent - Submission & DTO', () => {
  let component: AddCourseComponent;
  let fixture: ComponentFixture<AddCourseComponent>;
  let api: MockCourseApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [AddCourseComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router },
        FormBuilder
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddCourseComponent);
    component = fixture.componentInstance;

    spyOn(window, 'alert'); // Prevent real alerts
    fixture.detectChanges();
  });

  it('should not submit when form invalid', () => {
    component.form.controls['title'].setValue('');
    component.form.controls['categoryId'].setValue('');
    component.submit();
    expect(api.addCourse).not.toHaveBeenCalled();
  });

  it('should extract userId from token', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'USER123' }));
    component.form.controls['title'].setValue('Test Course');
    component.form.controls['categoryId'].setValue(1);
    component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });
    api.addCourse = jasmine.createSpy().and.returnValue(of({ id: 55 }));
    component.submit();
    expect(component.userId).toBe('USER123');
  });

  it('should call addCourse with correct DTO', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'USER123' }));
    component.form.controls['title'].setValue('Course');
    component.form.controls['categoryId'].setValue(1);
    component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });
    api.addCourse = jasmine.createSpy().and.returnValue(of({ id: 101 }));
    component.submit();
    expect(api.addCourse).toHaveBeenCalled();
    const dto = api.addCourse.calls.mostRecent().args[0];
    expect(dto.instructorUserId).toBe('USER123');
    expect(dto.title).toBe('Course');
  });
});

// =======================================================
// PART 2B: Navigation & Alerts
// =======================================================
describe('AddCourseComponent - Navigation & Alerts', () => {
  let component: AddCourseComponent;
  let fixture: ComponentFixture<AddCourseComponent>;
  let api: MockCourseApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [AddCourseComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router },
        FormBuilder
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddCourseComponent);
    component = fixture.componentInstance;

    spyOn(window, 'alert'); // Prevent real alerts
    fixture.detectChanges();
  });

  it('should navigate to quiz creation when course created', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'ID1' }));
    component.form.controls['title'].setValue('Course');
    component.form.controls['categoryId'].setValue(1);
    component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });
    api.addCourse = jasmine.createSpy().and.returnValue(of({ id: 777 }));
    component.submit();
    expect(router.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 777]);
  });

  it('should alert if courseId is missing', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'ID1' }));
    api.addCourse = jasmine.createSpy().and.returnValue(of({}));
    component.form.controls['title'].setValue('Course');
    component.form.controls['categoryId'].setValue(1);
    component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });
    component.submit();
    expect(window.alert).toHaveBeenCalledWith('Course created, but no course ID returned.');
  });

  it('should alert on API error', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'ID1' }));
    api.addCourse = jasmine.createSpy().and.returnValue(
      throwError(() => ({ message: 'Bad Error' }))
    );
    component.form.controls['title'].setValue('Course');
    component.form.controls['categoryId'].setValue(1);
    component.modules.at(0).patchValue({ title: 'M1', content: 'C1' });
    component.submit();
    expect(window.alert).toHaveBeenCalledWith('Error creating course');
  });

  it('cancel should navigate to /courses', () => {
    component.cancel();
    expect(router.navigate).toHaveBeenCalledWith(['/courses']);
  });
});

