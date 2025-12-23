import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { KeycloakService } from 'keycloak-angular';
import { map } from 'rxjs/operators';

import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ScheduleComponent } from './components/schedule/schedule.component';
import { AppointmentsComponent } from './components/appointments/appointments.component';
import { AuthService } from './services/auth.service';

const authGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const keycloak = inject(KeycloakService);
  const router: Router = inject(Router);

  const logged = await keycloak.isLoggedIn();
  if (logged) {
    if (!authService.currentUser()) {
      await authService.syncUserFromKeycloak();
    }
    return true;
  }

  await keycloak.login({ redirectUri: window.location.origin + '/#/dashboard' });
  return false;
};

export const APP_ROUTES: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'schedule', component: ScheduleComponent, canActivate: [authGuard] },
  { path: 'appointments', component: AppointmentsComponent, canActivate: [authGuard] },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '/dashboard' }
];