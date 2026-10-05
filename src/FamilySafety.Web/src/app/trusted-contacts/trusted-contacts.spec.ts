import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrustedContacts, TrustedContactView } from './trusted-contacts';

const entry: TrustedContactView = { trustedContactId: 'one', displayName: '<b>Manual friend</b>', phoneNumber: '+44 20 7946 0958',
  emailAddress: 'friend@example.invalid', relationshipLabel: 'Neighbor', version: 'version-one' };

describe('Private manual Trusted Contacts (WI-095-007)', () => {
  let fixture: ComponentFixture<TrustedContacts>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => [...root().querySelectorAll('button')].find(b => b.textContent?.trim() === label)!;
  function create(profile: string | null = 'owner') {
    fixture = TestBed.createComponent(TrustedContacts); fixture.componentRef.setInput('ownProfileId', profile); fixture.detectChanges();
  }
  function list(rows: TrustedContactView[] = []) { http.expectOne('/api/trusted-contacts').flush(rows); fixture.detectChanges(); }
  async function fill(name = ' Manual friend ', phone = ' +44 20 7946 0958 ', email = ' friend@example.invalid ', label = ' Neighbor ') {
    for (const [id, value] of [['name', name], ['phone', phone], ['email', email], ['label', label]]) {
      const input = root().querySelector<HTMLInputElement>(`#trusted-contact-${id}`)!;
      input.value = value; input.dispatchEvent(new Event('input'));
    }
    await fixture.whenStable(); fixture.detectChanges();
  }
  function submit() { root().querySelector('form')!.dispatchEvent(new Event('submit')); }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TrustedContacts], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });

  it('shows loading, error/Retry and an honest empty state without a Family selector', () => {
    create(); expect(text()).toContain('Loading Trusted Contacts');
    http.expectOne('/api/trusted-contacts').flush({}, { status: 500, statusText: 'Failure' }); fixture.detectChanges();
    expect(text()).toContain('could not be loaded'); expect(button('Add a Trusted Contact')).toBeUndefined();
    button('Retry loading Trusted Contacts').click(); list();
    expect(text()).toContain('You have no Trusted Contacts yet'); expect(root().querySelector('select')).toBeNull();
  });
  it('submits only the four manual fields and shows success', async () => {
    create(); list(); button('Add a Trusted Contact').click(); fixture.detectChanges(); await fixture.whenStable();
    expect(button('Create Trusted Contact').disabled).toBe(true); await fill(); submit();
    const request = http.expectOne('/api/trusted-contacts'); expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ displayName: 'Manual friend', phoneNumber: '+44 20 7946 0958', emailAddress: 'friend@example.invalid', relationshipLabel: 'Neighbor' });
    request.flush(entry); list([entry]); expect(text()).toContain('Trusted Contact added'); expect(root().querySelector('form')).toBeNull();
  });
  it('edits with the current version and permits a phone-only record', async () => {
    create(); list([entry]); button('Edit contact').click(); fixture.detectChanges(); await fixture.whenStable();
    await fill('Changed', '+1 202 555 0123', '', ''); submit();
    const request = http.expectOne('/api/trusted-contacts/one'); expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ version: 'version-one', displayName: 'Changed', phoneNumber: '+1 202 555 0123', emailAddress: null, relationshipLabel: null });
    request.flush({ ...entry, displayName: 'Changed' }); list([{ ...entry, displayName: 'Changed' }]); expect(text()).toContain('Trusted Contact updated');
  });
  it('requires a name and at least one valid method without requiring a relationship', async () => {
    create(); list(); button('Add a Trusted Contact').click(); fixture.detectChanges(); await fixture.whenStable();
    await fill('', '', 'x@example.invalid', ''); expect(button('Create Trusted Contact').disabled).toBe(true);
    await fill('Name', '', '', ''); expect(button('Create Trusted Contact').disabled).toBe(true);
    await fill('Name', 'invalid', 'x@example.invalid', ''); expect(button('Create Trusted Contact').disabled).toBe(true);
    await fill('Name', '', 'invalid', ''); expect(button('Create Trusted Contact').disabled).toBe(true);
    await fill('Name', '', 'x@example.invalid', ''); expect(button('Create Trusted Contact').disabled).toBe(false);
  });
  it('keeps a draft on duplicate/validation failure and allows correction', async () => {
    create(); list([entry]); button('Add a Trusted Contact').click(); fixture.detectChanges(); await fixture.whenStable();
    await fill(); submit(); http.expectOne('/api/trusted-contacts').flush({ code: 'DuplicateContact' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges(); expect(text()).toContain('already have a Trusted Contact'); expect(root().querySelector('form')).not.toBeNull();
    await fill('Corrected', '', 'new@example.invalid'); submit();
    http.expectOne('/api/trusted-contacts').flush({ errors: { EmailAddress: ['Check email.'] } }, { status: 400, statusText: 'Invalid' });
    fixture.detectChanges(); expect(text()).toContain('Check email.'); expect(button('Create Trusted Contact').disabled).toBe(false);
  });
  it('removes only after confirmation with its version and supports cancellation', () => {
    create(); list([entry]); button('Remove contact').click(); fixture.detectChanges();
    expect(text()).toContain('permanently deleted'); http.expectNone('/api/trusted-contacts/one?version=version-one');
    button('Cancel removal').click(); fixture.detectChanges(); expect(button('Confirm removal')).toBeUndefined();
    button('Remove contact').click(); fixture.detectChanges(); button('Confirm removal').click();
    const request = http.expectOne('/api/trusted-contacts/one?version=version-one'); expect(request.request.method).toBe('DELETE');
    request.flush(null); list(); expect(text()).toContain('permanently removed'); expect(text()).not.toContain(entry.emailAddress!);
  });
  it('renders owner text safely and explains the zero-access boundary', () => {
    create(); list([entry]); expect(text()).toContain(entry.displayName); expect(root().querySelector('b')).toBeNull();
    expect(text()).toContain('does not give them access'); expect(text()).toContain('does not give them access to your data or send notifications');
    expect(root().querySelector('a')).toBeNull(); // No generated contact/profile/use links.
  });
  it('clears private content on stale or denied mutation and requires a fresh read', async () => {
    create(); list([entry]); button('Edit contact').click(); fixture.detectChanges(); await fixture.whenStable(); await fill(); submit();
    http.expectOne('/api/trusted-contacts/one').flush({ code: 'ContactChanged' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges(); expect(text()).toContain('contact changed'); expect(text()).not.toContain(entry.emailAddress!);
    expect(root().querySelector('form')).toBeNull(); button('Retry loading Trusted Contacts').click(); list([entry]);
    button('Remove contact').click(); fixture.detectChanges(); button('Confirm removal').click();
    http.expectOne('/api/trusted-contacts/one?version=version-one').flush(null, { status: 404, statusText: 'Unavailable' });
    fixture.detectChanges(); expect(text()).toContain('currently unavailable'); expect(text()).not.toContain(entry.emailAddress!);
  });
  it('cancels old reads and mutations and clears all drafts on identity change', async () => {
    create(); const oldRead = http.expectOne('/api/trusted-contacts');
    fixture.componentRef.setInput('ownProfileId', 'other-owner'); fixture.detectChanges(); expect(oldRead.cancelled).toBe(true); list([entry]);
    button('Edit contact').click(); fixture.detectChanges(); await fixture.whenStable(); await fill(); submit();
    const oldWrite = http.expectOne('/api/trusted-contacts/one');
    fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges(); expect(oldWrite.cancelled).toBe(true);
    expect(text()).not.toContain(entry.emailAddress!); expect(root().querySelector('form')).toBeNull();
    fixture.componentRef.setInput('ownProfileId', 'next-owner'); fixture.detectChanges(); list();
    expect(text()).toContain('no Trusted Contacts yet'); expect(text()).not.toContain('updated');
  });
});
