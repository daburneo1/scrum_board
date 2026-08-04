import {Injectable} from "@angular/core";
import {HttpClient, HttpParams, HttpResponse} from "@angular/common/http";
import {Observable} from "rxjs";
import {environment} from "../../../../environments/environment";
import {ProjectTaskFilters} from "../models/board.models";

export type ProjectReportFormat =
    | 'pdf'
    | 'xlsx';

@Injectable({
    providedIn: 'root'
})
export class ProjectReportService {
    constructor(
        private readonly http: HttpClient
    ) {
    }

    getReport(
        projectId: string,
        format: ProjectReportFormat,
        filters?: ProjectTaskFilters
    ): Observable<HttpResponse<Blob>> {
        return this.http.get(
            `${environment.apiBaseUrl}` +
            `/projects/${projectId}` +
            `/reports/${format}`,
            {
                observe: 'response',
                responseType: 'blob',
                params: this.buildFilterParams(filters)
            }
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
