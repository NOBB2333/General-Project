export type DocumentType = 'DOCX' | 'PDF' | 'PPTX' | 'XLSX';

export interface DocumentRecord {
  id: string;
  name: string;
  description: string;
  type: DocumentType;
  size: string;
  publisher: string;
  department: string;
  updatedAt: string;
  version: string;
  downloads: number;
  references: number;
  folder: string;
  favorite?: boolean;
}

export interface DocumentDraft {
  name: string;
  description: string;
  type: DocumentType;
  folder: string;
}
