import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar';
import { TopbarComponent } from '../topbar/topbar';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, TopbarComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayoutComponent {}
