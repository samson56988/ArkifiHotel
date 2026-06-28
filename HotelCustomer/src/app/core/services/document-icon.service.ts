import { Injectable } from '@angular/core';
import { PropertyIconInput, resolvePropertyIconUrl } from '../utils/property-icon';

@Injectable({ providedIn: 'root' })
export class DocumentIconService {
  private readonly defaultHref = 'favicon.svg';
  private readonly defaultType = 'image/svg+xml';
  private readonly linkId = 'app-document-icon';

  applyPropertyIcon(input: PropertyIconInput): void {
    const url = resolvePropertyIconUrl(input);
    const link = this.ensureLink();
    link.href = url;
    link.type = this.guessMimeType(url);
  }

  resetToDefault(): void {
    const link = this.ensureLink();
    link.href = this.defaultHref;
    link.type = this.defaultType;
  }

  private ensureLink(): HTMLLinkElement {
    this.removeExtraIconLinks();

    let link = document.getElementById(this.linkId) as HTMLLinkElement | null;
    if (!link) {
      link = document.createElement('link');
      link.id = this.linkId;
      link.rel = 'icon';
      document.head.appendChild(link);
    }

    return link;
  }

  private removeExtraIconLinks(): void {
    document.querySelectorAll('link[rel~="icon"]').forEach((node) => {
      if (node.id !== this.linkId) {
        node.remove();
      }
    });
  }

  private guessMimeType(url: string): string {
    if (url.startsWith('data:image/svg+xml')) {
      return 'image/svg+xml';
    }

    const path = url.split('?')[0]?.toLowerCase() ?? '';
    if (path.endsWith('.svg')) {
      return 'image/svg+xml';
    }
    if (path.endsWith('.png')) {
      return 'image/png';
    }
    if (path.endsWith('.webp')) {
      return 'image/webp';
    }
    if (path.endsWith('.jpg') || path.endsWith('.jpeg')) {
      return 'image/jpeg';
    }
    if (path.endsWith('.ico')) {
      return 'image/x-icon';
    }
    if (url.startsWith('data:image/')) {
      const semi = url.indexOf(';');
      return semi > 5 ? url.slice(5, semi) : 'image/png';
    }
    return 'image/png';
  }
}
