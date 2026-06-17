import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  OnDestroy,
  signal,
} from '@angular/core';
import type {
  PublicStorefront,
  PublicStorefrontFacility,
  PublicStorefrontRoom,
} from '../../core/models/storefront-theme.models';
import { resolveSectionFont } from '../../core/data/storefront-theme-presets';
import { collectGalleryImages, galleryImages } from '../../core/utils/gallery-images';
import { hotelThemeStyle, formatNaira, facilityEmoji } from '../../core/utils/hotel-theme';
import { buildPreviewSocialLinks } from '../../core/utils/storefront-preview';
import { PreviewRoomSlideComponent } from './preview-room-slide.component';

export type GuestPreviewPage = 'home' | 'rooms' | 'facilities';

@Component({
  selector: 'app-storefront-guest-preview',
  standalone: true,
  imports: [PreviewRoomSlideComponent],
  templateUrl: './storefront-guest-preview.component.html',
  styleUrl: './storefront-guest-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontGuestPreviewComponent implements OnDestroy {
  readonly storefront = input.required<PublicStorefront>();
  readonly page = input<GuestPreviewPage>('home');

  readonly heroIndex = signal(0);
  readonly roomIndex = signal(0);
  readonly pageHeroIndex = signal(0);

  readonly galleryOpen = signal(false);
  readonly galleryTitle = signal('');
  readonly galleryImages = signal<string[]>([]);
  readonly gallerySlideIndex = signal(0);
  readonly gallerySubtitle = signal('');

  private heroTimer?: ReturnType<typeof setInterval>;
  private pageHeroTimer?: ReturnType<typeof setInterval>;

  readonly themeStyle = computed(() => hotelThemeStyle(this.storefront().theme));

  readonly heroImages = computed(() => this.storefront().heroImages ?? []);

  readonly socialLinks = computed(() =>
    buildPreviewSocialLinks(this.storefront().social).filter((l) => l.platform !== 'WhatsApp'),
  );

  readonly hasSocial = computed(() => this.socialLinks().length > 0);

  readonly aboutParagraphs = computed(() => {
    const text = this.storefront().theme.about.description;
    if (!text?.trim()) {
      return [];
    }
    return text
      .split(/\n\s*\n/)
      .map((p) => p.trim())
      .filter(Boolean);
  });

  readonly aboutLayout = computed(() => this.storefront().theme.about.layout);

  readonly aboutTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.titleFont),
  );

  readonly aboutBodyFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.bodyFont),
  );

  readonly heroBg = computed(() => {
    const images = this.heroImages();
    if (images.length === 0) {
      return null;
    }
    return images[this.heroIndex() % images.length];
  });

  readonly carouselProgress = computed(() => {
    const total = this.storefront().rooms.length;
    if (total === 0) {
      return '0 / 0';
    }
    return `${this.roomIndex() + 1} / ${total}`;
  });

  readonly roomHeroSlides = computed(() => {
    const fromRooms = collectGalleryImages(this.storefront().rooms);
    if (fromRooms.length > 0) {
      return fromRooms;
    }
    return this.heroImages().slice(0, 3);
  });

  readonly facilityHeroSlides = computed(() => {
    const fromFacilities = collectGalleryImages(this.storefront().facilities);
    if (fromFacilities.length > 0) {
      return fromFacilities;
    }
    return this.heroImages().slice(0, 3);
  });

  readonly pageHeroSlides = computed(() => {
    if (this.page() === 'facilities') {
      return this.facilityHeroSlides();
    }
    if (this.page() === 'rooms') {
      return this.roomHeroSlides();
    }
    return [];
  });

  readonly formatPrice = formatNaira;
  readonly facilityEmoji = facilityEmoji;

  constructor() {
    effect(() => {
      const images = this.heroImages();
      this.heroIndex.set(0);
      this.stopHeroCarousel();
      if (images.length > 1) {
        this.heroTimer = setInterval(() => {
          this.heroIndex.update((i) => (i + 1) % images.length);
        }, 6000);
      }
    });

    effect(() => {
      const page = this.page();
      const slides = this.pageHeroSlides();
      this.pageHeroIndex.set(0);
      this.stopPageHeroCarousel();
      if ((page === 'rooms' || page === 'facilities') && slides.length > 1) {
        this.pageHeroTimer = setInterval(() => {
          this.pageHeroIndex.update((i) => (i + 1) % slides.length);
        }, 5000);
      }
    });
  }

  ngOnDestroy(): void {
    this.stopHeroCarousel();
    this.stopPageHeroCarousel();
  }

  prevRoom(): void {
    const rooms = this.storefront().rooms;
    if (rooms.length === 0) {
      return;
    }
    this.roomIndex.update((i) => (i - 1 + rooms.length) % rooms.length);
  }

  nextRoom(): void {
    const rooms = this.storefront().rooms;
    if (rooms.length === 0) {
      return;
    }
    this.roomIndex.update((i) => (i + 1) % rooms.length);
  }

  goToRoom(index: number): void {
    const rooms = this.storefront().rooms;
    if (index >= 0 && index < rooms.length) {
      this.roomIndex.set(index);
    }
  }

  openRoomGallery(room: PublicStorefrontRoom): void {
    const images = galleryImages(room);
    this.galleryTitle.set(room.name);
    this.gallerySubtitle.set(
      this.storefront().theme.rooms.showPrice
        ? `${formatNaira(room.basePricePerNight)} / night · ${room.maxOccupancy} guests`
        : `${room.maxOccupancy} guests`,
    );
    this.galleryImages.set(images);
    this.gallerySlideIndex.set(0);
    this.galleryOpen.set(true);
  }

  openFacilityGallery(facility: PublicStorefrontFacility): void {
    const images = galleryImages(facility);
    this.galleryTitle.set(facility.name);
    this.gallerySubtitle.set(facility.locationName ?? '');
    this.galleryImages.set(images);
    this.gallerySlideIndex.set(0);
    this.galleryOpen.set(true);
  }

  closeGallery(): void {
    this.galleryOpen.set(false);
    this.galleryImages.set([]);
    this.gallerySlideIndex.set(0);
  }

  prevGallerySlide(): void {
    const total = this.galleryImages().length;
    if (total <= 1) {
      return;
    }
    this.gallerySlideIndex.update((i) => (i - 1 + total) % total);
  }

  nextGallerySlide(): void {
    const total = this.galleryImages().length;
    if (total <= 1) {
      return;
    }
    this.gallerySlideIndex.update((i) => (i + 1) % total);
  }

  goToGallerySlide(index: number): void {
    this.gallerySlideIndex.set(index);
  }

  private stopHeroCarousel(): void {
    if (this.heroTimer) {
      clearInterval(this.heroTimer);
      this.heroTimer = undefined;
    }
  }

  private stopPageHeroCarousel(): void {
    if (this.pageHeroTimer) {
      clearInterval(this.pageHeroTimer);
      this.pageHeroTimer = undefined;
    }
  }
}
