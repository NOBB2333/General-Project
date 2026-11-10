export type AttendanceStatus = '正常' | '迟到' | '早退' | '迟到+早退' | '旷工' | '请假' | '出差' | '加班';

export interface AttendanceRecord {
  id: string;
  date: string;
  weekday: string;
  name: string;
  department: string;
  scheduledIn: string;
  scheduledOut: string;
  actualIn: string;
  actualOut: string;
  hours: string;
  status: AttendanceStatus;
  lateMinutes: number | null;
  earlyMinutes: number | null;
  remark: string;
}

export interface AttendanceCorrectionDraft {
  person: string;
  date: string;
  time: string;
  kind: '签到' | '签退';
  reason: string;
}
