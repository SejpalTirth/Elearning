import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { ModuleContentComponent } from './module-content';
import { CourseApiService } from '../services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';

// --------------------------------------------------------------
// Mock Services
// --------------------------------------------------------------

class MockCourseApi {
  getModuleById = jasmine.createSpy().and.returnValue(
    of({ id: 5, title: 'Module A', content: '...' })
  );
}

class MockAssessmentApi {
  getQuizForModule = jasmine.createSpy().and.returnValue(
    of({ quizId: 200 })
  );
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

// Fake ActivatedRoute → moduleId = 5
const mockRoute = {
  snapshot: {
    paramMap: {
      get: (): string=> '5'
    }
  }
};

// Utility to create fake JWT
function fakeJWT(payload: any):string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

describe('ModuleContentComponent', () => {

  let component: ModuleContentComponent;
  let fixture: ComponentFixture<ModuleContentComponent>;
  let router: MockRouter;
  let courseApi: MockCourseApi;
  let assessmentApi: MockAssessmentApi;

  beforeEach(async () => {
    router = new MockRouter();
    courseApi = new MockCourseApi();
    assessmentApi = new MockAssessmentApi();

    await TestBed.configureTestingModule({
      imports: [ModuleContentComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: CourseApiService, useValue: courseApi },
        { provide: AssessmentApiService, useValue: assessmentApi }
      ]
    }).compileComponents();

    // Mock local storage JWT
    localStorage.setItem('accessToken', fakeJWT({ sub: 'USER123' }));

    fixture = TestBed.createComponent(ModuleContentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // --------------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // --------------------------------------------------------------
  it('should load module data on init', () => {
    expect(courseApi.getModuleById).toHaveBeenCalledWith(5);
    expect(component.module).toEqual(jasmine.objectContaining({ id: 5 }));
  });

  // --------------------------------------------------------------
  it('should load quiz status', () => {
    expect(assessmentApi.getQuizForModule).toHaveBeenCalledWith(5);
    expect(component.quizStatus).toEqual(jasmine.objectContaining({ quizId: 200 }));
  });

  // --------------------------------------------------------------
  it('should navigate to quiz when quiz exists', () => {
    component.quizStatus = { quizId: 200 };

    component.takeQuiz();

    expect(router.navigate).toHaveBeenCalledWith(['/assessment/take/5']);
  });

  // --------------------------------------------------------------
  it('should alert when no quiz exists', () => {
    spyOn(window, 'alert');

    component.quizStatus = null;
    component.takeQuiz();

    expect(window.alert).toHaveBeenCalledWith('No quiz available.');
    expect(router.navigate).not.toHaveBeenCalled();
  });

});
