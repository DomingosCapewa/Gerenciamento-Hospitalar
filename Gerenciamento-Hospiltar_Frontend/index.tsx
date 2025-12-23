

import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, withHashLocation } from '@angular/router';
import { provideZoneChangeDetection } from '@angular/core';
import { FormsModule } from '@angular/forms';  
import { importProvidersFrom } from '@angular/core';

import { AppComponent } from './src/app.component';
import { APP_ROUTES } from './src/app.routes';

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection(),
    provideRouter(APP_ROUTES, withHashLocation()),
    importProvidersFrom(FormsModule)
  ]
}).catch(err => console.error(err));

