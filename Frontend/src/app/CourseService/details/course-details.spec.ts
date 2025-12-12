import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseDetailsComponent } from './course-details';
import { CourseApiService } from '../services/course-api';

class MockCourseApi {
  getById = jasmine.createSpy().and.returnValue(of({ id: 10, title: 'Course A' }));
  getEnrolledCourses = jasmine.createSpy().and.returnValue(of([]));
  enroll = jasmine.createSpy().and.returnValue(of({}));
}

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

const mockRoute = {
  snapshot: {
    paramMap: { get: (): string => '10' }
  }
};

function fakeJWT(payload: any): string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

// =======================================================
// PART 1: Creation, Initialization, Navbar, User Extraction
// =======================================================
describe('CourseDetailsComponent - Initialization & Navbar', () => {
  let component: CourseDetailsComponent;
  let fixture: ComponentFixture<CourseDetailsComponent>;
  let api: MockCourseApi;
  let router: MockRouter;
  let fakeNav: HTMLElement;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [CourseDetailsComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailsComponent);
    component = fixture.componentInstance;

    fakeNav = document.createElement('nav');
    document.body.appendChild(fakeNav);
    spyOn(document, 'querySelector').and.returnValue(fakeNav);
    spyOn(component['renderer'], 'setStyle').and.callThrough();

    fixture.detectChanges();
  });

  afterEach(() => fakeNav.remove());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should extract userId', () => {
    localStorage.setItem('accessToken', fakeJWT({ sub: 'USER999' }));
    component.extractUserId();
    expect(component.userId).toBe('USER999');
  });

  it('should NOT extract when token missing', () => {
    component.userId = 'OLD';
    localStorage.removeItem('accessToken');
    component.extractUserId();
    expect(component.userId).toBe('');
  });

  it('should disable navbar clicks', () => {
    component.disableNavbarClicks();
    expect(component['renderer'].setStyle)
      .toHaveBeenCalledWith(fakeNav, 'pointer-events', 'none');
  });

  it('should enable navbar clicks', () => {
    component.enableNavbarClicks();
    expect(component['renderer'].setStyle)
      .toHaveBeenCalledWith(fakeNav, 'pointer-events', 'auto');
  });
});

// =======================================================
// PART 2: Enrollment / Navigation / API interactions
// =======================================================
describe('CourseDetailsComponent - Enrollment & Navigation', () => {
  let component: CourseDetailsComponent;
  let fixture: ComponentFixture<CourseDetailsComponent>;
  let api: MockCourseApi;
  let router: MockRouter;
  let fakeNav: HTMLElement;

  beforeEach(async () => {
    api = new MockCourseApi();
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [CourseDetailsComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: CourseApiService, useValue: api },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailsComponent);
    component = fixture.componentInstance;

    fakeNav = document.createElement('nav');
    document.body.appendChild(fakeNav);
    spyOn(document, 'querySelector').and.returnValue(fakeNav);
    spyOn(component['renderer'], 'setStyle').and.callThrough();

    fixture.detectChanges();
  });

  afterEach(() => fakeNav.remove());

  it('should enroll user when not enrolled', fakeAsync(() => {
    component.isEnrolled = false;
    component.userId = 'U1';
    component.courseId = 10;

    component.enrollOrContinue();

    expect(api.enroll).toHaveBeenCalledWith({ courseId: 10, userId: 'U1' });

    tick(800);

    expect(component['renderer'].setStyle).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/courses/10/modules']);
  }));

  it('should handle enrollment failure', () => {
    api.enroll.and.returnValue(throwError(() => 'ERR'));

    component.isEnrolled = false;
    component.userId = 'U1';
    component.courseId = 10;

    component.enrollOrContinue();

    expect(component['renderer'].setStyle).toHaveBeenCalled();
    expect(component.btnLoading).toBeFalse();
    expect(component.isLoading).toBeFalse();
  });

  it('should navigate when already enrolled', () => {
    component.isEnrolled = true;
    component.courseId = 10;

    component.enrollOrContinue();

    expect(router.navigate).toHaveBeenCalledWith(['/courses/10/modules']);
  });
});
