import type { RouteRecordRaw } from 'vue-router';

export const company_calendar_routes: RouteRecordRaw[] = [
  {
    path: 'calendar/company-calendar',
    name: 'page.company-calendar.company-calendar',
    component: () => import('@/features/company-calendar/views/CompanyCalendarPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '公司日历',
      title: '公司日历',
      module: '公司日历',
      pageLabel: '公司日历',
      feature: 'company-calendar',
    },
  },
];
