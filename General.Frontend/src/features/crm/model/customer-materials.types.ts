export interface CustomerMaterialRow {
  id: string;
  customer: string;
  internalCode: string;
  internalName: string;
  internalSpec: string;
  unit: string;
  customerCode: string;
  customerName: string;
  remark: string;
  maintainedAt: string;
  maintainedBy: string;
}

export interface CustomerMaterialDraft {
  customer: string;
  internalCode: string;
  internalName: string;
  internalSpec: string;
  unit: string;
  customerCode: string;
  customerName: string;
  remark: string;
}

export type CustomerMaterialImportMode = 'file' | 'paste';
export type CustomerMaterialToolbarAction = 'create' | 'import-file' | 'import-paste' | 'export';
