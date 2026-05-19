import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import { AuditLogResponse } from '../../models/security-admin';
import { SecurityAdminService } from '../../services/security-admin';

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
  loading = signal(false);
  errorMessage = signal('');

  constructor(private readonly securityAdminService: SecurityAdminService) { }

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.securityAdminService.getAuditLogs().subscribe({
      next: response => {
        this.auditLogs.set(response.items);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด audit logs ได้');
        this.loading.set(false);
      }
    });
  }
}