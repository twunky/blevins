/**
 * Item URLs (REQ-UX-014, WI-009-005). An item route carries the item ID only; the URL, the navigation area, the
 * selected Family, and the client's Family list never authorize anything (REQ-PRIV-027). The item is read first,
 * the server authorizes it, and only then does the shell decide the Family context.
 */

export type ItemKind = 'event' | 'plan' | 'trip' | 'concern' | 'incident';

/**
 * The shell's Family decision for an item the server has just returned (200):
 * - `current`: the item's Family is already selected; nothing changes and nothing is announced.
 * - `switched`: the selection changed to the item's Family; Family-scoped state was cleared, reloaded, and announced.
 * - `retained`: Trip only (OD-A). The Family is still absent after one refresh; nothing changes and nothing is announced.
 * - `unavailable`: any other type (OD-B). The Family is still absent after one refresh; show the generic unavailable state.
 */
export type ItemFamilyOutcome = 'current' | 'switched' | 'retained' | 'unavailable';

/** Call only with the FamilyGroupId of an item the server has just returned. Hosts render after it resolves. */
export type AdoptItemFamily = (familyGroupId: string, kind: ItemKind) => Promise<ItemFamilyOutcome>;

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** A route ID that is not a GUID is shown as unavailable without any API request. */
export function isItemId(value: unknown): value is string {
  return typeof value === 'string' && guidPattern.test(value);
}
