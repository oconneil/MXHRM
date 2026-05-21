import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
    EmployeeSummaryReportRequest,
    EmployeeSummaryReportResponse
} from '../models/report';

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private readonly apiUrl = `${environment.apiBaseUrl}/api/reports`;

    constructor(private readonly http: HttpClient) { }

    getEmployeeSummary(
        request: EmployeeSummaryReportRequest = {}
    ): Observable<EmployeeSummaryReportResponse> {
        let params = new HttpParams();

        if (request.companyID?.trim()) {
            params = params.set('companyID', request.companyID.trim());
        }

        if (request.isActive !== null && request.isActive !== undefined) {
            params = params.set('isActive', request.isActive);
        }

        if (request.hireDateFrom) {
            params = params.set('hireDateFrom', request.hireDateFrom);
        }

        if (request.hireDateTo) {
            params = params.set('hireDateTo', request.hireDateTo);
        }

        return this.http.get<EmployeeSummaryReportResponse>(
            `${this.apiUrl}/employee-summary`,
            { params }
        );
    }

    exportEmployeeSummaryExcel(
        request: EmployeeSummaryReportRequest = {}
    ): Observable<HttpResponse<Blob>> {
        let params = new HttpParams();

        if (request.companyID?.trim()) {
            params = params.set('companyID', request.companyID.trim());
        }

        if (request.isActive !== null && request.isActive !== undefined) {
            params = params.set('isActive', request.isActive);
        }

        if (request.hireDateFrom) {
            params = params.set('hireDateFrom', request.hireDateFrom);
        }

        if (request.hireDateTo) {
            params = params.set('hireDateTo', request.hireDateTo);
        }

        return this.http.get(`${this.apiUrl}/employee-summary/export/excel`, {
            params,
            responseType: 'blob',
            observe: 'response' // get full response to access headers for filename
        });
    }
}