import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EmployeeSummaryReportRequest } from '../models/report';
import {
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
}
