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

// ------------------------ Helper Functions ------------------------
function setupSelectedUser(component: ManageUsersComponent, userId: number, roleId: number | null = null): void {
  component.selectedUser = { id: userId };
  component.selectedRoleId = roleId;
}

function mockApiCall(api: MockUserApiService, method: keyof MockUserApiService, returnValue: any): void {
  (api[method] as jasmine.Spy).and.returnValue(of(returnValue));
}

function mockApiError(api: MockUserApiService, method: keyof MockUserApiService): void {
  (api[method] as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
}

// ------------------------ Initialization Tests ------------------------
describe('ManageUsersComponent - Initialization', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [ManageUsersComponent],
      providers: [
        { provide: UserApiService, useClass: MockUserApiService },
        { provide: ToastService, useClass: MockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create component', (): void => {
    expect(component).toBeTruthy();
  });

  it('should call loadUsers and loadRoles on init', (): void => {
    spyOn(component, 'loadUsers');
    spyOn(component, 'loadRoles');

    component.ngOnInit();

    expect(component.loadUsers).toHaveBeenCalled();
    expect(component.loadRoles).toHaveBeenCalled();
  });
});

// ------------------------ Users Tests ------------------------
describe('ManageUsersComponent - Users', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;
  let api: MockUserApiService;
  let toast: MockToastService;

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(UserApiService) as any;
    toast = TestBed.inject(ToastService) as any;
  });

  it('should load users successfully', (): void => {
    const mockUsers = [{ id: 1, name: 'A' }];
    mockApiCall(api, 'getAllUsers', mockUsers);

    component.loadUsers();

    expect(api.getAllUsers).toHaveBeenCalled();
    expect(component.users).toEqual(mockUsers);
    expect(component.loading).toBeFalse();
  });

  it('should show error on failed loadUsers', (): void => {
    mockApiError(api, 'getAllUsers');

    component.loadUsers();

    expect(api.getAllUsers).toHaveBeenCalled();
    expect(toast.showError).toHaveBeenCalledWith('Failed to load users');
    expect(component.loading).toBeFalse();
  });
});

// ------------------------ Roles Tests ------------------------
describe('ManageUsersComponent - Roles', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;
  let api: MockUserApiService;
  let toast: MockToastService;

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(UserApiService) as any;
    toast = TestBed.inject(ToastService) as any;
  });

  it('should load roles successfully', (): void => {
    const mockRoles = [{ id: 2, name: 'Admin' }];
    mockApiCall(api, 'getAllRoles', mockRoles);

    component.loadRoles();

    expect(api.getAllRoles).toHaveBeenCalled();
    expect(component.roles).toEqual(mockRoles);
  });

  it('should show error on failed loadRoles', (): void => {
    mockApiError(api, 'getAllRoles');

    component.loadRoles();

    expect(api.getAllRoles).toHaveBeenCalled();
    expect(toast.showError).toHaveBeenCalledWith('Failed to load roles');
  });
});

// ------------------------ Role Management Tests ------------------------
describe('ManageUsersComponent - Role Management', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;
  let api: MockUserApiService;
  let toast: MockToastService;

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(UserApiService) as any;
    toast = TestBed.inject(ToastService) as any;
  });

  it('should open role modal and set selectedUser', (): void => {
    const user = { id: 1, name: 'Test' };
    component.openRoleModal(user);

    expect(component.selectedUser).toEqual(user);
    expect(component.selectedRoleId).toBeNull();
  });

  it('should NOT call updateRole if no selectedRoleId or selectedUser', (): void => {
    setupSelectedUser(component, 1, null);

    component.updateRole();

    expect(api.updateUserRole).not.toHaveBeenCalled();
  });

  it('should update role successfully', fakeAsync((): void => {
    setupSelectedUser(component, 1, 3);
    mockApiCall(api, 'updateUserRole', {});
    spyOn(component, 'loadUsers');

    component.updateRole();
    tick();

    expect(api.updateUserRole).toHaveBeenCalledWith({ userId: 1, roleId: 3 });
    expect(toast.showSuccess).toHaveBeenCalledWith('Role updated successfully');
    expect(component.selectedUser).toBeNull();
    expect(component.updating).toBeFalse();
    expect(component.loadUsers).toHaveBeenCalled();
  }));

  it('should show error when updateRole fails', fakeAsync((): void => {
    setupSelectedUser(component, 1, 2);
    mockApiError(api, 'updateUserRole');

    component.updateRole();
    tick();

    expect(toast.showError).toHaveBeenCalledWith('Failed to update role');
    expect(component.updating).toBeFalse();
  }));
});
