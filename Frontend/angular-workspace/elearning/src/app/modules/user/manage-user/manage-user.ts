import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../service/auth-service';
import { AuthStateService } from '../../../service/auth-state-service';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { GatewayUsersService } from 'api';

@Component({
  selector: 'app-manage-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-user.html',
  styleUrls: ['./manage-user.css']
})
export class ManageUsersComponent implements OnInit {

  users: any[] = [];
  roles: any[] = [];
  userId: string | null = null;

  loading = false;
  updating = false;

  selectedUser: any = null;
  selectedRoleId: number | null = null;

  private readonly toast = inject(ToastService);
  private readonly authstate = inject(AuthStateService);
  private readonly auth = inject(AuthService);
  private readonly userapi = inject(GatewayUsersService);

  private authSub?: Subscription;

  ngOnInit(): void {
    this.authSub = this.authstate.user$.subscribe(user => {
      if(!user)
      {
        return;
      }
      this.userId = user.userId;

    })
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers(): void {
    this.loading = true;

    this.userapi.postApiUsersAll().subscribe({
      next: (res) => {
        this.users = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toast.showError('Failed to load users');
      }
    });
  }

  loadRoles(): void {
    this.userapi.postApiUsersRolesAll().subscribe({
      next: (res) => this.roles = res,
      error: () => this.toast.showError('Failed to load roles')
    });
  }

  openRoleModal(user: any): void {
    this.selectedUser = user;
    this.selectedRoleId = null;
  }

  updateRole(): void {
    if (!this.selectedUser || !this.selectedRoleId) {
      return;
    }

    const currentRole = this.roles.find(
      r =>
        r.name?.toLowerCase() ===
        this.selectedUser.role?.toLowerCase()
    );

    const selectedRole = this.roles.find(
      r => r.id === this.selectedRoleId
    );

    if (
      currentRole &&
      selectedRole &&
      currentRole.id === selectedRole.id
    ) {
      this.toast.showError(
        `User is already a ${selectedRole.name}`
      );
      return;
    }

    this.updating = true;

    this.userapi.postApiUsersRolesUpdate({
      userId: this.selectedUser.id,
      roleId: this.selectedRoleId
    }).subscribe({
      next: (res: any) => {
        this.toast.showSuccess(
          res?.message ?? 'Role updated successfully'
        );
        if (this.selectedUser.id === this.userId) {
          this.toast.showInfo(
            'Your role has been updated. Please log in again to continue using the Learning Management System.'
          );

          setTimeout(() => {
            this.auth.logout();
          }, 3500);

          return;
        }

        this.selectedUser = null;
        this.updating = false;
        this.loadUsers();
      },
      error: (err) => {
        const msg =
          err?.error?.message ?? 'Failed to update role';
        this.toast.showError(msg);
        this.updating = false;
      }
    });
  }
}