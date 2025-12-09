import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';

import { Pending } from './pending';
import { ToastService } from '../../shared/toast.service';

// ---------------------------------------------------------
// Mocks
// ---------------------------------------------------------

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

class MockToast {
  showSuccess = jasmine.createSpy('success');
  showError = jasmine.createSpy('error');
}

function fakeJWT(payload: any) {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

// ---------------------------------------------------------

describe('Pending Component', () => {

  let component: Pending;
  let fixture: ComponentFixture<Pending>;
  let httpMock: HttpTestingController;
  let router: MockRouter;
  let toast: MockToast;

  const courseGateway = "https://localhost:7249/api/GatewayCourse";
  const assessmentGateway = "https://localhost:7249/api/AssessmentGateway";

  beforeEach(async () => {

    router = new MockRouter();
    toast = new MockToast();

    // Mock JWT
    spyOn(localStorage, 'getItem').and.returnValue(fakeJWT({ sub: 'USER123' }));

    await TestBed.configureTestingModule({
      imports: [Pending, HttpClientTestingModule],
      providers: [
        { provide: Router, useValue: router },
        { provide: ToastService, useValue: toast }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Pending);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    fixture.detectChanges();

    // ⭐ Always flush the FIRST request from ngOnInit
    const initReq = httpMock.expectOne(`${courseGateway}/unfinished/USER123`);
    initReq.flush(null); // default for tests that do not override this
  });

  afterEach(() => {
    httpMock.verify();
  });

  // ---------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------------------------------------------------
  it('should extract userId from token', () => {
    expect(component.userId).toBe('USER123');
  });

  // ---------------------------------------------------------
  it('should show success toast when no unfinished course', () => {
    // After beforeEach default flush(null), component already processed it
    expect(component.course).toBeNull();
    expect(toast.showSuccess).toHaveBeenCalled();
  });

  // ---------------------------------------------------------
  it('should load pending modules when course returned', () => {
    // Re-trigger API manually
    component.loadUnfinishedCourse();

    const req = httpMock.expectOne(`${courseGateway}/unfinished/USER123`);
    req.flush({ id: 10 });

    const pendingReq = httpMock.expectOne(`${assessmentGateway}/unquizzed-modules/10`);
    pendingReq.flush([1, 2, 3]);

    expect(component.pendingModules).toEqual([1, 2, 3]);
    expect(component.allQuizzesDone).toBeFalse();
    expect(toast.showError).toHaveBeenCalled();
  });

  // ---------------------------------------------------------
  it('should mark allQuizzesDone=true when no pending modules', () => {
    component.loadUnfinishedCourse();

    const req = httpMock.expectOne(`${courseGateway}/unfinished/USER123`);
    req.flush({ id: 10 });

    const pendingReq = httpMock.expectOne(`${assessmentGateway}/unquizzed-modules/10`);
    pendingReq.flush([]);

    expect(component.allQuizzesDone).toBeTrue();
    expect(toast.showSuccess).toHaveBeenCalled();
  });

  // ---------------------------------------------------------
  it('continueQuiz() should navigate correctly', () => {
    component.course = { id: 10 };
    component.continueQuiz();

    expect(router.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 10]);
  });

  // ---------------------------------------------------------
  it('publishCourse() should POST publish and navigate home', () => {
    component.course = { id: 10 };

    component.publishCourse();

    const publishReq = httpMock.expectOne(`${courseGateway}/10/publish`);
    expect(publishReq.request.method).toBe('POST');
    publishReq.flush({});

    expect(toast.showSuccess).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  // ---------------------------------------------------------
  it('publishCourse() should show error toast on fail', () => {
    component.course = { id: 10 };

    component.publishCourse();

    const publishReq = httpMock.expectOne(`${courseGateway}/10/publish`);
    publishReq.flush('Error', { status: 500, statusText: 'Error' });

    expect(toast.showError).toHaveBeenCalled();
  });

});
