import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { ListCoursesComponent } from './list-courses';
import { CourseApiService } from '../services/course-api';

// -------------------- Mock Services --------------------

class MockCourseApi {
  getAll = jasmine.createSpy().and.returnValue(
    of([
      { id: 1, title: 'Course A' },
      { id: 2, title: 'Course B' }
    ])
  );
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// --------------------------------------------------------

describe('ListCoursesComponent', () => {

  let component: ListCoursesComponent;
  let fixture: ComponentFixture<ListCoursesComponent>;
  let api: MockCourseApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [ListCoursesComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // --------------------------------------------------------

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // --------------------------------------------------------

  it('should load courses on init', () => {
    expect(api.getAll).toHaveBeenCalled();
    expect(component.courses.length).toBe(2);
    expect(component.loading).toBeFalse();
  });

  // --------------------------------------------------------

  it('should set loading to false on error', () => {
    api.getAll.and.returnValue(throwError(() => 'Error'));

    component.loadCourses();

    expect(component.loading).toBeFalse();
  });

  // --------------------------------------------------------

  it('should navigate to course page', () => {
    component.openCourse(5);

    expect(router.navigate).toHaveBeenCalledWith(['/courses', 5]);
  });

});
