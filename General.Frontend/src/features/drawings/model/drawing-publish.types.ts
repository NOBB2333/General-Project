import type { ListPageRow } from '@/components/page';

export type DrawingPublishStatus = '待发布' | '待检查' | '检查通过' | '待确认发布' | '已发布' | '发布失败' | '已取消';

export interface DrawingPublishRow extends ListPageRow {
  drawingNo: string;
  name: string;
  pendingVersion: string;
  currentVersion: string;
  reviewStatus: string;
  publishStatus: DrawingPublishStatus;
  majorChange: '是' | '否';
  approvedAt: string;
  effectiveDate: string;
  impactCount: string;
  publishedAt: string;
}

export interface DrawingPublishHistoryRow {
  id: string;
  drawingNo: string;
  name: string;
  publishedVersion: string;
  previousVersion: string;
  publishedAt: string;
  publisher: string;
  effectiveDate: string;
}

export interface DrawingPublishFilters {
  drawingNo: string;
  name: string;
  pendingVersion: string;
  publishStatus: string;
  reviewStatus: string;
  currentVersion: string;
  publishedFrom: string;
  publishedTo: string;
  effectiveFrom: string;
  effectiveTo: string;
  majorChange: string;
  hasImpact: string;
}
