import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { EditCourseComponent } from './edit';
import { CourseApiService } from '../services/course-api';

// -------------------- Mock Services --------------------

class MockCourseApi {
  getCategories = jasmine.createSpy().and.returnValue(of([
    { id: 1, name: 'Programming' },
    { id: 2, name: 'Math' }
  ]));

  getById = jasmine.createSpy().and.returnValue(of({
    id: 10,
    title: 'Course A',
    description: 'Desc A',
    categoryId: 1,
    modules: [
      { id: 101, title: 'Module 1', content: 'Content 1' },
      { id: 102, title: 'Module 2', content: 'Content 2' }
    ]
  }));

  updateCourse = jasmine.createSpy().and.returnValue(of({}));
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

const mockRoute = {
  snapshot: {
    paramMap: {
      get: (): string=> '10'
    }
  }
};

// ----------------------------------------------------------

describe('EditCourseComponent', () => {

  let component: EditCourseComponent;
  let fixture: ComponentFixture<EditCourseComponent>;
  let api: MockCourseApi;

  beforeEach(async () => {
    api = new MockCourseApi();

    await TestBed.configureTestingModule({
      imports: [EditCourseComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: Router, useValue: new MockRouter() }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ----------------------------------------------------------

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ----------------------------------------------------------

  it('should load course and patch form', () => {
    expect(api.getById).toHaveBeenCalledWith(10);

    expect(component.form.value.title).toBe('Course A');
    expect(component.form.value.description).toBe('Desc A');
    expect(component.form.value.categoryId).toBe(1);

    expect(component.modules.length).toBe(2);
  });

  // ----------------------------------------------------------

  it('should load categories', () => {
    expect(api.getCategories).toHaveBeenCalled();
    expect(component.categories.length).toBe(2);
  });

  // ----------------------------------------------------------

  it('should add a module', () => {
    const count = component.modules.length;
    component.addModule();
    expect(component.modules.length).toBe(count + 1);
  });

  // ----------------------------------------------------------

  it('should remove a module', () => {
    const initial = component.modules.length;
    component.removeModule(0);
    expect(component.modules.length).toBe(initial - 1);
  });

});
