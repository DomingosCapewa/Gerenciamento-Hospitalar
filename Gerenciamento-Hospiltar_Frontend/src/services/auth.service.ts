
import { Injectable, signal } from '@angular/core';
import { User } from '../models/types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  currentUser = signal<User | null>(null);

  login(role: 'Usuário' | 'Médico' | 'Admin'): void {
    let user: User;
    switch (role) {
      case 'Admin':
        user = { id: 'admin1', name: 'Admin User', cpf: '000.000.000-00', role: 'Admin' };
        break;
      case 'Médico':
        user = { id: 'doc1', name: 'Dr. House', cpf: '111.111.111-11', role: 'Médico' };
        break;
      case 'Usuário':
      default:
        user = { id: 'user1', name: 'John Doe', cpf: '123.456.789-00', role: 'Usuário' };
        break;
    }
    this.currentUser.set(user);
  }

  logout(): void {
    this.currentUser.set(null);
  }
}
