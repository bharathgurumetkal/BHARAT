import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminSidebarComponent } from '../sidebar/sidebar';
import { AdminTopbarComponent } from '../topbar/topbar';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, AdminSidebarComponent, AdminTopbarComponent],
  templateUrl: './admin-layout.html'
})
export class AdminLayoutComponent {}
