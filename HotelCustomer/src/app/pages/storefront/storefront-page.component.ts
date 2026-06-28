import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';
import { PublicStorefrontApiService } from '../../core/services/public-storefront-api.service';
import { DocumentIconService } from '../../core/services/document-icon.service';
import { StorefrontRendererComponent } from '../../shared/storefront-renderer/storefront-renderer.component';

@Component({
  selector: 'app-storefront-page',
  standalone: true,
  imports: [StorefrontRendererComponent],
  templateUrl: './storefront-page.component.html',
  styleUrl: './storefront-page.component.scss',
})
export class StorefrontPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PublicStorefrontApiService);
  private readonly documentIcon = inject(DocumentIconService);

  readonly storefront = signal<PublicStorefront | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.api.getBySlug(slug).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.storefront.set(res.data);
          document.title = `${res.data.businessName} — Book your stay`;
          this.documentIcon.applyPropertyIcon({
            logoUrl: res.data.logoUrl,
            businessName: res.data.businessName,
            primaryColor: res.data.theme.colors.primary,
            accentColor: res.data.theme.colors.accent,
          });
          return;
        }

        this.notFound.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.notFound.set(true);
      },
    });
  }
}
