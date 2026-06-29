import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
  createShortletDefaultTheme,
  MAX_ABOUT_STATS,
  MAX_FACILITY_PERKS,
  type PublicStorefront,
  type StorefrontAboutStat,
  type StorefrontTheme,
} from '../../core/models/storefront-theme.models';
import { EMPTY_BUSINESS_SOCIAL_PROFILE } from '../../core/models/business-social-profile.models';
import { BusinessProfileApiService } from '../../core/services/business-profile-api.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { BusinessSocialProfileApiService } from '../../core/services/business-social-profile-api.service';
import { StorefrontBannerApiService } from '../../core/services/storefront-banner-api.service';
import { StorefrontAboutApiService } from '../../core/services/storefront-about-api.service';
import { OrganizationLocationService } from '../../core/services/organization-location.service';
import { StorefrontThemeApiService } from '../../core/services/storefront-theme-api.service';
import { ToastService } from '../../core/services/toast.service';
import {
  ALLOWED_IMAGE_ACCEPT,
  filterAllowedImageFiles,
} from '../../core/utils/image-upload';
import {
  MAX_BANNER_IMAGES,
  type StorefrontBannerImageDto,
} from '../../core/models/storefront-banner.models';
import type { StorefrontAboutImageDto } from '../../core/models/storefront-about.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { StorefrontGuestPreviewComponent, type GuestPreviewPage } from '../../shared/storefront-guest-preview/storefront-guest-preview.component';
import { ShortletGuestPreviewComponent } from '../../shared/shortlet-guest-preview/shortlet-guest-preview.component';
import { BusinessContextService } from '../../core/services/business-context.service';
import { BusinessAmenitiesApiService } from '../../core/services/business-amenities-api.service';
import type { AmenityDto } from '../../core/models/amenities.models';
import type { ShortletPreviewPage } from '../../core/utils/shortlet-preview.mapper';

type DesignerTab = 'global' | 'banner' | 'about' | 'rooms' | 'facilities' | 'footer' | 'contact' | 'colors';
type PreviewPage = GuestPreviewPage | ShortletPreviewPage;

