import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import type {
  BusinessTeamMemberDto,
  OrganizationModuleDefinitionDto,
} from '../../core/models/team.models';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { BusinessTeamApiService } from '../../core/services/business-team-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-team-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './team-list.component.html',
  styleUrl: './team-list.component.scss',
})
export class TeamListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessTeamApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly toast = inject(ToastService);

  readonly members = signal<BusinessTeamMemberDto[]>([]);
  readonly modules = signal<OrganizationModuleDefinitionDto[]>([]);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly statusUpdatingId = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
    hasAllModuleAccess: [false],
    isActive: [true],
    moduleCodes: this.fb.nonNullable.control<string[]>([]),
    locationIds: this.fb.nonNullable.control<string[]>([]),
    defaultLocationId: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  get isEditing(): boolean {
    return this.editingId() !== null;
  }

  load(): void {
    this.loading.set(true);
    this.api.listModules().subscribe({
      next: (modRes) => {
        if (modRes.success && modRes.data) {
          this.modules.set(modRes.data);
        }
        this.api.listMembers().subscribe({
          next: (res) => {
            this.locationsApi.listLocations().subscribe({
              next: (locRes) => {
                this.loading.set(false);
                if (locRes.success && locRes.data) {
                  this.locations.set(locRes.data);
                }
                if (res.success && res.data) {
                  this.members.set(res.data);
                } else {
                  this.toast.showFailedApi(res, 'Team');
                }
              },
              error: () => {
                this.loading.set(false);
                if (res.success && res.data) {
                  this.members.set(res.data);
                }
              },
            });
          },
          error: () => {
            this.loading.set(false);
            this.toast.error('Could not load team members.', 'Team');
          },
        });
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Could not load modules.', 'Team');
      },
    });
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.controls.username.setValidators([
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(64)]);
    this.form.controls.username.updateValueAndValidity();
    this.form.reset({
      firstName: '',
      lastName: '',
      email: '',
      username: '',
      hasAllModuleAccess: false,
      isActive: true,
    });
    this.rebuildModuleChecks([]);
    this.rebuildLocationChecks([]);
  }

  startEdit(member: BusinessTeamMemberDto): void {
    if (member.isSuperAdmin) {
      return;
    }
    this.editingId.set(member.id);
    this.form.controls.username.clearValidators();
    this.form.controls.username.updateValueAndValidity();
    this.form.patchValue({
      firstName: member.firstName,
      lastName: member.lastName,
      email: member.email,
      username: member.username ?? '',
      hasAllModuleAccess: member.hasAllModuleAccess,
      isActive: member.isActive,
      defaultLocationId: member.defaultLocationId,
    });
    this.rebuildModuleChecks(member.moduleCodes);
    this.rebuildLocationChecks(member.locationIds);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset();
    this.rebuildModuleChecks([]);
    this.rebuildLocationChecks([]);
  }

  toggleAllModules(checked: boolean): void {
    this.form.controls.hasAllModuleAccess.setValue(checked);
  }

  isModuleSelected(code: string): boolean {
    return this.form.controls.moduleCodes.value.includes(code);
  }

  toggleModule(code: string, checked: boolean): void {
    const current = this.form.controls.moduleCodes.value;
    if (checked) {
      if (!current.includes(code)) {
        this.form.controls.moduleCodes.setValue([...current, code]);
      }
    } else {
      this.form.controls.moduleCodes.setValue(current.filter((c) => c !== code));
    }
  }

  isLocationSelected(id: string): boolean {
    return this.form.controls.locationIds.value.includes(id);
  }

  toggleLocation(id: string, checked: boolean): void {
    const current = this.form.controls.locationIds.value;
    let next: string[];
    if (checked) {
      next = current.includes(id) ? current : [...current, id];
    } else {
      next = current.filter((x) => x !== id);
    }
    this.form.controls.locationIds.setValue(next);
    const defaultId = this.form.controls.defaultLocationId.value;
    if (defaultId && !next.includes(defaultId)) {
      this.form.controls.defaultLocationId.setValue(next[0] ?? null);
    } else if (!defaultId && next.length === 1) {
      this.form.controls.defaultLocationId.setValue(next[0]);
    }
  }

  locationLabel(id: string): string {
    return this.locations().find((l) => l.id === id)?.name ?? id;
  }

  blockMember(member: BusinessTeamMemberDto): void {
    if (!member.isActive || member.isSuperAdmin) {
      return;
    }

    const name = `${member.firstName} ${member.lastName}`.trim();
    if (!confirm(`Block ${name}? They will not be able to sign in until you unblock them.`)) {
      return;
    }

    this.setMemberActive(member, false);
  }

  unblockMember(member: BusinessTeamMemberDto): void {
    if (member.isActive || member.isSuperAdmin) {
      return;
    }

    this.setMemberActive(member, true);
  }

  isStatusUpdating(memberId: string): boolean {
    return this.statusUpdatingId() === memberId;
  }

  private setMemberActive(member: BusinessTeamMemberDto, isActive: boolean): void {
    this.statusUpdatingId.set(member.id);
    this.api
      .setMemberStatus(member.id, isActive)
      .pipe(finalize(() => this.statusUpdatingId.set(null)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Team');
            return;
          }

          this.toast.success(
            isActive ? `${member.firstName} can sign in again.` : `${member.firstName} has been blocked.`,
            'Team',
          );

          if (this.editingId() === member.id) {
            this.form.controls.isActive.setValue(isActive);
          }

          this.load();
        },
        error: (err: unknown) => this.handleSaveError(err),
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (!raw.hasAllModuleAccess && raw.moduleCodes.length === 0) {
      this.toast.warning('Select at least one module or grant all-module access.', 'Team');
      return;
    }

    if (raw.locationIds.length === 0) {
      this.toast.warning('Assign at least one branch to this staff member.', 'Team');
      return;
    }

    const defaultLocationId = raw.defaultLocationId ?? raw.locationIds[0] ?? null;
    this.saving.set(true);
    const editing = this.editingId();

    if (editing) {
      this.api
        .updateMember(editing, {
          firstName: raw.firstName.trim(),
          lastName: raw.lastName.trim(),
          email: raw.email.trim(),
          hasAllModuleAccess: raw.hasAllModuleAccess,
          hasAllLocationAccess: false,
          defaultLocationId,
          isActive: raw.isActive,
          moduleCodes: raw.moduleCodes,
          locationIds: raw.locationIds,
        })
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: (res) => this.handleSaveResponse(res, 'updated'),
          error: (err: unknown) => this.handleSaveError(err),
        });
      return;
    }

    this.api
      .createMember({
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        email: raw.email.trim(),
        username: raw.username.trim().toLowerCase(),
        hasAllModuleAccess: raw.hasAllModuleAccess,
        hasAllLocationAccess: false,
        defaultLocationId,
        moduleCodes: raw.moduleCodes,
        locationIds: raw.locationIds,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => this.handleSaveResponse(res, 'invited'),
        error: (err: unknown) => this.handleSaveError(err),
      });
  }

  private handleSaveResponse(res: ApiResult<BusinessTeamMemberDto>, action: 'invited' | 'updated'): void {
    if (!res.success || !res.data) {
      this.toast.showFailedApi(res, 'Team');
      return;
    }

    this.toast.success(
      action === 'invited'
        ? 'Team member invited. They will receive an email with a temporary password.'
        : 'Team member updated.',
      'Team',
    );
    this.cancelEdit();
    this.load();
  }

  private handleSaveError(err: unknown): void {
    const res = err as ApiResult<BusinessTeamMemberDto>;
    if (res && typeof res === 'object' && 'message' in res) {
      this.toast.showFailedApi(res, 'Team');
      return;
    }
    this.toast.error('Could not save team member.', 'Team');
  }

  private rebuildModuleChecks(selected: string[]): void {
    this.form.controls.moduleCodes.setValue([...selected]);
  }

  private rebuildLocationChecks(selected: string[]): void {
    this.form.controls.locationIds.setValue([...selected]);
  }
}
