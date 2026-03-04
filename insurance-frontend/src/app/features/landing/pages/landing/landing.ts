import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class LandingComponent {
  workflow = [
    { name: 'Customer', icon: 'person_outline', desc: 'Secure Registration' },
    { name: 'Application', icon: 'description', desc: 'Property Submission' },
    { name: 'Agent Review', icon: 'fact_check', desc: 'Risk Assessment' },
    { name: 'Policy Approval', icon: 'verified', desc: 'Instant Activation' },
    { name: 'Claim Filing', icon: 'assignment_late', desc: 'Digital Reporting' },
    { name: 'Settlement', icon: 'account_balance_wallet', desc: 'Fast Payouts' }
  ];
}

