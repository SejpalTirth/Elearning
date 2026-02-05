import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CourseDetailsComponent } from './course-details';
import { ActivatedRoute, Router } from '@angular/router';
import { GatewayCourseService } from 'api';
import { of, throwError } from 'rxjs';

describe('CourseDetailsComponent', () => {
  let component: CourseDetailsComponent;
  let fixture: ComponentFixture<CourseDetailsComponent>;
  let mockRouter: any;
  let mockApi: any;

  beforeEach(async () => {
    mockRouter = {
      navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true))
    };

    mockApi = {
      postApiCourseById: jasmine.createSpy('postApiCourseById').and.returnValue(of({ id: 101, title: 'Angular Pro' })),
      postApiCourseEnrolled: jasmine.createSpy('postApiCourseEnrolled').and.returnValue(of([{ id: 101 }])),
      postApiCourseEnroll: jasmine.createSpy('postApiCourseEnroll').and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [CourseDetailsComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: GatewayCourseService, useValue: mockApi },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => '101'
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load course details and check enrollment on init', () => {
    expect(component.courseId).toBe(101);
    expect(mockApi.postApiCourseById).toHaveBeenCalledWith({ courseId: 101 });
    expect(mockApi.postApiCourseEnrolled).toHaveBeenCalled();
    expect(component.isEnrolled).toBeTrue();
  });

  describe('enrollOrContinue', () => {
    it('should navigate directly if already enrolled', () => {
      component.isEnrolled = true;
      component.enrollOrContinue();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses', 101, 'modules']);
    });

    it('should call enroll API and navigate on success', fakeAsync(() => {
      component.isEnrolled = false;
      component.enrollOrContinue();

      expect(component.isLoading).toBeTrue();
      expect(mockApi.postApiCourseEnroll).toHaveBeenCalledWith({ courseId: 101 });
      
      tick(); 
      
      expect(component.isEnrolled).toBeTrue();
      expect(component.isLoading).toBeFalse();
    }));

    it('should handle 400 error as an edge-case enrollment', fakeAsync(() => {
      component.isEnrolled = false;
      mockApi.postApiCourseEnroll.and.returnValue(throwError(() => ({ status: 400 })));

      component.enrollOrContinue();
      tick();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses', 101, 'modules']);
      expect(component.isLoading).toBeFalse();
    }));

    it('should stop loading on real API error', () => {
      component.isEnrolled = false;

      mockApi.postApiCourseEnroll.and.returnValue(throwError(() => ({ status: 500 })));
      
      mockRouter.navigate.calls.reset();
      
      component.enrollOrContinue();
      
      expect(component.isLoading).toBeFalse();
      expect(component.btnLoading).toBeFalse();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });
});