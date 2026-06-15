import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import {
  HOME_FACILITY_PREVIEW_COUNT,
  HOME_ROOM_PREVIEW_COUNT,
  heroImageUrl,
} from '../../core/utils/hotel-theme';
import { FacilityCardComponent } from '../../shared/hotel-storefront/facility-card.component';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';
import { RoomCardComponent } from '../../shared/hotel-storefront/room-card.component';

@Component({
  selector: 'app-storefront-home',
  standalone: true,
  imports: [RouterLink, RoomCardComponent, FacilityCardComponent, HotelFooterComponent],
  templateUrl: './storefront-home.component.html',
  styleUrl: './storefront-home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontHomeComponent {
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroBg = computed(() => heroImageUrl(this.storefront()));

  readonly previewRooms = computed(() =>
    this.storefront().rooms.slice(0, HOME_ROOM_PREVIEW_COUNT),
  );

  readonly previewFacilities = computed(() =>
    this.storefront().facilities.slice(0, HOME_FACILITY_PREVIEW_COUNT),
  );

  readonly hasMoreRooms = computed(() => this.storefront().rooms.length > HOME_ROOM_PREVIEW_COUNT);

  readonly hasMoreFacilities = computed(() =>
    this.storefront().facilities.length > HOME_FACILITY_PREVIEW_COUNT,
  );

  readonly socialLinks = computed(() => {
    const s = this.storefront().social;
    const links: { platform: string; url: string; emoji: string }[] = [];
    if (s.instagramUrl) links.push({ platform: 'Instagram', url: s.instagramUrl, emoji: '📸' });
    if (s.facebookUrl) links.push({ platform: 'Facebook', url: s.facebookUrl, emoji: '👤' });
    if (s.tikTokUrl) links.push({ platform: 'TikTok', url: s.tikTokUrl, emoji: '🎵' });
    if (s.xUrl) links.push({ platform: 'X', url: s.xUrl, emoji: '𝕏' });
    return links;
  });

  readonly hasSocial = computed(() => this.socialLinks().length > 0);
}
