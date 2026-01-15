import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { UserFacade } from './user.facade';
import { GatewayUsersService } from '@frontend/api';
import { UserVm, RoleVm } from './user.models';

describe('UserFacade', () => {
  let facade: UserFacade;
  let gatewaySpy: jasmine.SpyObj<GatewayUsersService>;

  beforeEach(() => {
    gatewaySpy = jasmine.createSpyObj('GatewayUsersService', [
      'postApiUsersAll',
      'postApiUsersById',
      'postApiUsersDelete',
      'postApiUsersRolesAll',
      'postApiUsersRolesUser',
      'postApiUsersRolesUpdate'
    ]);

    TestBed.configureTestingModule({
      providers: [
        UserFacade,
        { provide: GatewayUsersService, useValue: gatewaySpy }
      ]
    });

    facade = TestBed.inject(UserFacade);
  });

  it('should be created', () => {
    expect(facade).toBeTruthy();
  });

  // =========================
  // USERS
  // =========================

  it('should get all users', (done) => {
  const users: UserVm[] = [{ id: '1', name: 'Alice', role: 'Admin' }];

  // Return the array directly
  gatewaySpy.postApiUsersAll.and.returnValue(of(users) as any);

  facade.getAllUsers().subscribe(result => {
    expect(result).toEqual(users);
    done();
  });

  expect(gatewaySpy.postApiUsersAll)
    .toHaveBeenCalledWith(jasmine.objectContaining({ withCredentials: true }));
});


  it('should get user by id', (done) => {
  const user: UserVm = { id: '1', name: 'Alice' };

  // Return the UserVm directly, NOT HttpResponse
  gatewaySpy.postApiUsersById.and.returnValue(of(user) as any);

  facade.getUserById('1').subscribe(result => {
    expect(result).toEqual(user);
    done();
  });

  expect(gatewaySpy.postApiUsersById).toHaveBeenCalledWith(
    { userId: '1' },
    jasmine.objectContaining({ withCredentials: true })
  );
});


  it('should delete a user', (done) => {
  gatewaySpy.postApiUsersDelete.and.returnValue(of(undefined) as any);

  facade.deleteUser('1').subscribe(res => {
    expect(res).toBeUndefined();
    done();
  });

  expect(gatewaySpy.postApiUsersDelete).toHaveBeenCalledWith(
    { userId: '1' },
    jasmine.objectContaining({ withCredentials: true })
  );
});

  // =========================
  // ROLES
  // =========================

  it('should get all roles', (done) => {
  const roles: RoleVm[] = [{ id: 1, name: 'Admin' }];

  // Return the array directly, not HttpResponse
  gatewaySpy.postApiUsersRolesAll.and.returnValue(of(roles) as any);

  facade.getAllRoles().subscribe(result => {
    expect(result).toEqual(roles);
    done();
  });

  expect(gatewaySpy.postApiUsersRolesAll)
    .toHaveBeenCalledWith(jasmine.objectContaining({ withCredentials: true }));
});

  it('should get roles of a user', (done) => {
  const roles: RoleVm[] = [{ id: 2, name: 'User' }];

  // Return the array directly, not HttpResponse
  gatewaySpy.postApiUsersRolesUser.and.returnValue(of(roles) as any);

  facade.getUserRoles('1').subscribe(result => {
    expect(result).toEqual(roles);
    done();
  });

  expect(gatewaySpy.postApiUsersRolesUser)
    .toHaveBeenCalledWith(
      { userId: '1' },
      jasmine.objectContaining({ withCredentials: true })
    );
});

  it('should update user role', (done) => {
  const payload = { userId: '1', roleId: 2 };
  const response = { message: 'Updated successfully' };

  // Return the object directly
  gatewaySpy.postApiUsersRolesUpdate.and.returnValue(of(response) as any);

  facade.updateUserRole(payload).subscribe(result => {
    expect(result).toEqual(response);
    done();
  });

  expect(gatewaySpy.postApiUsersRolesUpdate).toHaveBeenCalledWith(
    payload,
    jasmine.objectContaining({ withCredentials: true })
  );
});
});