@Component({
  selector: 'app-storefront-designer',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, StorefrontGuestPreviewComponent, ShortletGuestPreviewComponent],
  templateUrl: './storefront-designer.component.html',
  styleUrl: './storefront-designer.component.scss',
})
export class StorefrontDesignerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly themeApi = inject(StorefrontThemeApiService);
  private readonly bannerApi = inject(StorefrontBannerApiService);
  private readonly aboutApi = inject(StorefrontAboutApiService);
  private readonly profileApi = inject(BusinessProfileApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly orgLocation = inject(OrganizationLocationService);
  private readonly socialApi = inject(BusinessSocialProfileApiService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  readonly biz = inject(BusinessContextService);
  private readonly amenitiesApi = inject(BusinessAmenitiesApiService);

  readonly globalFonts = GLOBAL_FONT_PRESETS;
  readonly bannerStyles = BANNER_STYLE_OPTIONS;
  readonly sectionFonts = SECTION_FONT_OPTIONS;
  readonly colorPresets = Object.entries(COLOR_PRESETS).map(([id, palette]) => ({ id, palette }));

  readonly activeTab = signal<DesignerTab>('banner');
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly uploadingBanner = signal(false);
  readonly uploadingAbout = signal(false);
  readonly profile = signal<BusinessProfileDto | null>(null);
  readonly catalog = signal<PublicStorefront | null>(null);
  readonly businessAmenities = signal<AmenityDto[]>([]);
  readonly bannerImages = signal<StorefrontBannerImageDto[]>([]);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly bannerBranchId = signal<string | null>(null);
  readonly aboutImage = signal<StorefrontAboutImageDto | null>(null);
  readonly socialProfile = signal(EMPTY_BUSINESS_SOCIAL_PROFILE);

  readonly bannerImageAccept = ALLOWED_IMAGE_ACCEPT;
  readonly maxBannerImages = MAX_BANNER_IMAGES;
  readonly maxAboutStats = MAX_ABOUT_STATS;
  readonly maxFacilityPerks = MAX_FACILITY_PERKS;

  readonly aboutLayoutOptions = [
    {
      id: 'side-by-side' as StorefrontTheme['about']['layout'],
      label: 'Side by side',
      description: 'Story and photo in two columns',
    },
    {
      id: 'stacked' as StorefrontTheme['about']['layout'],
      label: 'Stacked',
      description: 'Photo on top, copy centered below',
    },
    {
      id: 'quote' as StorefrontTheme['about']['layout'],
      label: 'Quote highlight',
      description: 'Large quote leads the section',
    }];

  readonly bannerImagesForBranch = computed(() => {
    const branchId = this.bannerBranchId();
    if (!branchId) {
      return [];
    }
    return this.bannerImages().filter((i) => i.locationId === branchId);
  });

  readonly bannerSlotsRemaining = computed(
    () => MAX_BANNER_IMAGES - this.bannerImagesForBranch().length,
  );

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
      badgeText: ['Your stay awaits'],
    }),
    about: this.fb.nonNullable.group({
      enabled: [true],
      eyebrow: ['About us'],
      title: ['Our story'],
      description: [''],
      titleFont: ['display' as StorefrontTheme['about']['titleFont']],
      bodyFont: ['body' as StorefrontTheme['about']['bodyFont']],
      layout: ['side-by-side' as StorefrontTheme['about']['layout']],
      quote: [''],
      quoteBy: [''],
      showStats: [false],
      stats: this.fb.array<FormGroup>([]),
    }),
    rooms: this.fb.nonNullable.group({
      enabled: [true],
      eyebrow: ['Accommodations'],
      title: ['Our rooms'],
      subtitle: [''],
      titleFont: ['display' as StorefrontTheme['rooms']['titleFont']],
      cardStyle: ['elevated' as StorefrontTheme['rooms']['cardStyle']],
      showPrice: [true],
      showFeaturedSection: [true],
      featuredEyebrow: ['Signature Stay'],
      featuredTitle: ['Our most sought-after room'],
      showPageStats: [true],
      showPolicies: [true],
      policyBreakfast: ['Complimentary for suite guests'],
      policyPets: ['Small pets welcome on request'],
      policyCancellation: ['Free up to 48 hours before arrival'],
      ctaTitle: ['Ready to book your stay?'],
      ctaSubtitle: ['Reserve directly — no payment required until confirmation.'],
      ctaButtonText: ['Check availability'],
    }),
    facilities: this.fb.nonNullable.group({
      enabled: [true],
      eyebrow: ['On Property'],
      title: ['Facilities & amenities'],
      subtitle: [''],
      gridEyebrow: ['Browse amenities'],
      gridTitle: ["What's on offer"],
      gridSubtitle: ['Tap any facility to view photos and details.'],
      titleFont: ['display' as StorefrontTheme['facilities']['titleFont']],
      displayStyle: ['grid' as StorefrontTheme['facilities']['displayStyle']],
      showPageStats: [true],
      supportStatValue: ['24/7'],
      supportStatLabel: ['Guest support'],
      showGuestPerks: [true],
      perksEyebrow: ['Guest Perks'],
      perksTitle: ['Everything included in your stay'],
      perksSubtitle: [
        'Complimentary access to most on-property amenities for all registered guests.'],
      perksItems: this.fb.array<FormGroup>([]),
    }),
    footer: this.fb.nonNullable.group({
      style: ['columns' as StorefrontTheme['footer']['style']],
      tagline: [''],
      copyrightText: [''],
      showContact: [true],
      backgroundStyle: ['dark-band' as StorefrontTheme['footer']['backgroundStyle']],
    }),
    contact: this.fb.nonNullable.group({
      location: [''],
      checkIn: [''],
      checkOut: [''],
      introText: [
        'Questions about your stay? Send us a message and our team will respond within a few hours.'],
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
  readonly previewPage = signal<PreviewPage>('home');

  readonly isShortletPreview = computed(() => {
    const profileType = this.profile()?.businessType;
    if (profileType) {
      return profileType === 'Shortlet';
    }
    return this.biz.businessType() === 'Shortlet';
  });

  readonly shortletPreviewPage = computed(() => this.previewPage() as ShortletPreviewPage);
  readonly hotelPreviewPage = computed(() => this.previewPage() as GuestPreviewPage);

  readonly previewPageLabel = computed(() => {
    if (this.isShortletPreview()) {
      switch (this.previewPage()) {
        case 'listings':
          return 'Listings page';
        case 'amenities':
          return 'Amenities page';
        case 'host':
          return 'Host page';
        default:
          return 'Home page';
      }
    }
    switch (this.previewPage()) {
      case 'rooms':
        return 'Rooms page';
      case 'facilities':
        return 'Facilities page';
      default:
        return 'Home page';
    }
  });

  readonly liveUrl = computed(() => {
    const slug = this.profile()?.slug;
    return slug ? buildStorefrontUrl(slug) : null;
  });

  ngOnInit(): void {
    this.orgLocation.hydrateFromStorage();
    this.biz.ensureLoaded();
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
    this.syncPreviewPageFromTab(tab);
  }

  setPreviewPage(page: PreviewPage): void {
    this.previewPage.set(page);
  }

  private syncPreviewPageFromTab(tab: DesignerTab): void {
    if (this.isShortletPreview()) {
      if (tab === 'rooms') {
        this.previewPage.set('listings');
      } else if (tab === 'facilities') {
        this.previewPage.set('amenities');
      } else if (tab === 'about') {
        this.previewPage.set('host');
      } else {
        this.previewPage.set('home');
      }
      return;
    }
    if (tab === 'rooms') {
      this.previewPage.set('rooms');
    } else if (tab === 'facilities') {
      this.previewPage.set('facilities');
    } else if (tab === 'about' || tab === 'banner' || tab === 'global') {
      this.previewPage.set('home');
    }
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
          if (profileRes.data.businessType) {
            this.biz.setType(profileRes.data.businessType);
          } else {
            this.biz.ensureLoaded();
          }
          this.loadBusinessAmenities();
          this.loadBannerImages();
          this.loadAboutImage();
          this.loadSocialProfile();

          const slug = profileRes.data.slug;
          this.locationsApi.listLocations().subscribe({
            next: (locRes) => {
              if (locRes.success && locRes.data) {
                const allowed = this.orgLocation.filterLocations(locRes.data);
                this.locations.set(allowed);
                if (allowed.length > 0) {
                  const preferred = this.orgLocation.primaryLocationId(allowed);
                  this.bannerBranchId.set(this.bannerBranchId() ?? preferred ?? allowed[0].id);
                }
              }
              this.themeApi.getTheme().subscribe({
                next: (themeRes) => {
                  const theme =
                    themeRes.success && themeRes.data
                      ? themeRes.data
                      : this.defaultThemeForBusiness(profileRes.data!.businessName);
                  this.patchTheme(theme);
                  if (slug) {
                    this.loadCatalog(slug);
                  } else {
                    this.refreshPreview();
                  }
                },
              });
            },
            error: () => {
              this.themeApi.getTheme().subscribe({
                next: (themeRes) => {
                  const theme =
                    themeRes.success && themeRes.data
                      ? themeRes.data
                      : this.defaultThemeForBusiness(profileRes.data!.businessName);
                  this.patchTheme(theme);
                  if (slug) {
                    this.loadCatalog(slug);
                  } else {
                    this.refreshPreview();
                  }
                },
              });
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

    const theme = this.buildThemeFromForm();
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
          this.refreshPreview();
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
    this.patchTheme(this.defaultThemeForBusiness(name));
    this.toast.info('Defaults loaded — save to apply.', 'Storefront');
  }

  selectColorPreset(preset: string): void {
    this.form.controls.colors.controls.preset.setValue(preset as StorefrontTheme['colors']['preset']);
  }

  get aboutStatsArray(): FormArray<FormGroup> {
    return this.form.controls.about.controls.stats;
  }

  get perksArray(): FormArray<FormGroup> {
    return this.form.controls.facilities.controls.perksItems;
  }

  addAboutStat(): void {
    if (this.aboutStatsArray.length >= MAX_ABOUT_STATS) {
      return;
    }
    this.aboutStatsArray.push(
      this.fb.nonNullable.group({
        num: [''],
        label: [''],
      }),
    );
  }

  removeAboutStat(index: number): void {
    if (index >= 0 && index < this.aboutStatsArray.length) {
      this.aboutStatsArray.removeAt(index);
    }
  }

  setAboutLayout(layout: StorefrontTheme['about']['layout']): void {
    this.form.controls.about.controls.layout.setValue(layout);
    this.refreshPreview();
  }

  addPerk(): void {
    if (this.perksArray.length >= MAX_FACILITY_PERKS) {
      return;
    }
    this.perksArray.push(this.fb.nonNullable.group({ text: [''] }));
  }

  removePerk(index: number): void {
    if (index >= 0 && index < this.perksArray.length) {
      this.perksArray.removeAt(index);
    }
  }

  onBannerFilesSelected(event: Event): void {
    const branchId = this.bannerBranchId();
    if (!branchId) {
      this.toast.warning('Select a branch before uploading banner photos.', 'Banner');
      return;
    }

    const remaining = this.bannerSlotsRemaining();
    if (remaining <= 0) {
      this.toast.warning(`You can upload up to ${MAX_BANNER_IMAGES} banner images.`, 'Banner');
      return;
    }

    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (picked.length === 0) {
      return;
    }

    const { accepted, skipped } = filterAllowedImageFiles(picked.slice(0, remaining));
    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Banner');
    }
    if (accepted.length === 0) {
      return;
    }

    this.uploadingBanner.set(true);
    this.bannerApi
      .uploadImages(accepted, branchId)
      .pipe(finalize(() => this.uploadingBanner.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Banner');
            return;
          }

          const branchId = this.bannerBranchId();
          this.bannerImages.update((items) => {
            const others = branchId
              ? items.filter((i) => i.locationId !== branchId)
              : items;
            return [...others, ...res.data!];
          });
          this.refreshPreview();
          this.toast.success(res.message ?? 'Banner photo(s) uploaded.', 'Banner');
        },
        error: () => this.toast.error('Could not upload banner photos.', 'Banner'),
      });
  }

  removeBannerImage(image: StorefrontBannerImageDto): void {
    this.uploadingBanner.set(true);
    this.bannerApi
      .deleteImage(image.id)
      .pipe(finalize(() => this.uploadingBanner.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Banner');
            return;
          }
          this.bannerImages.update((items) => items.filter((i) => i.id !== image.id));
          this.refreshPreview();
          this.toast.success('Banner photo removed.', 'Banner');
        },
        error: () => this.toast.error('Could not remove banner photo.', 'Banner'),
      });
  }

  private loadBannerImages(): void {
    this.bannerApi.listImages().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.bannerImages.set(res.data);
          this.refreshPreview();
        }
      },
    });
  }

  private loadBusinessAmenities(): void {
    this.amenitiesApi.listAmenities().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.businessAmenities.set(res.data);
          this.refreshPreview();
        }
      },
    });
  }

  private loadCatalog(slug: string): void {
    const locationId = this.bannerBranchId();
    this.themeApi.getPublicStorefront(slug, locationId).subscribe({
      next: (pub) => {
        if (pub.success && pub.data) {
          this.catalog.set({
            ...pub.data,
            heroImages: pub.data.heroImages ?? [],
            aboutImageUrl: pub.data.aboutImageUrl ?? null,
            amenities: pub.data.amenities ?? [],
          });
        }
        this.refreshPreview();
      },
      error: () => this.refreshPreview(),
    });
  }

  setBannerBranch(locationId: string): void {
    this.bannerBranchId.set(locationId);
    const slug = this.profile()?.slug;
    if (slug) {
      this.loadCatalog(slug);
    } else {
      this.refreshPreview();
    }
  }

  onAboutFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const picked = input.files?.[0];
    input.value = '';
    if (!picked) {
      return;
    }

    const { accepted, skipped } = filterAllowedImageFiles([picked]);
    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'About us');
    }
    if (accepted.length === 0) {
      return;
    }

    this.uploadingAbout.set(true);
    this.aboutApi
      .uploadImage(accepted[0])
      .pipe(finalize(() => this.uploadingAbout.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'About us');
            return;
          }
          this.aboutImage.set(res.data);
          this.refreshPreview();
          this.toast.success('About photo uploaded.', 'About us');
        },
        error: () => this.toast.error('Could not upload about photo.', 'About us'),
      });
  }

  removeAboutImage(): void {
    this.uploadingAbout.set(true);
    this.aboutApi
      .deleteImage()
      .pipe(finalize(() => this.uploadingAbout.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'About us');
            return;
          }
          this.aboutImage.set(null);
          this.refreshPreview();
          this.toast.success('About photo removed.', 'About us');
        },
        error: () => this.toast.error('Could not remove about photo.', 'About us'),
      });
  }

  private loadAboutImage(): void {
    this.aboutApi.getImage().subscribe({
      next: (res) => {
        if (res.success) {
          this.aboutImage.set(res.data ?? null);
          this.refreshPreview();
        }
      },
    });
  }

  private loadSocialProfile(): void {
    this.socialApi.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.socialProfile.set(res.data);
          this.refreshPreview();
        }
      },
    });
  }

  private patchTheme(theme: StorefrontTheme): void {
    const defaults = this.defaultThemeForBusiness(this.profile()?.businessName ?? 'Your hotel');
    const merged: StorefrontTheme = {
      ...defaults,
      ...theme,
      banner: { ...defaults.banner, ...theme.banner },
      about: {
        ...defaults.about,
        ...theme.about,
        stats: theme.about.stats ?? defaults.about.stats,
      },
      rooms: { ...defaults.rooms, ...theme.rooms },
      facilities: {
        ...defaults.facilities,
        ...theme.facilities,
        perksItems: theme.facilities.perksItems ?? defaults.facilities.perksItems,
      },
      footer: { ...defaults.footer, ...theme.footer },
      contact: { ...defaults.contact, ...theme.contact },
      colors: { ...defaults.colors, ...theme.colors },
    };

    const { stats, ...aboutFields } = merged.about;
    const { perksItems, ...facilitiesFields } = merged.facilities;
    this.form.patchValue({ ...merged, about: aboutFields, facilities: facilitiesFields }, { emitEvent: true });
    this.rebuildAboutStatsForm(stats);
    this.rebuildPerksForm(perksItems);
    this.refreshPreview();
  }

  private rebuildPerksForm(items: string[]): void {
    const arr = this.perksArray;
    while (arr.length > 0) {
      arr.removeAt(0);
    }
    for (const text of items) {
      arr.push(this.fb.nonNullable.group({ text: [text] }));
    }
  }

  private rebuildAboutStatsForm(stats: StorefrontAboutStat[]): void {
    const arr = this.aboutStatsArray;
    while (arr.length > 0) {
      arr.removeAt(0);
    }
    for (const stat of stats) {
      arr.push(
        this.fb.nonNullable.group({
          num: [stat.num],
          label: [stat.label],
        }),
      );
    }
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

    const theme = this.buildThemeFromForm();
    const catalog = this.catalog();
    const social = this.socialProfile();
    const previewLocationId = this.bannerBranchId();

    const rooms = (catalog?.rooms ?? []).filter(
      (r) => !previewLocationId || !r.locationId || r.locationId === previewLocationId,
    );
    const facilities = (catalog?.facilities ?? []).filter(
      (f) => !previewLocationId || !f.locationId || f.locationId === previewLocationId,
    );
    const heroImages = this.bannerImagesForBranch().map((i) => i.url);
    const catalogAmenities = catalog?.amenities ?? [];
    const amenities =
      catalogAmenities.length > 0
        ? catalogAmenities
        : this.businessAmenities().map((a) => ({
            id: a.id,
            name: a.name,
            category: a.category,
          }));

    this.previewStorefront.set({
      businessId: profile.id,
      businessName: profile.businessName,
      businessType: profile.businessType ?? this.biz.businessType(),
      slug: profile.slug ?? 'preview',
      logoUrl: profile.logoUrl,
      theme,
      heroImages,
      aboutImageUrl: this.aboutImage()?.url ?? null,
      rooms,
      facilities,
      amenities,
      locations: catalog?.locations ?? this.locations().map((l) => ({ id: l.id, name: l.name, address: l.address ?? null })),
      requiresBranchSelection: false,
      activeLocationId: previewLocationId,
      social: {
        ...social,
        contactEmail: social.contactEmail || profile.contactEmail || null,
        contactPhone: social.contactPhone || profile.phoneNumber || null,
      },
    });
  }

  private defaultThemeForBusiness(businessName: string): StorefrontTheme {
    const profileType = this.profile()?.businessType ?? this.biz.businessType();
    return profileType === 'Shortlet'
      ? createShortletDefaultTheme(businessName)
      : createDefaultTheme(businessName);
  }

  private buildThemeFromForm(): StorefrontTheme {
    const raw = this.form.getRawValue();
    const perksRaw = this.perksArray.getRawValue() as { text: string }[];
    const { perksItems: _perks, ...facilitiesFields } = raw.facilities;
    const { stats: _stats, ...aboutFields } = raw.about;
    return {
      globalFont: raw.globalFont,
      banner: raw.banner,
      about: {
        ...(aboutFields as StorefrontTheme['about']),
        stats: this.aboutStatsArray.getRawValue() as StorefrontAboutStat[],
      },
      rooms: raw.rooms,
      facilities: {
        ...(facilitiesFields as StorefrontTheme['facilities']),
        perksItems: perksRaw.map((p) => p.text.trim()).filter(Boolean),
      },
      footer: raw.footer,
      contact: raw.contact,
      colors: raw.colors,
    };
  }
}
