import { Injectable } from '@angular/core';
import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel
} from '@microsoft/signalr';
import {
    BehaviorSubject,
    Observable,
    Subject
} from 'rxjs';
import {
    BoardChangedNotification,
    BoardPresenceSnapshot,
    RealtimeConnectionState
} from '../models/board.models';
import {TokenStorageService} from "../../../core/auth/token-storage.service";
import {environment} from "../../../../environments/environment";

@Injectable({
    providedIn: 'root'
})
export class BoardRealtimeService {
    private connection: HubConnection | null = null;
    private currentProjectId: string | null = null;

    private readonly boardChangedSubject =
        new Subject<BoardChangedNotification>();

    private readonly connectionStateSubject =
        new BehaviorSubject<RealtimeConnectionState>(
            'disconnected'
        );

    private readonly presenceSubject =
        new BehaviorSubject<BoardPresenceSnapshot>(
            this.createEmptyPresenceSnapshot(null)
        );

    readonly boardChanged$: Observable<BoardChangedNotification> =
        this.boardChangedSubject.asObservable();

    readonly connectionState$: Observable<RealtimeConnectionState> =
        this.connectionStateSubject.asObservable();

    readonly presence$: Observable<BoardPresenceSnapshot> =
        this.presenceSubject.asObservable();

    constructor(
        private readonly tokenStorage: TokenStorageService
    ) {
    }

    async connect(projectId: string): Promise<void> {
        await this.disconnect();

        this.currentProjectId = projectId;
        this.connectionStateSubject.next('connecting');
        this.presenceSubject.next(
            this.createEmptyPresenceSnapshot(projectId)
        );

        const connection =
            new HubConnectionBuilder()
                .withUrl(
                    environment.realtimeBaseUrl,
                    {
                        accessTokenFactory: () =>
                            this.tokenStorage
                                .getAccessToken() ?? ''
                    }
                )
                .withAutomaticReconnect([
                    0,
                    2000,
                    5000,
                    10000
                ])
                .configureLogging(
                    LogLevel.Warning
                )
                .build();

        connection.on(
            'BoardChanged',
            (
                notification:
                BoardChangedNotification
            ) => {
                if (
                    notification.projectId !==
                    this.currentProjectId
                ) {
                    return;
                }

                this.boardChangedSubject.next(
                    notification
                );
            }
        );

        connection.on(
            'BoardPresenceChanged',
            (
                snapshot:
                BoardPresenceSnapshot
            ) => {
                if (
                    snapshot.projectId !==
                    this.currentProjectId
                ) {
                    return;
                }

                this.presenceSubject.next(
                    snapshot
                );
            }
        );

        connection.onreconnecting(() => {
            this.connectionStateSubject.next(
                'reconnecting'
            );
        });

        connection.onreconnected(async () => {
            const activeProjectId =
                this.currentProjectId;

            if (!activeProjectId) {
                return;
            }

            try {
                /*
                 * Los grupos no sobreviven
                 * a una reconexión.
                 */
                await connection.invoke(
                    'JoinBoard',
                    activeProjectId
                );

                this.connectionStateSubject.next(
                    'connected'
                );
            } catch (error) {
                console.error(
                    'Could not rejoin board group.',
                    error
                );

                this.connectionStateSubject.next(
                    'disconnected'
                );
            }
        });

        connection.onclose(error => {
            if (error) {
                console.error(
                    'SignalR connection closed.',
                    error
                );
            }

            this.connectionStateSubject.next(
                'disconnected'
            );
        });

        this.connection = connection;

        try {
            await connection.start();

            if (this.connection !== connection) {
                await connection.stop();
                return;
            }

            await connection.invoke(
                'JoinBoard',
                projectId
            );

            this.connectionStateSubject.next(
                'connected'
            );
        } catch (error) {
            connection.off('BoardChanged');
            connection.off('BoardPresenceChanged');

            try {
                await connection.stop();
            } catch {
                // La conexión posiblemente nunca inició.
            }

            if (this.connection === connection) {
                this.connection = null;
                this.currentProjectId = null;
                this.presenceSubject.next(
                    this.createEmptyPresenceSnapshot(null)
                );
            }

            this.connectionStateSubject.next(
                'disconnected'
            );

            throw error;
        }
    }

    async disconnect(): Promise<void> {
        const connection = this.connection;
        const projectId = this.currentProjectId;

        console.log(
            '[BoardRealtimeService] Iniciando desconexión',
            {
                projectId,
                connectionState: connection?.state
            }
        );

        this.connection = null;
        this.currentProjectId = null;

        if (!connection) {
            this.connectionStateSubject.next(
                'disconnected'
            );
            this.presenceSubject.next(
                this.createEmptyPresenceSnapshot(null)
            );

            console.log(
                '[BoardRealtimeService] No había conexión activa'
            );

            return;
        }

        try {
            if (
                projectId &&
                connection.state ===
                HubConnectionState.Connected
            ) {
                console.log(
                    '[BoardRealtimeService] Saliendo del grupo',
                    projectId
                );

                await connection.invoke(
                    'LeaveBoard',
                    projectId
                );
            }
        } catch (error) {
            console.warn(
                '[BoardRealtimeService] No se pudo salir explícitamente del grupo',
                error
            );
        }

        connection.off('BoardChanged');
        connection.off('BoardPresenceChanged');

        try {
            await connection.stop();

            console.log(
                '[BoardRealtimeService] Conexión detenida'
            );
        } finally {
            this.connectionStateSubject.next(
                'disconnected'
            );
            this.presenceSubject.next(
                this.createEmptyPresenceSnapshot(null)
            );
        }
    }

    private createEmptyPresenceSnapshot(
        projectId: string | null
    ): BoardPresenceSnapshot {
        return {
            projectId: projectId ?? '',
            connectedUserCount: 0,
            users: [],
            occurredAtUtc: new Date(0).toISOString()
        };
    }
}
