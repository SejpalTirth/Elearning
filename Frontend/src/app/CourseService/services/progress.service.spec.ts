import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { ProgressService } from './progress.service';

describe('ProgressService', () => {
  let service: ProgressService;
  let httpMock: HttpTestingController;

  const baseUrl = 'https://localhost:7249/api/progress';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProgressService]
    });

    service = TestBed.inject(ProgressService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  // -------------------------------------------------------
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // -------------------------------------------------------
  it('markModuleCompleted() should POST correct URL and payload', () => {
    const payload = { userId: 'U1', moduleId: 101 };

    service.markModuleCompleted(payload).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/complete-module`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    expect(req.request.responseType).toBe('text');

    req.flush('OK');
  });

  // -------------------------------------------------------
  it('getUserProgress() should GET correct URL', () => {
    service.getUserProgress('U1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/U1`);
    expect(req.request.method).toBe('GET');

    req.flush([]);
  });

});
