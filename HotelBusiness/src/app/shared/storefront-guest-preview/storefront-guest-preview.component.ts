import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  OnDestroy,
  signal,
} from '@angular/core';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';
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
  private heroTimer?: ReturnType<typeof setInterval>;

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

  readonly pageHeroImage = computed(() => this.heroImages()[0] ?? null);

  readonly roomPageStats = computed(() => {
    const rooms = this.storefront().rooms;
    const prices = rooms.map((r) => r.basePricePerNight);
    return {
      total: rooms.length,
      available: rooms.length,
      minPrice: prices.length ? Math.min(...prices) : 0,
    };
  });

  readonly featuredRoom = computed(() => this.storefront().rooms[0] ?? null);

  readonly gridRooms = computed(() => {
    const rooms = this.storefront().rooms;
    const featured = this.featuredRoom();
    if (!this.storefront().theme.rooms.showFeaturedSection || !featured) {
      return rooms.slice(0, 4);
    }
    return rooms.filter((r) => r.id !== featured.id).slice(0, 4);
  });

  readonly featuredFacility = computed(() => this.storefront().facilities[0] ?? null);

  readonly gridFacilities = computed(() => this.storefront().facilities.slice(0, 6));

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
  }

  ngOnDestroy(): void {
    this.stopHeroCarousel();
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

  private stopHeroCarousel(): void {
    if (this.heroTimer) {
      clearInterval(this.heroTimer);
      this.heroTimer = undefined;
    }
  }
}
