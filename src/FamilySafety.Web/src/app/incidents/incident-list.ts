import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppIcon } from '../icon';
import { focusHeadingState } from '../navigation';
import { IncidentSummary } from './incident-api';
import { dateOnlyFormat, dateTimeFormat, initials } from './incident-shared';

/** The authorized Incident list, grouped into open and ended coordination in server order. No reason or lineage exists here. Each row links to the Incident's own URL. */
@Component({
  selector: 'app-incident-list',
  imports: [DatePipe, AppIcon, RouterLink],
  templateUrl: './incident-list.html',
  styleUrls: ['./incident-common.scss', './incident-list.scss'],
})
export class IncidentList {
  @Input() rows: readonly IncidentSummary[] = [];
  @Input() loading = false;
  @Input() error = '';
  @Input() ownProfileId: string | null = null;
  @Input() showFamilyName = false;
  @Input() canCreate = false;
  @Output() readonly create = new EventEmitter<void>();
  @Output() readonly refresh = new EventEmitter<void>();

  protected readonly focusHeadingState = focusHeadingState;
  protected readonly dateTimeFormat = dateTimeFormat;
  protected readonly dateOnlyFormat = dateOnlyFormat;
  protected readonly initials = initials;

  protected openRows(): readonly IncidentSummary[] {
    return this.rows.filter(row => row.state === 'Open');
  }

  protected endedRows(): readonly IncidentSummary[] {
    return this.rows.filter(row => row.state !== 'Open');
  }
}
