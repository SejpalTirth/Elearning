import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { UserApiService } from './user-api';

describe('UserApiService', () => {
  let service: UserApiService;
  let httpMock: HttpTestingController;

  const baseUrl = 'https://localhost:7130/api';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserApiService]
    });

    service = TestBed.inject(UserApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Ensures no open HTTP requests
  });

  // ------------------------------------------------------------
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ------------------------------------------------------------
  it('should get all users', () => {
    const mockUsers = [{ id: 1, name: 'User1' }];

    service.getAllUsers().subscribe((users) => {
      expect(users).toEqual(mockUsers);
    });

    const req = httpMock.expectOne(`${baseUrl}/users`);
    expect(req.request.method).toBe('GET');

    req.flush(mockUsers);
  });

  // ------------------------------------------------------------
  it('should get user roles by userId', () => {
    const mockRoles = ['Admin', 'User'];

    service.getUserRoles('123').subscribe((roles) => {
      expect(roles).toEqual(mockRoles);
    });

    const req = httpMock.expectOne(`${baseUrl}/roles/123`);
    expect(req.request.method).toBe('GET');

    req.flush(mockRoles);
  });

  // ------------------------------------------------------------
  it('should update user role', () => {
    const mockBody = { userId: '123', roleId: 2 };
    const mockResponse = 'Role updated';

    service.updateUserRole(mockBody).subscribe((res) => {
      expect(res).toBe(mockResponse);
    });

    const req = httpMock.expectOne(`${baseUrl}/roles/update`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(mockBody);

    req.flush(mockResponse);
  });

  // ------------------------------------------------------------
  it('should get all roles', () => {
    const mockRoles = [{ id: 1, name: 'Admin' }];

    service.getAllRoles().subscribe((roles) => {
      expect(roles).toEqual(mockRoles);
    });

    const req = httpMock.expectOne(`${baseUrl}/roles`);
    expect(req.request.method).toBe('GET');

    req.flush(mockRoles);
  });
});
