import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CompleteProfileComponent } from './complete-profile';

describe('CompleteProfileComponent', () => {
  let component: CompleteProfileComponent;
  let fixture: ComponentFixture<CompleteProfileComponent>;
  let httpMock: HttpTestingController;
  let router: Router;

  function mockUserId(id: string | null) {
    return {
      snapshot: {
        queryParamMap: {
          get: () => id
        }
      }
    } as any;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CompleteProfileComponent,
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockUserId("123") }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CompleteProfileComponent);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    httpMock = TestBed.inject(HttpTestingController);

    spyOn(window, "alert");  // prevent real alerts

    fixture.detectChanges();
  });

  // ----------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ----------------------------------------------
  it('should redirect to login if userId is missing', () => {
    const routeMock = mockUserId(null);

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [CompleteProfileComponent, RouterTestingModule, HttpClientTestingModule],
      providers: [{ provide: ActivatedRoute, useValue: routeMock }]
    }).compileComponents();

    const fix = TestBed.createComponent(CompleteProfileComponent);
    const comp = fix.componentInstance;

    const nav = spyOn(TestBed.inject(Router), 'navigate');

    comp.ngOnInit();

    expect(window.alert).toHaveBeenCalledWith("User ID missing. Please login again.");
    expect(nav).toHaveBeenCalledWith(['/login']);
  });

  // ----------------------------------------------
  it('should not save if name is empty', () => {
    component.name = "   "; // invalid name

    component.saveProfile();

    expect(window.alert).toHaveBeenCalledWith("Please enter your name.");
  });

  // ----------------------------------------------
  it('should send POST request when saving profile', () => {
    component.name = "Kira";

    const navSpy = spyOn(router, 'navigate');

    component.saveProfile();

    const req = httpMock.expectOne("https://localhost:7130/api/users/complete-profile");

    expect(req.request.method).toBe("POST");
    expect(req.request.body).toEqual({
      userId: "123",
      name: "Kira",
      roleId: 3
    });

    req.flush({}); // simulate success

    expect(window.alert).toHaveBeenCalledWith("Profile completed successfully! Please login again.");
    expect(navSpy).toHaveBeenCalledWith(['/login']);
  });

  // ----------------------------------------------
  it('should handle API error correctly', () => {
    component.name = "Kira";

    component.saveProfile();
    expect(component.loading).toBeTrue();

    const req = httpMock.expectOne("https://localhost:7130/api/users/complete-profile");

    req.flush({ message: "Something failed" }, { status: 400, statusText: "Bad Request" });

    expect(component.loading).toBeFalse();
    expect(window.alert).toHaveBeenCalledWith("Something failed");
  });

  // ----------------------------------------------
  it('should show generic error if API response has no message', () => {
    component.name = "Kira";

    component.saveProfile();

    const req = httpMock.expectOne("https://localhost:7130/api/users/complete-profile");

    req.flush({}, { status: 500, statusText: "Server Error" });

    expect(component.loading).toBeFalse();
    expect(window.alert).toHaveBeenCalledWith("Something went wrong.");
  });
});
