import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import { UserActivityLogResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

@Component({
  selector: 'app-user-activity-logs',
  imports: [
    CommonModule,
    DatePipe,
    KENDO_BUTTONS,
    KENDO_GRID
  ],
  templateUrl: './user-activity-logs.html',
  styleUrl: './user-activity-logs.scss'
})
export class UserActivityLogs implements OnInit {
  userActivityLogs = signal<UserActivityLogResponse[]>([]);
  loading = signal(false);
  errorMessage = signal('');

  constructor(private readonly securityAdminService: SecurityAdminService) { }

  ngOnInit(): void {
    this.loadUserActivityLogs();
  }

  loadUserActivityLogs(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.getUserActivityLogs().subscribe({
      next: response => {
        this.userActivityLogs.set(response.items);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด user activity logs ได้');
        this.loading.set(false);
      }
    });
  }
}