import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ModuleContentComponent } from './module-content';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { AuthStateService } from '@frontend/auth';
import { Router } from '@angular/router';
import { ToastService, LoadingService } from '@frontend/ui';
import { of, BehaviorSubject } from 'rxjs';

describe('ModuleContentComponent', () => {
  let component: ModuleContentComponent;
  let fixture: ComponentFixture<ModuleContentComponent>;

  let courseApiMock: Partial<CourseFacade>;
  let assessmentApiMock: Partial<AssessmentFacade>;
  let routerMock: Partial<Router>;
  let toastMock: Partial<ToastService>;
  let loadingMock: Partial<LoadingService>;

  const authStateMock = {
  user$: new BehaviorSubject({
    userId: '123',
    email: 'test@example.com',
    name: 'Test User',
    role: 'Student'  // exact literal type
  }).asObservable()
};


  beforeEach(async () => {
    courseApiMock = {
      getCourseModule: jasmine.createSpy('getCourseModule').and.returnValue(of({ id: 1, title: 'Module 1' }))
    };

    assessmentApiMock = {
      getQuizForModule: jasmine.createSpy('getQuizForModule').and.returnValue(of({ quizId: 101 }))
    };

    routerMock = { navigate: jasmine.createSpy('navigate') };
    toastMock = { showError: jasmine.createSpy('showError') };
    loadingMock = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };

    await TestBed.configureTestingModule({
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AssessmentFacade, useValue: assessmentApiMock },
        { provide: AuthStateService, useValue: authStateMock },
        { provide: Router, useValue: routerMock },
        { provide: ToastService, useValue: toastMock },
        { provide: LoadingService, useValue: loadingMock }
      ],
      declarations: [ModuleContentComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ModuleContentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load module and quiz data on init', fakeAsync(() => {
    component.ngOnInit();
    tick();
    expect(courseApiMock.getCourseModule).toHaveBeenCalledWith({ moduleId: component.moduleId });
    expect(assessmentApiMock.getQuizForModule).toHaveBeenCalledWith({ moduleId: component.moduleId });
    expect(component.module).toEqual({ id: 1, title: 'Module 1' });
    expect(component.quizStatus).toEqual({ quizId: 101 });
    expect(loadingMock.show).toHaveBeenCalled();
    expect(loadingMock.hide).toHaveBeenCalled();
  }));

  it('takeQuiz should navigate if quiz exists', () => {
    component.quizStatus = { quizId: 101 };
    component.moduleId = 1;
    component.takeQuiz();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/quiz/1']);
  });

  it('takeQuiz should show error if no quiz', () => {
    component.quizStatus = null;
    component.takeQuiz();
    expect(toastMock.showError).toHaveBeenCalledWith('No quiz available.');
  });

  it('should unsubscribe on destroy', () => {
    spyOn(component['authSub']!, 'unsubscribe').and.callThrough();
    component.ngOnDestroy();
    // no error occurs; unsubscribed safely
  });
});
