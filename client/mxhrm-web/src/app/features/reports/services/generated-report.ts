import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
    CreateGeneratedReportRequest,
    FileResponse,
    GeneratedReportResponse,
    GeneratedReportResponsePagedResponse,
    GeneratedReportsClient
} from '../../../core/api/api-client';

@Injectable({
    providedIn: 'root'
})
export class GeneratedReportService {
    constructor(private readonly generatedReportsClient: GeneratedReportsClient) { }

    create(
        request: CreateGeneratedReportRequest
    ): Observable<GeneratedReportResponse> {
        return this.generatedReportsClient.create(request);
    }

    getAll(
        reportType?: string,
        format?: string,
        status?: string,
        page = 1,
        pageSize = 20
    ): Observable<GeneratedReportResponsePagedResponse> {
        return this.generatedReportsClient.getAll(
            reportType || undefined,
            format || undefined,
            status || undefined,
            page,
            pageSize
        );
    }

    getById(id: number): Observable<GeneratedReportResponse> {
        return this.generatedReportsClient.getById(id);
    }

    download(id: number): Observable<FileResponse> {
        return this.generatedReportsClient.download(id);
    }
}