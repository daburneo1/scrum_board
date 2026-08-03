import {Injectable} from "@angular/core";
import {HttpClient, HttpResponse} from "@angular/common/http";
import {Observable} from "rxjs";
import {environment} from "../../../../environments/environment";

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
        format: ProjectReportFormat
    ): Observable<HttpResponse<Blob>> {
        return this.http.get(
            `${environment.apiBaseUrl}` +
            `/projects/${projectId}` +
            `/reports/${format}`,
            {
                observe: 'response',
                responseType: 'blob'
            }
        );
    }
}
