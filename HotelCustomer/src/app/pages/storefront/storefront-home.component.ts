import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { resolveSectionFont } from '../../core/data/storefront-theme-presets';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';
import { RoomCarouselSlideComponent } from '../../shared/hotel-storefront/room-carousel-slide.component';

@Component({
  selector: 'app-storefront-home',
  standalone: true,
  imports: [RouterLink, RoomCarouselSlideComponent, HotelFooterComponent],
  templateUrl: './storefront-home.component.html',
  styleUrl: './storefront-home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontHomeComponent implements OnInit, OnDestroy {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroIndex = signal(0);
  private heroTimer?: ReturnType<typeof setInterval>;

  readonly roomIndex = signal(0);
  readonly viewedRoomIds = signal<Set<string>>(new Set());

  readonly heroBg = computed(() => {
    const images = this.storefront().heroImages;
    if (images.length === 0) {
      return null;
    }
    return images[this.heroIndex() % images.length];
  });

  readonly allRooms = computed(() => this.storefront().rooms);

  readonly allRoomsViewed = computed(() => {
    const rooms = this.allRooms();
    const viewed = this.viewedRoomIds();
    return rooms.length > 0 && rooms.every((r) => viewed.has(r.id));
  });

  readonly roomsRemaining = computed(() => {
    const rooms = this.allRooms();
    const viewed = this.viewedRoomIds();
    return rooms.filter((r) => !viewed.has(r.id)).length;
  });

  readonly carouselProgress = computed(() => {
    const total = this.allRooms().length;
    if (total === 0) {
      return '0 / 0';
    }
    return `${this.roomIndex() + 1} / ${total}`;
  });

  readonly socialLinks = computed(() =>
    this.storefront().socialLinks.filter((l) => l.platform !== 'WhatsApp'),
  );

  readonly hasSocial = computed(() => this.socialLinks().length > 0);

  readonly starDisplay = computed(() => '★'.repeat(this.storefront().stars));

  readonly aboutTeaser = computed(() => {
    const sf = this.storefront();
    const text = sf.aboutStory[0] ?? sf.theme.about.description?.trim() ?? '';
    if (!text) {
      return '';
    }
    if (text.length <= 200) {
      return text;
    }
    return `${text.slice(0, 197).trim()}…`;
  });

  readonly bannerHeadlineFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.banner.headlineFont),
  );

  readonly bannerSubheadlineFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.banner.subheadlineFont),
  );

  readonly roomsTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.rooms.titleFont),
  );

  readonly aboutTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.titleFont),
  );

  readonly aboutBodyFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.bodyFont),
  );

  ngOnInit(): void {
    const images = this.storefront().heroImages;
    if (images.length > 1) {
      this.heroTimer = setInterval(() => {
        this.heroIndex.update((i) => (i + 1) % images.length);
      }, 6000);
    }

    const firstRoom = this.allRooms()[0];
    if (firstRoom) {
      this.markRoomViewed(firstRoom.id);
    }
  }

  ngOnDestroy(): void {
    if (this.heroTimer) {
      clearInterval(this.heroTimer);
    }
  }

  openBooking(): void {
    this.ui.openBooking();
  }

  goToRoom(index: number): void {
    const rooms = this.allRooms();
    if (index < 0 || index >= rooms.length) {
      return;
    }
    this.roomIndex.set(index);
    this.markRoomViewed(rooms[index].id);
  }

  nextRoom(): void {
    const rooms = this.allRooms();
    if (rooms.length === 0) {
      return;
    }
    const next = (this.roomIndex() + 1) % rooms.length;
    this.goToRoom(next);
  }

  prevRoom(): void {
    const rooms = this.allRooms();
    if (rooms.length === 0) {
      return;
    }
    const prev = (this.roomIndex() - 1 + rooms.length) % rooms.length;
    this.goToRoom(prev);
  }

  isRoomViewed(roomId: string): boolean {
    return this.viewedRoomIds().has(roomId);
  }

  private markRoomViewed(roomId: string): void {
    this.viewedRoomIds.update((ids) => {
      const next = new Set(ids);
      next.add(roomId);
      return next;
    });
  }
}
