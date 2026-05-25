import { Injectable, inject, signal } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import {
    NotificationsClient,
    UserNotificationResponse
} from '../api/api-client';
import { RealtimeMessage } from '../realtime/realtime-message';
import { NotificationItem, NotificationTone } from './notification';

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private readonly notificationsClient = inject(NotificationsClient);

    private readonly _notifications = signal<NotificationItem[]>([]);
    private readonly _latestNotification = signal<NotificationItem | null>(null);
    private readonly _unreadCount = signal(0);
    private readonly _loading = signal(false);
    private readonly _loadError = signal<string | null>(null);

    readonly notifications = this._notifications.asReadonly();
    readonly latestNotification = this._latestNotification.asReadonly();
    readonly unreadCount = this._unreadCount.asReadonly();
    readonly loading = this._loading.asReadonly();
    readonly loadError = this._loadError.asReadonly();

    load(): void {
        if (this._loading()) {
            return;
        }

        this._loadError.set(null);
        this._loading.set(true);

        forkJoin({
            notifications: this.notificationsClient.getAll(
                undefined,
                1,
                50
            ),
            unreadCount: this.notificationsClient.getUnreadCount()
        })
            .pipe(finalize(() => this._loading.set(false)))
            .subscribe({
                next: ({ notifications, unreadCount }) => {
                    const items = (notifications.items ?? [])
                        .map(item => this.mapApiNotification(item))
                        .filter(
                            (item): item is NotificationItem =>
                                item !== null
                        );

                    this._notifications.set(items);
                    this._unreadCount.set(unreadCount.count ?? 0);
                },
                error: () => {
                    this._loadError.set(
                        'Notifications could not be loaded. Please try again.'
                    );
                }
            });
    }

    addFromRealtimeMessage(message: RealtimeMessage): void {
        const notification = this.mapRealtimeMessage(message);

        if (!notification) {
            return;
        }

        this.upsert(notification);
        this._latestNotification.set(notification);
        this.refreshUnreadCount();
    }

    markAsRead(id: number): void {
        this.notificationsClient.markAsRead(id).subscribe({
            next: () => {
                this._notifications.update(notifications =>
                    notifications.map(notification =>
                        notification.id === id
                            ? { ...notification, isRead: true }
                            : notification
                    )
                );

                this.refreshUnreadCount();
            },
            error: () => undefined
        });
    }

    markAllAsRead(): void {
        this.notificationsClient.markAllAsRead().subscribe({
            next: () => {
                this._notifications.update(notifications =>
                    notifications.map(notification => ({
                        ...notification,
                        isRead: true
                    }))
                );

                this._unreadCount.set(0);
            },
            error: () => undefined
        });
    }

    clear(): void {
        this._notifications.set([]);
        this._latestNotification.set(null);
        this._unreadCount.set(0);
        this._loading.set(false);
        this._loadError.set(null);
    }

    private refreshUnreadCount(): void {
        this.notificationsClient.getUnreadCount().subscribe({
            next: response => {
                this._unreadCount.set(response.count ?? 0);
            },
            error: () => undefined
        });
    }

    private upsert(notification: NotificationItem): void {
        this._notifications.update(notifications => [
            notification,
            ...notifications.filter(item => item.id !== notification.id)
        ].slice(0, 50));
    }

    private mapApiNotification(
        response: UserNotificationResponse
    ): NotificationItem | null {
        if (
            response.id === undefined ||
            !response.type ||
            !response.title ||
            !response.message ||
            !response.createdAtUtc
        ) {
            return null;
        }

        return {
            id: response.id,
            key: response.key,
            type: response.type,
            title: response.title,
            message: response.message,
            data: this.parseDataJson(response.dataJson),
            route: response.route,
            createdAtUtc: response.updatedAtUtc ?? response.createdAtUtc,
            isRead: response.isRead ?? false,
            tone: this.toTone(response.tone)
        };
    }

    private mapRealtimeMessage(
        message: RealtimeMessage
    ): NotificationItem | null {
        if (message.notificationId === undefined ||
            message.notificationId === null) {
            return null;
        }

        return {
            id: message.notificationId,
            key: message.key ?? undefined,
            type: message.type,
            title: message.title ?? 'Notification',
            message: message.message ?? '',
            data: message.data,
            route: message.route ?? undefined,
            createdAtUtc: message.createdAtUtc,
            isRead: false,
            tone: this.toTone(message.tone)
        };
    }

    private parseDataJson(value?: string): unknown | null {
        if (!value) {
            return null;
        }

        try {
            return JSON.parse(value) as unknown;
        } catch {
            return null;
        }
    }

    private toTone(value?: string | null): NotificationTone {
        switch (value) {
            case 'success':
                return 'success';

            case 'warning':
                return 'warning';

            case 'danger':
                return 'danger';

            default:
                return 'info';
        }
    }
}