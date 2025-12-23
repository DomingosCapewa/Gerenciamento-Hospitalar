
import { Injectable, signal, inject } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';
import { User } from '../models/types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private keycloak = inject(KeycloakService);
  currentUser = signal<User | null>(null);

  constructor() {
    this.initializeFromSession();
  }

  async initializeFromSession(): Promise<void> {
    try {
      const logged = await this.keycloak.isLoggedIn();
      if (logged) {
        await this.syncUserFromKeycloak();
      } else {
        this.currentUser.set(null);
      }
    } catch {
      this.currentUser.set(null);
    }
  }

  private mapRoleToApp(role: string): User['role'] | null {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'Admin';
      case 'medico':
      case 'médico':
      case 'doctor':
        return 'Médico';
      case 'usuario':
      case 'usuário':
      case 'user':
        return 'Usuário';
      default:
        return null;
    }
  }

  async syncUserFromKeycloak(): Promise<void> {
    const profile = await this.keycloak.loadUserProfile();
    const roles = this.keycloak.getUserRoles(true);
    const priority = ['Admin', 'Médico', 'Usuário'] as const;
    const mappedRoles = roles.map(r => this.mapRoleToApp(r)).filter((r): r is User['role'] => !!r);
    const selectedRole = priority.find(p => mappedRoles.includes(p)) ?? mappedRoles[0] ?? 'Usuário';
    const token = await this.keycloak.getToken();

    const user: User = {
      id: profile.id ?? profile.username ?? 'unknown',
      name: `${profile.firstName ?? ''} ${profile.lastName ?? ''}`.trim() || (profile.username ?? 'Usuário'),
      cpf: '',
      role: selectedRole,
    };
    this.currentUser.set(user);
  }

  async login(): Promise<void> {
    await this.keycloak.login({ redirectUri: window.location.origin + '/#/dashboard' });
  }

  async register(): Promise<void> {
    await this.keycloak.register({ redirectUri: window.location.origin + '/#/dashboard' });
  }

  async logout(): Promise<void> {
    this.currentUser.set(null);
    await this.keycloak.logout(window.location.origin + '/#/login');
  }

  async getToken(): Promise<string | null> {
    try {
      const token = await this.keycloak.getToken();
      return token ?? null;
    } catch {
      return null;
    }
  }
}
