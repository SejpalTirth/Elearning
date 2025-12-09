import { ComponentFixture, TestBed } from '@angular/core/testing';
import { QuizResultComponent } from './quiz-result';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { AssessmentApiService } from '../services/assessment-api';

describe('QuizResultComponent', () => {
  let component: QuizResultComponent;
  let fixture: ComponentFixture<QuizResultComponent>;

  let mockApi: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockApi = {
      getResult: jasmine.createSpy('getResult').and.returnValue(
        of({ score: 90, passed: true })
      )
    };

    mockRouter = {
      navigate: jasmine.createSpy('navigate')
    };

    await TestBed.configureTestingModule({
      imports: [QuizResultComponent],
      providers: [
        { 
          provide: ActivatedRoute, 
          useValue: {
            snapshot: {
              paramMap: { get: () => 'SUB123' },
              queryParamMap: { get: () => '55' }
            }
          }
        },
        { provide: AssessmentApiService, useValue: mockApi },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit()
  });

  // ---------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------------------------------------------------
  it('should read submissionId and moduleId from route', () => {
    expect(component.submissionId).toBe('SUB123');
    expect(component.moduleId).toBe(55);
  });

  // ---------------------------------------------------------
  it('should load quiz result from API', () => {
    expect(mockApi.getResult).toHaveBeenCalledWith('SUB123');
    expect(component.result).toEqual({ score: 90, passed: true });
    expect(component.loading).toBeFalse();
  });

  // ---------------------------------------------------------
  it('should navigate back to module when moduleId exists', () => {
    component.backToModules();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/module/55']);
  });

  // ---------------------------------------------------------
  it('should fallback navigate when no moduleId exists', () => {
    component.moduleId = 0; // simulate missing moduleId

    component.backToModules();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
  });

});
