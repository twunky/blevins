import { Component, computed, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppIcon, IconName } from '../icon';
import { dateTile } from '../date-format';
import { focusHeadingState } from '../navigation';

export interface ActivityRow {
  id: string;
  type: 'Event' | 'Plan' | 'Trip';
  title: string;
  date: string;
  time: string;
  timing: string;
  location: string;
  path: string;
  upcoming: boolean;
  completed?: boolean;
}
export const activityTypes: readonly { type: ActivityRow['type']; icon: IconName; path: string; description: string }[] = [
  { type: 'Event', icon: 'calendar', path: '/events/new', description: 'A specific date and time activity, such as a game, concert, or gathering.' },
  { type: 'Plan', icon: 'clipboard', path: '/plans/new', description: 'A lightweight plan for a future activity or destination.' },
  { type: 'Trip', icon: 'plane', path: '/trips/new', description: 'Travel details with a start and end date, such as a vacation or work trip.' },
];

/** Presentation over authorized projections only. No new audience or cross-domain API. */
@Component({
  selector: 'app-activities',
  imports: [RouterLink, AppIcon],
  templateUrl: './activities.html',
  styleUrl: './activities.scss',
})
export class Activities {
  readonly rows = input<readonly ActivityRow[]>([]);
  readonly loading = input(false);
  readonly errors = input<readonly string[]>([]);
  readonly initialType = input('All');
  readonly initialPeriod = input('upcoming');
  readonly pending = input<readonly { invitationId: string; eventId: string; name: string }[]>([]);
  readonly pendingLoading = input(false);
  readonly pendingError = input('');
  readonly retry = output<void>();
  readonly retryPending = output<void>();
  protected readonly types = activityTypes;
  protected readonly focusHeadingState = focusHeadingState;
  protected readonly dateTile = dateTile;
  protected readonly chosenType = signal<string | null>(null);
  protected readonly chosenPeriod = signal<string | null>(null);
  protected readonly search = signal('');
  protected readonly type = computed(() => this.chosenType() ?? this.initialType());
  protected readonly period = computed(() => this.chosenPeriod() ?? this.initialPeriod());
  protected readonly periodRows = computed(() => this.rows().filter(row => this.period() === 'all' || row.upcoming));
  protected readonly visible = computed(() => {
    const query = this.search().trim().toLocaleLowerCase();
    return this.periodRows().filter(row => (this.type() === 'All' || row.type === this.type())
      && `${row.title} ${row.location}`.toLocaleLowerCase().includes(query))
      .slice().sort((a, b) => `${a.date}T${a.time}`.localeCompare(`${b.date}T${b.time}`) || a.id.localeCompare(b.id));
  });
  protected count(type: string): number { return this.periodRows().filter(row => row.type === type).length; }
  protected icon(type: string): IconName { return activityTypes.find(item => item.type === type)!.icon; }
}
