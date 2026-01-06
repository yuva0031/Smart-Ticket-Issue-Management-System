import { Routes } from '@angular/router';
import { Login } from './components/auth/login/login';
import { RegisterComponent } from './components/auth/register/register';
import { UserDashboardNav } from './components/user-dashboard-nav/user-dashboard-nav';
import { CreateTicket } from './components/create-ticket/create-ticket';
import { TicketDetails } from './components/ticket-details/ticket-details';
import { UserBoard } from './components/user-board/user-board';
import { AgentDashboardNav } from './components/agent-dashboard-nav/agent-dashboard-nav';
import { ManagerDashboard } from './components/manager-dashboard/manager-dashboard';
import { ManagerDashboardNav } from './components/manager-dashboard-nav/manager-dashboard-nav';
import { TicketsView } from './components/tickets-view/tickets-view';
import { AdminDashboardNav } from './components/admin-dashboard-nav/admin-dashboard-nav';
import { AdminDashboard } from './components/admin-dashboard/admin-dashboard';
import { PendingApprovals } from './components/admin/pending-requests/pending-requests';
import { SupportAgents } from './components/admin/support-agents/support-agents';
import { AllUsers } from './components/admin/all-users/all-users';
import { Managers } from './components/admin/managers/managers';
import { AgentDashboard } from './components/agent-dashboard/agent-dashboard';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { guestGuard } from './guards/guest.guard';
import { canDeactivateGuard } from './guards/can-deactivate.guard';
import { SlaRules } from './components/sla-rules/sla-rules';
import { UserAnalytics } from './components/user-analytics/user-analytics';
import { AgentAnalytics } from './components/agent-analytics/agent-analytics';
import { AgentSkillProfile } from './components/agent-profile/agent-profile';
import { ManagerAnalytics } from './components/manager-analytics/manager-analytics';

export const routes: Routes = [
  { 
    path: 'login', 
    component: Login,
    canActivate: [guestGuard]
  },
  { 
    path: 'register', 
    component: RegisterComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'app',
    component: UserDashboardNav,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['EndUser'] }, 
    children: [
      { 
        path: 'dashboard', 
        component: UserBoard 
      },
      { 
        path: 'create-ticket', 
        component: CreateTicket,
        canDeactivate: [canDeactivateGuard]
      },
      { 
        path: 'tickets', 
        component: TicketsView 
      },
      { 
        path: 'tickets/:ticketId', 
        component: TicketDetails 
      },
      {
        path: 'analytics',
        component: UserAnalytics
      },
      { 
        path: '', 
        redirectTo: 'dashboard', 
        pathMatch: 'full' 
      }
    ]
  },
  {
    path: 'agent',
    component: AgentDashboardNav,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['SupportAgent', 'SupportManager', 'Admin'] },
    children: [
      { 
        path: '', 
        redirectTo: 'dashboard', 
        pathMatch: 'full' 
      },
      { 
        path: 'dashboard', 
        component: AgentDashboard 
      },
      { 
        path: 'assigned', 
        component: TicketsView 
      },
      {
        path: 'agent-profile',
        component: AgentSkillProfile
      },
      { 
        path: 'all', 
        component: TicketsView 
      },
      { 
        path: 'tickets/:ticketId', 
        component: TicketDetails 
      },
      {
        path: 'analytics',
        component: AgentAnalytics
      }
    ]
  },
  {
    path: 'manager',
    component: ManagerDashboardNav,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['SupportManager', 'Admin'] }, 
    children: [
      { 
        path: '', 
        redirectTo: 'dashboard', 
        pathMatch: 'full' 
      },
      { 
        path: 'dashboard', 
        component: ManagerDashboard
      },
      { 
        path: 'tickets', 
        component: TicketsView 
      },
      { 
        path: 'unassigned', 
        component: TicketsView 
      },
      { 
        path: 'sla-rules', 
        component: SlaRules 
      },
      { 
        path: 'tickets/:ticketId', 
        component: TicketDetails 
      },
      { 
        path: 'analytics', 
        component: ManagerAnalytics
      }
    ]
  },
  {
    path: 'admin',
    component: AdminDashboardNav,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { 
        path: '', 
        redirectTo: 'dashboard', 
        pathMatch: 'full' 
      },
      { 
        path: 'dashboard', 
        component: AdminDashboard 
      },
      { 
        path: 'requests', 
        component: PendingApprovals 
      },
      { 
        path: 'users', 
        component: AllUsers 
      },
      { 
        path: 'agents', 
        component: SupportAgents 
      },
      { 
        path: 'managers', 
        component: Managers 
      },
      { 
        path: 'tickets', 
        component: TicketsView 
      }
    ]
  }, 
  { 
    path: '', 
    redirectTo: 'login', 
    pathMatch: 'full' 
  },
  { 
    path: '**', 
    redirectTo: 'login' 
  }
];