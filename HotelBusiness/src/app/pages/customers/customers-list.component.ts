import { Component, inject, OnInit } from '@angular/core';
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

  customers: CustomerSummaryDto[] = [];
  initialLoadDone = false;
  loadFailed = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loadFailed = false;
    this.api.listCustomers().subscribe({
      next: (res) => {
        if (res.success) {
          this.customers = Array.isArray(res.data) ? res.data! : [];
          this.loadFailed = false;
        } else {
          this.customers = [];
          this.loadFailed = true;
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again.', 'Customers');
          } else {
            this.toast.showFailedApi(res, 'Customers');
          }
        }

        this.initialLoadDone = true;
      },
      error: (err: unknown) => {
        this.customers = [];
        this.loadFailed = true;
        this.initialLoadDone = true;
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
