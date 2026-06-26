import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ShortletContextService } from '../../core/services/shortlet-context.service';

@Component({
  selector: 'app-shortlet-branch-gateway',
  standalone: true,
  template: `<div class="gate">Finding your apartment…</div>`,
  styles: [
    `
      .gate {
        min-height: 100vh;
        display: grid;
        place-items: center;
        font-family: 'DM Sans', system-ui, sans-serif;
        color: #6b6560;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletBranchGatewayComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly ctx = inject(ShortletContextService);

  private sub?: Subscription;

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(() => {
      const slug = this.resolveSlug();
      if (!slug) return;
      this.ctx.load(slug).subscribe((data) => {
        if (!data) {
          return;
        }
        const targetId = data.activeLocationId ?? data.locations[0]?.id ?? 'default';
        void this.router.navigate(['/', slug, 'l', targetId]);
      });
    });
  }

  private resolveSlug(): string {
    return (
      this.route.snapshot.paramMap.get('slug') ??
      this.route.parent?.snapshot.paramMap.get('slug') ??
      this.route.parent?.snapshot.url[0]?.path ??
      ''
    );
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
