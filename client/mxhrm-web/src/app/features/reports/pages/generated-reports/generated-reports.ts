import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, effect, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  CreateGeneratedReportRequest,
  GeneratedReportResponse
} from '../../../../core/api/api-client';
import { GeneratedReportService } from '../../services/generated-report';
import { RealtimeService } from '../../../../core/realtime/realtime';

@Component({
  selector: 'app-generated-reports',
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    KENDO_BUTTONS,
    KENDO_GRID
  ],
  templateUrl: './generated-reports.html',
  styleUrl: './generated-reports.scss'
})
export class GeneratedReports implements OnInit {
  reports = signal<GeneratedReportResponse[]>([]);
  totalItems = signal(0);
  loading = signal(false);
  creating = signal(false);
  errorMessage = signal('');
  realtimeNotice = signal('');

  reportType = signal('');
  format = signal('');
  status = signal('');
  page = signal(1);
  pageSize = signal(20);

  createReportType = signal('EmployeeSummary');
  createFormat = signal('Pdf');

  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalItems() / this.pageSize()))
  );

  realtimeConnected = computed(() => this.realtimeService.connected());

  constructor(
    private readonly generatedReportService: GeneratedReportService,
    private readonly realtimeService: RealtimeService
  ) {
    effect(() => {
      const message = this.realtimeService.latestMessage();

      if (message?.type !== 'generated-report.updated' || !message.data) {
        return;
      }

      const updatedReport = message.data as GeneratedReportResponse;

      this.realtimeNotice.set(message.message ?? this.buildRealtimeNotice(updatedReport));
      this.upsertRealtimeReport(updatedReport);
    });
  }

  ngOnInit(): void {
    this.realtimeService.start();
    this.loadReports();
  }

  loadReports(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.generatedReportService.getAll(
      this.reportType() || undefined,
      this.format() || undefined,
      this.status() || undefined,
      this.page(),
      this.pageSize()
    ).subscribe({
      next: response => {
        this.reports.set(response.items ?? []);
        this.totalItems.set(response.totalItems ?? 0);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลด generated reports ได้');
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.page.set(1);
    this.loadReports();
  }

  clearFilters(): void {
    this.reportType.set('');
    this.format.set('');
    this.status.set('');
    this.page.set(1);
    this.loadReports();
  }

  createReport(): void {
    this.creating.set(true);
    this.errorMessage.set('');

    const request: CreateGeneratedReportRequest = {
      reportType: this.createReportType(),
      format: this.createFormat()
    };

    if (this.createReportType() === 'EmployeeSummary') {
      request.employeeSummaryRequest = {};
    }

    if (this.createReportType() === 'AuditReport') {
      request.auditReportRequest = {};
    }

    this.generatedReportService.create(request).subscribe({
      next: () => {
        this.creating.set(false);
        this.page.set(1);
        this.loadReports();
      },
      error: err => {
        console.error('Error creating report job:', err);
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถสร้าง report job ได้');
        this.creating.set(false);
      }
    });
  }

  download(report: GeneratedReportResponse): void {
    if (!report.id || report.status !== 'Completed') {
      return;
    }

    this.generatedReportService.download(report.id).subscribe({
      next: response => {
        const blob = response.data;

        if (!blob) {
          this.errorMessage.set('ไม่พบไฟล์สำหรับ download');
          return;
        }

        const fileName = response.fileName ?? report.fileName ?? 'generated-report';
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');

        anchor.href = url;
        anchor.download = fileName;
        anchor.click();

        window.URL.revokeObjectURL(url);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถ download report ได้');
      }
    });
  }

  nextPage(): void {
    if (this.page() >= this.totalPages()) {
      return;
    }

    this.page.update(value => value + 1);
    this.loadReports();
  }

  previousPage(): void {
    if (this.page() <= 1) {
      return;
    }

    this.page.update(value => value - 1);
    this.loadReports();
  }

  canDownload(report: GeneratedReportResponse): boolean {
    return !!report.id && report.status === 'Completed';
  }

  getDownloadButtonText(report: GeneratedReportResponse): string {
    if (report.status === 'Processing') {
      return 'Preparing...';
    }

    if (report.status === 'Failed') {
      return 'Failed';
    }

    if (report.status === 'Pending') {
      return 'Waiting...';
    }

    return 'Download';
  }

  getStatusHelpText(status?: string | null): string {
    switch (status) {
      case 'Pending':
        return 'Waiting for Hangfire worker';
      case 'Processing':
        return 'Generating file in background';
      case 'Completed':
        return 'Ready to download';
      case 'Failed':
        return 'Generation failed';
      default:
        return 'Unknown status';
    }
  }

  private upsertRealtimeReport(updatedReport: GeneratedReportResponse): void {
    if (!updatedReport.id) {
      return;
    }

    let alreadyExists = false;
    const shouldShow = this.matchesCurrentFilters(updatedReport);

    this.reports.update(reports => {
      alreadyExists = reports.some(report => report.id === updatedReport.id);

      if (!shouldShow) {
        return reports.filter(report => report.id !== updatedReport.id);
      }

      if (alreadyExists) {
        return reports.map(report =>
          report.id === updatedReport.id ? updatedReport : report
        );
      }

      if (this.page() !== 1) {
        return reports;
      }

      return [updatedReport, ...reports].slice(0, this.pageSize());
    });

    if (shouldShow && !alreadyExists && this.page() === 1) {
      this.totalItems.update(value => value + 1);
    }

    if (!shouldShow && alreadyExists) {
      this.totalItems.update(value => Math.max(0, value - 1));
    }
  }

  private matchesCurrentFilters(report: GeneratedReportResponse): boolean {
    const reportType = this.reportType();
    const format = this.format();
    const status = this.status();

    return (!reportType || report.reportType === reportType) &&
      (!format || report.format === format) &&
      (!status || report.status === status);
  }

  private buildRealtimeNotice(report: GeneratedReportResponse): string {
    return `${report.reportType ?? 'Report'} ${report.format ?? ''} is now ${report.status ?? 'updated'}.`;
  }
}
