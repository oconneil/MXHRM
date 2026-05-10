import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { ApiError } from '../models/api-error';
import { ErrorCodes } from '../models/error-codes';
import { ValidationErrors } from '../models/validation-errors';

@Injectable({
    providedIn: 'root',
})
export class ErrorService {
    private readonly _lastError = signal<ApiError | null>(null);

    readonly lastError = this._lastError.asReadonly();

    normalize(error: unknown): ApiError {
        if (this.isApiError(error)) {
            return error;
        }

        if (error instanceof HttpErrorResponse) {
            const body = error.error;

            if (this.isApiError(body)) {
                return body;
            }

            return {
                statusCode: error.status,
                code: this.getCodeByStatus(error.status),
                message: error.message || 'Request failed.',
                details: body,
            };
        }

        return {
            statusCode: 0,
            code: ErrorCodes.InternalServerError,
            message: 'An unexpected error occurred.',
            details: error,
        };
    }

    setLastError(error: ApiError): void {
        this._lastError.set(error);
    }

    clear(): void {
        this._lastError.set(null);
    }

    getUserMessage(error: ApiError): string {
        switch (error.code) {
            case ErrorCodes.ValidationError:
                return 'Please check the form and try again.';

            case ErrorCodes.Unauthorized:
                return 'Your session has expired. Please sign in again.';

            case ErrorCodes.Forbidden:
                return 'You do not have permission to perform this action.';

            case ErrorCodes.NotFound:
                return 'The requested data was not found.';

            case ErrorCodes.Conflict:
                return 'This data was changed by someone else. Please reload and try again.';

            case ErrorCodes.BadRequest:
                return error.message || 'The request could not be processed.';

            default:
                return 'Something went wrong. Please try again.';
        }
    }

    private isApiError(value: unknown): value is ApiError {
        if (!value || typeof value !== 'object') {
            return false;
        }

        const candidate = value as Partial<ApiError>;

        return (
            typeof candidate.statusCode === 'number' &&
            typeof candidate.code === 'string' &&
            typeof candidate.message === 'string'
        );
    }

    private getCodeByStatus(status: number): string {
        switch (status) {
            case 400:
                return ErrorCodes.BadRequest;
            case 401:
                return ErrorCodes.Unauthorized;
            case 403:
                return ErrorCodes.Forbidden;
            case 404:
                return ErrorCodes.NotFound;
            case 409:
                return ErrorCodes.Conflict;
            default:
                return ErrorCodes.InternalServerError;
        }
    }
    
    getValidationErrors(error: ApiError): ValidationErrors {
        if (error.code !== ErrorCodes.ValidationError) {
            return {};
        }

        if (!error.details || typeof error.details !== 'object') {
            return {};
        }

        return error.details as ValidationErrors;
    }

    getFieldErrors(error: ApiError | null, fieldName: string): string[] {
        if (!error) {
            return [];
        }

        const errors = this.getValidationErrors(error);

        return errors[fieldName] ?? errors[this.toPascalCase(fieldName)] ?? [];
    }

    private toPascalCase(value: string): string {
        if (!value) {
            return value;
        }

        return value.charAt(0).toUpperCase() + value.slice(1);
    }

}
