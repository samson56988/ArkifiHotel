import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { PropertyFacilitySummaryDto } from '../../core/models/facilities.models';
import { BusinessFacilitiesApiService } from '../../core/services/business-facilities-api.service';
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

  facilities: PropertyFacilitySummaryDto[] = [];
  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.api
      .listFacilities()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((res) => {
        if (res.success && res.data) {
          this.facilities = res.data;
          return;
        }

        this.error = res.message ?? 'Could not load property facilities.';
      });
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) {
      return '';
    }

    return this.api.resolveImageUrl(path);
  }
}
