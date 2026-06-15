import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import {
  BANNER_STYLE_OPTIONS,
  COLOR_PRESETS,
  GLOBAL_FONT_PRESETS,
  SECTION_FONT_OPTIONS,
} from '../../core/data/storefront-theme-presets';
import { buildStorefrontUrl } from '../../core/constants/storefront';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessProfileDto } from '../../core/models/business-profile.models';
import {
  createDefaultTheme,
  type PublicStorefront,
  type StorefrontTheme,
} from '../../core/models/storefront-theme.models';
import { EMPTY_BUSINESS_SOCIAL_PROFILE } from '../../core/models/business-social-profile.models';
import { BusinessProfileApiService } from '../../core/services/business-profile-api.service';
import { StorefrontThemeApiService } from '../../core/services/storefront-theme-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';
import { StorefrontRendererComponent } from '../../shared/storefront-renderer/storefront-renderer.component';

type DesignerTab = 'global' | 'banner' | 'about' | 'rooms' | 'facilities' | 'footer' | 'colors';

@Component({
  selector: 'app-storefront-designer',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent, StorefrontRendererComponent],
  templateUrl: './storefront-designer.component.html',
  styleUrl: './storefront-designer.component.scss',
})
export class StorefrontDesignerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly themeApi = inject(StorefrontThemeApiService);
  private readonly profileApi = inject(BusinessProfileApiService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly globalFonts = GLOBAL_FONT_PRESETS;
  readonly bannerStyles = BANNER_STYLE_OPTIONS;
  readonly sectionFonts = SECTION_FONT_OPTIONS;
  readonly colorPresets = Object.entries(COLOR_PRESETS).map(([id, palette]) => ({ id, palette }));

  readonly activeTab = signal<DesignerTab>('banner');
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly profile = signal<BusinessProfileDto | null>(null);
  readonly catalog = signal<PublicStorefront | null>(null);

  readonly form = this.fb.nonNullable.group({
    globalFont: ['modern-sans' as StorefrontTheme['globalFont'], Validators.required],
    banner: this.fb.nonNullable.group({
      style: ['grand-hero' as StorefrontTheme['banner']['style']],
      headlineFont: ['display' as StorefrontTheme['banner']['headlineFont']],
      subheadlineFont: ['body' as StorefrontTheme['banner']['subheadlineFont']],
      headline: ['', Validators.required],
      subheadline: [''],
      textAlign: ['center' as StorefrontTheme['banner']['textAlign']],
      overlayOpacity: [55, [Validators.min(0), Validators.max(90)]],
    }),
    about: this.fb.nonNullable.group({
      enabled: [true],
      title: ['Who we are'],
      description: [''],
      titleFont: ['display' as StorefrontTheme['about']['titleFont']],
      bodyFont: ['body' as StorefrontTheme['about']['bodyFont']],
      layout: ['side-by-side' as StorefrontTheme['about']['layout']],
    }),
    rooms: this.fb.nonNullable.group({
      enabled: [true],
      title: ['Our rooms'],
      subtitle: [''],
      titleFont: ['display' as StorefrontTheme['rooms']['titleFont']],
      cardStyle: ['elevated' as StorefrontTheme['rooms']['cardStyle']],
      showPrice: [true],
    }),
    facilities: this.fb.nonNullable.group({
      enabled: [true],
      title: ['Facilities & amenities'],
      subtitle: [''],
      titleFont: ['display' as StorefrontTheme['facilities']['titleFont']],
      displayStyle: ['grid' as StorefrontTheme['facilities']['displayStyle']],
    }),
    footer: this.fb.nonNullable.group({
      style: ['columns' as StorefrontTheme['footer']['style']],
      tagline: [''],
      copyrightText: [''],
      showContact: [true],
      backgroundStyle: ['dark-band' as StorefrontTheme['footer']['backgroundStyle']],
    }),
    colors: this.fb.nonNullable.group({
      preset: ['sage-luxe' as StorefrontTheme['colors']['preset']],
      primary: ['#5c7a5c'],
      accent: ['#c8dcc8'],
      background: ['#faf9f6'],
      text: ['#1f2a1f'],
    }),
  });

  readonly previewStorefront = signal<PublicStorefront | null>(null);

  readonly liveUrl = computed(() => {
    const slug = this.profile()?.slug;
    return slug ? buildStorefrontUrl(slug) : null;
  });

  ngOnInit(): void {
    this.load();

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.refreshPreview();
    });

    this.form.controls.colors.controls.preset.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((preset) => this.applyColorPreset(preset));
  }

  setTab(tab: DesignerTab): void {
    this.activeTab.set(tab);
  }

  load(): void {
    this.loading.set(true);
    this.profileApi
      .getProfile()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (profileRes) => {
          if (!profileRes.success || !profileRes.data) {
            this.toast.showFailedApi(profileRes, 'Profile');
            return;
          }

          this.profile.set(profileRes.data);
          this.themeApi.getTheme().subscribe({
            next: (themeRes) => {
              const theme =
                themeRes.success && themeRes.data
                  ? themeRes.data
                  : createDefaultTheme(profileRes.data!.businessName);
              this.patchTheme(theme);

              const slug = profileRes.data!.slug;
              if (slug) {
                this.themeApi.getPublicStorefront(slug).subscribe({
                  next: (pub) => {
                    if (pub.success && pub.data) {
                      this.catalog.set(pub.data);
                    }
                    this.refreshPreview();
                  },
                  error: () => this.refreshPreview(),
                });
              } else {
                this.refreshPreview();
              }
            },
          });
        },
        error: () => this.toast.error('Could not load profile.', 'Storefront'),
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const theme = this.form.getRawValue() as StorefrontTheme;
    this.saving.set(true);
    this.themeApi
      .updateTheme(theme)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Storefront');
            return;
          }

          this.patchTheme(res.data);
          this.toast.success('Storefront theme saved.', 'Storefront');
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Storefront');
            return;
          }

          this.toast.error('Could not save theme.', 'Storefront');
        },
      });
  }

  resetToDefaults(): void {
    const name = this.profile()?.businessName ?? 'Your hotel';
    this.patchTheme(createDefaultTheme(name));
    this.toast.info('Defaults loaded — save to apply.', 'Storefront');
  }

  selectColorPreset(preset: string): void {
    this.form.controls.colors.controls.preset.setValue(preset as StorefrontTheme['colors']['preset']);
  }

  private patchTheme(theme: StorefrontTheme): void {
    this.form.patchValue(theme, { emitEvent: true });
    this.refreshPreview();
  }

  private applyColorPreset(preset: StorefrontTheme['colors']['preset']): void {
    const palette = COLOR_PRESETS[preset];
    if (!palette) {
      return;
    }

    this.form.controls.colors.patchValue(
      {
        primary: palette.primary,
        accent: palette.accent,
        background: palette.background,
        text: palette.text,
      },
      { emitEvent: true },
    );
  }

  private refreshPreview(): void {
    const profile = this.profile();
    if (!profile) {
      return;
    }

    const theme = this.form.getRawValue() as StorefrontTheme;
    const catalog = this.catalog();

    this.previewStorefront.set({
      businessId: profile.id,
      businessName: profile.businessName,
      slug: profile.slug ?? 'preview',
      logoUrl: profile.logoUrl,
      theme,
      rooms: catalog?.rooms ?? [],
      facilities: catalog?.facilities ?? [],
      social: catalog?.social ?? EMPTY_BUSINESS_SOCIAL_PROFILE,
    });
  }
}
