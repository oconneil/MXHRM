export interface RealtimeMessage<TData = unknown> {
    type: string;
    title?: string | null;
    message?: string | null;
    data?: TData | null;
    createdAtUtc: string;
}