import { IconName } from './icon';

/**
 * Main navigation (REQ-UX-013, WI-009-003). This file is the one place to read or change the
 * primary destinations and which destination is current for each page.
 *
 * Navigation is presentation only. Nothing here grants or implies access to any Family,
 * Event, Plan, Trip, Concern, or Safety Incident (REQ-PRIV-027); the server decides that.
 */

/**
 * Every routed page the shell renders, keyed by the `view` the shell uses. The `…Item` views are the item URLs
 * (REQ-UX-014, WI-009-005); an Incident item is the `incidents` view, served by its existing shared route.
 */
export type NavigableView =
  | 'activities' | 'activityCreate' | 'planCreate' | 'tripCreate' | 'eventEdit' | 'planEdit' | 'tripEdit'
  | 'home' | 'family' | 'familySettings' | 'events' | 'eventCreate' | 'eventsAll' | 'eventItem'
  | 'plansTrips' | 'plans' | 'planItem' | 'trips' | 'tripItem'
  | 'safety' | 'concerns' | 'concernItem' | 'incidents'
  | 'memberProfile' | 'profile';

/** The five primary areas. Activities and Safety are presentation groupings only. */
export type PrimaryArea = 'home' | 'family' | 'activities' | 'safety' | 'me';

export interface PrimaryDestination {
  readonly area: PrimaryArea;
  readonly label: string;
  readonly icon: IconName;
  readonly path: string;
  /** The area's own page; `aria-current="page"` there, `"true"` on the area's child pages. */
  readonly view: NavigableView;
}

/** The five REQ-UX-013 destinations, in the required order at every width. */
export const primaryDestinations: readonly PrimaryDestination[] = [
  { area: 'home', label: 'Home', icon: 'home', path: '/home', view: 'home' },
  { area: 'family', label: 'Family', icon: 'users', path: '/family', view: 'family' },
  { area: 'activities', label: 'Activities', icon: 'calendar', path: '/activities', view: 'activities' },
  { area: 'safety', label: 'Safety', icon: 'shield', path: '/safety', view: 'safety' },
  { area: 'me', label: 'Me / Settings', icon: 'user', path: '/me', view: 'profile' },
];

/** Which primary area is current for each page. UI state only; never used for authorization. */
export const areaForView: Readonly<Record<NavigableView, PrimaryArea>> = {
  activities: 'activities', activityCreate: 'activities', planCreate: 'activities', tripCreate: 'activities',
  eventEdit: 'activities', planEdit: 'activities', tripEdit: 'activities',
  home: 'home',
  family: 'family', familySettings: 'family', memberProfile: 'family',
  events: 'activities', eventCreate: 'activities', eventsAll: 'activities', eventItem: 'activities',
  plansTrips: 'activities', plans: 'activities', planItem: 'activities', trips: 'activities', tripItem: 'activities',
  safety: 'safety', concerns: 'safety', concernItem: 'safety', incidents: 'safety',
  profile: 'me',
};

/**
 * The route path of each page. The Incident sub-routes (/incidents/new, /incidents/:id) share `incidents`, and
 * /concerns and /concerns/:id share one matcher (app.routes.ts), so moving between them keeps the shell.
 */
export const pathForView: Readonly<Record<NavigableView, string>> = {
  activities: 'activities', activityCreate: 'activities/new', planCreate: 'plans/new', tripCreate: 'trips/new',
  eventEdit: 'events/:eventId/edit', planEdit: 'plans/:planId/edit', tripEdit: 'trips/:tripId/edit',
  home: 'home', family: 'family', memberProfile: 'family/:familyId/members/:profileId', familySettings: 'family/settings', events: 'events', eventCreate: 'events/new',
  eventsAll: 'events/all', eventItem: 'events/:eventId',
  plansTrips: 'plans-trips', plans: 'plans', planItem: 'plans/:planId', trips: 'trips', tripItem: 'trips/:tripId',
  safety: 'safety', concerns: 'concerns', concernItem: 'concerns/:concernId', incidents: 'incidents',
  profile: 'me',
};

/** Areas served by one matcher-based route definition, so their list and item pages share the shell. */
export const matchedAreas: readonly string[] = ['concerns', 'incidents'];

/** The `pathForView` value for a matcher route's consumed URL segments, for example `concerns/:concernId`. */
export function matchedPath(segments: readonly string[]): string | undefined {
  if (!matchedAreas.includes(segments[0])) return undefined;
  return segments[0] === 'concerns' && segments.length > 1 ? 'concerns/:concernId' : segments[0];
}

export function viewForPath(path: string): NavigableView | undefined {
  return (Object.keys(pathForView) as NavigableView[]).find(view => pathForView[view] === path);
}

/**
 * Router navigation state carried by main-navigation and landing links. The shell moves focus to the
 * page heading only for user-initiated navigations that carry it (WI-009-003 D4).
 */
export const focusHeadingState = { focusHeading: true } as const;
