import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {ProjectListComponent} from "./project-list/project-list.component";
import {ProjectBoardComponent} from "./project-board/project-board.component";

const routes: Routes = [
    {
        path: '',
        component: ProjectListComponent
    },
    {
        path: ':projectId/board',
        component: ProjectBoardComponent
    }
];

@NgModule({
    imports: [
        RouterModule.forChild(routes)
    ],
    exports: [
        RouterModule
    ]
})
export class ProjectsRoutingModule {
}
