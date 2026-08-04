import { TestBed } from '@angular/core/testing';
import {
    HttpClientTestingModule,
    HttpTestingController
} from '@angular/common/http/testing';

import { environment } from '../../../../environments/environment';
import { WorkItemPriority } from '../models/board.models';
import { BoardService } from './board.service';

describe('BoardService', () => {
    let service: BoardService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });

        service = TestBed.inject(BoardService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('requests the board for a project', () => {
        service.getBoard('project-1').subscribe();

        const request = httpMock.expectOne(
            `${environment.apiBaseUrl}/projects/project-1/board`
        );

        expect(request.request.method).toBe('GET');
        request.flush({
            projectId: 'project-1',
            projectName: 'ScrumBoard',
            columns: []
        });
    });

    it('updates a task without sending columnId in the body', () => {
        service.updateTask('project-1', 'task-1', {
            title: 'Actualizar UI',
            description: 'Mejorar tablero',
            priority: WorkItemPriority.High,
            assignedUserId: 'user-1',
            columnId: 'column-1'
        }).subscribe();

        const request = httpMock.expectOne(
            `${environment.apiBaseUrl}/projects/project-1/tasks/task-1`
        );

        expect(request.request.method).toBe('PUT');
        expect(request.request.body).toEqual({
            title: 'Actualizar UI',
            description: 'Mejorar tablero',
            priority: WorkItemPriority.High,
            assignedUserId: 'user-1'
        });
        request.flush({});
    });

    it('moves a task to the requested column and index', () => {
        const payload = {
            targetColumnId: 'done',
            targetIndex: 0
        };

        service.moveTask('project-1', 'task-1', payload).subscribe();

        const request = httpMock.expectOne(
            `${environment.apiBaseUrl}/projects/project-1/tasks/task-1/position`
        );

        expect(request.request.method).toBe('PUT');
        expect(request.request.body).toEqual(payload);
        request.flush({
            taskId: 'task-1',
            sourceColumnId: 'todo',
            targetColumnId: 'done',
            affectedColumns: []
        });
    });
});
