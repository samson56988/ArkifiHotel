import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { collectGalleryImages } from '../../core/utils/gallery-images';
import { FacilityCardComponent } from '../../shared/hotel-storefront/facility-card.component';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';

@Component({
  selector: 'app-storefront-facilities',
  standalone: true,
  imports: [RouterLink, FacilityCardComponent, HotelFooterComponent],
  templateUrl: './storefront-facilities.component.html',
  styleUrl: './storefront-facilities.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontFacilitiesComponent implements OnInit, OnDestroy {
  readonly ctx = inject(StorefrontContextService);

  readonly heroIndex = signal(0);
  private heroTimer?: ReturnType<typeof setInterval>;

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroSlides = computed(() => {
    const fromFacilities = collectGalleryImages(this.storefront().facilities);
    if (fromFacilities.length > 0) {
      return fromFacilities;
    }
    const sf = this.storefront();
    return sf.galleryImages.length > 0 ? sf.galleryImages.slice(0, 4) : sf.heroImages;
  });

  readonly allFacilities = computed(() => this.storefront().facilities);

  ngOnInit(): void {
    const slides = this.heroSlides();
    if (slides.length > 1) {
      this.heroTimer = setInterval(() => {
        this.heroIndex.update((i) => (i + 1) % slides.length);
      }, 5000);
    }
  }

  ngOnDestroy(): void {
    if (this.heroTimer) {
      clearInterval(this.heroTimer);
    }
  }
}
