export enum ProjectStatus {
    Planned = 1,
    Active = 2,
    Completed = 3,
    Cancelled = 4
}

export interface Project {
    id: string;
    name: string;
    description: string;
    startDate: string;
    expectedEndDate: string;
    status: ProjectStatus;
}

export interface SaveProjectRequest {
    name: string;
    description: string;
    startDate: string;
    expectedEndDate: string;
    status: ProjectStatus;
}

export interface ProjectQuery {
    pageNumber: number;
    pageSize: number;
    name?: string;
}

export interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}
