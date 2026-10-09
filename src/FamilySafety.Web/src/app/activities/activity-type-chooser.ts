import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppIcon } from '../icon';
import { focusHeadingState } from '../navigation';
import { activityTypes } from './activities';

@Component({
  selector: 'app-activity-type-chooser',
  imports: [RouterLink, AppIcon],
  template: `<nav class="type-options" aria-label="Choose activity type">
    @for (item of types; track item.type) {
      <a [routerLink]="item.path" [state]="focusHeadingState" [attr.aria-current]="selected() === item.type ? 'page' : null">
        <span [class]="'type-icon type-' + item.type.toLowerCase()"><app-icon [name]="item.icon" /></span>
        <span><strong>{{ item.type }}</strong><span class="type-description">{{ item.description }}</span></span>
        @if (selected() === item.type) { <app-icon name="check" /> }
      </a>
    }
  </nav>`,
  styles: `:host { display: block; margin-bottom: 1.2rem; }
    .type-options { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; }
    a { display: flex; align-items: center; gap: 1rem; padding: 1.2rem; border: 1px solid var(--color-border); border-radius: var(--radius-lg); background: var(--color-surface); color: var(--color-text); text-decoration: none; }
    a[aria-current], a:hover { border-color: var(--color-primary); }
    a > app-icon { color: var(--color-primary); margin-left: auto; }
    strong { font-size: 1.1rem; }
    .type-description { display: block; color: var(--color-muted); line-height: 1.45; margin-top: .5rem; font-size: .9rem; }
    .type-icon { display: grid; place-items: center; flex: 0 0 3.5rem; height: 3.5rem; border-radius: var(--radius-md); }
    .type-event { color: #64e7ff; background: #082637; } .type-plan { color: #51e7a0; background: #072b1d; } .type-trip { color: #d895ff; background: #28123a; }
    @media (max-width: 50rem) { .type-options { grid-template-columns: 1fr; } a { padding: 1rem; } }
  `,
})
export class ActivityTypeChooser {
  readonly selected = input('');
  protected readonly types = activityTypes;
  protected readonly focusHeadingState = focusHeadingState;
}
