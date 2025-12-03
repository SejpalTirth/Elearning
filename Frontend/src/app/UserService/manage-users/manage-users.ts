import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserApiService } from '../services/user-api';
import { ToastService } from '../../shared/toast.service';

@Component({
  selector: 'manage-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-users.html',
  styleUrls: ['./manage-users.css']
})
export class ManageUsersComponent implements OnInit {

  users: any[] = [];
  roles: any[] = [];

  loading = false;
  updating = false;

  selectedUser: any = null;
  selectedRoleId: number | null = null;

  constructor(
    private userApi: UserApiService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers() {
    this.loading = true;

    this.userApi.getAllUsers().subscribe({
      next: (res) => {
        this.users = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toast.showError("Failed to load users");
      }
    });
  }

  loadRoles() {
    this.userApi.getAllRoles().subscribe({
      next: (res) => this.roles = res,
      error: () => this.toast.showError("Failed to load roles")
    });
  }

  openRoleModal(user: any) {
    this.selectedUser = user;
    this.selectedRoleId = null;
  }

  updateRole() {
    if (!this.selectedRoleId || !this.selectedUser) return;

    this.updating = true;

    this.userApi.updateUserRole({
      userId: this.selectedUser.id,
      roleId: this.selectedRoleId
    }).subscribe({
      next: () => {
        this.toast.showSuccess("Role updated successfully");
        this.selectedUser = null;
        this.updating = false;
        this.loadUsers();
      },
      error: () => {
        this.toast.showError("Failed to update role");
        this.updating = false;
      }
    });
  }
}
