<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Descriptions, Empty, Space, Spin, Tag } from 'ant-design-vue';

import { getUserInfoApi } from '#/api/core';

defineOptions({ name: 'PlatformProfilePage' });

const router = useRouter();
const loading = ref(true);
const userInfo = ref<any>(null);

async function loadProfile() {
  loading.value = true;
  try {
    userInfo.value = await getUserInfoApi();
  } finally {
    loading.value = false;
  }
}

onMounted(loadProfile);
</script>

<template>
  <Page description="个人中心用于查看当前登录人身份、角色与常用入口。" title="个人中心">
    <Spin :spinning="loading">
      <div v-if="userInfo" class="platform-profile">
        <Card :bordered="false" class="platform-profile__hero">
          <h2>{{ userInfo.realName }}</h2>
          <p>{{ userInfo.desc }}</p>
          <div class="platform-profile__tags">
            <Tag v-for="role in userInfo.roles || []" :key="role" color="blue">
              {{ role }}
            </Tag>
          </div>
        </Card>

        <Card :bordered="false" title="账号信息">
          <Descriptions :column="1" bordered size="small">
            <Descriptions.Item label="用户名">
              {{ userInfo.username }}
            </Descriptions.Item>
            <Descriptions.Item label="默认首页">
              {{ userInfo.homePath }}
            </Descriptions.Item>
            <Descriptions.Item label="角色">
              {{ (userInfo.roles || []).join(', ') || '-' }}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        <Card :bordered="false" title="常用动作">
          <Space wrap>
            <Button type="primary" @click="router.push('/platform/users')">查看用户管理</Button>
            <Button @click="router.push('/platform/roles')">查看角色权限</Button>
            <Button @click="router.push('/platform/files')">查看文件管理</Button>
          </Space>
        </Card>
      </div>
      <Empty v-else description="未获取到个人信息" />
    </Spin>
  </Page>
</template>

<style scoped>
.platform-profile {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-profile__hero {
  background:
    radial-gradient(circle at top right, rgb(59 130 246 / 0.18), transparent 26%),
    var(--ant-color-bg-container);
}

.platform-profile__hero h2 {
  margin: 0 0 8px;
  color: var(--ant-color-text);
}

.platform-profile__hero p {
  margin: 0 0 16px;
  color: var(--ant-color-text-secondary);
}

.platform-profile__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>
