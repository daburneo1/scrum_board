export enum WorkItemPriority {
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

export interface ProjectBoard {
    projectId: string;
    projectName: string;
    columns: BoardColumn[];
}

export interface BoardColumn {
    id: string;
    name: string;
    sortOrder: number;
    tasks: BoardTask[];
}

export interface BoardTask {
    id: string;
    title: string;
    description: string;
    priority: WorkItemPriority;
    assignedUserId: string | null;
    assignedUserName: string | null;
    columnId: string;
    sortOrder: number;
    createdAtUtc: string;
}

export interface UserOption {
    id: string;
    name: string;
    email: string;
}

export interface ProjectTaskFilters {
    assigneeId?: string | null;
    priority?: WorkItemPriority | null;
    search?: string | null;
}

export interface SaveTaskRequest {
    title: string;
    description: string;
    priority: WorkItemPriority;
    assignedUserId: string | null;
    columnId?: string;
}

export interface MoveTaskRequest {
    targetColumnId: string;
    targetIndex: number;
}

export interface MoveTaskResponse {
    taskId: string;
    sourceColumnId: string;
    targetColumnId: string;
    affectedColumns: BoardColumn[];
}

export type BoardChangeType =
    | 'TaskCreated'
    | 'TaskUpdated'
    | 'TaskDeleted'
    | 'TaskMoved';

export interface BoardChangedNotification {
    eventId: string;
    projectId: string;
    changeType: BoardChangeType;
    taskId: string;
    occurredAtUtc: string;
}

export type RealtimeConnectionState =
    | 'connecting'
    | 'connected'
    | 'reconnecting'
    | 'disconnected';
