import { Routes, UrlMatcher, UrlSegment } from '@angular/router';
import { App } from './app';

/** One definition for /incidents, /incidents/new, and /incidents/:incidentId, so moving between them reuses the shell. */
export const incidentsMatcher: UrlMatcher = (segments: UrlSegment[]) =>
  segments[0]?.path === 'incidents' && segments.length <= 2 ? { consumed: segments } : null;

/**
 * One definition for /concerns and /concerns/:concernId (WI-009-005, OD-G), so moving between them reuses the shell
 * and the Concerns component keeps its create recovery state.
 */
export const concernsMatcher: UrlMatcher = (segments: UrlSegment[]) =>
  segments[0]?.path === 'concerns' && segments.length <= 2 ? { consumed: segments } : null;

export const routes: Routes = [
  { path: '', component: App, pathMatch: 'full' }, // Keeps #invite= links intact before Home routing.
  { path: 'home', component: App },
  { path: 'family', component: App },
  { path: 'family/:familyId/members/:profileId', component: App },
  { path: 'family/settings', component: App }, // Family Settings (WI-009-007); its own definition, so the gear and Back to Family move focus to the h1.
  { path: 'activities', component: App },
  { path: 'activities/new', component: App },
  { path: 'events', redirectTo: '/activities?type=Event', pathMatch: 'full' }, // Events dashboard (WI-009-008).
  // WI-009-008: declared before events/:eventId so "new" and "all" are never read as item IDs.
  { path: 'events/new', component: App },
  { path: 'events/all', redirectTo: '/activities?type=Event&period=all', pathMatch: 'full' },
  { path: 'events/:eventId/edit', component: App },
  { path: 'events/:eventId', component: App }, // Item URL (REQ-UX-014).
  { path: 'plans-trips', redirectTo: '/activities', pathMatch: 'full' }, // Navigation landing only (REQ-UX-013).
  { path: 'plans', redirectTo: '/activities?type=Plan', pathMatch: 'full' },
  { path: 'plans/new', component: App },
  { path: 'plans/:planId/edit', component: App },
  { path: 'plans/:planId', component: App }, // Item URL (REQ-UX-014).
  { path: 'trips', component: App }, // Retains creator Trips outside active Families.
  { path: 'trips/new', component: App },
  { path: 'trips/:tripId/edit', component: App },
  { path: 'trips/:tripId', component: App }, // Item URL (REQ-UX-014).
  { path: 'safety', component: App }, // Read-only Safety dashboard (DEC-079).
  { matcher: concernsMatcher, component: App },
  { matcher: incidentsMatcher, component: App },
  { path: 'me', component: App },
  { path: '**', redirectTo: 'home' },
];
