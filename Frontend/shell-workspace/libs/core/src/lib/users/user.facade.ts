import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import {
  GatewayUsersService,
  GatewayContractsUsersUpdateUserRoleRequest,
  GatewayContractsUsersCompleteProfileRequest
} from '@frontend/api';

import { UserVm, RoleVm } from './user.models';

@Injectable({ providedIn: 'root' })
export class UserFacade {

  private readonly api = inject(GatewayUsersService);

  // =========================
  // USERS
  // =========================

  getAllUsers(): Observable<UserVm[]> {
    return this.api
      .postApiUsersAll({ withCredentials: true })
      .pipe(map(res => res as unknown as UserVm[]));
  }

  getUserById(userId: string): Observable<UserVm> {
    return this.api
      .postApiUsersById({ userId }, { withCredentials: true })
      .pipe(map(res => res as unknown as UserVm));
  }

  deleteUser(userId: string): Observable<void> {
    return this.api.postApiUsersDelete(
      { userId },
      { withCredentials: true }
    );
  }

  // =========================
  // ROLES
  // =========================

  getAllRoles(): Observable<RoleVm[]> {
    return this.api
      .postApiUsersRolesAll({ withCredentials: true })
      .pipe(map(res => res as unknown as RoleVm[]));
  }

  getUserRoles(userId: string): Observable<RoleVm[]> {
    return this.api
      .postApiUsersRolesUser({ userId }, { withCredentials: true })
      .pipe(map(res => res as unknown as RoleVm[]));
  }

  updateUserRole(
    payload: GatewayContractsUsersUpdateUserRoleRequest
  ): Observable<{ message?: string }> {
    return this.api
      .postApiUsersRolesUpdate(payload, { withCredentials: true })
      .pipe(map(res => res as unknown as { message?: string }));
  }

    // =========================
  // COMPLETE PROFILE
  // =========================

  completeProfile(
    payload: GatewayContractsUsersCompleteProfileRequest
  ): Observable<void> {
    return this.api.postApiUsersCompleteProfile(
      payload
    );
  }
}
