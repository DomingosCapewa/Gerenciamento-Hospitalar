
import { Injectable } from '@angular/core';
import { Appointment } from '../models/types';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private appointments: Appointment[] = [
    { id: '1', patientName: 'John Doe', patientCpf: '123.456.789-00', type: 'Consulta', specialtyOrExam: 'Pediatria', dateTime: '2024-08-10T10:00', status: 'Marcado' },
    { id: '2', patientName: 'John Doe', patientCpf: '123.456.789-00', type: 'Exame', specialtyOrExam: 'Coleta de sangue', dateTime: '2024-08-15T08:30', status: 'Marcado' },
    { id: '3', patientName: 'Jane Smith', patientCpf: '987.654.321-00', type: 'Consulta', specialtyOrExam: 'Cardiologia', dateTime: '2024-08-12T14:00', status: 'Marcado' },
  ];

  getAppointments(userCpf: string, userRole: 'Usuário' | 'Médico' | 'Admin'): Promise<Appointment[]> {
    return new Promise(resolve => {
      setTimeout(() => {
        if (userRole === 'Usuário') {
          resolve(this.appointments.filter(a => a.patientCpf === userCpf));
        } else {
          // In a real app, doctors would see their appointments, admins all.
          resolve(this.appointments);
        }
      }, 500);
    });
  }

  schedule(data: any): Promise<{ success: boolean; message: string; code?: string }> {
    return new Promise(resolve => {
      setTimeout(() => {
        const conflict = this.appointments.some(a => a.dateTime === data.horario && (a.specialtyOrExam === data.medico || a.specialtyOrExam === data.exame));
        
        if (conflict) {
          resolve({ success: false, message: `O (${data.medico || data.exame}) já possui agendamento no (${data.horario})` });
        } else {
          const newId = (this.appointments.length + 1).toString();
          const newAppointment: Appointment = {
              id: newId,
              patientName: data.paciente.nome,
              patientCpf: data.paciente.cpf,
              type: data.medico ? 'Consulta' : 'Exame',
              specialtyOrExam: data.medico || data.exame,
              dateTime: data.horario,
              status: 'Marcado'
          };
          this.appointments.push(newAppointment);
          resolve({ success: true, message: `Agendamento para ${data.paciente.nome} marcado com sucesso!`, code: `ID-${newId}` });
        }
      }, 1000);
    });
  }
}
