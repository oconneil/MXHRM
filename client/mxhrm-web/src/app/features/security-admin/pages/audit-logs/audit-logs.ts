import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { State } from '@progress/kendo-data-query';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { AuditLogResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';
import {
  DataStateChangeEvent,
  GridDataResult,
  KENDO_GRID
} from '@progress/kendo-angular-grid';

@Component({
  selector: 'app-audit-logs',
  imports: [
    CommonModule,
    DatePipe,
    KENDO_BUTTONS,
    KENDO_GRID
  ],
  templateUrl: './audit-logs.html',
  styleUrl: './audit-logs.scss'
})
export class AuditLogs implements OnInit {
  auditLogs = signal<AuditLogResponse[]>([]);
  totalItems = signal(0);
  loading = signal(false);
  errorMessage = signal('');
  gridData = computed<GridDataResult>(() => ({
    data: this.auditLogs(),
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
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.getAuditLogsGrid(this.gridState()).subscribe({
      next: (response: GridDataResult) => {
        this.auditLogs.set(response.data as AuditLogResponse[]);
        this.totalItems.set(response.total);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด audit logs ได้');
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

    this.loadAuditLogs();
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

    this.loadAuditLogs();
  }
}