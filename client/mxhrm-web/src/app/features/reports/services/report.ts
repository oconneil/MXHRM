import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
    AuditReportRequest,
    EmployeeSummaryReportRequest
} from '../models/report';
import {
    AuditReportResponse,
    EmployeeSummaryReportResponse,
    FileResponse,
    ReportsClient
} from '../../../core/api/api-client';

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    constructor(private readonly reportsClient: ReportsClient) { }

    getEmployeeSummary(
        request: EmployeeSummaryReportRequest = {}
    ): Observable<EmployeeSummaryReportResponse> {
        return this.reportsClient.getEmployeeSummary(
            request.companyID?.trim() || undefined,
            request.isActive ?? undefined,
            request.hireDateFrom || undefined,
            request.hireDateTo || undefined
        );
    }

    exportEmployeeSummaryExcel(
        request: EmployeeSummaryReportRequest = {}
    ): Observable<FileResponse> {
        return this.reportsClient.exportEmployeeSummaryExcel(
            request.companyID?.trim() || undefined,
            request.isActive ?? undefined,
            request.hireDateFrom || undefined,
            request.hireDateTo || undefined
        );
    }

    exportEmployeeSummaryPdf(
        request: EmployeeSummaryReportRequest = {}
    ): Observable<FileResponse> {
        return this.reportsClient.exportEmployeeSummaryPdf(
            request.companyID?.trim() || undefined,
            request.isActive ?? undefined,
            request.hireDateFrom || undefined,
            request.hireDateTo || undefined
        );
    }

    getAuditReport(
        request: AuditReportRequest = {}
    ): Observable<AuditReportResponse> {
        return this.reportsClient.getAuditReport(
            request.tableName?.trim() || undefined,
            request.action?.trim() || undefined,
            request.userId?.trim() || undefined,
            request.fromUtc || undefined,
            request.toUtc || undefined
        );
    }

    exportAuditReportExcel(
        request: AuditReportRequest = {}
    ): Observable<FileResponse> {
        return this.reportsClient.exportAuditReportExcel(
            request.tableName?.trim() || undefined,
            request.action?.trim() || undefined,
            request.userId?.trim() || undefined,
            request.fromUtc || undefined,
            request.toUtc || undefined
        );
    }
}
