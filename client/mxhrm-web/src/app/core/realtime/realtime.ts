import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth';
import { RealtimeMessage } from './realtime-message';

@Injectable({
    providedIn: 'root'
})
export class RealtimeService {
    latestMessage = signal<RealtimeMessage | null>(null);
    connected = signal(false);

    private connection?: signalR.HubConnection;

    constructor(private readonly authService: AuthService) { }

    start(): void {
        if (this.connection) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.apiBaseUrl}/hubs/realtime`, {
                accessTokenFactory: () => this.authService.getToken() ?? ''
            })
            .withAutomaticReconnect()
            .build();

        this.connection.on('realtimeMessage', message => {
            this.latestMessage.set(message);
        });

        this.connection.onreconnected(() => {
            this.connected.set(true);
        });

        this.connection.onreconnecting(() => {
            this.connected.set(false);
        });

        this.connection.onclose(() => {
            this.connected.set(false);
            this.connection = undefined;
        });

        this.connection
            .start()
            .then(() => this.connected.set(true))
            .catch(() => {
                this.connected.set(false);
                this.connection = undefined;
            });
    }

    stop(): void {
        this.connection?.stop();
        this.connection = undefined;
        this.connected.set(false);
    }

    joinGroup(groupName: string): void {
        this.connection?.invoke('JoinGroup', groupName);
    }

    leaveGroup(groupName: string): void {
        this.connection?.invoke('LeaveGroup', groupName);
    }
}
