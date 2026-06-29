import { Injectable, signal } from '@angular/core';
import type { ApiResult } from '../models/api-result.model';
import { getApiResultMessage } from '../utils/http-api-result';
import { createClientId } from '../utils/client-id';

export type ToastVariant = 'success' | 'error' | 'info' | 'warning';

export interface ToastItem {
  id: string;
  message: string;
  title?: string;
  variant: ToastVariant;
}

const DEFAULT_DURATION: Record<ToastVariant, number> = {
  success: 4200,
  info: 4800,
  warning: 5600,
  error: 7800,
};

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _items = signal<ToastItem[]>([]);
  readonly items = this._items.asReadonly();
  private readonly timers = new Map<string, ReturnType<typeof setTimeout>>();

  success(message: string, title?: string): void {
    this.show(message, 'success', title);
  }

  error(message: string, title?: string): void {
    this.show(message, 'error', title);
  }

  info(message: string, title?: string): void {
    this.show(message, 'info', title);
  }

  showFailedApi(result: Pick<ApiResult<unknown>, 'message' | 'validationErrors'>, title?: string): void {
    this.error(getApiResultMessage(result), title ?? 'Something went wrong');
  }

  dismiss(id: string): void {
    const t = this.timers.get(id);
    if (t) {
      clearTimeout(t);
      this.timers.delete(id);
    }
    this._items.update((list) => list.filter((item) => item.id !== id));
  }

  private show(message: string, variant: ToastVariant, title?: string): void {
    const id = createClientId();
    this._items.update((list) => [...list.slice(-4), { id, message, title, variant }]);
    const timer = setTimeout(() => this.dismiss(id), DEFAULT_DURATION[variant]);
    this.timers.set(id, timer);
  }
}
