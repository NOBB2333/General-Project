<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Avatar, Button, Card, Descriptions, Form, Input, Spin, Tag, message } from 'ant-design-vue';

import { changePasswordApi, getUserInfoApi } from '#/api/core';

defineOptions({ name: 'PlatformProfilePage' });

const loading = ref(true);
const saving = ref(false);
const userInfo = ref<any>(null);

const passwordForm = reactive({
  confirmPassword: '',
  currentPassword: '',
  newPassword: '',
});

async function loadProfile() {
  loading.value = true;
  try {
    userInfo.value = await getUserInfoApi();
  } finally {
    loading.value = false;
  }
}

async function handleChangePassword() {
  if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
    message.warning('请填写完整密码信息');
    return;
  }
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    message.error('两次新密码输入不一致');
    return;
  }
  saving.value = true;
  try {
    await changePasswordApi({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    });
    message.success('密码修改成功');
    passwordForm.currentPassword = '';
    passwordForm.newPassword = '';
    passwordForm.confirmPassword = '';
  } finally {
    saving.value = false;
  }
}

onMounted(loadProfile);
</script>

<template>
  <Page description="查看个人信息与账号安全设置。" title="个人中心">
    <Spin :spinning="loading">
      <div class="platform-profile">
        <Card :bordered="false" class="platform-profile__hero">
          <div class="platform-profile__hero-inner">
            <Avatar :size="72" class="platform-profile__avatar">
              {{ (userInfo?.realName || userInfo?.username || '?').charAt(0).toUpperCase() }}
            </Avatar>
            <div>
              <h2>{{ userInfo?.realName || userInfo?.username }}</h2>
              <p>{{ userInfo?.desc || '暂无简介' }}</p>
            </div>
          </div>
        </Card>

        <Card :bordered="false" title="基本信息">
          <Descriptions :column="{ sm: 2, xs: 1 }" bordered size="small">
            <Descriptions.Item label="用户名">{{ userInfo?.username || '-' }}</Descriptions.Item>
            <Descriptions.Item label="姓名">{{ userInfo?.realName || '-' }}</Descriptions.Item>
            <Descriptions.Item label="最近登录">
              {{ userInfo?.lastLoginTime ? new Date(userInfo.lastLoginTime).toLocaleString() : '-' }}
            </Descriptions.Item>
            <Descriptions.Item label="默认首页">{{ userInfo?.homePath || '-' }}</Descriptions.Item>
          </Descriptions>
        </Card>

        <Card :bordered="false" title="所属角色">
          <div v-if="(userInfo?.roles || []).length > 0" class="platform-profile__tags">
            <Tag v-for="role in userInfo.roles" :key="role" color="blue">{{ role }}</Tag>
          </div>
          <span v-else class="platform-profile__empty">未分配角色</span>
        </Card>

        <Card :bordered="false" title="所属组织">
          <div v-if="(userInfo?.organizationUnitNames || []).length > 0" class="platform-profile__tags">
            <Tag v-for="org in userInfo.organizationUnitNames" :key="org" color="cyan">{{ org }}</Tag>
          </div>
          <span v-else class="platform-profile__empty">未分配组织</span>
        </Card>

        <Card :bordered="false" title="修改密码">
          <Form layout="vertical" style="max-width: 400px">
            <Form.Item label="当前密码">
              <Input.Password v-model:value="passwordForm.currentPassword" placeholder="请输入当前密码" />
            </Form.Item>
            <Form.Item label="新密码">
              <Input.Password v-model:value="passwordForm.newPassword" placeholder="请输入新密码" />
            </Form.Item>
            <Form.Item label="确认新密码">
              <Input.Password v-model:value="passwordForm.confirmPassword" placeholder="再次输入新密码" />
            </Form.Item>
            <Form.Item>
              <Button :loading="saving" type="primary" @click="handleChangePassword">保存密码</Button>
            </Form.Item>
          </Form>
        </Card>
      </div>
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

.platform-profile__hero-inner {
  display: flex;
  gap: 20px;
  align-items: center;
}

.platform-profile__avatar {
  flex-shrink: 0;
  font-size: 28px;
  background: var(--ant-color-primary);
}

.platform-profile__hero h2 {
  margin: 0 0 4px;
  font-size: 20px;
  color: var(--ant-color-text);
}

.platform-profile__hero p {
  margin: 0;
  color: var(--ant-color-text-secondary);
}

.platform-profile__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.platform-profile__empty {
  font-size: 13px;
  color: var(--ant-color-text-quaternary);
}
</style>

