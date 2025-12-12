import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { ManageCoursesComponent } from './manage';
import { CourseApiService } from '../services/course-api';

// -----------------------------------------------------
// Mocks
// -----------------------------------------------------

class MockApi {
  getCoursesByInstructor = jasmine
    .createSpy()
    .and.returnValue(of([{ id: 1, title: 'Course A' }]));
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// Helper to create a fake JWT
function fakeJWT(payload: any): string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

// -----------------------------------------------------

describe('ManageCoursesComponent', () => {
  let component: ManageCoursesComponent;
  let fixture: ComponentFixture<ManageCoursesComponent>;
  let api: MockApi;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockApi();
    router = new MockRouter();

    // Mock localStorage
    spyOn(localStorage, 'getItem').and.returnValue(
      fakeJWT({ sub: 'U123' })
    );

    await TestBed.configureTestingModule({
      imports: [ManageCoursesComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // -----------------------------------------------------

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  // -----------------------------------------------------

  it('should extract userId from token on init', () => {
    expect(component.userId).toBe('U123');
  });

  // -----------------------------------------------------

  it('should load courses on init', () => {
    expect(api.getCoursesByInstructor).toHaveBeenCalledWith('U123');
    expect(component.courses.length).toBe(1);
    expect(component.loading).toBeFalse();
  });

  // -----------------------------------------------------

  it('should stop loading if no token exists', () => {
    (localStorage.getItem as jasmine.Spy).and.returnValue(null);

    const fixture2 = TestBed.createComponent(ManageCoursesComponent);
    const comp2 = fixture2.componentInstance;

    fixture2.detectChanges();

    expect(comp2.loading).toBeFalse();
    expect(comp2.userId).toBeNull();
  });

  // -----------------------------------------------------

  it('should stop loading if JWT is invalid', () => {
    (localStorage.getItem as jasmine.Spy).and.returnValue('invalid.token');

    const fixture3 = TestBed.createComponent(ManageCoursesComponent);
    const comp3 = fixture3.componentInstance;

    fixture3.detectChanges();

    expect(comp3.userId).toBeNull();
    expect(comp3.loading).toBeFalse();
  });

  // -----------------------------------------------------

  it('should navigate to view course page', () => {
    component.viewCourse(5);
    expect(router.navigate).toHaveBeenCalledWith(['/courses', 5]);
  });

  // -----------------------------------------------------

  it('should handle API error gracefully', () => {
    api.getCoursesByInstructor.and.returnValue(
      throwError(() => new Error('Failed'))
    );

    component.loadCourses();

    expect(component.loading).toBeFalse();
  });

});
