import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  CreateGeneratedReportRequest,
  GeneratedReportResponse
} from '../../../../core/api/api-client';
import { GeneratedReportService } from '../../services/generated-report';

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

  constructor(private readonly generatedReportService: GeneratedReportService) { }

  ngOnInit(): void {
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
}