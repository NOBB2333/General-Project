export type PublicSeaLevel = 'A' | 'B' | 'C';
export type PublicSeaSource = '主动释放' | '超期回收' | '合并释放' | '新建';
export type PublicSeaTab = 'all' | 'recent' | 'dormant' | 'valuable';

export interface PublicSeaRow {
  id: string;
  customer: string;
  industry: string;
  level: PublicSeaLevel;
  contact: string;
  city: string;
  source: PublicSeaSource;
  estimatedValue: number;
  releaseDays: number;
  followed?: boolean;
}

export interface PublicSeaDraft {
  customer: string;
  industry: string;
  level: PublicSeaLevel;
  contact: string;
  city: string;
  source: PublicSeaSource;
  estimatedValue: string;
}
