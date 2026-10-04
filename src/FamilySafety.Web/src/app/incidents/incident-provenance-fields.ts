import { Component, DestroyRef, Input, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { IncidentCategory } from './incident-api';
import { FieldErrors, ProvenanceGroup, categoryHelp } from './incident-shared';

/**
 * Category radios, the earlier classification for Outdated, and "source or explicitly unknown".
 * Confirmed always needs a described source, so "Source is unknown" is disabled while it is chosen.
 */
@Component({
  selector: 'app-incident-provenance-fields',
  imports: [ReactiveFormsModule],
  template: `
    @if (showCategory) {
    <fieldset class="incident-fieldset" [attr.aria-describedby]="errors['category'] ? prefix + '-category-error' : null">
      <legend>{{ legend }}</legend>
      <div class="incident-options">
        @for (category of categories; track category) {
          <label class="incident-option">
            <input type="radio" [name]="prefix + '-category'" [value]="category" [formControl]="group.controls.category" />
            <span><strong>{{ category }}</strong> <span class="incident-hint">{{ help(category) }}</span></span>
          </label>
        }
      </div>
      @if (group.controls.category.value === 'Outdated') {
        <label [for]="prefix + '-original'">Earlier classification</label>
        <select [id]="prefix + '-original'" [formControl]="group.controls.originalCategory">
          <option value="Confirmed">Confirmed</option>
          <option value="Reported">Reported</option>
          <option value="Assumption">Assumption</option>
        </select>
        <p class="incident-hint">The entry keeps its earlier classification and is also marked Outdated.</p>
      }
      @if (errors['category']) { <p class="field-error" [id]="prefix + '-category-error'">{{ errors['category'] }}</p> }
    </fieldset>
    }
    <fieldset class="incident-fieldset" [attr.aria-describedby]="errors['source'] ? prefix + '-source-error' : null">
      <legend>{{ sourceLegend }}</legend>
      @if (!group.controls.sourceUnknown.value) {
        <label [for]="prefix + '-source'">Source description</label>
        <input [id]="prefix + '-source'" type="text" maxlength="500" [formControl]="group.controls.sourceDescription"
          [attr.aria-invalid]="errors['source'] ? true : null" [attr.aria-describedby]="errors['source'] ? prefix + '-source-error' : null" />
      }
      <label class="incident-option">
        <input type="checkbox" [formControl]="group.controls.sourceUnknown" /> <span>Source is unknown</span>
      </label>
      @if (group.controls.category.value === 'Confirmed') {
        <p class="incident-hint">Confirmed information needs a described source.</p>
      }
      @if (errors['source']) { <p class="field-error" [id]="prefix + '-source-error'">{{ errors['source'] }}</p> }
    </fieldset>
  `,
  styleUrl: './incident-forms.scss',
})
export class IncidentProvenanceFields implements OnInit {
  @Input({ required: true }) group!: ProvenanceGroup;
  @Input({ required: true }) prefix!: string;
  @Input() categories: readonly IncidentCategory[] = ['Confirmed', 'Reported', 'Assumption', 'Outdated'];
  @Input() showCategory = true;
  @Input() legend = 'How is this known?';
  @Input() sourceLegend = 'Source';
  @Input() errors: FieldErrors = {};

  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.syncSource(this.group.controls.category.value);
    this.group.controls.category.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(category => this.syncSource(category));
  }

  protected help(category: IncidentCategory): string {
    return category === 'Outdated' ? 'Was known earlier but may no longer be current.' : categoryHelp[category];
  }

  private syncSource(category: string): void {
    const unknown = this.group.controls.sourceUnknown;
    if (category === 'Confirmed') {
      unknown.setValue(false, { emitEvent: false });
      unknown.disable({ emitEvent: false });
    } else if (unknown.disabled) {
      unknown.enable({ emitEvent: false });
    }
  }
}
