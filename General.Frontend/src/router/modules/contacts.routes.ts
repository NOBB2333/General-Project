import type { RouteRecordRaw } from 'vue-router';

export const contacts_routes: RouteRecordRaw[] = [
  {
    path: 'contacts/contacts',
    name: 'page.contacts.contacts',
    component: () => import('@/features/contacts/views/ContactsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '通讯录',
      title: '通讯录',
      module: '通讯录',
      pageLabel: '通讯录',
      feature: 'contacts',
    },
  },
];
