<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Row, Skeleton, Space, Statistic, Tag, Timeline } from 'ant-design-vue';

import {
  getFileListApi,
  getOrganizationTreeApi,
  getOnlineUserListApi,
  getRoleListApi,
  getTenantListApi,
  getUpdateLogListApi,
  getUserInfoApi,
  getUserListApi,
} from '#/api/core';

defineOptions({ name: 'PlatformWorkspacePage' });

const router = useRouter();

const loading = ref(true);
const summary = ref({
  files: 0,
  onlineUsers: 0,
  organizations: 0,
  roles: 0,
  tenants: 0,
  updateLogs: 0,
  users: 0,
});
const userInfo = ref<null | {
  desc?: string;
  realName?: string;
  roles?: string[];
}>(null);

const quickEntries = [
  { path: '/platform/organization', tag: '组织', title: '调整组织结构' },
  { path: '/platform/users', tag: '用户', title: '新增或维护用户' },
  { path: '/platform/roles', tag: '角色', title: '分配角色与菜单' },
  { path: '/platform/tenants', tag: '租户', title: '维护租户空间' },
  { path: '/platform/menus', tag: '菜单', title: '配置平台导航与按钮码' },
  { path: '/platform/files', tag: '文件', title: '统一管理制度与模板文件' },
  { path: '/platform/online-users', tag: '在线', title: '查看当前在线会话' },
  { path: '/platform/audit-logs', tag: '审计', title: '查看最新访问留痕' },
  { path: '/platform/update-logs', tag: '版本', title: '维护平台更新记录' },
  { path: '/platform/system-monitor', tag: '监控', title: '查看服务器资源状态' },
];

const timelineItems = computed(() => [
  `当前登录：${userInfo.value?.realName || '-'}`,
  `身份说明：${userInfo.value?.desc || '-'}`,
  `当前角色：${(userInfo.value?.roles || []).join(' / ') || '-'}`,
  '平台首页已切到独立工作台，避免默认落到项目端页面。',
]);

function flattenOrganizationCount(items: Array<{ children: any[] }>): number {
  return items.reduce((count, item) => count + 1 + flattenOrganizationCount(item.children || []), 0);
}

async function loadWorkspace() {
  loading.value = true;
  try {
    const [organizations, users, roles, tenants, files, onlineUsers, updateLogs, currentUser] = await Promise.all([
      getOrganizationTreeApi(),
      getUserListApi(),
      getRoleListApi(),
      getTenantListApi(),
      getFileListApi(),
      getOnlineUserListApi(),
      getUpdateLogListApi(),
      getUserInfoApi(),
    ]);

    summary.value = {
      files: files.length,
      onlineUsers: onlineUsers.length,
      organizations: flattenOrganizationCount(organizations),
      roles: roles.length,
      tenants: tenants.length,
      updateLogs: updateLogs.length,
      users: users.length,
    };
    userInfo.value = currentUser as any;
  } finally {
    loading.value = false;
  }
}

onMounted(loadWorkspace);
</script>

<template>
  <Page
    description="平台中心首页聚合一期基础建设入口，默认作为 web-general-platform 的登录后首页。"
    title="平台总览"
  >
    <Skeleton :loading="loading" active>
      <div class="platform-workspace">
        <Card class="platform-workspace__hero" :bordered="false">
          <div class="platform-workspace__hero-copy">
            <p class="platform-workspace__eyebrow">Phase 1 Platform</p>
            <h2>组织、用户、角色、租户、菜单、文件统一在平台中心收口。</h2>
            <p>
              当前工作台用于承接一期平台治理动作。左侧菜单已切换为真实后端路由与图标，
              这里提供常用维护入口和基础统计。
            </p>
            <Space wrap>
              <Button type="primary" @click="router.push('/platform/users')">进入用户管理</Button>
              <Button @click="router.push('/platform/menus')">进入菜单管理</Button>
            </Space>
          </div>
          <div class="platform-workspace__tags">
            <Tag color="blue">平台首页</Tag>
            <Tag color="green">真实接口</Tag>
            <Tag color="orange">一期范围</Tag>
          </div>
        </Card>

        <Row :gutter="[16, 16]">
          <Col v-for="item in [
            ['组织节点', summary.organizations],
            ['平台用户', summary.users],
            ['角色数量', summary.roles],
            ['租户数量', summary.tenants],
            ['文件数量', summary.files],
            ['在线会话', summary.onlineUsers],
            ['更新记录', summary.updateLogs],
          ]" :key="item[0]" :lg="4" :md="8" :span="24">
            <Card :bordered="false" class="platform-workspace__metric">
              <Statistic :title="item[0]" :value="item[1]" />
            </Card>
          </Col>
        </Row>

        <Row :gutter="[16, 16]" class="mt-1">
          <Col :lg="14" :span="24">
            <Card :bordered="false" title="快捷入口">
              <div v-if="quickEntries.length > 0" class="platform-workspace__grid">
                <button
                  v-for="entry in quickEntries"
                  :key="entry.path"
                  class="platform-workspace__entry"
                  type="button"
                  @click="router.push(entry.path)"
                >
                  <span>{{ entry.tag }}</span>
                  <strong>{{ entry.title }}</strong>
                </button>
              </div>
              <Empty v-else description="暂无快捷入口" />
            </Card>
          </Col>

          <Col :lg="10" :span="24">
            <Card :bordered="false" title="当前上下文">
              <Timeline>
                <Timeline.Item v-for="item in timelineItems" :key="item">
                  {{ item }}
                </Timeline.Item>
              </Timeline>
            </Card>
          </Col>
        </Row>
      </div>
    </Skeleton>
  </Page>
</template>

<style scoped>
.platform-workspace {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-workspace__hero {
  background:
    radial-gradient(circle at top left, rgb(59 130 246 / 0.18), transparent 34%),
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.platform-workspace__hero-copy h2 {
  margin: 0 0 12px;
  color: var(--ant-color-text);
  font-size: 28px;
  line-height: 1.25;
}

.platform-workspace__hero-copy p {
  max-width: 70ch;
  color: var(--ant-color-text-secondary);
  line-height: 1.8;
}

.platform-workspace__eyebrow {
  margin: 0 0 10px;
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.platform-workspace__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 18px;
}

.platform-workspace__metric,
.platform-workspace__entry {
  transition:
    background-color 0.24s ease,
    border-color 0.24s ease,
    color 0.24s ease,
    box-shadow 0.24s ease;
}

.platform-workspace__grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.platform-workspace__entry {
  padding: 16px;
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 16px;
  background: var(--ant-color-fill-quaternary);
  text-align: left;
}

.platform-workspace__entry span {
  display: block;
  margin-bottom: 6px;
  color: var(--ant-color-primary);
  font-size: 12px;
}

.platform-workspace__entry strong {
  color: var(--ant-color-text);
  font-size: 15px;
}

@media (max-width: 960px) {
  .platform-workspace__grid {
    grid-template-columns: 1fr;
  }
}
</style>
