export interface TrainingCourse {
  id: string;
  title: string;
  category: string;
  required: boolean;
  hours: number;
  learners: number;
  completion: number;
  status: '已发布' | '草稿';
}
export interface TrainingTask {
  id: string;
  title: string;
  date: string;
  urgent?: boolean;
  kind: '考试' | '课程' | '证书';
}
