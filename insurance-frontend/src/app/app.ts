import { Component, signal, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('insurance-frontend');

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadGoogleMapsApi();
    }
  }

  private loadGoogleMapsApi() {
    // Prevent multiple injections
    if (document.getElementById('google-maps-script')) return;

    const script = document.createElement('script');
    script.id = 'google-maps-script';
    // Use the secured environment config
    script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleMapsApiKey}&libraries=places`;
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
  }
}
