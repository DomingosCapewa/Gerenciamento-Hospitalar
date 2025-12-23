
import { Injectable, signal } from '@angular/core';
import { Notification } from '../models/types';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  notification = signal<Notification | null>(null);

  show(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    this.notification.set({ message, type });
    setTimeout(() => this.clear(), 4000);
  }

  clear(): void {
    this.notification.set(null);
  }
}
