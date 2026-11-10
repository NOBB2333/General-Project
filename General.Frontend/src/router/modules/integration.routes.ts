import type { RouteRecordRaw } from 'vue-router';
import { routeNames } from '../route.types';

export const integration_routes: RouteRecordRaw[] = [
  {
    path: 'integration/templates',
    name: routeNames.integrationTemplates,
    component: () => import('@/features/integration/notifications/IntegrationNotificationsPage.vue'),
    props: { initialTab: 'templates' },
    meta: {
      appView: 'feature',
      workspace: '消息模板',
      title: '消息模板',
      module: '外部集成',
      pageLabel: '消息模板',
      feature: 'integration',
    },
  },
  {
    path: 'integration/rules',
    name: routeNames.integrationRules,
    component: () => import('@/features/integration/notifications/IntegrationNotificationsPage.vue'),
    props: { initialTab: 'rules' },
    meta: {
      appView: 'feature',
      workspace: '通知规则',
      title: '通知规则',
      module: '外部集成',
      pageLabel: '通知规则',
      feature: 'integration',
    },
  },
  {
    path: 'integration/delivery-records',
    name: routeNames.integrationDeliveryRecords,
    component: () => import('@/features/integration/operations/IntegrationOperationsPage.vue'),
    props: { initialSection: 'delivery' },
    meta: {
      appView: 'feature',
      workspace: '投递日志',
      title: '投递日志',
      module: '外部集成',
      pageLabel: '投递日志',
      feature: 'integration',
    },
  },
  {
    path: 'integration/sync-jobs',
    name: routeNames.integrationSyncJobs,
    component: () => import('@/features/integration/operations/IntegrationOperationsPage.vue'),
    props: { initialSection: 'sync' },
    meta: {
      appView: 'feature',
      workspace: '同步任务',
      title: '同步任务',
      module: '外部集成',
      pageLabel: '同步任务',
      feature: 'integration',
    },
  },
  {
    path: 'integration/webhook-events',
    name: routeNames.integrationWebhookEvents,
    component: () => import('@/features/integration/operations/IntegrationOperationsPage.vue'),
    props: { initialSection: 'webhook' },
    meta: {
      appView: 'feature',
      workspace: 'Webhook 事件',
      title: 'Webhook 事件',
      module: '外部集成',
      pageLabel: 'Webhook 事件',
      feature: 'integration',
    },
  },
];
