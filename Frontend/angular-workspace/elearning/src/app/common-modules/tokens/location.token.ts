import { InjectionToken } from '@angular/core';

export const LOCATION_TOKEN = new InjectionToken<Location>('WindowLocation', {
  factory: () => window.location
});