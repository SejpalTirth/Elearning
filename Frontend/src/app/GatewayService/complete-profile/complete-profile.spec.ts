import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CompleteProfileComponent } from './complete-profile';
import { ParamMap } from '@angular/router';

// ----------------- Helpers -----------------
function mockActivatedRoute(userId: string | null): Partial<ActivatedRoute> {
  const paramMap: Partial<ParamMap> = {
    get: () => userId,
    has: (key: string) => key === 'userId' && userId !== null,
    getAll: (key: string) => key === 'userId' && userId !== null ? [userId as string] : [],
    keys: ['userId'],
  };

  return {
    snapshot: {
      queryParamMap: paramMap as ParamMap,
      params: {},
      queryParams: {},
      fragment: null,
    } as ActivatedRouteSnapshot
  };
}

async function createComponent(userId: string | null = '123'): Promise<{
  component: CompleteProfileComponent;
  fixture: ComponentFixture<CompleteProfileComponent>;
  router: Router;
  httpMock: HttpTestingController;
}> {
  await TestBed.configureTestingModule({
    imports: [CompleteProfileComponent, HttpClientTestingModule, RouterTestingModule],
    providers: [{ provide: ActivatedRoute, useValue: mockActivatedRoute(userId) }]
  }).compileComponents();

  const fixture = TestBed.createComponent(CompleteProfileComponent);
  const component = fixture.componentInstance;
  const router = TestBed.inject(Router);
  const httpMock = TestBed.inject(HttpTestingController);

  spyOn(window, 'alert');
  fixture.detectChanges();

  return { component, fixture, router, httpMock };
}

// ----------------- Tests -----------------
describe('CompleteProfileComponent', (): void => {
  let component: CompleteProfileComponent;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(async (): Promise<void> => {
    const result = await createComponent();
    component = result.component;
    router = result.router;
    httpMock = result.httpMock;
  });

  afterEach((): void => {
    httpMock.verify();
    localStorage.clear();
  });

  // -------- Creation Tests --------
  it('should create', (): void => {
    expect(component).toBeTruthy();
  });

  it('should redirect to login if userId is missing', async (): Promise<void> => {
    const result = await createComponent(null);
    component = result.component;
    router = result.router;

    const navSpy = spyOn(router, 'navigate');
    component.ngOnInit();

    expect(window.alert).toHaveBeenCalledWith('User ID missing. Please login again.');
    expect(navSpy).toHaveBeenCalledWith(['/login']);
  });

  // -------- Validation Tests --------
  it('should not save if name is empty', (): void => {
    component.name = '   ';
    component.saveProfile();
    expect(window.alert).toHaveBeenCalledWith('Please enter your name.');
  });

  // -------- API Tests --------
  it('should send POST request when saving profile', (): void => {
    component.name = 'Kira';
    const navSpy = spyOn(router, 'navigate');

    component.saveProfile();

    const req = httpMock.expectOne('https://localhost:7130/api/users/complete-profile');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ userId: '123', name: 'Kira', roleId: 3 });

    req.flush({});

    expect(window.alert).toHaveBeenCalledWith('Profile completed successfully! Please login again.');
    expect(navSpy).toHaveBeenCalledWith(['/login']);
  });

  it('should handle API error correctly', (): void => {
    component.name = 'Kira';
    component.saveProfile();
    expect(component.loading).toBeTrue();

    const req = httpMock.expectOne('https://localhost:7130/api/users/complete-profile');
    req.flush({ message: 'Something failed' }, { status: 400, statusText: 'Bad Request' });

    expect(component.loading).toBeFalse();
    expect(window.alert).toHaveBeenCalledWith('Something failed');
  });

  it('should show generic error if API response has no message', (): void => {
    component.name = 'Kira';
    component.saveProfile();

    const req = httpMock.expectOne('https://localhost:7130/api/users/complete-profile');
    req.flush({}, { status: 500, statusText: 'Server Error' });

    expect(component.loading).toBeFalse();
    expect(window.alert).toHaveBeenCalledWith('Something went wrong.');
  });
});
