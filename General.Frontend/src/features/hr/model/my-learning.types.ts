export type LearningView = '学习总览' | '必修任务' | '我的课程' | '我的考试' | '我的证书';
export interface LearningCourse {
  id: string;
  title: string;
  category: string;
  lastLearned: string;
  progress: number;
  deadline: string;
  required: boolean;
  status: '在学' | '已完成';
}
export interface LearningCertificate {
  id: string;
  title: string;
  code: string;
  expiresAt: string;
  status: '有效' | '即将到期';
}
