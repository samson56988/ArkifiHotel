import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { hotelThemeStyle } from '../../core/utils/hotel-theme';
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
  readonly ctx = inject(StorefrontContextService);

  private sub?: Subscription;
  slug = '';
  branches: ShowcaseLocation[] = [];
  businessName = '';
  themeStyle: Record<string, string> = {};

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug') ?? '';
      this.slug = slug;
      this.ctx.load(slug).subscribe((data) => {
        if (!data) {
          return;
        }

        this.businessName = data.businessName;
        this.themeStyle = hotelThemeStyle(data.theme);
        this.branches = data.locations;

        if (!data.requiresBranchSelection) {
          const targetId = data.activeLocationId ?? data.locations[0]?.id ?? 'default';
          void this.router.navigate(['/', slug, 'l', targetId]);
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
