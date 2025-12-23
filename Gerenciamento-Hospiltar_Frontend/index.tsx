import 'zone.js';

import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, withHashLocation } from '@angular/router';
import { provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { FormsModule } from '@angular/forms';  
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { KeycloakAngularModule, KeycloakBearerInterceptor, KeycloakService } from 'keycloak-angular';

import { AppComponent } from './src/app.component';
import { APP_ROUTES } from './src/app.routes';

function initializeKeycloak(keycloak: KeycloakService) {
  return async () => {
    const env = (window as any).__env || {};
    const url = env.KEYCLOAK_URL || 'http://localhost:8081';
    const realm = env.KEYCLOAK_REALM || 'hospital';
    const clientId = env.KEYCLOAK_CLIENT_ID || 'hospital-api';
    const redirectUri = env.KEYCLOAK_REDIRECT_URI || `${window.location.origin}/#/dashboard`;
    await keycloak.init({
      config: { url, realm, clientId },
      enableBearerInterceptor: true,
      bearerExcludedUrls: ['/assets'],
      initOptions: {
        onLoad: 'check-sso',
        checkLoginIframe: false,
        pkceMethod: 'S256',
        redirectUri
      }
    });
  };
}

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection(),
    provideRouter(APP_ROUTES, withHashLocation()),
    importProvidersFrom(FormsModule),
    importProvidersFrom(HttpClientModule, KeycloakAngularModule),
    KeycloakService,
    { provide: APP_INITIALIZER, useFactory: initializeKeycloak, deps: [KeycloakService], multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: KeycloakBearerInterceptor, multi: true }
  ]
}).catch(err => console.error(err));

