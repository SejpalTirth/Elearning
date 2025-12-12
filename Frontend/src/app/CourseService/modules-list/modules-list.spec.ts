import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { ModulesListComponent } from './modules-list';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../../CourseService/services/progress.service';

// ------------------------------------------------------
// Mock Services
// ------------------------------------------------------

class MockCourseApi {
  getModules = jasmine.createSpy().and.returnValue(
    of([
      { id: 1, title: 'Module 1' },
      { id: 2, title: 'Module 2' }
    ])
  );
}

class MockProgressService {
  getUserProgress = jasmine.createSpy().and.returnValue(
    of([
      { moduleId: 1, progressPercent: 100 },
      { moduleId: 2, progressPercent: 50 }
    ])
  );
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// Fake route → id = 10
const mockRoute = {
  snapshot: {
    paramMap: { get: (): string => '10' }
  }
};

// Fake JWT generator
function fakeJWT(payload: any): string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

describe('ModulesListComponent', () => {

  let component: ModulesListComponent;
  let fixture: ComponentFixture<ModulesListComponent>;
  let api: MockCourseApi;
  let progress: MockProgressService;
  let router: MockRouter;

  beforeEach(async () => {
    api = new MockCourseApi();
    progress = new MockProgressService();
    router = new MockRouter();

    // Mock JWT in localStorage
    spyOn(localStorage, 'getItem').and.returnValue(fakeJWT({ sub: 'USER123' }));

    await TestBed.configureTestingModule({
      imports: [ModulesListComponent],
      providers: [
        { provide: CourseApiService, useValue: api },
        { provide: ProgressService, useValue: progress },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: mockRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModulesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ------------------------------------------------------
  it('should extract userId from token', () => {
    expect(component.userId).toBe('USER123');
  });

  // ------------------------------------------------------
  it('should load progress and modules on init', () => {
    expect(progress.getUserProgress).toHaveBeenCalledWith('USER123');
    expect(api.getModules).toHaveBeenCalledWith(10);

    expect(component.modules.length).toBe(2);

    const m1 = component.modules[0];
    const m2 = component.modules[1];

    // Check merged progress values
    expect(m1.progressPercent).toBe(100);
    expect(m1.isCompleted).toBeTrue();

    expect(m2.progressPercent).toBe(50);
    expect(m2.isCompleted).toBeFalse();

    expect(component.loading).toBeFalse();
  });

  // ------------------------------------------------------
  // ------------------------------------------------------
it('should stop loading if userId missing', () => {
  // Reset spies so previous test calls do NOT affect this one
  progress.getUserProgress.calls.reset();
  api.getModules.calls.reset();

  // Now simulate missing token
  (localStorage.getItem as jasmine.Spy).and.returnValue(null);

    const fixture2 = TestBed.createComponent(ModulesListComponent);
    const c2 = fixture2.componentInstance;

    fixture2.detectChanges();

    expect(c2.loading).toBeFalse();
    expect(progress.getUserProgress).not.toHaveBeenCalled();
    expect(api.getModules).not.toHaveBeenCalled();
  });


  // ------------------------------------------------------
  it('should navigate when openModule is called', () => {
    component.openModule(5);

    expect(router.navigate).toHaveBeenCalledWith(['/courses/module/5']);
  });

});
