export interface TicketResponseDto {
  ticketId: number;
  title: string;
  description: string;

  categoryId?: number;
  category: string;     

  priorityId: number;
  priority: string;

  statusId: number;
  status: string;

  ownerId?: string;
  owner: string;     

  assignedToId?: string;
  assignedTo: string;   

  createdAt: string;
  updatedAt?: string;
  dueDate?: string;
}

export interface UserDashboardMetrics {
  totalTickets: number;
  pending: number;
  resolved: number;
  open: number;
}

export interface RoleRequestDto {
  id: string;
  userId: string;
  userEmail: string;
  requestedRole: string;
  status: string;
  requestedAt: Date;
  reviewedAt?: Date;
  reviewedBy?: string;
}

export interface UpdateUserProfileDto {
  fullName?: string;
  phoneNumber?: string;
  address?:string;
}


export interface UpdateTicketDto {
  description: string;
  categoryId?: number;
  statusId?: number;
  priorityId?: number;
  assignedToId?: string | null;
  dueDate?: string | null;
}

export interface TicketComment {
  commentId: number;
  message: string;
  createdAt: string;
  userId: string;
  userName: string;
  isInternal: boolean;
}

export interface TicketHistory {
  historyId: number;
  fieldName: string;
  oldValue: string;
  newValue: string;
  changedAt: string;
  changedBy: string;
}

export interface UserProfileDto {
  userProfileId: string;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
}

export interface UserDto {
  userId: string;
  fullName: string;
  email: string;
  role?: string;    
  roles?: string[]; 
  createdAt: string | null;
  isActive?: boolean;
}

export interface RegisterRequest {
  FirstName: string;
  LastName: string;
  email: string;
  password: string;
  roleId: number;
  categorySkillIds: number[];
  phoneNumber: string;
  address: string;
}