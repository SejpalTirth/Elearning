import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ManageUsersComponent } from './manage-users';
import { UserApiService } from '../services/user-api';
import { ToastService } from '../../shared/toast.service';

// ------------------------ MOCK SERVICES ------------------------
class MockUserApiService {
  getAllUsers = jasmine.createSpy().and.returnValue(of([]));
  getAllRoles = jasmine.createSpy().and.returnValue(of([]));
  updateUserRole = jasmine.createSpy().and.returnValue(of({}));
}

class MockToastService {
  showError = jasmine.createSpy();
  showSuccess = jasmine.createSpy();
}

describe('ManageUsersComponent', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;
  let api: MockUserApiService;
  let toast: MockToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageUsersComponent],
      providers: [
        { provide: UserApiService, useClass: MockUserApiService },
        { provide: ToastService, useClass: MockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;

    api = TestBed.inject(UserApiService) as any;
    toast = TestBed.inject(ToastService) as any;

    fixture.detectChanges();
  });

  // -------------------------------------------------------------
  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------
  it('should call loadUsers and loadRoles on init', () => {
    spyOn(component, 'loadUsers');
    spyOn(component, 'loadRoles');

    component.ngOnInit();

    expect(component.loadUsers).toHaveBeenCalled();
    expect(component.loadRoles).toHaveBeenCalled();
  });

  // -------------------------------------------------------------
  it('should load users successfully', () => {
    const mockUsers = [{ id: 1, name: 'A' }];
    api.getAllUsers.and.returnValue(of(mockUsers));

    component.loadUsers();

    expect(api.getAllUsers).toHaveBeenCalled();
    expect(component.users).toEqual(mockUsers);
    expect(component.loading).toBeFalse();
  });

  // -------------------------------------------------------------
  it('should show error on failed loadUsers', () => {
    api.getAllUsers.and.returnValue(throwError(() => new Error('Fail')));

    component.loadUsers();

    expect(api.getAllUsers).toHaveBeenCalled();
    expect(toast.showError).toHaveBeenCalledWith('Failed to load users');
    expect(component.loading).toBeFalse();
  });

  // -------------------------------------------------------------
  it('should load roles successfully', () => {
    const mockRoles = [{ id: 2, name: 'Admin' }];
    api.getAllRoles.and.returnValue(of(mockRoles));

    component.loadRoles();

    expect(api.getAllRoles).toHaveBeenCalled();
    expect(component.roles).toEqual(mockRoles);
  });

  // -------------------------------------------------------------
  it('should show error on failed loadRoles', () => {
    api.getAllRoles.and.returnValue(throwError(() => new Error('Fail')));

    component.loadRoles();

    expect(api.getAllRoles).toHaveBeenCalled();
    expect(toast.showError).toHaveBeenCalledWith('Failed to load roles');
  });

  // -------------------------------------------------------------
  it('should open role modal and set selectedUser', () => {
    const user = { id: 1, name: 'Test' };

    component.openRoleModal(user);

    expect(component.selectedUser).toEqual(user);
    expect(component.selectedRoleId).toBeNull();
  });

  // -------------------------------------------------------------
  it('should NOT call updateRole if no selectedRoleId or selectedUser', () => {

    component.selectedUser = { id: 1 };
    component.selectedRoleId = null;

    component.updateRole();

    expect(api.updateUserRole).not.toHaveBeenCalled();
  });

  // -------------------------------------------------------------
  it('should update role successfully', fakeAsync(() => {
    component.selectedUser = { id: 1 };
    component.selectedRoleId = 3;

    api.updateUserRole.and.returnValue(of({}));

    spyOn(component, 'loadUsers');

    component.updateRole();
    tick();

    expect(api.updateUserRole).toHaveBeenCalledWith({ userId: 1, roleId: 3 });
    expect(toast.showSuccess).toHaveBeenCalledWith('Role updated successfully');
    expect(component.selectedUser).toBeNull();
    expect(component.updating).toBeFalse();
    expect(component.loadUsers).toHaveBeenCalled();
  }));

  // -------------------------------------------------------------
  it('should show error when updateRole fails', fakeAsync(() => {
    component.selectedUser = { id: 1 };
    component.selectedRoleId = 2;

    api.updateUserRole.and.returnValue(throwError(() => new Error('fail')));

    component.updateRole();
    tick();

    expect(toast.showError).toHaveBeenCalledWith('Failed to update role');
    expect(component.updating).toBeFalse();
  }));
});
