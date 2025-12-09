import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { CourseApiService } from './course-api';

describe('CourseApiService', () => {
  let service: CourseApiService;
  let httpMock: HttpTestingController;

  const baseUrl = 'https://localhost:7249/api/GatewayCourse';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });

    service = TestBed.inject(CourseApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // ensure no pending HTTP calls
  });

  // ---------------------------------------------------------
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ---------------------------------------------------------
  it('getAll() should call GET /', () => {
    service.getAll().subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('getById() should call GET /:id', () => {
    service.getById(10).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/10`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  // ---------------------------------------------------------
  it('addCourse() should POST to /', () => {
    const payload = { title: 'New Course' };

    service.addCourse(payload).subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  // ---------------------------------------------------------
  it('updateCourse() should PUT /:id', () => {
    const payload = { title: 'Updated' };

    service.updateCourse(5, payload).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  // ---------------------------------------------------------
  it('enroll() should POST /enroll', () => {
    const payload = { userId: 'U1', courseId: 5 };

    service.enroll(payload).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/enroll`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  // ---------------------------------------------------------
  it('getModules() should GET /:id/modules', () => {
    service.getModules(7).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/7/modules`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('getModuleById() should GET /module/:id', () => {
    service.getModuleById(55).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/module/55`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  // ---------------------------------------------------------
  it('getEnrolledCourses() should GET /enrolled/:id', () => {
    service.getEnrolledCourses('U1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/enrolled/U1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('getCategories() should GET /categories', () => {
    service.getCategories().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/categories`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('getCoursesByInstructor() should GET /instructor/:id', () => {
    service.getCoursesByInstructor('U1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/instructor/U1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('deleteCourse() should DELETE /:id', () => {
    service.deleteCourse(99).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/99`);
    expect(req.request.method).toBe('DELETE');
    req.flush('OK');
  });

  // ---------------------------------------------------------
  it('publishCourse() should POST /publish/:id', () => {
    service.publishCourse(21).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/publish/21`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  // ---------------------------------------------------------
  it('getInstructorUnpublishedCourse() should GET /instructor/unpublished/:id', () => {
    service.getInstructorUnpublishedCourse('U1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/instructor/unpublished/U1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  // ---------------------------------------------------------
  it('getInstructorDraftCourse() should GET /instructor/draft', () => {
    service.getInstructorDraftCourse().subscribe();

    const req = httpMock.expectOne(`${baseUrl}/instructor/draft`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

});
