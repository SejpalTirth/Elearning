import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ManageUsersComponent } from './manage-user';
import { AuthService } from '../../../service/auth-service';
import { AuthStateService } from '../../../service/auth-state-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { GatewayUsersService } from 'api';
import { of, BehaviorSubject, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('ManageUsersComponent', () => {
  let component: ManageUsersComponent;
  let fixture: ComponentFixture<ManageUsersComponent>;
  let mockToast: any;
  let mockAuthState: any;
  let mockAuth: any;
  let mockUserApi: any;

  const userSubject = new BehaviorSubject<any>(null);
  const mockUsers = [
    { id: 'u1', name: 'User One', role: 'Student' },
    { id: 'u2', name: 'Admin User', role: 'Admin' }
  ];
  const mockRoles = [
    { id: 1, name: 'Student' },
    { id: 2, name: 'Instructor' },
    { id: 3, name: 'Admin' }
  ];

  beforeEach(async () => {
    mockToast = {
      showError: jasmine.createSpy('showError'),
      showSuccess: jasmine.createSpy('showSuccess'),
      showInfo: jasmine.createSpy('showInfo')
    };
    mockAuthState = { user$: userSubject.asObservable() };
    mockAuth = { logout: jasmine.createSpy('logout') };
    mockUserApi = {
      postApiUsersAll: jasmine.createSpy('postApiUsersAll').and.returnValue(of(mockUsers)),
      postApiUsersRolesAll: jasmine.createSpy('postApiUsersRolesAll').and.returnValue(of(mockRoles)),
      postApiUsersRolesUpdate: jasmine.createSpy('postApiUsersRolesUpdate').and.returnValue(of({ message: 'Success' }))
    };

    await TestBed.configureTestingModule({
      imports: [ManageUsersComponent, FormsModule],
      providers: [
        { provide: ToastService, useValue: mockToast },
        { provide: AuthStateService, useValue: mockAuthState },
        { provide: AuthService, useValue: mockAuth },
        { provide: GatewayUsersService, useValue: mockUserApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManageUsersComponent);
    component = fixture.componentInstance;
  });

  it('should initialize with users and roles', () => {
    userSubject.next({ userId: 'u2' });
    fixture.detectChanges();

    expect(component.userId).toBe('u2');
    expect(mockUserApi.postApiUsersAll).toHaveBeenCalled();
    expect(mockUserApi.postApiUsersRolesAll).toHaveBeenCalled();
    expect(component.users.length).toBe(2);
    expect(component.roles.length).toBe(3);
  });

  it('should open role modal and set selected user', () => {
    const user = mockUsers[0];
    component.openRoleModal(user);
    expect(component.selectedUser).toEqual(user);
    expect(component.selectedRoleId).toBeNull();
  });

  describe('updateRole', () => {
    beforeEach(() => {
      userSubject.next({ userId: 'u2' });
      fixture.detectChanges();
      component.roles = mockRoles;
    });

    it('should error if user is already in the selected role', () => {
      component.selectedUser = { id: 'u1', role: 'Student' };
      component.selectedRoleId = 1; // 1 is Student

      component.updateRole();

      expect(mockToast.showError).toHaveBeenCalledWith('User is already a Student');
      expect(mockUserApi.postApiUsersRolesUpdate).not.toHaveBeenCalled();
    });

    it('should update role successfully for another user', () => {
      component.selectedUser = { id: 'u1', role: 'Student' };
      component.selectedRoleId = 2; // Instructor

      component.updateRole();

      expect(mockUserApi.postApiUsersRolesUpdate).toHaveBeenCalledWith({ userId: 'u1', roleId: 2 });
      expect(mockToast.showSuccess).toHaveBeenCalled();
      expect(mockUserApi.postApiUsersAll).toHaveBeenCalledTimes(2); // Initial + reload
    });

    it('should logout and show info if updating own role', fakeAsync(() => {
      component.selectedUser = { id: 'u2', role: 'Admin' }; // current logged in user
      component.selectedRoleId = 1;

      component.updateRole();

      expect(mockToast.showInfo).toHaveBeenCalledWith(jasmine.stringMatching(/Please log in again/));
      
      tick(3500);
      expect(mockAuth.logout).toHaveBeenCalled();
    }));

    it('should handle update errors gracefully', () => {
      mockUserApi.postApiUsersRolesUpdate.and.returnValue(throwError(() => ({ error: { message: 'API Error' } })));
      component.selectedUser = { id: 'u1', role: 'Student' };
      component.selectedRoleId = 2;

      component.updateRole();

      expect(mockToast.showError).toHaveBeenCalledWith('API Error');
      expect(component.updating).toBeFalse();
    });
  });
});