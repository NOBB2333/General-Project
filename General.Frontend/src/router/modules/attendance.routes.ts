import type { RouteRecordRaw } from 'vue-router';

export const attendance_routes: RouteRecordRaw[] = [
  {
    path: 'attendance/attendance-mgmt',
    name: 'page.attendance.attendance-mgmt',
    component: () => import('@/features/attendance/views/AttendanceMgmtPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '考勤管理',
      title: '考勤管理',
      module: '考勤假期',
      pageLabel: '考勤管理',
      feature: 'attendance',
    },
  },
  {
    path: 'attendance/attendance-stats',
    name: 'page.attendance.attendance-stats',
    component: () => import('@/features/attendance/views/AttendanceStatsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '考勤统计',
      title: '考勤统计',
      module: '考勤假期',
      pageLabel: '考勤统计',
      feature: 'attendance',
    },
  },
  {
    path: 'attendance/team-calendar',
    name: 'page.attendance.team-calendar',
    component: () => import('@/features/attendance/views/TeamCalendarPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '团队日历',
      title: '团队日历',
      module: '考勤假期',
      pageLabel: '团队日历',
      feature: 'attendance',
    },
  },
  {
    path: 'attendance/leave',
    name: 'page.attendance.leave',
    component: () => import('@/features/attendance/views/LeavePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '请假管理',
      title: '请假管理',
      module: '考勤假期',
      pageLabel: '请假管理',
      feature: 'attendance',
    },
  },
  {
    path: 'attendance/travel',
    name: 'page.attendance.travel',
    component: () => import('@/features/attendance/views/TravelPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '出差申请',
      title: '出差申请',
      module: '考勤假期',
      pageLabel: '出差申请',
      feature: 'attendance',
    },
  },
  {
    path: 'attendance/overtime',
    name: 'page.attendance.overtime',
    component: () => import('@/features/attendance/views/OvertimePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '加班申请',
      title: '加班申请',
      module: '考勤假期',
      pageLabel: '加班申请',
      feature: 'attendance',
    },
  },
];
