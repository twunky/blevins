import { UrlSegment } from '@angular/router';
import { routes } from './app.routes';
import { NavigableView, areaForView, matchedPath, pathForView, primaryDestinations, viewForPath } from './navigation';

describe('Main navigation definition (REQ-UX-013)', () => {
  it('has exactly the five destinations in the required order', () => {
    expect(primaryDestinations.map(destination => destination.label))
      .toEqual(['Home', 'Family', 'Activities', 'Safety', 'Me / Settings']);
    expect(primaryDestinations.map(destination => destination.path))
      .toEqual(['/home', '/family', '/activities', '/safety', '/me']);
  });

  it('maps each destination to its own page and that page to its own area', () => {
    for (const destination of primaryDestinations) {
      expect(areaForView[destination.view]).toBe(destination.area);
      expect(`/${pathForView[destination.view]}`).toBe(destination.path);
    }
  });

  it.each([
    ['home', 'home'], ['family', 'family'], ['events', 'activities'], ['eventItem', 'activities'],
    ['plansTrips', 'activities'], ['plans', 'activities'], ['planItem', 'activities'], ['trips', 'activities'], ['tripItem', 'activities'],
    ['safety', 'safety'], ['concerns', 'safety'], ['concernItem', 'safety'], ['incidents', 'safety'],
    ['profile', 'me'],
  ] as const)('places the %s page in the %s area', (view, area) => {
    expect(areaForView[view]).toBe(area);
  });

  // WI-009-005: /concerns and /concerns/:id now share one matcher, as the Incident routes already did.
  it('covers every page exactly once, and every page has a route', () => {
    const views = Object.keys(areaForView) as NavigableView[];
    expect(views.sort()).toEqual((Object.keys(pathForView) as NavigableView[]).sort());
    for (const view of views) {
      expect(viewForPath(pathForView[view])).toBe(view);
      const path = pathForView[view];
      const segments = path.replace(/:\w+/, '40000000-0000-0000-0000-000000000004').split('/').map(part => new UrlSegment(part, {}));
      expect(routes.some(route => route.path === path
        || (!!route.matcher && matchedPath(segments.map(segment => segment.path)) === path
          && !!route.matcher(segments, { segments: [], children: {} } as never, route)))).toBe(true);
    }
    expect(viewForPath('unknown')).toBeUndefined();
  });

  it('serves each item URL from a route that maps to its item page (REQ-UX-014)', () => {
    expect(routes.map(route => route.path)).toEqual(expect.arrayContaining(['events/:eventId', 'plans/:planId', 'trips/:tripId']));
    expect(matchedPath(['concerns', 'x'])).toBe('concerns/:concernId');
    expect(matchedPath(['concerns'])).toBe('concerns');
    expect(matchedPath(['incidents', 'x'])).toBe('incidents');
    expect(matchedPath(['events', 'x'])).toBeUndefined();
  });
});
