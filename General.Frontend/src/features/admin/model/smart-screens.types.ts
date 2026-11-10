export type ScreenActivityTone = '生成' | '发布' | '建议' | '播插' | '审核';

export interface ScreenActivity {
  id: string;
  time: string;
  tone: ScreenActivityTone;
  tag: string;
  text: string;
  status: '成功' | '等待审核';
}
