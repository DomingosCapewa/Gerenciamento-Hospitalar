
import { Component, ChangeDetectionStrategy, inject, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class NotificationComponent {
  notificationService = inject(NotificationService);
  notification = this.notificationService.notification;

  iconName = computed(() => {
    switch (this.notification()?.type) {
      case 'success':
        return 'checkmark-circle-outline';
      case 'error':
        return 'alert-circle-outline';
      case 'info':
        return 'information-circle-outline';
      default:
        return '';
    }
  });

  bgColor = computed(() => {
    switch (this.notification()?.type) {
      case 'success':
        return 'bg-green-500';
      case 'error':
        return 'bg-red-500';
      case 'info':
      default:
        return 'bg-blue-500';
    }
  });

  close(): void {
    this.notificationService.clear();
  }
}
