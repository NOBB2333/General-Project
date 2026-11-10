import { defineReferenceFixture } from '@/components/page';
import { warrantyModel } from './warranty.model';

export const warrantyReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/service/warranty',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  rows: warrantyModel.rows,
});
