
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
        return 'Medico';
      case 'usuario':
      case 'usuário':
      case 'user':
        return 'Usuario';
      default:
        return null;
    }
  }

  async syncUserFromKeycloak(): Promise<void> {
    try { await this.keycloak.updateToken(30); } catch {}
    const instance = this.keycloak.getKeycloakInstance();
    const tokenParsed: any = (instance?.tokenParsed as any) || {};
    const roles = this.keycloak.getUserRoles(true);
    console.log('🟢 Token parsed:', tokenParsed);
    console.log('🟢 Realm roles:', roles);
    const priority = ['Admin', 'Medico', 'Usuario'] as const;
    const mappedRoles = roles.map(r => this.mapRoleToApp(r)).filter((r): r is User['role'] => !!r);
    console.log('🟢 Mapped roles:', mappedRoles);
    const selectedRole = priority.find(p => mappedRoles.includes(p)) ?? mappedRoles[0] ?? 'Usuario';
    console.log('🟢 Selected role:', selectedRole);

    const displayName = tokenParsed.name
      ?? `${tokenParsed.given_name ?? ''} ${tokenParsed.family_name ?? ''}`.trim()
      ?? tokenParsed.preferred_username
      ?? 'Usuario';

    const user: User = {
      id: tokenParsed.sub ?? tokenParsed.preferred_username ?? 'unknown',
      name: displayName,
      cpf: (tokenParsed.preferred_username ?? '').toString(),
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
      console.log('🔑 JWT Token:', token?.substring(0, 50) + '...');
      return token ?? null;
    } catch {
      return null;
    }
  }
}
