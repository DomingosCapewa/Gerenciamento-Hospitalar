
import { Component, ChangeDetectionStrategy, inject, signal, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { Appointment, User } from '../../models/types';

@Component({
  selector: 'app-appointments',
  templateUrl: './appointments.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, DatePipe],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppointmentsComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);

  appointments = signal<Appointment[]>([]);
  isLoading = signal(true);
  currentUser = this.authService.currentUser;

  ngOnInit(): void {
    this.loadAppointments();
  }

  async loadAppointments(): Promise<void> {
    const user = this.currentUser();
    if (!user) {
        this.isLoading.set(false);
        return;
    }

    this.isLoading.set(true);
    try {
        const data = await this.apiService.getAppointments(user.cpf, user.role);
        this.appointments.set(data);
    } catch (error) {
        console.error('Failed to load appointments', error);
        this.appointments.set([]);
    } finally {
        this.isLoading.set(false);
    }
  }
}
