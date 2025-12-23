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
    { name: 'Atendimento', href: '/doctor/attend', icon: 'medkit-outline' },
    { name: 'Procedimentos', href: '/clinica/procedures', icon: 'flask-outline' },
  ];

  adminNav = [
    ...this.doctorNav,
  ];

  navigation = computed(() => {
    const role = this.currentUser()?.role;
    console.log('🔍 Current role:', role);
    console.log('🔍 Current user:', this.currentUser());
    console.log('🔍 Doctor nav items:', this.doctorNav);
    switch (role) {
      case 'Admin':
        console.log('✅ Returning adminNav');
        return this.adminNav;
      case 'Medico':
        console.log('✅ Returning doctorNav');
        return this.doctorNav;
      case 'Usuario':
        console.log('✅ Returning userNav');
        return this.userNav;
      default:
        console.log('❌ No role matched, returning empty array');
        return [];
    }
  });

  logout(): void {
    this.authService.logout();
  }
}