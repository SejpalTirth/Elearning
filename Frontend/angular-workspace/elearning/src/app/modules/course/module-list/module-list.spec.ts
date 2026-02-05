import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModulesListComponent } from './module-list';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthStateService } from '../../../service/auth-state-service';
import { GatewayCourseService, ProgressGatewayService } from 'api';
import { of, BehaviorSubject, throwError } from 'rxjs';

describe('ModulesListComponent', () => {
  let component: ModulesListComponent;
  let fixture: ComponentFixture<ModulesListComponent>;
  let mockRouter: any;
  let mockAuthState: any;
  let mockCourseApi: any;
  let mockProgressApi: any;
  const userSubject = new BehaviorSubject<any>(null);

  const mockModules = [
    { id: 101, title: 'Module 1' },
    { id: 102, title: 'Module 2' }
  ];

  const mockProgress = [
    { courseId: 50, moduleId: 101, progressPercent: 100, isCompleted: true }
  ];

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockAuthState = { user$: userSubject.asObservable() };
    mockCourseApi = {
      postApiCourseModules: jasmine.createSpy('postApiCourseModules').and.returnValue(of(mockModules))
    };
    mockProgressApi = {
      postApiProgressUser: jasmine.createSpy('postApiProgressUser').and.returnValue(of(mockProgress))
    };

    await TestBed.configureTestingModule({
      imports: [ModulesListComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: GatewayCourseService, useValue: mockCourseApi },
        { provide: ProgressGatewayService, useValue: mockProgressApi },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => '50' } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModulesListComponent);
    component = fixture.componentInstance;
  });

  it('should not fetch modules if user is not logged in', () => {
    userSubject.next(null);
    fixture.detectChanges();

    expect(component.modules).toEqual([]);
    expect(component.loading).toBeFalse();
    expect(mockCourseApi.postApiCourseModules).not.toHaveBeenCalled();
  });

  it('should bind modules with user progress on login', () => {
    userSubject.next({ userId: 'user_123' });
    fixture.detectChanges();

    expect(component.courseId).toBe(50);
    expect(mockCourseApi.postApiCourseModules).toHaveBeenCalledWith({ courseId: 50 });
    expect(mockProgressApi.postApiProgressUser).toHaveBeenCalled();
    
    expect(component.modules[0].progressPercent).toBe(100);
    expect(component.modules[0].isCompleted).toBeTrue();
    expect(component.modules[1].progressPercent).toBe(0);
    expect(component.modules[1].isCompleted).toBeFalse();
    expect(component.loading).toBeFalse();
  });

  it('should navigate to module content when opened', () => {
    component.openModule(101);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/module/101']);
  });

  it('should handle API errors gracefully', () => {
    mockCourseApi.postApiCourseModules.and.returnValue(throwError(() => new Error('Fail')));
    userSubject.next({ userId: 'user_123' });
    fixture.detectChanges();

    expect(component.loading).toBeFalse();
  });

  it('should unsubscribe on destroy', () => {
    userSubject.next({ userId: 'user_123' });
    fixture.detectChanges();
    const unsubscribeSpy = spyOn((component as any).sub, 'unsubscribe');
    component.ngOnDestroy();
    expect(unsubscribeSpy).toHaveBeenCalled();
  });
});