import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';
import { IncidentCategory, IncidentContactMethod, IncidentContactOutcome, IncidentEntry, IncidentEntryKind, IncidentState, KnownCategory } from './incident-api';

/** Field key → UI-owned message. Raw server validation text is never shown. */
export type FieldErrors = Record<string, string>;

export const dateTimeFormat = 'MMM d, y, h:mm a';
export const dateOnlyFormat = 'MMM d, y';
export const timelineTimeFormat = 'MMM d, h:mm a';
export const sessionEndedMessage = 'Your session may have ended. Please sign in again.';

export const categoryHelp: Record<Exclude<IncidentCategory, 'Outdated'>, string> = {
  Confirmed: 'Verified directly, with a described source.',
  Reported: 'Shared by someone, not yet verified.',
  Assumption: 'A reasonable belief that has not been confirmed.',
  Unknown: 'No reliable answer is available yet.',
};

export const contactMethods: ReadonlyArray<{ value: IncidentContactMethod; label: string }> = [
  { value: 'Call', label: 'Call' },
  { value: 'TextMessage', label: 'Text message' },
  { value: 'Email', label: 'Email' },
  { value: 'InAppMessage', label: 'In-app message' },
  { value: 'InPerson', label: 'In person' },
  { value: 'Other', label: 'Other' },
];

export const contactOutcomes: ReadonlyArray<{ value: IncidentContactOutcome; label: string }> = [
  { value: 'Reached', label: 'Reached' },
  { value: 'NoAnswer', label: 'No answer' },
  { value: 'LeftMessage', label: 'Left message' },
  { value: 'SentNoReplyYet', label: 'Sent — no reply yet' },
  { value: 'Other', label: 'Other' },
  { value: 'Unknown', label: 'Unknown' },
];

export function methodLabel(value: IncidentContactMethod | null): string {
  return contactMethods.find(item => item.value === value)?.label ?? '';
}

export function outcomeLabel(value: IncidentContactOutcome | null): string {
  return contactOutcomes.find(item => item.value === value)?.label ?? '';
}

/** Outdated is freshness: the stored original category stays visible ("Reported · Outdated"). */
export function categoryText(category: IncidentCategory | null, isOutdated: boolean): string {
  if (!category) return '';
  return isOutdated ? `${category} · Outdated` : category;
}

export function entryKindLabel(kind: IncidentEntryKind): string {
  return ({
    Observation: 'Finding', Unknown: 'Unknown', Summary: 'Coordinator summary', ContactAttempt: 'Contact attempt',
    TaskResult: 'Task result', Correction: 'Correction', Challenge: 'Another report',
  } as const)[kind];
}

export function sourceText(entry: Pick<IncidentEntry, 'sourceUnknown' | 'sourceDescription'>): string {
  return entry.sourceUnknown || !entry.sourceDescription ? 'Source unknown' : `Source: ${entry.sourceDescription}`;
}

export function stateDescription(state: IncidentState): string {
  return ({ Open: 'Coordination is open', Frozen: 'Read-only record', Resolved: 'Coordination ended' } as const)[state];
}

export function initials(name: string | null | undefined): string {
  return (name ?? '').split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join('');
}

/** Converts a datetime-local value to an ISO instant, or null when blank. */
export function toInstant(value: string): string | null {
  return value ? new Date(value).toISOString() : null;
}

export function isStatus(error: unknown, status: number): boolean {
  return error instanceof HttpErrorResponse && error.status === status;
}

/**
 * Maps ValidationProblem keys to form fields. Only keys are read; messages stay UI-owned, so
 * the field's own client message (or a neutral fallback) is shown.
 */
export function mapValidation(error: unknown, keyToField: Record<string, string>, messages: FieldErrors): FieldErrors {
  const raw = error instanceof HttpErrorResponse ? (error.error?.errors ?? {}) : {};
  const result: FieldErrors = {};
  for (const key of Object.keys(raw)) {
    const field = Object.entries(keyToField).find(([serverKey]) => serverKey.toLowerCase() === key.toLowerCase())?.[1] ?? 'form';
    result[field] = messages[field] ?? 'Check these details and try again.';
  }
  if (!Object.keys(result).length) result['form'] = 'Check these details and try again.';
  return result;
}

/** Moves focus to an element after Angular renders it. */
export function focusById(id: string): void {
  setTimeout(() => document.getElementById(id)?.focus());
}

/** Validates the shared "source or explicitly unknown" rule used by every information form. */
export function sourceError(category: IncidentCategory | '', description: string, unknown: boolean): string {
  const source = description.trim();
  if (unknown === !!source) return 'Enter a source or mark the source as unknown.';
  if (source.length > 500) return 'Source descriptions can be up to 500 characters.';
  if (category === 'Confirmed' && unknown) return 'Confirmed information needs a described source.';
  return '';
}


export function textError(value: string, max: number, label = 'information'): string {
  const text = value.trim();
  return !text || text.length > max ? `Enter ${label} of 1–${max.toLocaleString('en-US')} characters.` : '';
}

export type ProvenanceGroup = FormGroup<{
  category: FormControl<string>;
  originalCategory: FormControl<string>;
  sourceDescription: FormControl<string>;
  sourceUnknown: FormControl<boolean>;
}>;

/** Category, earlier classification (for Outdated), and source-or-unknown controls shared by every information form. */
export function provenanceGroup(category = ''): ProvenanceGroup {
  return new FormGroup({
    category: new FormControl(category, { nonNullable: true }),
    originalCategory: new FormControl('Reported', { nonNullable: true }),
    sourceDescription: new FormControl('', { nonNullable: true }),
    sourceUnknown: new FormControl(false, { nonNullable: true }),
  });
}

/** Client checks for a provenance group; returns field errors keyed `category` and `source`. */
export function provenanceErrors(group: ProvenanceGroup): FieldErrors {
  const { category, originalCategory, sourceDescription, sourceUnknown } = group.getRawValue();
  const errors: FieldErrors = {};
  if (!category) errors['category'] = 'Choose how this information is known.';
  else if (category === 'Outdated' && !['Confirmed', 'Reported', 'Assumption'].includes(originalCategory))
    errors['category'] = 'Choose the earlier classification for this outdated information.';
  const source = sourceError(category as IncidentCategory, sourceDescription, sourceUnknown);
  if (source) errors['source'] = source;
  return errors;
}

export function provenanceValue(group: ProvenanceGroup) {
  const { category, originalCategory, sourceDescription, sourceUnknown } = group.getRawValue();
  return {
    category: category as IncidentCategory,
    originalCategory: category === 'Outdated' ? originalCategory as KnownCategory : null,
    sourceDescription: sourceUnknown ? null : sourceDescription.trim(),
    sourceUnknown,
  };
}

/** In-page link to a timeline entry: focus the target without a router navigation. */
export function focusEntry(event: Event, entryId: string): void {
  event.preventDefault();
  const target = document.getElementById(`incident-entry-${entryId}`);
  target?.scrollIntoView?.({ block: 'center' });
  target?.focus();
}
