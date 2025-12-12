import { TestBed, ComponentFixture } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';

import { Pending } from './pending';
import { ToastService } from '../../shared/toast.service';

class MockRouter {
  navigate = jasmine.createSpy('navigate');
}

class MockToast {
  showSuccess = jasmine.createSpy('success');
  showError = jasmine.createSpy('error');
}

function fakeJWT(payload: any): string {
  return `aaa.${btoa(JSON.stringify(payload))}.bbb`;
}

/* ============================================================
   PART 1 — Initialization & User ID Extraction
   ============================================================ */

describe('Pending Component - Initialization', () => {
  let component: Pending;
  let fixture: ComponentFixture<Pending>;
  let httpMock: HttpTestingController;
  let toast: MockToast;
  let router: MockRouter;

  const courseGateway = 'https://localhost:7249/api/GatewayCourse';

  beforeEach(async () => {
    router = new MockRouter();
    toast = new MockToast();

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

    // Respond to initial GET unfinished request
    const req = httpMock.expectOne(`${courseGateway}/unfinished/USER123`);
    req.flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should extract userId from token', () => {
    expect(component.userId).toBe('USER123');
  });

  it('should handle no pending courses', () => {
    expect(component.pendingCourses.length).toBe(0);
  });
});

/* ============================================================
   PART 2 — Pending Modules Loading
   ============================================================ */

describe('Pending Component - Pending Module Loading', () => {
  let component: Pending;
  let fixture: ComponentFixture<Pending>;
  let httpMock: HttpTestingController;

  const courseGateway = 'https://localhost:7249/api/GatewayCourse';
  const assessmentGateway = 'https://localhost:7249/api/AssessmentGateway';

  beforeEach(async () => {
    spyOn(localStorage, 'getItem').and.returnValue(fakeJWT({ sub: 'USER1' }));

    await TestBed.configureTestingModule({
      imports: [Pending, HttpClientTestingModule],
      providers: [{ provide: ToastService, useValue: new MockToast() }]
    }).compileComponents();

    fixture = TestBed.createComponent(Pending);
    component = fixture.componentInstance;

    httpMock = TestBed.inject(HttpTestingController);

    fixture.detectChanges();

    // Respond to unfinished courses call
    const req = httpMock.expectOne(`${courseGateway}/unfinished/USER1`);
    req.flush([{ id: 10, title: 'C# Basics', isDeleted: false }]);
  });

  afterEach(() => httpMock.verify());

  it('should load modules pending quiz creation', () => {
    const quizReq = httpMock.expectOne(`${assessmentGateway}/unquizzed/10`);
    expect(quizReq.request.method).toBe('GET');

    quizReq.flush([100, 101, 102]);

    expect(component.pendingCourses[0].pendingModules.length).toBe(3);
  });
});

/* ============================================================
   PART 3 — Actions (continueQuiz + publishCourse)
   ============================================================ */

describe('Pending Component - Actions', () => {
  let component: Pending;
  let fixture: ComponentFixture<Pending>;
  let httpMock: HttpTestingController;
  let router: MockRouter;
  let toast: MockToast;

  const courseGateway = 'https://localhost:7249/api/GatewayCourse';

  beforeEach(async () => {
    router = new MockRouter();
    toast = new MockToast();

    spyOn(localStorage, 'getItem').and.returnValue(fakeJWT({ sub: 'ABC123' }));

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

    // Respond to unfinished courses call
    const req = httpMock.expectOne(`${courseGateway}/unfinished/ABC123`);
    req.flush([{ id: 10, title: 'TS Advanced', isDeleted: false }]);
  });

  afterEach(() => httpMock.verify());

  it('should navigate on continueQuiz()', () => {
    component.continueQuiz(10);
    expect(router.navigate).toHaveBeenCalledWith(['/assessment/add-quiz', 10]);
  });

  it('should publish course successfully', () => {
    component.publishCourse(10);

    const req = httpMock.expectOne(`${courseGateway}/10/publish`);
    expect(req.request.method).toBe('POST');
    req.flush({});

    expect(toast.showSuccess).toHaveBeenCalled();
  });

  it('should show error toast if publish fails', () => {
    component.publishCourse(10);

    const req = httpMock.expectOne(`${courseGateway}/10/publish`);
    req.flush('err', { status: 500, statusText: 'Server Error' });

    expect(toast.showError).toHaveBeenCalled();
  });
});
