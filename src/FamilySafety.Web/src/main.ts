import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { DevelopmentSession } from './app/development-session';

bootstrapApplication(DevelopmentSession, appConfig).catch((err) => console.error(err));
