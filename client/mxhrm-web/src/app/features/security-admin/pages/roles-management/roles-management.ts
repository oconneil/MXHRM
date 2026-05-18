import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import { RoleResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

@Component({
  selector: 'app-roles-management',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    KENDO_GRID,
    KENDO_BUTTONS
  ],
  templateUrl: './roles-management.html',
  styleUrl: './roles-management.scss'
})
export class RolesManagement implements OnInit {
  roles = signal<RoleResponse[]>([]);
  loading = signal(false);
  errorMessage = signal('');
  newRoleName = signal('');

  editingRoleId = signal<string | null>(null);
  editingRoleName = signal('');

  constructor(private readonly securityAdminService: SecurityAdminService) { }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.getRoles().subscribe({
      next: roles => {
        this.roles.set(roles);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('ไม่สามารถโหลดข้อมูล roles ได้');
        this.loading.set(false);
      }
    });
  }

  createRole(): void {
    const name = this.newRoleName().trim();

    if (!name) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.createRole({ name }).subscribe({
      next: () => {
        this.newRoleName.set('');
        this.loadRoles();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถสร้าง role ได้');
        this.loading.set(false);
      }
    });
  }

  startEdit(role: RoleResponse): void {
    this.editingRoleId.set(role.id);
    this.editingRoleName.set(role.name);
  }

  cancelEdit(): void {
    this.editingRoleId.set(null);
    this.editingRoleName.set('');
  }

  saveRole(role: RoleResponse): void {
    const name = this.editingRoleName().trim();

    if (!name) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.updateRole(role.id, { name }).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadRoles();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถแก้ไข role ได้');
        this.loading.set(false);
      }
    });
  }

  deleteRole(role: RoleResponse): void {
    const confirmed = confirm(`ต้องการลบ role ${role.name} ใช่ไหม?`);

    if (!confirmed) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.deleteRole(role.id).subscribe({
      next: () => {
        this.loadRoles();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถลบ role ได้');
        this.loading.set(false);
      }
    });
  }

  isProtectedRole(roleName: string): boolean {
    return ['Admin', 'HR', 'Employee'].includes(roleName);
  }
}