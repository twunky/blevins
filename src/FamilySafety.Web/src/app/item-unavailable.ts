import { Component, ElementRef, Injector, Input, afterNextRender, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { focusHeadingState } from './navigation';

const copy = {
  event: { noun: 'Event', plural: 'Events', path: '/events' },
  plan: { noun: 'Plan', plural: 'Plans', path: '/plans' },
  trip: { noun: 'Trip', plural: 'Trips', path: '/trips' },
} as const;

/**
 * The generic unavailable state for an Event, Plan, or Trip URL (REQ-PRIV-027, WI-009-005). The same card is shown for
 * an invalid ID, a missing item, an item the caller may not read, and an item whose Family is not the caller's (OD-B),
 * so nothing reveals whether the item exists. Its heading takes focus; the Family selection is never changed here.
 */
@Component({
  selector: 'app-item-unavailable',
  imports: [RouterLink],
  template: `
    <section class="card focused-state item-unavailable" aria-labelledby="item-unavailable-heading">
      <h2 id="item-unavailable-heading" tabindex="-1">This {{ text.noun }} is unavailable</h2>
      <p>Only {{ text.plural }} currently shared with you can be opened.</p>
      <a class="button-link button--secondary" [routerLink]="text.path" [state]="focusHeadingState">Return to {{ text.plural }}</a>
    </section>
  `,
})
export class ItemUnavailable {
  @Input({ required: true }) kind!: keyof typeof copy;

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  protected readonly focusHeadingState = focusHeadingState;

  constructor() {
    afterNextRender(() => this.host.nativeElement.querySelector<HTMLElement>('h2')?.focus(), { injector: inject(Injector) });
  }

  protected get text(): (typeof copy)[keyof typeof copy] {
    return copy[this.kind];
  }
}
