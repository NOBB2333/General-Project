export type MeetingType = '项目会议' | '经营分析' | '技术评审' | '部门会议' | '全体会议' | '专题讨论';
export type MeetingStatus = '草稿' | '已发送' | '已归档';
export interface MeetingNote {
  id: string;
  code: string;
  title: string;
  type: MeetingType;
  date: string;
  time: string;
  host: string;
  attendees: string[];
  attendeeExtra: number;
  todoCount: number;
  pendingTodoCount: number;
  status: MeetingStatus;
  summary: string;
}
export interface MeetingNoteDraft {
  title: string;
  type: MeetingType;
  date: string;
  time: string;
  host: string;
  attendees: string;
  summary: string;
}
