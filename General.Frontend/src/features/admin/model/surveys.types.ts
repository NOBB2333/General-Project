export type SurveyStatus = '已发布' | '草稿' | '进行中' | '已结束';

export interface SurveyRecord {
  id: string;
  code: string;
  category: string;
  name: string;
  audience: string;
  responses: string;
  score: string;
  owner: string;
  date: string;
  status: SurveyStatus;
}

export interface SurveyDraft {
  name: string;
  category: string;
  audience: string;
}
