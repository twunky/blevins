import { HttpClient } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, switchMap, takeUntil, throwError } from 'rxjs';
import { AppIcon } from '../icon';
import { AdoptItemFamily, isItemId } from '../item-route';
import { focusHeadingState } from '../navigation';

interface SharedProfile {
  profileId: string; displayName: string; contactEmail: string | null; contactPhone: string | null;
  birthdayMonth: number | null; birthdayDay: number | null;
}

/** Both reads are server-authorized. A Family selection or known profile ID never grants access. */
@Component({ selector: 'app-member-profile', imports: [RouterLink, AppIcon],
  templateUrl: './member-profile.html', styleUrl: './member-profile.scss' })
export class MemberProfile implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() profileId: string | null = null;
  @Input({ required: true }) adoptFamily!: AdoptItemFamily;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  private generation = 0;
  protected readonly state = signal<'loading' | 'ready' | 'unavailable'>('loading');
  protected readonly profile = signal<SharedProfile | null>(null);
  protected readonly focusHeadingState = focusHeadingState;
  constructor() { this.destroyRef.onDestroy(() => { ++this.generation; this.changed.next(); }); }
  ngOnChanges(): void { this.refresh(); }
  protected initials(name: string): string { return name.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join('').toLocaleUpperCase(); }
  protected refresh(): void {
    const generation = ++this.generation;
    this.changed.next(); this.profile.set(null); this.state.set('loading');
    const familyId = this.familyId, profileId = this.profileId;
    if (!isItemId(familyId) || !isItemId(profileId)) { this.state.set('unavailable'); return; }
    this.http.get<{ profileId: string }[]>('/api/family-groups/' + encodeURIComponent(familyId) + '/members')
      .pipe(switchMap(members => Array.isArray(members) && members.some(member => member.profileId === profileId)
        ? this.http.get<SharedProfile>('/api/family-members/' + encodeURIComponent(profileId) + '/profile')
        : throwError(() => new Error('Unavailable'))), takeUntil(this.changed), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: async profile => {
          if (generation !== this.generation) return;
          if (profile.profileId !== profileId) { this.state.set('unavailable'); return; }
          const outcome = await this.adoptFamily(familyId, 'member', () => generation === this.generation);
          if (generation !== this.generation) return;
          if (outcome === 'unavailable') { this.state.set('unavailable'); return; }
          this.profile.set(profile); this.state.set('ready');
        },
        error: () => { if (generation === this.generation) this.state.set('unavailable'); },
      });
  }
}
