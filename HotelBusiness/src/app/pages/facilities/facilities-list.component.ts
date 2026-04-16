import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { PropertyFacilitySummaryDto } from '../../core/models/facilities.models';
import { BusinessFacilitiesApiService } from '../../core/services/business-facilities-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-facilities-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent],
  templateUrl: './facilities-list.component.html',
  styleUrl: './facilities-list.component.scss',
})
export class FacilitiesListComponent implements OnInit {
  private readonly api = inject(BusinessFacilitiesApiService);
  private readonly toast = inject(ToastService);

  facilities: PropertyFacilitySummaryDto[] = [];
  loading = false;
  loadFailed = false;
  showArchived = false;

  ngOnInit(): void {
    this.load();
  }

  onShowArchivedChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.showArchived = input.checked;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.loadFailed = false;
    this.api
      .listFacilities(this.showArchived)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.facilities = res.data;
            this.loadFailed = false;
            return;
          }

          this.facilities = [];
          this.loadFailed = true;
          this.toast.showFailedApi(res, 'Facilities');
        },
        error: (err: unknown) => {
          this.facilities = [];
          this.loadFailed = true;
          const res = err as ApiResult<PropertyFacilitySummaryDto[]>;
          if (res && typeof res === 'object' && 'message' in res) {
            this.toast.showFailedApi(res, 'Facilities');
            return;
          }

          this.toast.error('Could not load facilities.', 'Facilities');
        },
      });
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) {
      return '';
    }

    return this.api.resolveImageUrl(path);
  }
}
