import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

export interface TrustedContactView {
  trustedContactId: string; displayName: string; phoneNumber: string | null;
  emailAddress: string | null; relationshipLabel: string | null; version: string;
}

/** Personal manual records. No Family input, contact identity lookup or downstream permissions. */
@Component({
  selector: 'app-trusted-contacts', imports: [FormsModule],
  templateUrl: './trusted-contacts.html', styleUrl: './trusted-contacts.scss',
})
export class TrustedContacts implements OnChanges {
  @Input() ownProfileId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  private generation = 0;
  protected readonly state = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly contacts = signal<TrustedContactView[]>([]);
  protected readonly formOpen = signal(false);
  protected readonly editing = signal<TrustedContactView | null>(null);
  protected readonly removing = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected displayName = '';
  protected phoneNumber = '';
  protected emailAddress = '';
  protected relationshipLabel = '';

  ngOnChanges(): void { this.refresh(); }

  protected refresh(preserveMessage = false): void {
    ++this.generation; this.changed.next(); this.cancel(); this.busy.set(false);
    this.contacts.set([]); this.error.set('');
    if (!preserveMessage) this.message.set('');
    if (!this.ownProfileId) { this.state.set('unavailable'); return; }
    this.state.set('loading');
    const generation = this.generation;
    this.http.get<TrustedContactView[]>('/api/trusted-contacts')
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: rows => {
          if (generation !== this.generation) return;
          this.contacts.set(rows); this.state.set('ready');
        },
        error: (error: HttpErrorResponse) => {
          if (generation !== this.generation) return;
          this.state.set(error.status === 401 || error.status === 404 ? 'unavailable' : 'error');
        },
      });
  }
  protected add(): void {
    if (this.busy() || this.state() !== 'ready') return;
    this.cancel(); this.error.set(''); this.message.set(''); this.formOpen.set(true);
  }
  protected edit(row: TrustedContactView): void {
    this.add(); this.editing.set(row);
    this.displayName = row.displayName; this.phoneNumber = row.phoneNumber ?? '';
    this.emailAddress = row.emailAddress ?? ''; this.relationshipLabel = row.relationshipLabel ?? '';
  }
  protected cancel(): void {
    this.formOpen.set(false); this.editing.set(null); this.removing.set(null);
    this.displayName = ''; this.phoneNumber = ''; this.emailAddress = ''; this.relationshipLabel = '';
  }
  protected valid(): boolean {
    const phone = this.phoneNumber.trim(), email = this.emailAddress.trim();
    let digits = phone.replace(/[^0-9]/g, '');
    if (!phone.startsWith('+') && digits.startsWith('00')) digits = digits.slice(2);
    return this.displayName.trim().length > 0 && this.displayName.trim().length <= 100
      && this.relationshipLabel.trim().length <= 100 && !!(phone || email)
      && (!email || (email.length <= 254 && /^[^@\s]+@[^@\s]+$/.test(email)))
      && (!phone || (phone.length <= 32 && /^\+?[0-9 ().-]+$/.test(phone) && digits.length >= 7 && digits.length <= 15));
  }
  protected save(): void {
    if (!this.valid() || this.busy() || this.state() !== 'ready') return;
    const draft = {
      displayName: this.displayName.trim(), phoneNumber: this.phoneNumber.trim() || null,
      emailAddress: this.emailAddress.trim() || null, relationshipLabel: this.relationshipLabel.trim() || null,
    };
    const editing = this.editing();
    const request = editing
      ? this.http.put<TrustedContactView>(`/api/trusted-contacts/${encodeURIComponent(editing.trustedContactId)}`, { ...draft, version: editing.version })
      : this.http.post<TrustedContactView>('/api/trusted-contacts', draft);
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    request.pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (generation === this.generation) { this.message.set(editing ? 'Trusted Contact updated.' : 'Trusted Contact added.'); this.refresh(true); } },
      error: (error: HttpErrorResponse) => { if (generation === this.generation) this.mutationFailed(error); },
    });
  }
  protected confirmRemoval(row: TrustedContactView): void {
    if (this.busy() || this.state() !== 'ready' || this.removing() !== row.trustedContactId) return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    this.http.delete(`/api/trusted-contacts/${encodeURIComponent(row.trustedContactId)}?version=${encodeURIComponent(row.version)}`)
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => { if (generation === this.generation) { this.message.set('Trusted Contact permanently removed.'); this.refresh(true); } },
        error: (error: HttpErrorResponse) => { if (generation === this.generation) this.mutationFailed(error); },
      });
  }
  private mutationFailed(error: HttpErrorResponse): void {
    this.busy.set(false);
    if (error.status === 400) {
      const errors = error.error?.errors as Record<string, string[]> | undefined;
      this.error.set(errors ? Object.values(errors).flat().join(' ') : 'Check the name and contact information. At least one valid phone number or email is required.');
      return;
    }
    if (error.status === 409 && error.error?.code === 'DuplicateContact') {
      this.error.set('You already have a Trusted Contact with this phone number or email address.'); return;
    }
    // Unknown outcome, stale version or lost authorization: discard private state and require a fresh read.
    this.cancel(); this.contacts.set([]);
    this.state.set(error.status === 401 || error.status === 404 ? 'unavailable' : 'error');
    this.error.set(error.status === 409 ? 'This contact changed. Refresh your contacts before trying again.'
      : 'The change could not be confirmed. Refresh your contacts to check their current state and access.');
  }
}
