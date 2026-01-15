import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ManageUsersComponent } from './manage-users';
import { UserFacade } from '@frontend/core';
import { ToastService } from '@frontend/ui';
import { AuthStateService, AuthService, AuthUser } from '@frontend/auth';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';

describe('ManageUsersComponent', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;

  // Spies
  let userFacadeSpy: jasmine.SpyObj<UserFacade>;
  let toastSpy: jasmine.SpyObj<ToastService>;
  let authStateSpy: jasmine.SpyObj<AuthStateService>;
  let authSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
  userFacadeSpy = jasmine.createSpyObj('UserFacade', [
    'getAllUsers',
    'getAllRoles',
    'updateUserRole'
  ]);

  userFacadeSpy.getAllUsers.and.returnValue(of([]));
  userFacadeSpy.getAllRoles.and.returnValue(of([]));

  toastSpy = jasmine.createSpyObj('ToastService', [
    'showError',
    'showSuccess',
    'showInfo'
  ]);

  const mockAuthUser: AuthUser = {
  userId: '123',
  email: 'test@example.com',
  name: 'Test User',
  role: 'Admin'
};

  authStateSpy = jasmine.createSpyObj<AuthStateService>(
  'AuthStateService',
  [],
  {
    user$: of(mockAuthUser)
  }
);


  authSpy = jasmine.createSpyObj('AuthService', ['logout']);

  await TestBed.configureTestingModule({
    imports: [ManageUsersComponent],
    providers: [
      { provide: UserFacade, useValue: userFacadeSpy },
      { provide: ToastService, useValue: toastSpy },
      { provide: AuthStateService, useValue: authStateSpy },
      { provide: AuthService, useValue: authSpy },
      ...provideCommonMocks
    ]
  }).compileComponents();

  fixture = TestBed.createComponent(ManageUsersComponent);
  component = fixture.componentInstance;
  fixture.detectChanges();
});


  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    expect(userFacadeSpy.getAllUsers).toHaveBeenCalled();
  });

  it('should load roles on init', () => {
    expect(userFacadeSpy.getAllRoles).toHaveBeenCalled();
  });

  it('should set users when loadUsers succeeds', () => {
    const mockUsers = [{ id: '1', name: 'Test User' }];
    userFacadeSpy.getAllUsers.and.returnValue(of(mockUsers));

    component.loadUsers();

    expect(component.users).toEqual(mockUsers);
    expect(component.loading).toBeFalse();
  });

  it('should show error when loadUsers fails', () => {
    userFacadeSpy.getAllUsers.and.returnValue(throwError(() => ({})));

    component.loadUsers();

    expect(toastSpy.showError).toHaveBeenCalledWith('Failed to load users');
    expect(component.loading).toBeFalse();
  });

  it('should open role modal and set selected user', () => {
    const user = { id: 1, role: 'Admin' };

    component.openRoleModal(user);

    expect(component.selectedUser).toBe(user);
    expect(component.selectedRoleId).toBeNull();
  });

  it('should not update role if selectedUser or selectedRoleId is missing', () => {
    component.updateRole();

    expect(userFacadeSpy.updateUserRole).not.toHaveBeenCalled();
  });

  it('should show error if user already has selected role', () => {
    component.roles = [
      { id: 1, name: 'Admin' }
    ];

    component.selectedUser = { id: 1, role: 'Admin' };
    component.selectedRoleId = 1;

    component.updateRole();

    expect(toastSpy.showError).toHaveBeenCalled();
  });

  it('should update role successfully for another user', () => {
    component.roles = [
      { id: 1, name: 'Admin' },
      { id: 2, name: 'User' }
    ];

    component.selectedUser = { id: 2, role: 'User' };
    component.selectedRoleId = 1;

    userFacadeSpy.updateUserRole.and.returnValue(
      of({ message: 'Updated successfully' })
    );

    component.updateRole();

    expect(userFacadeSpy.updateUserRole).toHaveBeenCalled();
    expect(toastSpy.showSuccess).toHaveBeenCalled();
  });

  it('should logout if updated user is current user', () => {
    component.roles = [
      { id: 1, name: 'Admin' }
    ];

    component.selectedUser = { id: '123', role: 'User' };
    component.selectedRoleId = 1;

    userFacadeSpy.updateUserRole.and.returnValue(
      of({ message: 'Updated' })
    );

    component.updateRole();

    expect(toastSpy.showInfo).toHaveBeenCalled();
  });

  it('should show error if update role fails', () => {
    component.roles = [
      { id: 1, name: 'Admin' }
    ];

    component.selectedUser = { id: 2, role: 'User' };
    component.selectedRoleId = 1;

    userFacadeSpy.updateUserRole.and.returnValue(
      throwError(() => ({
        error: { message: 'Update failed' }
      }))
    );

    component.updateRole();

    expect(toastSpy.showError).toHaveBeenCalledWith('Update failed');
    expect(component.updating).toBeFalse();
  });
});
