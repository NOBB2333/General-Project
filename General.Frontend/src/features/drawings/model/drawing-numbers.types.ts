export interface DrawingArchiveRow {
  id: string;
  drawingNo: string;
  name: string;
  type: string;
  version: string;
  status: string;
  pdf: boolean;
  cad: boolean;
  model3d: boolean;
  updatedAt: string;
  folder: string;
}

export interface DrawingFolder {
  id: string;
  label: string;
  count: number;
  depth: number;
}

export interface DrawingNumbersFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  rows: readonly DrawingArchiveRow[];
  folders: readonly DrawingFolder[];
}
