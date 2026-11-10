export interface ReferenceViewport {
  width: number;
  height: number;
}

export interface PageReferenceFixture<TRow = { id: string }> {
  sourceUrl: string;
  capturedAt: string;
  viewport?: ReferenceViewport;
  rows: readonly TRow[];
  defaults?: Readonly<Record<string, string | number | boolean | null | readonly (string | number | boolean | null)[]>>;
}

/** Type-only fixture boundary: feature fixtures remain literal and reviewable. */
export function defineReferenceFixture<const TFixture extends PageReferenceFixture<unknown>>(
  fixture: TFixture,
): TFixture;
export function defineReferenceFixture<
  TRow,
  const TFixture extends PageReferenceFixture<TRow> = PageReferenceFixture<TRow> & Record<string, unknown>,
>(fixture: TFixture): TFixture;
export function defineReferenceFixture(fixture: PageReferenceFixture<unknown>): PageReferenceFixture<unknown> {
  return fixture;
}

/** Local interactions always mutate a clone, never the captured reference rows. */
export function cloneReferenceRows<TRow extends object>(rows: readonly TRow[]): TRow[] {
  return structuredClone(toRaw(rows).map((row) => toRaw(row))) as TRow[];
}
import { toRaw } from 'vue';
