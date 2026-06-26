import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { StorefrontEntryService } from '../../core/services/storefront-entry.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { hotelThemeStyle } from '../../core/utils/hotel-theme';
import { shortletThemeStyle } from '../../core/utils/shortlet-theme';
import type { ShowcaseLocation } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-storefront-branch-gateway',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './storefront-branch-gateway.component.html',
  styleUrl: './storefront-branch-gateway.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontBranchGatewayComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly entry = inject(StorefrontEntryService);
  readonly hotelCtx = inject(StorefrontContextService);
  readonly shortletCtx = inject(ShortletContextService);

  private sub?: Subscription;
  slug = '';
  branches: ShowcaseLocation[] = [];
  businessName = '';
  isShortlet = false;

  readonly themeStyle = computed(() => {
    if (this.entry.kind() === 'shortlet') {
      const sl = this.shortletCtx.shortlet();
      return sl ? shortletThemeStyle(sl.theme) : {};
    }
    const sf = this.hotelCtx.storefront();
    return sf ? hotelThemeStyle(sf.theme) : {};
  });

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug') ?? '';
      this.slug = slug;
      this.entry.load(slug).subscribe((kind) => {
        if (!kind) {
          return;
        }

        this.isShortlet = kind === 'shortlet';
        this.businessName = this.entry.businessName();
        this.branches = this.entry.branchLocations();

        if (!this.entry.requiresBranchSelection()) {
          const targetId = this.entry.activeLocationId() ?? this.branches[0]?.id ?? 'default';
          void this.router.navigate(['/', slug, 'l', targetId]);
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
