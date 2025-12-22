import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Router } from '@angular/router';

import { MyLearningComponent } from './my-learning';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../services/progress.service';

// ---------------------------------------------------------------------
// Mocks
// ---------------------------------------------------------------------

class MockProgressService {
  getUserProgress = jasmine.createSpy().and.returnValue(
    of([
      { courseId: 1, moduleId: 10, progressPercent: 100 },
      { courseId: 1, moduleId: 11, progressPercent: 100 },
      { courseId: 2, moduleId: 20, progressPercent: 50 }
    ])
  );
}

class MockCourseApi {
  getEnrolledCourses = jasmine.createSpy().and.returnValue(
    of([
      { id: 1, title: 'Course A', modules: [{}, {}] }, // 2 modules
      { id: 2, title: 'Course B', modules: [{}, {}, {}] } // 3 modules
    ])
  );
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// Fake JWT generator
function fakeJWT(payload: any): any {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

// ---------------------------------------------------------------------

describe('MyLearningComponent', () => {

  let component: MyLearningComponent;
  let fixture: ComponentFixture<MyLearningComponent>;
  let api: MockCourseApi;
  let progress: MockProgressService;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    progress = new MockProgressService();
    router = new MockRouter();

    // Mock JWT for most tests
    spyOn(localStorage, 'getItem').and.returnValue(
      fakeJWT({ sub: 'USER123' })
    );
    spyOn(localStorage, 'setItem');

    await TestBed.configureTestingModule({
      imports: [MyLearningComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: ProgressService, useValue: progress },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyLearningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ---------------------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------------------------------------------------------------
  it('should extract userId from token', () => {
    expect(component.userId).toBe('USER123');
  });

  // ---------------------------------------------------------------------
  it('should load progress + enrolled courses and compute progress', () => {
    expect(progress.getUserProgress).toHaveBeenCalledWith('USER123');
    expect(api.getEnrolledCourses).toHaveBeenCalledWith('USER123');

    expect(component.courses.length).toBe(2);

    const c1 = component.courses[0]; // Course A
    const c2 = component.courses[1]; // Course B

    // Course A: 2 completed / 2 modules = 100%
    expect(c1.progressPercent).toBe(100);
    expect(c1.completedModules).toBe(2);
    expect(c1.totalModules).toBe(2);

    // Course B: 1 completed / 3 modules = 33%
    expect(c2.progressPercent).toBe(33);
    expect(c2.completedModules).toBe(1);
    expect(c2.totalModules).toBe(3);

    expect(component.loading).toBeFalse();
  });

  // ---------------------------------------------------------------------
  it('getProgress() should correctly compute average progress', () => {
    const avg = component.getProgress(1);
    expect(avg).toBe(100);
  });

  // ---------------------------------------------------------------------
  it('should navigate when continueLearning() is called', () => {
    component.continueLearning(5);
    expect(router.navigate).toHaveBeenCalledWith(['/courses/5/modules']);
  });

  // ---------------------------------------------------------------------
  // ⭐ FIXED TEST — spies reset before creating a new component
  // ---------------------------------------------------------------------
  it('should stop loading if no userId is found', () => {

    // Make getItem return null
    (localStorage.getItem as jasmine.Spy).and.returnValue(null);

    // Reset spies so no calls from previous tests remain
    progress.getUserProgress.calls.reset();
    api.getEnrolledCourses.calls.reset();

    // Create new component instance
    const fixture2 = TestBed.createComponent(MyLearningComponent);
    const c2 = fixture2.componentInstance;

    fixture2.detectChanges();

    // It must stop loading immediately
    expect(c2.loading).toBeFalse();

    // And should NOT call backend services
    expect(progress.getUserProgress).not.toHaveBeenCalled();
    expect(api.getEnrolledCourses).not.toHaveBeenCalled();
  });

});
