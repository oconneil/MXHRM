import { Injectable, computed, signal } from '@angular/core';
import { GeneratedReportResponse } from '../api/api-client';
import { RealtimeMessage } from '../realtime/realtime-message';
import { NotificationItem, NotificationTone } from './notification';

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private readonly _notifications = signal<NotificationItem[]>([]);
    private readonly _latestNotification = signal<NotificationItem | null>(null);

    readonly notifications = this._notifications.asReadonly();
    readonly latestNotification = this._latestNotification.asReadonly();

    readonly unreadCount = computed(() =>
        this.notifications().filter(notification => !notification.isRead).length
    );

    addFromRealtimeMessage(message: RealtimeMessage): void {
        const notification = this.mapRealtimeMessage(message);

        if (!notification) {
            return;
        }

        let notificationToDisplay = notification;

        this._notifications.update(notifications => {
            if (!notification.key) {
                return [notification, ...notifications].slice(0, 50);
            }

            const existingNotification = notifications.find(
                item => item.key === notification.key
            );

            if (!existingNotification) {
                return [notification, ...notifications].slice(0, 50);
            }

            notificationToDisplay = {
                ...notification,
                id: existingNotification.id,
                isRead: false
            };

            return [
                notificationToDisplay,
                ...notifications.filter(item => item.id !== existingNotification.id)
            ].slice(0, 50);
        });

        this._latestNotification.set(notificationToDisplay);
    }

    markAsRead(id: string): void {
        this._notifications.update(notifications =>
            notifications.map(notification =>
                notification.id === id
                    ? { ...notification, isRead: true }
                    : notification
            )
        );
    }

    markAllAsRead(): void {
        this._notifications.update(notifications =>
            notifications.map(notification => ({
                ...notification,
                isRead: true
            }))
        );
    }

    clear(): void {
        this._notifications.set([]);
        this._latestNotification.set(null);
    }

    private mapRealtimeMessage(message: RealtimeMessage): NotificationItem | null {
        switch (message.type) {
            case 'generated-report.updated':
                return this.mapGeneratedReportNotification(message);

            case 'chat.message.created':
                return {
                    id: crypto.randomUUID(),
                    type: message.type,
                    title: message.title ?? 'New Message',
                    message: message.message ?? 'You received a new message.',
                    data: message.data,
                    route: '/messages',
                    createdAtUtc: message.createdAtUtc,
                    isRead: false,
                    tone: 'info'
                };

            case 'system.notification.created':
                return {
                    id: crypto.randomUUID(),
                    type: message.type,
                    title: message.title ?? 'System Notification',
                    message: message.message ?? '',
                    data: message.data,
                    createdAtUtc: message.createdAtUtc,
                    isRead: false,
                    tone: 'warning'
                };

            default:
                return null;
        }
    }

    private mapGeneratedReportNotification(
        message: RealtimeMessage
    ): NotificationItem<GeneratedReportResponse> | null {
        const report = message.data as GeneratedReportResponse | null;

        if (!report?.id) {
            return null;
        }

        return {
            id: crypto.randomUUID(),
            key: `generated-report:${report.id}`,
            type: message.type,
            title: this.getGeneratedReportTitle(report.status),
            message: this.getGeneratedReportMessage(report),
            data: report,
            route: '/reports/generated',
            createdAtUtc: message.createdAtUtc,
            isRead: false,
            tone: this.getGeneratedReportTone(report.status)
        };
    }

    private getGeneratedReportTitle(status?: string): string {
        switch (status) {
            case 'Processing':
                return 'Report is being generated';

            case 'Completed':
                return 'Report is ready';

            case 'Failed':
                return 'Report generation failed';

            default:
                return 'Report status updated';
        }
    }

    private getGeneratedReportMessage(report: GeneratedReportResponse): string {
        const reportName = `${report.reportType ?? 'Report'} ${report.format ?? ''}`.trim();

        switch (report.status) {
            case 'Processing':
                return `${reportName} is processing in the background.`;

            case 'Completed':
                return `${reportName} is ready to download.`;

            case 'Failed':
                return report.errorMessage
                    ? `${reportName} failed: ${report.errorMessage}`
                    : `${reportName} could not be generated.`;

            default:
                return `${reportName} status has changed.`;
        }
    }

    private getGeneratedReportTone(status?: string): NotificationTone {
        switch (status) {
            case 'Processing':
                return 'info';

            case 'Completed':
                return 'success';

            case 'Failed':
                return 'danger';

            default:
                return 'warning';
        }
    }
}