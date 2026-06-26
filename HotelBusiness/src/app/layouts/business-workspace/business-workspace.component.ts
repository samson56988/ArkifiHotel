import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { BusinessContextService } from '../../core/services/business-context.service';
import { OrganizationAccessService } from '../../core/services/organization-access.service';
import { OrganizationLocationService } from '../../core/services/organization-location.service';

@Component({
  selector: 'app-business-workspace',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './business-workspace.component.html',
  styleUrl: './business-workspace.component.scss',
})
export class BusinessWorkspaceComponent implements OnInit {
  readonly biz = inject(BusinessContextService);
  readonly access = inject(OrganizationAccessService);
  readonly locations = inject(OrganizationLocationService);

  ngOnInit(): void {
    this.access.hydrateFromStorage();
    this.locations.hydrateFromStorage();
    this.biz.ensureLoaded();
  }

  canAccess(module: string): boolean {
    return this.access.canAccess(module);
  }
}
