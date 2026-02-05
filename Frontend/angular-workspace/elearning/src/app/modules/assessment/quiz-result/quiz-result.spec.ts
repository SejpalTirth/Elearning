import { ComponentFixture, TestBed } from '@angular/core/testing';
import { QuizResultComponent } from './quiz-result';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AssessmentGatewayService } from 'api';

describe('QuizResultComponent', () => {
  let component: QuizResultComponent;
  let fixture: ComponentFixture<QuizResultComponent>;
  let mockRouter: any;
  let mockActivatedRoute: any;
  let mockAssessmentApi: any;

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    
    mockActivatedRoute = {
      snapshot: {
        paramMap: { get: (key: string) => key === 'submissionId' ? 'sub-123' : null },
        queryParamMap: { get: (key: string) => key === 'moduleId' ? '456' : null }
      }
    };

    mockAssessmentApi = {
      postApiAssessmentQuizResult: jasmine.createSpy('postApiAssessmentQuizResult').and.returnValue(of({ score: 80, passed: true }))
    };

    await TestBed.configureTestingModule({
      imports: [QuizResultComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizResultComponent);
    component = fixture.componentInstance;
  });

  it('should initialize with params and load results', () => {
    fixture.detectChanges();

    expect(component.submissionId).toBe('sub-123');
    expect(component.moduleId).toBe(456);
    expect(mockAssessmentApi.postApiAssessmentQuizResult).toHaveBeenCalledWith({ submissionId: 'sub-123' });
    expect(component.result).toEqual({ score: 80, passed: true });
    expect(component.loading).toBeFalse();
  });

  it('should handle API error gracefully', () => {
    mockAssessmentApi.postApiAssessmentQuizResult.and.returnValue(throwError(() => new Error('Error')));
    spyOn(console, 'error');

    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
    expect(component.loading).toBeTrue();
  });

  describe('backToModules()', () => {
    it('should navigate to specific module path if moduleId exists', () => {
      fixture.detectChanges();
      component.backToModules();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/module/456']);
    });

    it('should navigate to courses if moduleId is missing', () => {
      fixture.detectChanges();
      
      component.moduleId = 0; 
      
      mockRouter.navigate.calls.reset();

      component.backToModules();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
    });
  });
});