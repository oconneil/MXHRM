import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  PermissionResponse,
  RolePermissionResponse
} from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

@Component({
  selector: 'app-role-permissions-management',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    KENDO_GRID,
    KENDO_BUTTONS
  ],
  templateUrl: './role-permissions-management.html',
  styleUrl: './role-permissions-management.scss'
})
export class RolePermissionsManagement implements OnInit {
  allPermissions = signal<PermissionResponse[]>([]);
  rolePermissions = signal<RolePermissionResponse | null>(null);
  selectedPermissionIds = signal<Set<number>>(new Set<number>());

  loading = signal(false);
  saving = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  roleId = '';

  roleName = computed(() => this.rolePermissions()?.roleName ?? '');

  constructor(
    private readonly route: ActivatedRoute,
    private readonly securityAdminService: SecurityAdminService
  ) { }

  ngOnInit(): void {
    this.roleId = this.route.snapshot.paramMap.get('roleId') ?? '';
    this.loadData();
  }

  loadData(): void {
    if (!this.roleId) {
      this.errorMessage.set('ไม่พบ role id');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    forkJoin({
      permissions: this.securityAdminService.getPermissions(),
      rolePermissions: this.securityAdminService.getRolePermissions(this.roleId)
    }).subscribe({
      next: result => {
        this.allPermissions.set(result.permissions);
        this.rolePermissions.set(result.rolePermissions);
        this.selectedPermissionIds.set(
          new Set(result.rolePermissions.permissions.map(permission => permission.id))
        );
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลดข้อมูล permission ได้');
        this.loading.set(false);
      }
    });
  }

  isSelected(permissionId: number): boolean {
    return this.selectedPermissionIds().has(permissionId);
  }

  togglePermission(permissionId: number, checked: boolean): void {
    const next = new Set(this.selectedPermissionIds());

    if (checked) {
      next.add(permissionId);
    } else {
      next.delete(permissionId);
    }

    this.selectedPermissionIds.set(next);
  }

  save(): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.securityAdminService.updateRolePermissions(this.roleId, {
      permissionIds: Array.from(this.selectedPermissionIds())
    }).subscribe({
      next: () => {
        this.successMessage.set('บันทึก permission ของ role สำเร็จ');
        this.saving.set(false);
        this.loadData();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถบันทึก permission ได้');
        this.saving.set(false);
      }
    });
  }
}