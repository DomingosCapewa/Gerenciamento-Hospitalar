import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { KeycloakService } from 'keycloak-angular';
import { map } from 'rxjs/operators';

import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ScheduleComponent } from './components/schedule/schedule.component';
import { AppointmentsComponent } from './components/appointments/appointments.component';
import { AttendComponent } from './components/doctor/attend/attend.component';
import { ProceduresComponent } from './components/procedures/procedures.component';
import { PatientRegisterComponent } from './components/patient-register/patient-register.component';
import { MedicoRegisterComponent } from './components/medico-register/medico-register.component';
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
    // Redirecionar para registro se for primeira vez
    const user = authService.currentUser();
    if (user && !user.registrationCompleted) {
      if (user.role === 'Usuario') {
        router.navigate(['/patient-register']);
        return false;
      } else if (user.role === 'Medico') {
        router.navigate(['/medico-register']);
        return false;
      }
    }
    return true;
  }

  await keycloak.login({ redirectUri: window.location.origin + '/#/dashboard' });
  return false;
};

const medicoGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const keycloak = inject(KeycloakService);
  const router: Router = inject(Router);

  const logged = await keycloak.isLoggedIn();
  if (!logged) {
    await keycloak.login({ redirectUri: window.location.origin + '/#/dashboard' });
    return false;
  }

  if (!authService.currentUser()) {
    await authService.syncUserFromKeycloak();
  }

  const role = authService.currentUser()?.role;
  return role === 'Medico' || role === 'Admin';
};

export const APP_ROUTES: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'patient-register', component: PatientRegisterComponent, canActivate: [authGuard] },
  { path: 'medico-register', component: MedicoRegisterComponent, canActivate: [authGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'schedule', component: ScheduleComponent, canActivate: [authGuard] },
  { path: 'appointments', component: AppointmentsComponent, canActivate: [authGuard] },
  { path: 'doctor/attend', component: AttendComponent, canActivate: [medicoGuard] },
  { path: 'clinica/procedures', component: ProceduresComponent, canActivate: [medicoGuard] },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '/dashboard' }
];