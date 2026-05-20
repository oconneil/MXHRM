import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { State } from '@progress/kendo-data-query';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { UserActivityLogResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';
import { KENDO_DIALOG } from '@progress/kendo-angular-dialog';
import {
  DataStateChangeEvent,
  GridDataResult,
  KENDO_GRID
} from '@progress/kendo-angular-grid';

@Component({
  selector: 'app-user-activity-logs',
  imports: [
    CommonModule,
    DatePipe,
    KENDO_BUTTONS,
    KENDO_GRID,
    KENDO_DIALOG
  ],
  templateUrl: './user-activity-logs.html',
  styleUrl: './user-activity-logs.scss'
})
export class UserActivityLogs implements OnInit {
  userActivityLogs = signal<UserActivityLogResponse[]>([]);
  selectedActivityLog = signal<UserActivityLogResponse | null>(null);
  totalItems = signal(0);
  loading = signal(false);
  errorMessage = signal('');
  gridData = computed<GridDataResult>(() => ({
    data: this.userActivityLogs(),
    total: this.totalItems()
  }));

  gridState = signal<State>({
    skip: 0,
    take: 20,
    sort: [
      {
        field: 'createdAtUtc',
        dir: 'desc'
      }
    ]
  });

  constructor(private readonly securityAdminService: SecurityAdminService) { }

  ngOnInit(): void {
    this.loadUserActivityLogs();
  }

  loadUserActivityLogs(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.getUserActivityLogsGrid(this.gridState()).subscribe({
      next: (response: GridDataResult) => {
        this.userActivityLogs.set(response.data as UserActivityLogResponse[]);
        this.totalItems.set(response.total);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด user activity logs ได้');
        this.loading.set(false);
      }
    });
  }

  onGridStateChange(state: DataStateChangeEvent): void {
    this.gridState.update(current => ({
      ...current,
      ...state,
      skip: state.skip ?? 0,
      take: state.take ?? current.take ?? 20
    }));

    this.loadUserActivityLogs();
  }

  clearGridState(): void {
    this.gridState.set({
      skip: 0,
      take: 20,
      sort: [
        {
          field: 'createdAtUtc',
          dir: 'desc'
        }
      ]
    });

    this.loadUserActivityLogs();
  }

  openDetails(activityLog: UserActivityLogResponse): void {
    this.selectedActivityLog.set(activityLog);
  }

  closeDetails(): void {
    this.selectedActivityLog.set(null);
  }

  formatJson(value: string | null): string {
    if (!value) {
      return '-';
    }

    try {
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      return value;
    }
  }

  getActivityBadgeClass(activityType: string): string {
    const normalizedActivityType = activityType.toLowerCase();

    if (normalizedActivityType.includes('success')) {
      return 'bg-success';
    }

    if (normalizedActivityType.includes('failed') || normalizedActivityType.includes('error')) {
      return 'bg-danger';
    }

    if (normalizedActivityType.includes('refresh')) {
      return 'bg-info text-dark';
    }

    if (normalizedActivityType.includes('logout')) {
      return 'bg-secondary';
    }

    return 'bg-primary';
  }
}