import { Injectable, signal } from '@angular/core';
import type { ApiResult } from '../models/api-result.model';
import { getApiResultMessage } from '../utils/http-api-result';

export type ToastVariant = 'success' | 'error' | 'info' | 'warning';

export interface ToastItem {
  id: string;
  message: string;
  title?: string;
  variant: ToastVariant;
}

export interface ToastShowOptions {
  message: string;
  title?: string;
  variant?: ToastVariant;
  /** Omit or pass `0` to keep the toast until dismissed. */
  durationMs?: number;
}

export interface ToastRef {
  readonly id: string;
  dismiss(): void;
  /** Resolves when the toast is dismissed or auto-closes. */
  afterDismissed(): Promise<void>;
}

const DEFAULT_DURATION: Record<ToastVariant, number> = {
  success: 4200,
  info: 4800,
  warning: 5600,
  error: 7800,
};

const MAX_TOASTS = 5;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _items = signal<ToastItem[]>([]);
  readonly items = this._items.asReadonly();

  private readonly timers = new Map<string, ReturnType<typeof setTimeout>>();
  private readonly waiters = new Map<string, Array<() => void>>();

  success(message: string, title?: string, durationMs?: number): ToastRef {
    return this.show({ message, title, variant: 'success', durationMs });
  }

  error(message: string, title?: string, durationMs?: number): ToastRef {
    return this.show({ message, title, variant: 'error', durationMs });
  }

  info(message: string, title?: string, durationMs?: number): ToastRef {
    return this.show({ message, title, variant: 'info', durationMs });
  }

  warning(message: string, title?: string, durationMs?: number): ToastRef {
    return this.show({ message, title, variant: 'warning', durationMs });
  }

  /** Maps a failed `ApiResult` (or normalized HTTP body) to an error toast. */
  showFailedApi(
    result: Pick<ApiResult<unknown>, 'message' | 'validationErrors'>,
    title?: string,
  ): ToastRef {
    return this.error(getApiResultMessage(result), title ?? 'Something went wrong');
  }

  show(options: ToastShowOptions): ToastRef {
    const variant = options.variant ?? 'info';
    const id = crypto.randomUUID();
    const item: ToastItem = {
      id,
      message: options.message,
      title: options.title,
      variant,
    };

    this._items.update((list) => {
      let next = [...list, item];
      while (next.length > MAX_TOASTS) {
        const removed = next.shift()!;
        this.clearTimer(removed.id);
        this.notifyClosed(removed.id);
      }

      return next;
    });

    const duration = options.durationMs ?? DEFAULT_DURATION[variant];
    if (duration > 0) {
      const t = setTimeout(() => this.dismiss(id), duration);
      this.timers.set(id, t);
    }

    return {
      id,
      dismiss: () => this.dismiss(id),
      afterDismissed: () => this.promiseWhenClosed(id),
    };
  }

  dismiss(id: string): void {
    this.clearTimer(id);
    this._items.update((list) => list.filter((t) => t.id !== id));
    this.notifyClosed(id);
  }

  clearAll(): void {
    for (const t of this.timers.values()) {
      clearTimeout(t);
    }

    this.timers.clear();
    const ids = this._items().map((i) => i.id);
    this._items.set([]);
    for (const id of ids) {
      this.notifyClosed(id);
    }
  }

  private promiseWhenClosed(id: string): Promise<void> {
    return new Promise((resolve) => {
      if (!this._items().some((i) => i.id === id)) {
        queueMicrotask(() => resolve());
        return;
      }

      const list = this.waiters.get(id) ?? [];
      list.push(resolve);
      this.waiters.set(id, list);
    });
  }

  private clearTimer(id: string): void {
    const t = this.timers.get(id);
    if (t) {
      clearTimeout(t);
      this.timers.delete(id);
    }
  }

  private notifyClosed(id: string): void {
    const ws = this.waiters.get(id);
    if (!ws) {
      return;
    }

    this.waiters.delete(id);
    for (const fn of ws) {
      fn();
    }
  }
}
