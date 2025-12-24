
export interface User {
  id: string;
  name: string;
  cpf: string;
  email?: string;
  role: 'Usuario' | 'Medico' | 'Admin';
  registrationCompleted?: boolean;
}

export interface Appointment {
  id: string;
  patientName: string;
  patientCpf: string;
  type: 'Consulta' | 'Exame';
  specialtyOrExam: string;
  dateTime: string;
  status: 'Marcado' | 'Concluído' | 'Cancelado';
}

export interface Notification {
  message: string;
  type: 'success' | 'error' | 'info';
}
