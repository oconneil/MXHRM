import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import { UserResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

@Component({
  selector: 'app-users-management',
  imports: [
    CommonModule,
    RouterLink,
    KENDO_GRID,
    KENDO_BUTTONS
  ],
  templateUrl: './users-management.html',
  styleUrl: './users-management.scss'
})
export class UsersManagement implements OnInit {
  users = signal<UserResponse[]>([]);
  loading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  constructor(private readonly securityAdminService: SecurityAdminService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.securityAdminService.getUsers().subscribe({
      next: users => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลดข้อมูล users ได้');
        this.loading.set(false);
      }
    });
  }

  activateUser(user: UserResponse): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.securityAdminService.activateUser(user.id).subscribe({
      next: () => {
        this.successMessage.set(`เปิดใช้งาน user ${user.userName} สำเร็จ`);
        this.loadUsers();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถเปิดใช้งาน user ได้');
        this.loading.set(false);
      }
    });
  }

  deactivateUser(user: UserResponse): void {
    const confirmed = confirm(`ต้องการปิดใช้งาน user ${user.userName} ใช่ไหม?`);

    if (!confirmed) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.securityAdminService.deactivateUser(user.id).subscribe({
      next: () => {
        this.successMessage.set(`ปิดใช้งาน user ${user.userName} สำเร็จ`);
        this.loadUsers();
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถปิดใช้งาน user ได้');
        this.loading.set(false);
      }
    });
  }
}