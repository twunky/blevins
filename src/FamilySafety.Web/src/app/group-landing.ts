import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppIcon, IconName } from './icon';
import { focusHeadingState } from './navigation';

interface LandingChoice {
  readonly id: string;
  readonly title: string;
  readonly description: string;
  readonly icon: IconName;
  readonly path: string;
  readonly linkText: string;
}

const choices: Readonly<Record<'plansTrips', readonly LandingChoice[]>> = {
  plansTrips: [
    { id: 'plans', title: 'Plans', icon: 'clipboard', path: '/plans', linkText: 'Open Plans',
      description: 'Share a lightweight plan for a future activity or destination with the people you choose.' },
    { id: 'trips', title: 'Trips', icon: 'plane', path: '/trips', linkText: 'Open Trips',
      description: 'Plan and share travel details with your chosen family audience.' },
  ],
};

/**
 * The Plans & Trips landing page (REQ-UX-013, WI-009-003 D1). Navigation only: two fixed links, no requests,
 * no Family data, no counts or states. /safety is the Safety dashboard instead (WI-009-009, DEC-079).
 */
@Component({
  selector: 'app-group-landing',
  imports: [RouterLink, AppIcon],
  template: `
    <div class="landing-grid">
      @for (choice of choices[group]; track choice.id) {
        <section class="card landing-choice" [attr.aria-labelledby]="'landing-' + choice.id + '-heading'">
          <div class="card-header">
            <span class="card-icon"><app-icon [name]="choice.icon" /></span>
            <h2 [id]="'landing-' + choice.id + '-heading'">{{ choice.title }}</h2>
          </div>
          <p class="item-meta">{{ choice.description }}</p>
          <a class="landing-link" [routerLink]="choice.path" [state]="focusHeadingState">{{ choice.linkText }}</a>
        </section>
      }
    </div>
  `,
})
export class GroupLanding {
  @Input({ required: true }) group!: 'plansTrips';
  protected readonly choices = choices;
  protected readonly focusHeadingState = focusHeadingState;
}
