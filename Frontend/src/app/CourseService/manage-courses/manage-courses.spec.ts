import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { ManageCoursesComponent } from './manage-courses';
import { CourseApiService } from '../services/course-api';

// ---------------------------------------------------------
// Mocks
// ---------------------------------------------------------

class MockApi {
  getAll = jasmine.createSpy().and.returnValue(
    of([{ id: 1, title: 'A' }, { id: 2, title: 'B' }])
  );

  deleteCourse = jasmine.createSpy().and.returnValue(of({}));
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// ---------------------------------------------------------

describe('ManageCoursesComponent', () => {
  let component: ManageCoursesComponent;
  let fixture: ComponentFixture<ManageCoursesComponent>;
  let api: MockApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockApi();
    router = new MockRouter();

    // Mock confirm + alert globally
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(window, 'alert');

    await TestBed.configureTestingModule({
      imports: [ManageCoursesComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit → loadCourses()
  });

  // ---------------------------------------------------------

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------------------------------------------------

  it('should load courses on init', () => {
    expect(api.getAll).toHaveBeenCalled();
    expect(component.courses()).toEqual([
      { id: 1, title: 'A' },
      { id: 2, title: 'B' }
    ]);
    expect(component.loading()).toBeFalse();
  });

  // ---------------------------------------------------------

  it('should navigate to edit page', () => {
    component.editCourse(5);
    expect(router.navigate).toHaveBeenCalledWith(['/courses/edit/5']);
  });

  // ---------------------------------------------------------

  it('should delete course when confirmed', () => {
    component.deleteCourse(1);

    expect(api.deleteCourse).toHaveBeenCalledWith(1);
    expect(window.alert).toHaveBeenCalled();
    expect(component.courses()).toEqual([{ id: 2, title: 'B' }]);
  });

  // ---------------------------------------------------------

  it('should NOT delete when confirm is cancelled', () => {
    (window.confirm as jasmine.Spy).and.returnValue(false);

    component.deleteCourse(1);

    expect(api.deleteCourse).not.toHaveBeenCalled();
    expect(component.courses().length).toBe(2);
  });

  // ---------------------------------------------------------

  it('should handle delete API error gracefully', () => {
    api.deleteCourse.and.returnValue(throwError(() => new Error('fail')));

    component.deleteCourse(1);

    expect(api.deleteCourse).toHaveBeenCalledWith(1);
    // list should NOT change
    expect(component.courses().length).toBe(2);
  });

});
