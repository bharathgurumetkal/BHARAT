import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ClaimsOfficerSidebarComponent } from '../sidebar/sidebar';
import { ClaimsOfficerTopbarComponent } from '../topbar/topbar';

@Component({
  selector: 'app-claims-officer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ClaimsOfficerSidebarComponent, ClaimsOfficerTopbarComponent],
  templateUrl: './claims-officer-layout.html',
  styleUrl: './claims-officer-layout.css'
})
export class ClaimsOfficerLayoutComponent {
}
