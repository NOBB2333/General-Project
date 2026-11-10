<script setup lang="ts">
import { UiButton, UiDialog, UiFormField } from '@/components/ui';
import type { AuditLogRow } from './audit-log.model';

const open = defineModel<boolean>('open', { required: true });
defineProps<{ row: AuditLogRow | null }>();
const fields = [
  { key: 'time', label: '执行时间' },
  { key: 'kind', label: '记录类型' },
  { key: 'actor', label: '用户' },
  { key: 'tenant', label: '租户名称 / 标识' },
  { key: 'source', label: '会话来源' },
  { key: 'summary', label: '动作 / 页面' },
  { key: 'path', label: '请求路径' },
  { key: 'method', label: '方法' },
  { key: 'status', label: '状态码' },
  { key: 'duration', label: '耗时' },
  { key: 'exception', label: '异常标记' },
];
</script>

<template>
  <UiDialog
    v-model:open="open"
    title="日志详情"
    description="查看记录的身份、请求路径与执行结果。"
  >
    <template v-if="row">
      <UiFormField
        v-for="field in fields"
        :key="field.key"
        :label="field.label"
      >
        <span>{{ row[field.key] }}</span>
      </UiFormField>
    </template>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="open = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>
