import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-business-workspace',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './business-workspace.component.html',
  styleUrl: './business-workspace.component.scss',
})
export class BusinessWorkspaceComponent {}
