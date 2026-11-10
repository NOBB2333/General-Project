export type KnowledgeStatus = '已发布' | '草稿';
export interface KnowledgeArticle {
  id: string;
  code: string;
  title: string;
  subtitle: string;
  category: string;
  productLine: string;
  status: KnowledgeStatus;
  tags: readonly string[];
  views: number;
  references: number;
  updatedAt: string;
  phenomenon: string;
  cause: string;
  steps: readonly string[];
  parts: readonly string[];
  tickets: readonly string[];
}
export interface KnowledgeDraft {
  title: string;
  category: string;
  productLine: string;
  phenomenon: string;
  solution: string;
}
