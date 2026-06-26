import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import type { OrganizationAuditLogDto } from '../../core/models/audit.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessAuditApiService } from '../../core/services/business-audit-api.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [ReactiveFormsModule, BusinessWorkspaceComponent],
  templateUrl: './audit-list.component.html',
  styleUrl: './audit-list.component.scss',
})
export class AuditListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessAuditApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly toast = inject(ToastService);

  readonly logs = signal<OrganizationAuditLogDto[]>([]);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly loading = signal(false);
  readonly page = signal(1);
  readonly totalPages = signal(0);

  readonly filters = this.fb.nonNullable.group({
    locationId: [''],
    entityType: [''],
    action: [''],
  });

  ngOnInit(): void {
    this.locationsApi.listLocations().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.locations.set(res.data);
        }
      },
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const raw = this.filters.getRawValue();
    this.api
      .list({
        page: this.page(),
        pageSize: 50,
        locationId: raw.locationId || undefined,
        entityType: raw.entityType || undefined,
        action: raw.action || undefined,
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.logs.set(res.data.items ?? []);
            this.totalPages.set(res.data.totalPages);
          } else {
            this.toast.showFailedApi(res, 'Audit');
          }
        },
        error: () => {
          this.loading.set(false);
          this.toast.error('Could not load audit log.', 'Audit');
        },
      });
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
  }

  actionLabel(action: string): string {
    return action.replace(/_/g, ' ');
  }
}
