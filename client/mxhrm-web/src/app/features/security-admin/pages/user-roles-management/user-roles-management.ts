import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  RoleResponse,
  UserRoleResponse
} from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

@Component({
  selector: 'app-user-roles-management',
  imports: [
    CommonModule,
    RouterLink,
    KENDO_GRID,
    KENDO_BUTTONS
  ],
  templateUrl: './user-roles-management.html',
  styleUrl: './user-roles-management.scss'
})
export class UserRolesManagement implements OnInit {
  allRoles = signal<RoleResponse[]>([]);
  userRoles = signal<UserRoleResponse | null>(null);
  selectedRoleIds = signal<Set<string>>(new Set<string>());

  loading = signal(false);
  saving = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  userId = '';

  userName = computed(() => this.userRoles()?.userName ?? '');

  constructor(
    private readonly route: ActivatedRoute,
    private readonly securityAdminService: SecurityAdminService
  ) { }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('userId') ?? '';
    this.loadData();
  }

  loadData(): void {
    if (!this.userId) {
      this.errorMessage.set('ไม่พบ user id');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    forkJoin({
      roles: this.securityAdminService.getRoles(),
      userRoles: this.securityAdminService.getUserRoles(this.userId)
    }).subscribe({
      next: result => {
        this.allRoles.set(result.roles);
        this.userRoles.set(result.userRoles);
        this.selectedRoleIds.set(
          new Set(result.userRoles.roles.map(role => role.id))
        );
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลดข้อมูล user roles ได้');
        this.loading.set(false);
      }
    });
  }

  isSelected(roleId: string): boolean {
    return this.selectedRoleIds().has(roleId);
  }

  toggleRole(roleId: string, checked: boolean): void {
    const next = new Set(this.selectedRoleIds());

    if (checked) {
      next.add(roleId);
    } else {
      next.delete(roleId);
    }

    this.selectedRoleIds.set(next);
  }

  save(): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.securityAdminService.updateUserRoles(this.userId, {
      roleIds: Array.from(this.selectedRoleIds())
    }).subscribe({
      next: () => {
        this.successMessage.set('บันทึก roles ของ user สำเร็จ');
        this.saving.set(false);
        this.loadData();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถบันทึก user roles ได้');
        this.saving.set(false);
      }
    });
  }
}