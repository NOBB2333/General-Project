export type PageActionKind =
  'create' | 'import' | 'export' | 'print' | 'refresh' | 'reset' | 'configure' | 'batch' | 'delete' | 'custom';

export interface PageAction {
  /** Stable behavior identifier; labels may change without changing behavior. */
  id: string;
  label: string;
  kind: PageActionKind;
  tone?: 'default' | 'primary' | 'danger';
  requiresSelection?: boolean;
}

export type PageReplicationStatus = 'scaffold' | 'evidence-captured' | 'implemented' | 'verified';

interface PageReplicationProgressMeta {
  status: Exclude<PageReplicationStatus, 'verified'>;
  referenceUrl: string;
  referenceScreenshot?: string;
  localScreenshot?: string;
  evidenceDocument?: string;
  capturedAt?: string;
}

interface VerifiedPageReplicationMeta {
  status: 'verified';
  referenceUrl: string;
  referenceScreenshot: string;
  localScreenshot: string;
  evidenceDocument: string;
  capturedAt: string;
}

export type PageReplicationMeta = PageReplicationProgressMeta | VerifiedPageReplicationMeta;
