import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
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
  /** First list response received (success or failure). */
  initialLoadDone = false;
  loadFailed = false;
  showArchived = false;

  ngOnInit(): void {
    this.scheduleLoad();
  }

  onShowArchivedChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.showArchived = input.checked;
    this.scheduleLoad();
  }

  private scheduleLoad(): void {
    setTimeout(() => this.load(), 0);
  }

  load(): void {
    this.loadFailed = false;
    this.api.listFacilities(this.showArchived).subscribe({
      next: (res) => {
        if (res.success) {
          this.facilities = Array.isArray(res.data) ? res.data! : [];
          this.loadFailed = false;
        } else {
          this.facilities = [];
          this.loadFailed = true;
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again to manage facilities.', 'Facilities');
          } else {
            this.toast.showFailedApi(res, 'Facilities');
          }
        }

        this.initialLoadDone = true;
      },
      error: (err: unknown) => {
        this.facilities = [];
        this.loadFailed = true;
        this.initialLoadDone = true;
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
