export interface DrawingRecentQuery {
  id: string;
  drawingNo: string;
  name: string;
  queriedAt: string;
  source: string;
}

export interface DrawingAssociationResult {
  id: string;
  category: string;
  code: string;
  description: string;
  status: string;
}

export interface DrawingLinksFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  rows: readonly DrawingRecentQuery[];
  associations: readonly DrawingAssociationResult[];
}
