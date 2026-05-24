export type NotificationTone = 'info' | 'success' | 'warning' | 'danger';

export interface NotificationItem<TData = unknown> {
    id: string;
    key?: string;
    type: string;
    title: string;
    message: string;
    data?: TData | null;
    route?: string;
    createdAtUtc: string;
    isRead: boolean;
    tone: NotificationTone;
}