import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { Home } from './home';
import { ToastService } from 'app/shared/toast.service';

class MockToastService {
  showError = jasmine.createSpy('showError');
}

function createFakeJWT(payload: any) {
  const encoded = btoa(JSON.stringify(payload));
  return `aaa.${encoded}.bbb`;
}

describe('Home Component', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let router: Router;
  let httpMock: HttpTestingController;
  let toastService: MockToastService;

  beforeEach(async () => {
    toastService = new MockToastService();

    await TestBed.configureTestingModule({
      imports: [Home, RouterTestingModule, HttpClientTestingModule],
      providers: [{ provide: ToastService, useValue: toastService }]
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    httpMock = TestBed.inject(HttpTestingController);

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  // ----------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ----------------------------------------------------------
  it('should redirect to "/" if accessToken is missing', () => {
    localStorage.removeItem('accessToken');

    const navSpy = spyOn(router, 'navigate');

    component.loadUserInfo();

    expect(navSpy).toHaveBeenCalledWith(['/']);
  });

  // ----------------------------------------------------------
  it('should decode token and load user info', () => {
    const token = createFakeJWT({ name: 'Kira', role: 'Instructor' });
    localStorage.setItem('accessToken', token);

    component.loadUserInfo();

    expect(component.userName).toBe('Kira');
    expect(component.role).toBe('Instructor');
  });

  // ----------------------------------------------------------
  it('should redirect to "/" if token cannot be decoded', () => {
    localStorage.setItem('accessToken', 'invalid.token');

    const navSpy = spyOn(router, 'navigate');

    component.loadUserInfo();

    expect(navSpy).toHaveBeenCalledWith(['/']);
  });

  // ----------------------------------------------------------
  it('should NOT call pending tasks if role is NOT Instructor', () => {
    const token = createFakeJWT({ name: 'User', role: 'Student' });
    localStorage.setItem('accessToken', token);

    const pendingSpy = spyOn(component, 'checkForPendingTasks');

    component.ngOnInit();

    expect(pendingSpy).not.toHaveBeenCalled();
  });

  // ----------------------------------------------------------
  it('should call checkForPendingTasks() if role is Instructor', () => {
    const token = createFakeJWT({ name: 'Teach', role: 'Instructor' });
    localStorage.setItem('accessToken', token);

    const pendingSpy = spyOn(component, 'checkForPendingTasks');

    component.ngOnInit();

    expect(pendingSpy).toHaveBeenCalled();
  });

  // ----------------------------------------------------------
  it('should not show toast if no unfinished course exists', () => {
    const token = createFakeJWT({ name: 'Teach', role: 'Instructor', sub: '111' });
    localStorage.setItem('accessToken', token);

    component.checkForPendingTasks();

    const req1 = httpMock.expectOne(`https://localhost:7249/api/GatewayCourse/unfinished/111`);
    req1.flush(null); // no course

    expect(toastService.showError).not.toHaveBeenCalled();
  });

  // ----------------------------------------------------------
  it('should not show toast if unfinished course exists but no unquizzed modules', () => {
    const token = createFakeJWT({ name: 'Teach', role: 'Instructor', sub: '222' });
    localStorage.setItem('accessToken', token);

    component.checkForPendingTasks();

    const req1 = httpMock.expectOne(`https://localhost:7249/api/GatewayCourse/unfinished/222`);
    req1.flush({ id: 10 });

    const req2 = httpMock.expectOne(`https://localhost:7249/api/AssessmentGateway/unquizzed-modules/10`);
    req2.flush([]); // no modules missing

    expect(toastService.showError).not.toHaveBeenCalled();
  });

  // ----------------------------------------------------------
  it('should show toast if unquizzed modules exist', () => {
    const token = createFakeJWT({ name: 'Teach', role: 'Instructor', sub: '333' });
    localStorage.setItem('accessToken', token);

    component.checkForPendingTasks();

    const req1 = httpMock.expectOne(`https://localhost:7249/api/GatewayCourse/unfinished/333`);
    req1.flush({ id: 55 });

    const req2 = httpMock.expectOne(`https://localhost:7249/api/AssessmentGateway/unquizzed-modules/55`);
    req2.flush([1, 2]); // missing modules

    expect(toastService.showError).toHaveBeenCalledWith(
      "You have pending course tasks — quizzes need to be completed."
    );
  });

  // ----------------------------------------------------------
  it('should navigate to courses page', () => {
    const navSpy = spyOn(router, 'navigate');

    component.goToCourses();

    expect(navSpy).toHaveBeenCalledWith(['/courses']);
  });
});
