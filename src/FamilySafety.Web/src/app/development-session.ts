import { HttpClient, HttpInterceptorFn } from '@angular/common/http';
import { Component, Injectable, inject, isDevMode, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { CurrentFamilyContext } from './family-context';

interface TestIdentity { accountId: string; label: string }
interface IdentityOptions { defaultAccountId: string; identities: TestIdentity[] }
interface OutboxMessage { recipientEmail: string; invitationUrl: string; createdAt: string }

const selectedDevelopmentIdentityStorageKey = 'family-safety.development-identity';

function readSelectedDevelopmentIdentity(identities: readonly TestIdentity[]): string | null {
  try {
    const selectedAccountId = sessionStorage.getItem(selectedDevelopmentIdentityStorageKey);
    if (selectedAccountId && identities.some(identity => identity.accountId === selectedAccountId))
      return selectedAccountId;
    if (selectedAccountId) sessionStorage.removeItem(selectedDevelopmentIdentityStorageKey);
  } catch {
    // Storage may be unavailable; Development can still use its normal fixture default.
  }
  return null;
}

function persistSelectedDevelopmentIdentity(accountId: string): void {
  if (!isDevMode()) return;
  try {
    sessionStorage.setItem(selectedDevelopmentIdentityStorageKey, accountId);
  } catch {
    // Storage may be unavailable; the active identity still applies until the next reload.
  }
}

@Injectable({ providedIn: 'root' })
export class DevelopmentIdentity {
  readonly accountId = signal<string | null>(null);
}

export const developmentIdentityInterceptor: HttpInterceptorFn = (request, next) => {
  const accountId = inject(DevelopmentIdentity).accountId();
  return next(isDevMode() && accountId && request.url.startsWith('/api/')
    ? request.clone({ setHeaders: { 'X-Development-Account': accountId } })
    : request);
};

// Route replacement destroys the previous user's forms, lists, selections,
// invitation state, and subscriptions, including Trips.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    @if (identities().length) {
      <aside aria-label="Development test tools" class="development-tools">
        <strong>Development test identities</strong>
        <label for="development-account">Active test identity</label>
        <select id="development-account"
          (change)="switchIdentity($any($event.target).value)">
          @for (account of identities(); track account.accountId) {
            <option [value]="account.accountId" [selected]="identity.accountId() === account.accountId">
              {{ account.label }}
            </option>
          }
        </select>
        <button type="button" (click)="loadOutbox()">Show Development invitation links</button>
        <p>Local test tools. Invitations below are not production email delivery.</p>
        @if (outboxError()) { <p role="alert">{{ outboxError() }}</p> }
        @for (message of messages(); track message.invitationUrl) {
          <p>{{ message.recipientEmail }} — {{ message.createdAt }}
            <a [href]="message.invitationUrl">Open invitation</a>
          </p>
        }
      </aside>
    }
    @if (ready()) {
      <router-outlet />
    } @else { <p role="status">Loading application…</p> }
  `,
  styles: [`
    .development-tools {
      align-items: center;
      background: var(--color-warning-bg);
      border-bottom: 1px solid #e8d48f;
      color: var(--color-warning);
      display: flex;
      flex-wrap: wrap;
      font-size: .85rem;
      gap: .35rem .75rem;
      padding: .35rem 1rem;
    }
    .development-tools label { font-weight: 700; }
    .development-tools select { width: auto; max-width: 100%; }
    .development-tools select,
    .development-tools button {
      border: 1px solid #947827;
      border-radius: var(--radius-sm);
      font: inherit;
      min-height: 2rem;
      padding: .2rem .6rem;
    }
    .development-tools button {
      background: #5c4811;
      color: #fff;
      cursor: pointer;
      font-weight: 700;
    }
    .development-tools p { margin: 0; }
    .development-tools p:has(a), .development-tools [role='alert'] { flex-basis: 100%; }
    .development-tools a { color: var(--color-warning); font-weight: 700; }
    .development-tools :is(select, button, a):focus-visible {
      outline: 3px solid var(--color-focus);
      outline-offset: 3px;
    }
    @media (max-width: 40rem) {
      .development-tools { margin-inline: .75rem; }
      .development-tools label,
      .development-tools select,
      .development-tools button { flex: 1 1 100%; }
    }
  `],
})
export class DevelopmentSession {
  protected readonly identity = inject(DevelopmentIdentity);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly familyContext = inject(CurrentFamilyContext);
  protected readonly identities = signal<TestIdentity[]>([]);
  protected readonly messages = signal<OutboxMessage[]>([]);
  protected readonly outboxError = signal('');
  protected readonly ready = signal(!isDevMode());
  private outboxVersion = 0;

  constructor() {
    if (!isDevMode()) return;
    this.http.get<IdentityOptions>('/api/development/identities').subscribe({
      next: (options) => {
        this.identities.set(options.identities);
        this.identity.accountId.set(
          readSelectedDevelopmentIdentity(options.identities) ?? options.defaultAccountId,
        );
        this.ready.set(true);
      },
      error: () => this.ready.set(true), // Ordinary Development configuration has no switcher.
    });
  }

  protected switchIdentity(accountId: string): void {
    if (!this.identities().some(account => account.accountId === accountId)
      || accountId === this.identity.accountId()) return;
    this.outboxVersion++;
    this.messages.set([]);
    this.outboxError.set('');
    history.replaceState(null, '', location.pathname + location.search);
    this.familyContext.clear();
    persistSelectedDevelopmentIdentity(accountId);
    this.identity.accountId.set(accountId);
    // A navigation to the same URL reuses its component. Pass through another
    // existing route so the old account's component is destroyed first.
    const currentPath = this.router.url.split(/[?#]/)[0];
    const resetPath = currentPath === '/me' ? '/family' : '/me';
    void this.router.navigateByUrl(resetPath, { skipLocationChange: true }).then(() =>
      this.router.navigateByUrl('/home', { replaceUrl: true }));
  }

  protected loadOutbox(): void {
    const version = ++this.outboxVersion;
    this.outboxError.set('');
    this.http.get<OutboxMessage[]>('/api/development/invitation-outbox').subscribe({
      next: (messages) => { if (version === this.outboxVersion) this.messages.set(messages); },
      error: () => { if (version === this.outboxVersion) this.outboxError.set('Development links could not be loaded.'); },
    });
  }
}
