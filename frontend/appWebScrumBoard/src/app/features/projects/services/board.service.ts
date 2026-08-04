import {HttpClient, HttpParams} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {
    BoardColumn,
    BoardTask,
    MoveTaskRequest, MoveTaskResponse,
    ProjectBoard,
    ProjectTaskFilters,
    SaveTaskRequest,
    UserOption
} from "../models/board.models";
import {environment} from "../../../../environments/environment";


@Injectable({
    providedIn: 'root'
})
export class BoardService {
    constructor(
        private readonly http: HttpClient
    ) {
    }

    getBoard(
        projectId: string,
        filters?: ProjectTaskFilters
    ): Observable<ProjectBoard> {
        return this.http.get<ProjectBoard>(
            `${environment.apiBaseUrl}/projects/${projectId}/board`,
            {
                params: this.buildFilterParams(filters)
            }
        );
    }

    getUsers(): Observable<UserOption[]> {
        return this.http.get<UserOption[]>(
            `${environment.apiBaseUrl}/users`
        );
    }

    createColumn(
        projectId: string,
        name: string
    ): Observable<BoardColumn> {
        return this.http.post<BoardColumn>(
            `${environment.apiBaseUrl}/projects/${projectId}/columns`,
            { name }
        );
    }

    updateColumn(
        projectId: string,
        columnId: string,
        name: string
    ): Observable<BoardColumn> {
        return this.http.put<BoardColumn>(
            `${environment.apiBaseUrl}/projects/${projectId}/columns/${columnId}`,
            { name }
        );
    }

    deleteColumn(
        projectId: string,
        columnId: string
    ): Observable<void> {
        return this.http.delete<void>(
            `${environment.apiBaseUrl}/projects/${projectId}/columns/${columnId}`
        );
    }

    reorderColumns(
        projectId: string,
        orderedColumnIds: string[]
    ): Observable<void> {
        return this.http.put<void>(
            `${environment.apiBaseUrl}/projects/${projectId}/columns/order`,
            { orderedColumnIds }
        );
    }

    createTask(
        projectId: string,
        request: SaveTaskRequest & { columnId: string }
    ): Observable<BoardTask> {
        return this.http.post<BoardTask>(
            `${environment.apiBaseUrl}/projects/${projectId}/tasks`,
            request
        );
    }

    updateTask(
        projectId: string,
        taskId: string,
        request: SaveTaskRequest
    ): Observable<BoardTask> {
        const {
            columnId: _columnId,
            ...updateRequest
        } = request;

        return this.http.put<BoardTask>(
            `${environment.apiBaseUrl}/projects/${projectId}/tasks/${taskId}`,
            updateRequest
        );
    }

    deleteTask(
        projectId: string,
        taskId: string
    ): Observable<void> {
        return this.http.delete<void>(
            `${environment.apiBaseUrl}/projects/${projectId}/tasks/${taskId}`
        );
    }

    moveTask(
        projectId: string,
        taskId: string,
        request: MoveTaskRequest
    ): Observable<MoveTaskResponse> {
        return this.http.put<MoveTaskResponse>(
            `${environment.apiBaseUrl}/projects/${projectId}/tasks/${taskId}/position`,
            request
        );
    }

    private buildFilterParams(
        filters?: ProjectTaskFilters
    ): HttpParams {
        let params = new HttpParams();

        if (!filters) {
            return params;
        }

        if (filters.assigneeId) {
            params = params.set(
                'assigneeId',
                filters.assigneeId
            );
        }

        if (filters.priority !== null &&
            filters.priority !== undefined) {
            params = params.set(
                'priority',
                String(filters.priority)
            );
        }

        const search = filters.search?.trim();

        if (search) {
            params = params.set(
                'search',
                search
            );
        }

        return params;
    }
}
