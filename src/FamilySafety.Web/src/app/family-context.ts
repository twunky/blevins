import { Injectable, signal } from '@angular/core';

// This is an in-memory presentation choice, never an authorization source.
@Injectable({ providedIn: 'root' })
export class CurrentFamilyContext {
  readonly familyId = signal<string | null>(null);

  clear(): void { this.familyId.set(null); }
}
