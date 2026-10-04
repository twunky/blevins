import { Component, Input } from '@angular/core';

export type IconName =
  | 'home' | 'users' | 'calendar' | 'clipboard' | 'plane' | 'user' | 'shield'
  | 'alert' | 'link' | 'chevron' | 'plus' | 'heart' | 'check' | 'message' | 'tasks' | 'share' | 'settings' | 'info' | 'back';

/** Decorative outline icons for navigation and card headers. Always hidden from assistive technology. */
@Component({
  selector: 'app-icon',
  template: `
    <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8"
      stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">
      @switch (name) {
        @case ('home') { <path d="M3 10.5 12 3l9 7.5"/><path d="M5 9.5V21h5v-6h4v6h5V9.5"/> }
        @case ('users') { <circle cx="9" cy="8" r="3.5"/><path d="M2.5 20a6.5 6.5 0 0 1 13 0"/><path d="M16 4.5a3.5 3.5 0 0 1 0 7"/><path d="M18 14.2A6.5 6.5 0 0 1 21.5 20"/> }
        @case ('calendar') { <rect x="3.5" y="5" width="17" height="15.5" rx="2.5"/><path d="M3.5 10h17M8 3v4M16 3v4"/> }
        @case ('clipboard') { <rect x="5" y="4.5" width="14" height="16.5" rx="2"/><path d="M9 3h6v3H9zM8.5 11h7M8.5 15h5"/> }
        @case ('plane') { <path d="M21 15.5 13.5 11V5a1.5 1.5 0 0 0-3 0v6L3 15.5V17l7.5-2.2V19L8 20.5V22l4-1 4 1v-1.5L13.5 19v-4.2L21 17z"/> }
        @case ('user') { <circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/> }
        @case ('shield') { <path d="M12 3 4.5 6v5.5c0 4.6 3.2 8.2 7.5 9.5 4.3-1.3 7.5-4.9 7.5-9.5V6z"/> }
        @case ('alert') { <circle cx="12" cy="12" r="9"/><path d="M12 7.5v5.5M12 16.5h.01"/> }
        @case ('link') { <path d="M10 14a4.5 4.5 0 0 0 6.4 0l3-3a4.5 4.5 0 0 0-6.4-6.4l-1.2 1.2"/><path d="M14 10a4.5 4.5 0 0 0-6.4 0l-3 3a4.5 4.5 0 0 0 6.4 6.4l1.2-1.2"/> }
        @case ('chevron') { <path d="m9 6 6 6-6 6"/> }
        @case ('plus') { <path d="M12 5v14M5 12h14"/> }
        @case ('check') { <circle cx="12" cy="12" r="9"/><path d="m8 12.5 2.8 2.8L16.5 9.5"/> }
        @case ('message') { <path d="M4 5.5h16v10.5H9.5L5 19.5V16H4z"/><path d="M8 9.5h8M8 12.5h5"/> }
        @case ('tasks') { <rect x="3.5" y="4.5" width="5" height="5" rx="1"/><path d="m4.8 15.8 1.4 1.4 2.6-2.8M12 7h8.5M12 16h8.5"/> }
        @case ('share') { <circle cx="18" cy="5.5" r="2.5"/><circle cx="6" cy="12" r="2.5"/><circle cx="18" cy="18.5" r="2.5"/><path d="m8.2 10.8 7.6-4.1M8.2 13.2l7.6 4.1"/> }
        @case ('settings') { <circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.6 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.6a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/> }
        @case ('info') { <circle cx="12" cy="12" r="9"/><path d="M12 11v5.5M12 7.5h.01"/> }
        @case ('back') { <path d="m15 6-6 6 6 6"/> }
        @case ('heart') { <path d="M12 20s-7.5-4.6-7.5-10.2A4.3 4.3 0 0 1 12 7a4.3 4.3 0 0 1 7.5 2.8C19.5 15.4 12 20 12 20z"/> }
      }
    </svg>
  `,
  styles: [':host { display: inline-flex; flex: none; line-height: 0; }'],
})
export class AppIcon {
  @Input({ required: true }) name!: IconName;
}
