import { expect, test, type Page } from '@playwright/test';
import { useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

async function alignedFields(page: Page, ids: string[]) {
  const boxes = await page.evaluate(ids => ids.map(id => {
    const element = document.getElementById(id)!;
    const control = element.getBoundingClientRect();
    const label = document.querySelector(`label[for="${id}"]`)!.getBoundingClientRect();
    return { top: control.top, height: control.height, labelTop: label.top };
  }), ids);
  for (const box of boxes.slice(1)) {
    expect(Math.abs(box.top - boxes[0].top), `${ids}: control tops`).toBeLessThan(0.5);
    expect(Math.abs(box.labelTop - boxes[0].labelTop), `${ids}: label tops`).toBeLessThan(0.5);
    expect(Math.abs(box.height - boxes[0].height), `${ids}: control heights`).toBeLessThan(0.5);
  }
}

for (const width of [1440, 1024, 768, 390]) {
  test(`paired fields tolerate wrapped labels and validation at ${width}px`, async ({ page, request }) => {
    await page.setViewportSize({ width, height: 1000 });
    const families = await (await request.get('http://localhost:5080/api/family-groups/mine')).json();
    const family = families.find((item: { name: string }) => item.name === 'Development Family');
    const open = async (path: string) => {
      await page.goto(path);
      await page.getByLabel('Current Family', { exact: true }).selectOption(family.familyGroupId);
      await page.waitForLoadState('networkidle');
    };
    const noOverflow = async () => expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);

    await open('/me');
    if (width >= 768) {
      await alignedFields(page, ['contact-email', 'contact-email-visibility']);
      await alignedFields(page, ['contact-phone', 'contact-phone-visibility']);
      await alignedFields(page, ['birthday-month', 'birthday-day', 'birthday-visibility']);
    }
    // DOM-only stress content; no profile is saved and no production copy is changed.
    await page.locator('label[for="contact-email-visibility"]').evaluate(element => {
      element.textContent = 'Who can see your optional contact email when you choose to share this detail with active adult members of your Family?';
    });
    await page.locator('label[for="birthday-visibility"]').evaluate(element => {
      element.textContent = 'Who can see your optional birthday month and day when you choose to share it?';
    });
    if (width >= 768) {
      await alignedFields(page, ['contact-email', 'contact-email-visibility']);
      await alignedFields(page, ['birthday-month', 'birthday-day', 'birthday-visibility']);
    }
    const privacyTop = () => page.locator('#contact-email-visibility').evaluate(element => element.getBoundingClientRect().top + window.scrollY);
    const privacyBefore = await privacyTop();
    await page.locator('#contact-email').fill('invalid-email');
    await page.locator('#contact-email').blur();
    await expect(page.locator('#contact-email-error')).toBeVisible();
    await page.locator('#contact-email-help').evaluate(element => {
      element.textContent = 'This optional detail is separate from sign-in information. Additional explanatory text should wrap below the input without moving the neighboring privacy control.';
    });
    const privacyAfter = await privacyTop();
    if (width >= 768) {
      expect(privacyAfter).toBe(privacyBefore);
      await alignedFields(page, ['contact-email', 'contact-email-visibility']);
    } else {
      const feedback = await page.locator('.field-feedback').boundingBox();
      const label = await page.locator('label[for="contact-email-visibility"]').boundingBox();
      expect(label!.y).toBeGreaterThanOrEqual(feedback!.y + feedback!.height);
    }
    await noOverflow();

    await open('/events/new');
    await page.locator('label[for="event-location"]').evaluate(element => {
      element.textContent = 'Location for this Family event, including an optional descriptive meeting place';
    });
    if (width >= 768) {
      await alignedFields(page, ['event-name', 'event-location']);
      await alignedFields(page, ['event-date', 'event-time']);
    }
    await noOverflow();

    await open('/trips');
    await page.getByRole('button', { name: 'Create trip', exact: true }).click();
    if (width >= 768) {
      await alignedFields(page, ['trip-start', 'trip-end']);
      await page.locator('label[for="trip-accommodation"]').evaluate(element => {
        element.textContent = 'Accommodation notes and other optional information about where travelers will stay';
      });
      await alignedFields(page, ['trip-transport', 'trip-accommodation']);
    } else {
      const start = await page.locator('#trip-start').boundingBox();
      const endLabel = await page.locator('label[for="trip-end"]').boundingBox();
      expect(endLabel!.y - start!.y - start!.height).toBeCloseTo(16, 0);
    }
    await noOverflow();

    await open('/family');
    const header = page.locator('.family-main .card-header').first();
    const icon = await header.locator('.card-icon').boundingBox();
    const heading = await header.locator('h2').boundingBox();
    expect(heading!.x).toBeGreaterThan(icon!.x + icon!.width);
    expect(Math.abs(heading!.y - icon!.y)).toBeLessThan(8);
    await noOverflow();
    if (width === 1024) {
      for (const boundaryWidth of [1024, 1100, 1200, 1280]) {
        await page.setViewportSize({ width: boundaryWidth, height: 1000 });
        await open('/me');
        expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(boundaryWidth);
        const card = await page.locator('.profile-birthday').boundingBox();
        const privacy = await page.locator('#birthday-visibility').boundingBox();
        expect(privacy!.x + privacy!.width).toBeLessThanOrEqual(card!.x + card!.width - 24);
      }
    }
  });
}
