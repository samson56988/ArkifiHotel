import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { ApiResult } from '../../core/models/api-result.model';
import type { CustomerSummaryDto } from '../../core/models/customers.models';
import { BusinessCustomersApiService } from '../../core/services/business-customers-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-customers-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent],
  templateUrl: './customers-list.component.html',
  styleUrl: './customers-list.component.scss',
})
export class CustomersListComponent implements OnInit {
  private readonly api = inject(BusinessCustomersApiService);
  private readonly toast = inject(ToastService);

  readonly customers = signal<CustomerSummaryDto[]>([]);
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api.listCustomers().subscribe({
      next: (res) => {
        if (res.success) {
          this.customers.set(Array.isArray(res.data) ? res.data! : []);
          this.loadFailed.set(false);
        } else {
          this.customers.set([]);
          this.loadFailed.set(true);
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again.', 'Customers');
          } else {
            this.toast.showFailedApi(res, 'Customers');
          }
        }

        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.customers.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        const r = err as ApiResult<CustomerSummaryDto[]>;
        if (r && typeof r === 'object' && 'message' in r) {
          this.toast.showFailedApi(r, 'Customers');
          return;
        }

        this.toast.error('Could not load customers.', 'Customers');
      },
    });
  }
}
