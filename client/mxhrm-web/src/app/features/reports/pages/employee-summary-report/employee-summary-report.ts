import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_GRID } from '@progress/kendo-angular-grid';
import {
  EmployeeSummaryByCompanyResponse,
  EmployeeSummaryReportResponse
} from '../../../../core/api/api-client';
import { ReportService } from '../../services/report';

@Component({
  selector: 'app-employee-summary-report',
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
    KENDO_BUTTONS,
    KENDO_GRID
  ],
  templateUrl: './employee-summary-report.html',
  styleUrl: './employee-summary-report.scss'
})
export class EmployeeSummaryReport implements OnInit {
  report = signal<EmployeeSummaryReportResponse | null>(null);
  loading = signal(false);
  exporting = signal(false);
  errorMessage = signal('');

  companyID = signal('');
  isActive = signal<string>('');
  hireDateFrom = signal('');
  hireDateTo = signal('');

  byCompany = computed<EmployeeSummaryByCompanyResponse[]>(() =>
    this.report()?.byCompany ?? []
  );

  constructor(private readonly reportService: ReportService) { }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.reportService.getEmployeeSummary(this.buildRequest()).subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: err => {
        this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถโหลดรายงานได้');
        this.loading.set(false);
      }
    });
  }

  clearFilters(): void {
    this.companyID.set('');
    this.isActive.set('');
    this.hireDateFrom.set('');
    this.hireDateTo.set('');
    this.loadReport();
  }

  private parseIsActive(): boolean | null {
    if (this.isActive() === '') {
      return null;
    }

    return this.isActive() === 'true';
  }

  private buildRequest() {
    return {
      companyID: this.companyID(),
      isActive: this.parseIsActive(),
      hireDateFrom: this.hireDateFrom() || null,
      hireDateTo: this.hireDateTo() || null
    };
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.errorMessage.set('');

    this.reportService.exportEmployeeSummaryExcel(this.buildRequest()).subscribe({
      next: response => {
        const blob = response.data;

        if (!blob) {
          this.errorMessage.set('ไม่พบข้อมูลไฟล์สำหรับ export');
          this.exporting.set(false);
          return;
        }

        const fileName = response.fileName ?? 'employee-summary-report.xlsx';
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
