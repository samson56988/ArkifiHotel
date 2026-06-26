import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { BusinessTeamInviteDto } from '../../core/models/team.models';
import { BusinessTeamApiService } from '../../core/services/business-team-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-team-invites-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent, DatePipe],
  templateUrl: './team-invites-list.component.html',
  styleUrl: './team-invites-list.component.scss',
})
export class TeamInvitesListComponent implements OnInit {
  private readonly api = inject(BusinessTeamApiService);
  private readonly toast = inject(ToastService);

  readonly invites = signal<BusinessTeamInviteDto[]>([]);
  readonly loading = signal(false);
  readonly resendingId = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api
      .listInvites()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.invites.set([]);
            this.toast.showFailedApi(res, 'Invites');
            return;
          }

          this.invites.set(res.data ?? []);
        },
        error: () => {
          this.invites.set([]);
          this.toast.error('Could not load sent invites.', 'Invites');
        },
      });
  }

  isResending(id: string): boolean {
    return this.resendingId() === id;
  }

  canResend(invite: BusinessTeamInviteDto): boolean {
    return invite.isActive && invite.isPending;
  }

  resendInvite(invite: BusinessTeamInviteDto): void {
    if (!this.canResend(invite)) {
      return;
    }

    const name = `${invite.firstName} ${invite.lastName}`.trim();
    if (
      !globalThis.confirm(
        `Resend invite email to ${name} at ${invite.email}? A new temporary password will be issued.`,
      )
    ) {
      return;
    }

    this.resendingId.set(invite.id);
    this.api
      .resendInvite(invite.id)
      .pipe(finalize(() => this.resendingId.set(null)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Resend invite');
            return;
          }

          this.invites.update((list) => list.map((row) => (row.id === res.data!.id ? res.data! : row)));
          this.toast.success(res.message ?? `Invite resent to ${invite.email}.`, 'Invites');
        },
        error: (err: unknown) => {
          const res = err as { message?: string };
          this.toast.error(res?.message ?? 'Could not resend invite.', 'Invites');
        },
      });
  }

  statusLabel(invite: BusinessTeamInviteDto): string {
    if (!invite.isActive) {
      return 'Blocked';
    }

    return invite.isPending ? 'Pending sign-in' : 'Joined';
  }
}
