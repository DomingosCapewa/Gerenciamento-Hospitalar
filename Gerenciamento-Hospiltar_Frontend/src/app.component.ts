import { Component, ChangeDetectionStrategy, computed, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { NotificationComponent } from './components/notification/notification.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, NotificationComponent, RouterOutlet, RouterLink, RouterLinkActive],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppComponent {
  private authService = inject(AuthService);
  private router: Router = inject(Router);

  currentUser = this.authService.currentUser;
  isLoggedIn = computed(() => !!this.currentUser());

  userNav = [
    { name: 'Dashboard', href: '/dashboard', icon: 'home-outline' },
    { name: 'Agendar', href: '/schedule', icon: 'calendar-outline' },
    { name: 'Meus Agendamentos', href: '/appointments', icon: 'list-outline' },
  ];

  doctorNav = [
    ...this.userNav,
    // Add doctor specific routes here in the future
  ];

  adminNav = [
    ...this.doctorNav,
    // Add admin specific routes here in the future
  ];

  navigation = computed(() => {
    const role = this.currentUser()?.role;
    switch (role) {
      case 'Admin':
        return this.adminNav;
      case 'Médico':
        return this.doctorNav;
      case 'Usuário':
        return this.userNav;
      default:
        return [];
    }
  });

  logout(): void {
    this.authService.logout();
  }
}