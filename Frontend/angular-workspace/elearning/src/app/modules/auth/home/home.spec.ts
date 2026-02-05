import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';
import { Router } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { AuthStateService } from '../../../service/auth-state-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { AssessmentGatewayService, GatewayCourseService } from 'api';

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let mockRouter: any;
  let mockAuthState: any;
  let mockToast: any;
  let mockCourseApi: any;
  let mockAssessmentApi: any;

  const statusSubject = new BehaviorSubject<string>('idle');

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showError: jasmine.createSpy('showError') };
    
    mockAuthState = {
      status$: statusSubject.asObservable(),
      user: { userId: 'u1', name: 'Test User', role: 'Student' }
    };

    mockCourseApi = {
      postApiCourseUnfinished: jasmine.createSpy('postApiCourseUnfinished').and.returnValue(of({ id: 505 }))
    };

    mockAssessmentApi = {
      postApiAssessmentCourseUnquizzedModules: jasmine.createSpy('postApiAssessmentCourseUnquizzedModules').and.returnValue(of([1, 2]))
    };

    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: AssessmentGatewayService, useValue: mockAssessmentApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
  });

  it('should navigate to root if status is idle', () => {
    statusSubject.next('idle');
    fixture.detectChanges();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should set user data when authenticated', () => {
    statusSubject.next('authenticated');
    fixture.detectChanges();

    expect(component.userId).toBe('u1');
    expect(component.userName).toBe('Test User');
    expect(component.role).toBe('Student');
  });

  describe('Instructor workflow', () => {
    it('should check for pending tasks and show toast if modules are unquizzed', () => {
      mockAuthState.user.role = 'Instructor';
      statusSubject.next('authenticated');
      fixture.detectChanges();

      expect(mockCourseApi.postApiCourseUnfinished).toHaveBeenCalled();
      expect(mockAssessmentApi.postApiAssessmentCourseUnquizzedModules).toHaveBeenCalledWith({ courseId: 505 });
      expect(mockToast.showError).toHaveBeenCalledWith(
        'You have pending course tasks — quizzes need to be completed.'
      );
    });

    it('should not show toast if there are no unquizzed modules', () => {
      mockAuthState.user.role = 'Instructor';
      mockAssessmentApi.postApiAssessmentCourseUnquizzedModules.and.returnValue(of([]));
      statusSubject.next('authenticated');
      fixture.detectChanges();

      expect(mockToast.showError).not.toHaveBeenCalled();
    });
  });

  it('should navigate to courses when goToCourses is called', () => {
    component.goToCourses();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
  });

  it('should unsubscribe on destroy', () => {
    fixture.detectChanges();
    const sub = component['authSub'];
    const spy = spyOn(sub!, 'unsubscribe');
    component.ngOnDestroy();
    expect(spy).toHaveBeenCalled();
  });
});