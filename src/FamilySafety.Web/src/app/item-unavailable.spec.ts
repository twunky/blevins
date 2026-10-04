import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ItemUnavailable } from './item-unavailable';

describe('Generic item unavailable card (REQ-PRIV-027, WI-009-005)', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideRouter([])] }));

  it.each([
    ['event', 'Event', 'Events', '/events'],
    ['plan', 'Plan', 'Plans', '/plans'],
    ['trip', 'Trip', 'Trips', '/trips'],
  ] as const)('shows one neutral %s card with a way back and focuses its heading', async (kind, noun, plural, path) => {
    const fixture = TestBed.createComponent(ItemUnavailable);
    fixture.componentRef.setInput('kind', kind);
    fixture.detectChanges();
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const heading = element.querySelector('h2')!;
    expect(heading.textContent).toBe(`This ${noun} is unavailable`);
    expect(heading.getAttribute('tabindex')).toBe('-1');
    expect(document.activeElement).toBe(heading);
    expect(element.querySelector('p')?.textContent).toBe(`Only ${plural} currently shared with you can be opened.`);
    const back = element.querySelector('a')!;
    expect(back.textContent).toBe(`Return to ${plural}`);
    expect(back.getAttribute('href')).toBe(path);
    // One card and nothing else: no alert, no reason, no Family.
    expect(element.querySelectorAll('[role="alert"]')).toHaveLength(0);
    expect(element.textContent).not.toMatch(/Family|removed|deleted|expired|permission/i);
  });
});
