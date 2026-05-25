export interface RealtimeMessage<TData = unknown> {
    notificationId?: number | null;
    key?: string | null;
    type: string;
    title?: string | null;
    message?: string | null;
    tone?: string | null;
    route?: string | null;
    data?: TData | null;
    createdAtUtc: string;
}