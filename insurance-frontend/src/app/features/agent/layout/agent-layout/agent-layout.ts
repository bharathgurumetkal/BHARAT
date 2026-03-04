import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AgentSidebarComponent } from '../sidebar/sidebar';
import { AgentTopbarComponent } from '../topbar/topbar';

@Component({
  selector: 'app-agent-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, AgentSidebarComponent, AgentTopbarComponent],
  templateUrl: './agent-layout.html',
  styleUrl: './agent-layout.css'
})
export class AgentLayoutComponent {
}
