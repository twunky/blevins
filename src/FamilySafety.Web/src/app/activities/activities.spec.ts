import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Activities, ActivityRow } from './activities';

describe('Activities browsing', () => {
  const rows: ActivityRow[] = [
    { id: 'e', type: 'Event', title: 'Dinner', date: '2026-11-10', time: '18:00', timing: 'Dinner time', location: 'Community room', path: '/events/e', upcoming: true },
    { id: 'p', type: 'Plan', title: 'Museum', date: '2026-11-01', time: '09:00', timing: 'Museum time', location: 'Planned activity', path: '/plans/p', upcoming: true },
    { id: 't', type: 'Trip', title: 'Coast', date: '2026-10-01', time: '', timing: 'Trip dates', location: 'Beach', path: '/trips/t', upcoming: false, completed: true },
  ];
  async function open() {
    await TestBed.configureTestingModule({ imports: [Activities], providers: [provideRouter([])] }).compileComponents();
    const fixture = TestBed.createComponent(Activities);
    fixture.componentRef.setInput('rows', rows); fixture.detectChanges();
    return { fixture, root: fixture.nativeElement as HTMLElement };
  }
  it('orders every type together, filters and searches, and keeps completed Trips available in all dates', async () => {
    const { fixture, root } = await open();
    const titles = () => Array.from(root.querySelectorAll('.activity-copy > strong')).map(x => x.textContent);
    expect(titles()).toEqual(['Museum', 'Dinner']);
    const period = root.querySelector('select')!; period.value = 'all'; period.dispatchEvent(new Event('change')); fixture.detectChanges();
    expect(titles()).toEqual(['Coast', 'Museum', 'Dinner']);
    expect(root.querySelector('.activity-row')?.textContent).toContain('Completed');
    const search = root.querySelector('input')!; search.value = ' BEACH '; search.dispatchEvent(new Event('input')); fixture.detectChanges();
    expect(titles()).toEqual(['Coast']);
    search.value = ''; search.dispatchEvent(new Event('input'));
    Array.from(root.querySelectorAll('button')).find(b => b.textContent?.includes('Plans'))!.click(); fixture.detectChanges();
    expect(titles()).toEqual(['Museum']);
    expect(root.querySelector('[aria-pressed="true"]')?.textContent).toContain('Plans');
  });
  it('distinguishes failed reads from empty data and never claims zero pending invitations on error', async () => {
    const { fixture, root } = await open();
    fixture.componentRef.setInput('errors', ['Plans could not be loaded.']);
    fixture.componentRef.setInput('pendingError', 'Invitations unavailable.'); fixture.detectChanges();
    expect(root.textContent).toContain('Counts are incomplete');
    expect(root.textContent).not.toContain('All caught up!');
    let retries = 0; fixture.componentInstance.retry.subscribe(() => retries++);
    Array.from(root.querySelectorAll('button')).find(b => b.textContent === 'Retry loading activities')!.click();
    expect(retries).toBe(1);
  });
});
