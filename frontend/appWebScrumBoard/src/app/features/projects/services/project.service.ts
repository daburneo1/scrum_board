import { Injectable } from '@angular/core';
import {
    HttpClient,
    HttpParams
} from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
    PagedResult,
    Project,
    ProjectQuery,
    SaveProjectRequest
} from '../models/project.models';

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private readonly baseUrl =
        `${environment.apiBaseUrl}/projects`;

    constructor(
        private readonly http: HttpClient
    ) {
    }

    getPaged(
        query: ProjectQuery
    ): Observable<PagedResult<Project>> {
        let params = new HttpParams()
            .set('pageNumber', query.pageNumber)
            .set('pageSize', query.pageSize);

        if (query.name?.trim()) {
            params = params.set(
                'name',
                query.name.trim()
            );
        }

        return this.http.get<PagedResult<Project>>(
            this.baseUrl,
            { params }
        );
    }

    create(
        request: SaveProjectRequest
    ): Observable<Project> {
        return this.http.post<Project>(
            this.baseUrl,
            request
        );
    }

    update(
        id: string,
        request: SaveProjectRequest
    ): Observable<Project> {
        return this.http.put<Project>(
            `${this.baseUrl}/${id}`,
            request
        );
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(
            `${this.baseUrl}/${id}`
        );
    }
}
