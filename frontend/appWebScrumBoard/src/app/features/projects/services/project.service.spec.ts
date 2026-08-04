import { TestBed } from '@angular/core/testing';
import {
    HttpClientTestingModule,
    HttpTestingController
} from '@angular/common/http/testing';

import { environment } from '../../../../environments/environment';
import { ProjectStatus } from '../models/project.models';
import { ProjectService } from './project.service';

describe('ProjectService', () => {
    let service: ProjectService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });

        service = TestBed.inject(ProjectService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('requests paged projects with trimmed name filter', () => {
        service.getPaged({
            pageNumber: 2,
            pageSize: 20,
            name: '  sprint  '
        }).subscribe();

        const request = httpMock.expectOne(req =>
            req.url === `${environment.apiBaseUrl}/projects` &&
            req.params.get('pageNumber') === '2' &&
            req.params.get('pageSize') === '20' &&
            req.params.get('name') === 'sprint'
        );

        expect(request.request.method).toBe('GET');
        request.flush({
            items: [],
            pageNumber: 2,
            pageSize: 20,
            totalCount: 0,
            totalPages: 0
        });
    });

    it('creates a project through POST', () => {
        const payload = {
            name: 'ScrumBoard',
            description: 'Proyecto frontend',
            startDate: '2026-08-04',
            expectedEndDate: '2026-08-31',
            status: ProjectStatus.Active
        };

        service.create(payload).subscribe();

        const request = httpMock.expectOne(`${environment.apiBaseUrl}/projects`);

        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual(payload);
        request.flush({ id: 'project-1', ...payload });
    });

    it('deletes a project by id', () => {
        service.delete('project-1').subscribe();

        const request = httpMock.expectOne(
            `${environment.apiBaseUrl}/projects/project-1`
        );

        expect(request.request.method).toBe('DELETE');
        request.flush(null);
    });
});
