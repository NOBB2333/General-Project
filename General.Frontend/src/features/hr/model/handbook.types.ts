export interface HandbookClause {
  id: string;
  number: string;
  title: string;
  content: string;
  important?: boolean;
}

export interface HandbookChapter {
  id: string;
  number: number;
  title: string;
  updatedAt: string;
  tone: 'blue' | 'green' | 'purple' | 'orange' | 'cyan';
  clauses: readonly HandbookClause[];
}
