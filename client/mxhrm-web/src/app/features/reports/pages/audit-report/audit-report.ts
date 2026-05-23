import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  AuditActionSummaryResponse,
  AuditReportResponse,
  AuditTableSummaryResponse,
  AuditUserSummaryResponse
} from '../../../../core/api/api-client';
import { ReportService } from '../../services/report';

@Component({
  selector: 'app-audit-report',
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    KENDO_BUTTONS,
    KENDO_GRID
  ],
  templateUrl: './audit-report.html',
  styleUrl: './audit-report.scss'
})
export class AuditReport implements OnInit {
  report = signal<AuditReportResponse | null>(null);
  loading = signal(false);
  exporting = signal(false);
  errorMessage = signal('');

  tableName = signal('');
  action = signal('');
  userId = signal('');
  fromUtc = signal('');
  toUtc = signal('');

  byAction = computed<AuditActionSummaryResponse[]>(() =>
    this.report()?.byAction ?? []
  );

  byTable = computed<AuditTableSummaryResponse[]>(() =>
    this.report()?.byTable ?? []
  );

  byUser = computed<AuditUserSummaryResponse[]>(() =>
    this.report()?.byUser ?? []
  );

  constructor(private readonly reportService: ReportService) { }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.reportService.getAuditReport(this.buildRequest()).subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด Audit Report ได้');
        this.loading.set(false);
      }
    });
  }

  clearFilters(): void {
    this.tableName.set('');
    this.action.set('');
    this.userId.set('');
    this.fromUtc.set('');
    this.toUtc.set('');
    this.loadReport();
  }

  private buildRequest() {
    return {
      tableName: this.tableName(),
      action: this.action(),
      userId: this.userId(),
      fromUtc: this.fromUtc() || null,
      toUtc: this.toUtc() || null
    };
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.errorMessage.set('');

    this.reportService.exportAuditReportExcel(this.buildRequest()).subscribe({
      next: response => {
        const blob = response.data;

        if (!blob) {
          this.errorMessage.set('ไม่พบข้อมูลไฟล์สำหรับ export');
          this.exporting.set(false);
          return;
        }

        const fileName = response.fileName ?? 'audit-report.xlsx';
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');

        anchor.href = url;
        anchor.download = fileName;
        anchor.click();

        window.URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถ export Excel ได้');
        this.exporting.set(false);
      }
    });
  }
}