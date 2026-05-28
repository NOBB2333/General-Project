<script lang="ts" setup>
import { Button, Card, Select, Space, Table, Tag } from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  detail,
  editingMemberId,
  formatDate,
  handleEditMemberRoles,
  handleSaveMemberRoles,
  loading,
  memberColumns,
  memberRoleForms,
  projectRoleOptions,
  savingMemberRoleId,
  worklogColumns,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <Card :bordered="false" class="project-detail__inner-card" title="项目成员">
    <Table
      :columns="memberColumns"
      :data-source="detail?.members || []"
      :loading="loading"
      :pagination="false"
      :scroll="{ x: 1100 }"
      row-key="id"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'roleNames'">
          <template v-if="editingMemberId === (record as ProjectApi.MemberItem).id">
            <Select
              :value="memberRoleForms[(record as ProjectApi.MemberItem).id] || []"
              mode="tags"
              :options="projectRoleOptions"
              size="small"
              style="width: 100%"
              @update:value="(value) => { memberRoleForms[(record as ProjectApi.MemberItem).id] = value as string[]; }"
            />
          </template>
          <template v-else>
            <div class="project-detail__tag-list">
              <Tag v-for="role in (record as ProjectApi.MemberItem).roleNames" :key="role" color="blue">
                {{ role }}
              </Tag>
              <span v-if="!(record as ProjectApi.MemberItem).roleNames?.length" class="project-detail__muted">未设置</span>
            </div>
          </template>
        </template>
        <template v-else-if="column.key === 'joinDate'">
          {{ formatDate((record as ProjectApi.MemberItem).joinDate) }}
        </template>
        <template v-else-if="column.key === 'actions'">
          <Space>
            <template v-if="editingMemberId === (record as ProjectApi.MemberItem).id">
              <Button size="small" type="primary" :loading="savingMemberRoleId === (record as ProjectApi.MemberItem).id" @click="handleSaveMemberRoles((record as ProjectApi.MemberItem).id)">保存</Button>
              <Button size="small" @click="editingMemberId = null">取消</Button>
            </template>
            <template v-else>
              <Button size="small" type="link" @click="handleEditMemberRoles(record as ProjectApi.MemberItem)">编辑角色</Button>
            </template>
          </Space>
        </template>
      </template>
    </Table>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card" title="工时记录（小时）">
    <Table
      :columns="worklogColumns"
      :data-source="detail?.worklogs || []"
      :loading="loading"
      :pagination="{ pageSize: 10, showSizeChanger: true, showTotal: (total: number) => `共 ${total} 条` }"
      row-key="id"
      size="small"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'workDate'">
          {{ formatDate((record as ProjectApi.WorklogItem).workDate) }}
        </template>
      </template>
    </Table>
  </Card>
</section>
</template>
