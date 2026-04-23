<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Descriptions, Empty, Form, Input, Space, Spin, Tag, message } from 'ant-design-vue';

import { changePasswordApi, getUserInfoApi } from '#/api/core';

defineOptions({ name: 'PlatformProfilePage' });

const loading = ref(true);
const userInfo = ref<any>(null);
const saving = ref(false);
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
});
const passwordChanged = ref(false);

async function loadProfile() {
  loading.value = true;
  try {
    userInfo.value = await getUserInfoApi();
  } finally {
    loading.value = false;
  }
}

async function handleChangePassword() {
  if (!passwordForm.currentPassword || !passwordForm.newPassword) {
    message.warning('请填写当前密码和新密码');
    return;
  }
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    message.warning('两次输入的新密码不一致');
    return;
  }

  saving.value = true;
  try {
    await changePasswordApi({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    });
    message.success('密码已修改');
    passwordForm.currentPassword = '';
    passwordForm.newPassword = '';
    passwordForm.confirmPassword = '';
    passwordChanged.value = true;
  } finally {
    saving.value = false;
  }
}

onMounted(loadProfile);
</script>

<template>
  <Page description="查看个人基本信息、所属组织与角色，并可在此修改登录密码。" title="个人中心">
    <Spin :spinning="loading">
      <div v-if="userInfo" class="platform-profile">

        <!-- 身份概览 -->
        <Card :bordered="false" class="platform-profile__hero">
          <div class="platform-profile__hero-body">
            <div class="platform-profile__avatar">
              {{ (userInfo.realName || userInfo.username || '?').slice(0, 1).toUpperCase() }}
            </div>
            <div class="platform-profile__hero-info">
              <h2>{{ userInfo.realName || userInfo.username }}</h2>
              <p>{{ userInfo.desc }}</p>
              <div class="platform-profile__tags">
                <Tag
                  v-for="role in (userInfo.roles || [])"
                  :key="role"
                  color="blue"
                >{{ role }}</Tag>
                <Tag
                  v-for="org in (userInfo.organizationUnitNames || [])"
                  :key="org"
                  color="default"
                >{{ org }}</Tag>
              </div>
            </div>
          </div>
        </Card>

        <!-- 基本信息 -->
        <Card :bordered="false" title="基本信息">
          <Descriptions :column="{ lg: 2, md: 1, sm: 1 }" bordered size="small">
            <Descriptions.Item label="用户名">{{ userInfo.username }}</Descriptions.Item>
            <Descriptions.Item label="姓名">{{ userInfo.realName || '-' }}</Descriptions.Item>
            <Descriptions.Item label="默认首页">{{ userInfo.homePath || '-' }}</Descriptions.Item>
            <Descriptions.Item label="最后登录">
              {{ userInfo.lastLoginTime ? new Date(userInfo.lastLoginTime).toLocaleString() : '-' }}
            </Descriptions.Item>
          </Descriptions>
        </Card>

        <!-- 所属角色 -->
        <Card :bordered="false" title="所属角色">
          <Space wrap>
            <Tag
              v-for="role in (userInfo.roles || [])"
              :key="role"
              color="blue"
            >{{ role }}</Tag>
            <span v-if="!(userInfo.roles || []).length" class="platform-profile__empty-text">暂无角色</span>
          </Space>
        </Card>

        <!-- 所属组织 -->
        <Card :bordered="false" title="所属组织">
          <Space wrap>
            <Tag
              v-for="org in (userInfo.organizationUnitNames || [])"
              :key="org"
            >{{ org }}</Tag>
            <span v-if="!(userInfo.organizationUnitNames || []).length" class="platform-profile__empty-text">暂无组织</span>
          </Space>
        </Card>

        <!-- 修改密码 -->
        <Card :bordered="false" title="修改密码">
          <Form layout="vertical" style="max-width: 480px">
            <Form.Item label="当前密码" required>
              <Input.Password
                v-model:value="passwordForm.currentPassword"
                autocomplete="current-password"
                placeholder="请输入当前密码"
              />
            </Form.Item>
            <Form.Item label="新密码" required>
              <Input.Password
                v-model:value="passwordForm.newPassword"
                autocomplete="new-password"
                placeholder="请输入新密码"
              />
            </Form.Item>
            <Form.Item label="确认新密码" required>
              <Input.Password
                v-model:value="passwordForm.confirmPassword"
                autocomplete="new-password"
                placeholder="再次输入新密码"
              />
            </Form.Item>
            <Form.Item>
              <Button
                :loading="saving"
                type="primary"
                @click="handleChangePassword"
              >
                确认修改
              </Button>
            </Form.Item>
          </Form>
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

.platform-profile__hero-body {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}

.platform-profile__avatar {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: var(--ant-color-primary);
  color: #fff;
  font-size: 22px;
  font-weight: 600;
}

.platform-profile__hero-info h2 {
  margin: 0 0 4px;
  color: var(--ant-color-text);
  font-size: 18px;
}

.platform-profile__hero-info p {
  margin: 0 0 12px;
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.platform-profile__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.platform-profile__empty-text {
  color: var(--ant-color-text-quaternary);
  font-size: 13px;
}
</style>
