import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { QuizResultComponent } from './quiz-result';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { Router, ActivatedRoute } from '@angular/router';
import { AssessmentFacade } from '@frontend/core';
import { of, throwError } from 'rxjs';

describe('QuizResultComponent', () => {
  let component: QuizResultComponent;
  let fixture: ComponentFixture<QuizResultComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate') };
  let apiMock: Partial<AssessmentFacade>;

  beforeEach(async () => {
    apiMock = {
      getQuizResult: jasmine.createSpy('getQuizResult').and.returnValue(of({ score: 90 }))
    };

    await TestBed.configureTestingModule({
      imports: [QuizResultComponent],
      providers: [
        provideCommonMocks,
        { provide: Router, useValue: routerMock },
        { provide: AssessmentFacade, useValue: apiMock },
        // Optional: override ActivatedRoute params for testing
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: { get: (key: string) => key === 'submissionId' ? 'sub123' : null },
              queryParamMap: { get: (key: string) => key === 'moduleId' ? '42' : null }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set submissionId and moduleId on init', () => {
    component.ngOnInit();
    expect(component.submissionId).toBe('sub123');
    expect(component.moduleId).toBe(42);
  });

  it('should load quiz result successfully', fakeAsync(() => {
    component.loadResult();
    tick();
    expect(component.result).toEqual({ score: 90 });
    expect(component.loading).toBe(false);
    expect(apiMock.getQuizResult).toHaveBeenCalledWith({ submissionId: 'sub123' });
  }));

  it('should handle error when loading quiz result', fakeAsync(() => {
    (apiMock.getQuizResult as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    spyOn(console, 'error');
    component.loadResult();
    tick();
    expect(component.loading).toBe(true); // stays true because error doesn't update it
    expect(console.error).toHaveBeenCalled();
  }));

  it('should navigate back to module if moduleId exists', () => {
    component.moduleId = 42;
    component.backToModules();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses/module/42']);
  });

  it('should navigate back to courses if no moduleId', () => {
    component.moduleId = 0;
    component.backToModules();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses']);
  });
});
