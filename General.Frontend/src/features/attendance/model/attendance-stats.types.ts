export interface AttendanceStatsRow {
  department: string;
  normal: string;
  late: string;
  early: string;
  absent: string;
  rate: string;
  hours: string;
}

export interface AttendanceStatsFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  trend: readonly { date: string; normal: number; late: number; early: number; absent: number; leave: number }[];
  status: readonly { label: string; value: string; tone: string }[];
  departments: readonly { rank: number; name: string; rate: string }[];
  lateDistribution: readonly { label: string; value: number }[];
  rows: readonly AttendanceStatsRow[];
}
