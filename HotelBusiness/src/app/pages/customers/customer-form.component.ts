import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable, of } from 'rxjs';
import { finalize } from 'rxjs/operators';
import type { CustomerDetailApiResponse } from '../../core/models/customers.models';
import { BusinessCustomersApiService } from '../../core/services/business-customers-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessCustomersApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    phone: [''],
    notes: [''],
  });

  customerId: string | null = null;
  loading = false;
  saving = false;

  get isCreateMode(): boolean {
    return this.customerId === null;
  }

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('customerId');
    this.loading = true;

    const detail$: Observable<CustomerDetailApiResponse> = this.customerId
      ? this.api.getCustomer(this.customerId)
      : of<CustomerDetailApiResponse>({
          success: true,
          data: null,
          message: null,
          code: null,
          validationErrors: null,
        });

    detail$.pipe(finalize(() => (this.loading = false))).subscribe((detail) => {
      if (!this.customerId) {
        return;
      }

      if (!detail.success || !detail.data) {
        this.toast.error(detail.message ?? 'Customer not found.', 'Customer');
        return;
      }

      const d = detail.data;
      this.form.patchValue({
        fullName: d.fullName,
        email: d.email,
        phone: d.phone ?? '',
        notes: d.notes ?? '',
      });
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      fullName: raw.fullName.trim(),
      email: raw.email.trim(),
      phone: raw.phone.trim() || null,
      notes: raw.notes.trim() || null,
    };

    this.saving = true;
    const req$ = this.isCreateMode
      ? this.api.createCustomer(body)
      : this.api.updateCustomer(this.customerId!, body);

    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.showFailedApi(res, 'Customer');
          return;
        }

        if (this.isCreateMode) {
          void this.router.navigate(['/customers', res.data.id], { replaceUrl: true });
          this.toast.success('Customer created.', 'Customer');
          return;
        }

        this.toast.success('Customer updated.', 'Customer');
      },
      error: (err: unknown) => {
        const r = err as { message?: string };
        this.toast.error(r?.message ?? 'Request failed.', 'Customer');
      },
    });
  }

  deleteCustomer(): void {
    if (!this.customerId || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Delete this customer? This cannot be undone.')) {
      return;
    }

    this.saving = true;
    this.api
      .deleteCustomer(this.customerId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Customer');
            return;
          }

          void this.router.navigateByUrl('/customers');
          this.toast.success('Customer removed.', 'Customer');
        },
        error: (err: unknown) => {
          const r = err as { message?: string };
          this.toast.error(r?.message ?? 'Could not delete.', 'Customer');
        },
      });
  }
}
