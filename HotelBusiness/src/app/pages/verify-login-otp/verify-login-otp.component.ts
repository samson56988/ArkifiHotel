import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';

@Component({
  selector: 'app-verify-login-otp',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-login-otp.component.html',
  styleUrl: './verify-login-otp.component.scss',
})
export class VerifyLoginOtpComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    challengeId: ['', [Validators.required]],
    otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    rememberMe: [false],
  });

  submitted = false;
  loading = false;
  apiError: string | null = null;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const challengeId = this.route.snapshot.queryParamMap.get('challengeId') ?? '';
    const rememberMe = this.route.snapshot.queryParamMap.get('rememberMe') === '1';
    this.form.patchValue({ email, challengeId, rememberMe });
  }

  onSubmit(): void {
    this.submitted = true;
    this.apiError = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .verifyLoginOtp(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((result) => {
        if (result.success) {
          void this.router.navigateByUrl('/dashboard');
          return;
        }
        this.apiError = result.message ?? 'Invalid or expired code.';
      });
  }

  showError(field: 'email' | 'challengeId' | 'otp'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
