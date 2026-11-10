export type TeamCalendarEventType = '年假' | '病假' | '事假' | '婚假' | '产假' | '调休' | '丧假' | '出差';

export interface TeamCalendarEvent {
  id: string;
  code: string;
  name: string;
  title: string;
  type: TeamCalendarEventType;
  status: TeamCalendarEventType;
  owner: string;
  dept: string;
  date: string;
  endDate: string;
  metric: string;
  value: TeamCalendarEventType;
  month: string;
}

export interface TeamCalendarCell {
  key: string;
  iso: string;
  day: number;
  inMonth: boolean;
  isToday: boolean;
  events: TeamCalendarEvent[];
}
