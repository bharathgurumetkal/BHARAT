import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../../core/services/admin-api.service';

@Component({
  selector: 'app-system-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './system-logs.html',
  styles: []
})
export class SystemLogsComponent implements OnInit {
  logs = signal<any[]>([]);
  isLoading = signal(true);
  searchTerm = signal('');
  selectedAction = signal('All');
  selectedRole = signal('All');
  expandedLogId = signal<string | null>(null);

  constructor(private adminService: AdminApiService) {}

  ngOnInit(): void { this.loadLogs(); }

  loadLogs(): void {
    this.isLoading.set(true);
    this.adminService.getAuditLogs().subscribe({
      next: (data) => {
        this.logs.set(data.sort((a: any, b: any) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        ));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  get uniqueActions(): string[] {
    const actions = [...new Set(this.logs().map(l => l.action).filter(Boolean))];
    return ['All', ...actions.sort()];
  }

  get uniqueRoles(): string[] {
    const roles = [...new Set(this.logs().map(l => l.userRole).filter(Boolean))];
    return ['All', ...roles.sort()];
  }

  get filteredLogs(): any[] {
    const term = this.searchTerm().toLowerCase().trim();
    return this.logs().filter(log => {
      const matchesAction = this.selectedAction() === 'All' || log.action === this.selectedAction();
      const matchesRole = this.selectedRole() === 'All' || log.userRole === this.selectedRole();
      const matchesSearch = !term ||
        log.action?.toLowerCase().includes(term) ||
        log.description?.toLowerCase().includes(term) ||
        log.entityType?.toLowerCase().includes(term) ||
        log.userEmail?.toLowerCase().includes(term) ||
        log.userRole?.toLowerCase().includes(term) ||
        log.ipAddress?.toLowerCase().includes(term);
      return matchesAction && matchesRole && matchesSearch;
    });
  }

  get stats() {
    const all = this.logs();
    return {
      total: all.length,
      today: all.filter(l => {
        const d = new Date(l.createdAt);
        const t = new Date();
        return d.toDateString() === t.toDateString();
      }).length,
      critical: all.filter(l => l.action?.includes('Rejected') || l.action?.includes('Deleted')).length,
      approved: all.filter(l => l.action?.includes('Approved') || l.action?.includes('Settled')).length,
    };
  }

  toggleExpand(id: string): void {
    this.expandedLogId.set(this.expandedLogId() === id ? null : id);
  }

  getActionStyle(action: string): { dot: string; badge: string; icon: string; leftBar: string } {
    if (!action) return { dot: 'bg-slate-400', badge: 'bg-slate-50 text-slate-600 border-slate-200', icon: 'info', leftBar: 'bg-slate-300' };
    const a = action.toLowerCase();
    if (a.includes('approved') || a.includes('settled') || a.includes('activated'))
      return { dot: 'bg-emerald-400', badge: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: 'task_alt', leftBar: 'bg-emerald-400' };
    if (a.includes('rejected') || a.includes('deleted') || a.includes('cancelled') || a.includes('failed'))
      return { dot: 'bg-red-400', badge: 'bg-red-50 text-red-700 border-red-200', icon: 'cancel', leftBar: 'bg-red-400' };
    if (a.includes('renewed') || a.includes('updated') || a.includes('assigned') || a.includes('modified'))
      return { dot: 'bg-amber-400', badge: 'bg-amber-50 text-amber-700 border-amber-200', icon: 'edit_note', leftBar: 'bg-amber-400' };
    if (a.includes('created') || a.includes('submitted') || a.includes('registered'))
      return { dot: 'bg-blue-400', badge: 'bg-blue-50 text-blue-700 border-blue-200', icon: 'add_circle', leftBar: 'bg-blue-400' };
    if (a.includes('login') || a.includes('logout') || a.includes('auth'))
      return { dot: 'bg-purple-400', badge: 'bg-purple-50 text-purple-700 border-purple-200', icon: 'fingerprint', leftBar: 'bg-purple-400' };
    return { dot: 'bg-indigo-400', badge: 'bg-indigo-50 text-indigo-700 border-indigo-200', icon: 'receipt_long', leftBar: 'bg-indigo-400' };
  }

  getRoleStyle(role: string): string {
    if (!role) return 'bg-slate-100 text-slate-600';
    switch (role.toLowerCase()) {
      case 'admin': return 'bg-purple-100 text-purple-700';
      case 'agent': return 'bg-blue-100 text-blue-700';
      case 'claimsofficer': return 'bg-amber-100 text-amber-700';
      case 'customer': return 'bg-emerald-100 text-emerald-700';
      default: return 'bg-slate-100 text-slate-600';
    }
  }

  getTimeAgo(dateStr: string): string {
    const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return `${diff}s ago`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.selectedAction.set('All');
    this.selectedRole.set('All');
  }
}
