<script setup lang="ts">
import { computed } from 'vue';
import { UiButton, UiCheckbox, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import {
  deliveryStatusLabel,
  formatOperationsDate,
  providerTypeLabel,
  signatureStatusLabel,
  syncDirectionLabel,
  syncModeLabel,
  syncRecordStatusLabel,
  syncTypeLabel,
  webhookStatusLabel,
} from './operations.model';
import type { IntegrationOperationsPageState } from './useIntegrationOperations';

const props = defineProps<{ page: IntegrationOperationsPageState }>();
const {
  closeDialog,
  connectorOptionError,
  connectorOptions,
  detailDelivery,
  detailSyncJob,
  detailSyncRecord,
  detailWebhook,
  dialogDescription,
  dialogError,
  dialogKind,
  dialogOpen,
  dialogTitle,
  exporting,
  hiddenConfigJson,
  saving,
  skipReason,
  submitDialog,
  syncJobDraft,
  syncTypeOptionsForForm,
} = props.page;

const syncDirectionOptions = [
  { label: '导入', value: 'Inbound' },
  { label: '导出', value: 'Outbound' },
  { label: '双向', value: 'Bidirectional' },
] as const;
const syncModeOptions = [
  { label: '增量', value: 'Incremental' },
  { label: '全量', value: 'Full' },
  { label: '手动', value: 'Manual' },
] as const;

const confirmLabel = computed(() => {
  if (dialogKind.value === 'delivery-export') return '导出 CSV';
  if (dialogKind.value === 'sync-job') return '保存任务';
  if (dialogKind.value === 'webhook-retry') return '重新解析';
  if (dialogKind.value?.endsWith('skip')) return '确认跳过';
  if (dialogKind.value?.endsWith('retry')) return '确认重试';
  if (dialogKind.value === 'sync-job-run') return '运行任务';
  if (dialogKind.value === 'sync-job-toggle') return '确认切换';
  return '确认';
});

function closeWhenIdle() {
  if (saving.value || exporting.value) return;
  closeDialog();
}
</script>

<template>
  <UiDialog
    v-model:open="dialogOpen"
    :title="dialogTitle"
    :description="dialogDescription"
    size="lg"
    @close="closeWhenIdle"
  >
    <section v-if="dialogKind === 'detail'">
      <dl v-if="detailDelivery">
        <dt>连接器</dt>
        <dd>{{ detailDelivery.connectorCode }}</dd>
        <dt>消息类型</dt>
        <dd>{{ detailDelivery.messageType }}</dd>
        <dt>目标类型</dt>
        <dd>{{ detailDelivery.targetType }}</dd>
        <dt>来源</dt>
        <dd>{{ detailDelivery.sourceType }}</dd>
        <dt>状态</dt>
        <dd>{{ deliveryStatusLabel(detailDelivery.status) }}</dd>
        <dt>尝试次数</dt>
        <dd>{{ detailDelivery.attemptCount }} / {{ detailDelivery.maxRetryCount }}</dd>
        <dt>创建时间</dt>
        <dd>{{ formatOperationsDate(detailDelivery.creationTime) }}</dd>
        <dt>最近尝试</dt>
        <dd>{{ formatOperationsDate(detailDelivery.lastAttemptTime) }}</dd>
        <dt>下次重试</dt>
        <dd>{{ formatOperationsDate(detailDelivery.nextRetryTime) }}</dd>
        <dt>耗时</dt>
        <dd>{{ detailDelivery.durationMilliseconds }} ms</dd>
      </dl>
      <dl v-else-if="detailSyncJob">
        <dt>任务名称</dt>
        <dd>{{ detailSyncJob.name }}</dd>
        <dt>任务编码</dt>
        <dd>{{ detailSyncJob.jobCode }}</dd>
        <dt>连接器</dt>
        <dd>{{ detailSyncJob.connectorCode }}</dd>
        <dt>同步类型</dt>
        <dd>{{ syncTypeLabel(detailSyncJob.syncType) }}</dd>
        <dt>同步方向</dt>
        <dd>{{ syncDirectionLabel(detailSyncJob.direction) }}</dd>
        <dt>执行模式</dt>
        <dd>{{ syncModeLabel(detailSyncJob.mode) }}</dd>
        <dt>任务状态</dt>
        <dd>{{ detailSyncJob.isEnabled ? '启用' : '停用' }}</dd>
        <dt>Cron</dt>
        <dd>{{ detailSyncJob.cronExpression || '仅手动' }}</dd>
        <dt>最近运行</dt>
        <dd>{{ formatOperationsDate(detailSyncJob.lastRunTime) }}</dd>
        <dt>最近状态</dt>
        <dd>{{ detailSyncJob.lastRunStatus ? syncRecordStatusLabel(detailSyncJob.lastRunStatus) : '未执行' }}</dd>
      </dl>
      <dl v-else-if="detailSyncRecord">
        <dt>连接器</dt>
        <dd>{{ detailSyncRecord.connectorCode }}</dd>
        <dt>同步类型</dt>
        <dd>{{ syncTypeLabel(detailSyncRecord.syncType) }}</dd>
        <dt>执行模式</dt>
        <dd>{{ syncModeLabel(detailSyncRecord.mode) }}</dd>
        <dt>状态</dt>
        <dd>{{ syncRecordStatusLabel(detailSyncRecord.status) }}</dd>
        <dt>开始时间</dt>
        <dd>{{ formatOperationsDate(detailSyncRecord.startedTime) }}</dd>
        <dt>完成时间</dt>
        <dd>{{ formatOperationsDate(detailSyncRecord.finishedTime) }}</dd>
        <dt>总数</dt>
        <dd>{{ detailSyncRecord.totalCount }}</dd>
        <dt>新增</dt>
        <dd>{{ detailSyncRecord.createdCount }}</dd>
        <dt>更新</dt>
        <dd>{{ detailSyncRecord.updatedCount }}</dd>
        <dt>跳过</dt>
        <dd>{{ detailSyncRecord.skippedCount }}</dd>
        <dt>失败</dt>
        <dd>{{ detailSyncRecord.failedCount }}</dd>
        <dt>冲突数</dt>
        <dd>{{ detailSyncRecord.conflictCount }}</dd>
      </dl>
      <dl v-else-if="detailWebhook">
        <dt>平台</dt>
        <dd>{{ providerTypeLabel(detailWebhook.providerType) }}</dd>
        <dt>连接器</dt>
        <dd>{{ detailWebhook.connectorCode || '—' }}</dd>
        <dt>事件类型</dt>
        <dd>{{ detailWebhook.eventType }}</dd>
        <dt>签名状态</dt>
        <dd>{{ signatureStatusLabel(detailWebhook.signatureStatus) }}</dd>
        <dt>处理状态</dt>
        <dd>{{ webhookStatusLabel(detailWebhook.status) }}</dd>
        <dt>接收时间</dt>
        <dd>{{ formatOperationsDate(detailWebhook.creationTime) }}</dd>
        <dt>处理时间</dt>
        <dd>{{ formatOperationsDate(detailWebhook.resolvedTime) }}</dd>
        <dt>重试次数</dt>
        <dd>{{ detailWebhook.retryCount }}</dd>
        <dt>来源地址</dt>
        <dd>{{ detailWebhook.sourceIp || '—' }}</dd>
      </dl>
      <p>后端返回的原始JSON、Payload、请求头、请求体、业务键及错误正文不会在页面展示。</p>
    </section>

    <section v-else-if="dialogKind === 'sync-job'">
      <UiFormField
        label="任务名称"
        required
      >
        <UiInput
          v-model="syncJobDraft.name"
          maxlength="128"
          aria-label="同步任务名称"
        />
      </UiFormField>
      <UiFormField
        label="任务编码"
        required
      >
        <UiInput
          v-model="syncJobDraft.jobCode"
          maxlength="64"
          aria-label="同步任务编码"
        />
      </UiFormField>
      <UiFormField
        label="连接器"
        required
      >
        <UiSelect
          v-if="connectorOptions.length"
          v-model="syncJobDraft.connectorId"
          :options="connectorOptions"
          label="连接器"
        />
        <UiInput
          v-else
          v-model="syncJobDraft.connectorId"
          aria-label="连接器 GUID"
          placeholder="填写已有连接器 GUID"
        />
      </UiFormField>
      <p
        v-if="connectorOptionError"
        role="status"
      >
        {{ connectorOptionError }}
      </p>
      <UiFormField label="同步类型">
        <UiSelect
          v-model="syncJobDraft.syncType"
          :options="syncTypeOptionsForForm"
          label="同步类型"
        />
      </UiFormField>
      <UiFormField label="同步方向">
        <UiSelect
          v-model="syncJobDraft.direction"
          :options="syncDirectionOptions"
          label="同步方向"
        />
      </UiFormField>
      <UiFormField label="执行模式">
        <UiSelect
          v-model="syncJobDraft.mode"
          :options="syncModeOptions"
          label="执行模式"
        />
      </UiFormField>
      <UiFormField
        label="Cron 表达式"
        hint="留空时任务仅能手动运行。"
      >
        <UiInput
          v-model="syncJobDraft.cronExpression"
          maxlength="128"
          aria-label="Cron 表达式"
        />
      </UiFormField>
      <UiFormField label="任务状态">
        <label>
          <UiCheckbox
            :model-value="syncJobDraft.isEnabled"
            label="启用同步任务"
            @update:model-value="syncJobDraft.isEnabled = $event === true"
          />
          启用任务
        </label>
      </UiFormField>
      <UiFormField
        v-if="!syncJobDraft.id"
        label="同步配置 JSON"
        hint="仅新建时可输入；禁止写入账号密码、Token或外部连接器凭证。"
      >
        <UiTextarea
          v-model="syncJobDraft.configJson"
          rows="5"
          maxlength="16000"
          spellcheck="false"
          aria-label="同步配置 JSON"
        />
      </UiFormField>
      <p v-else-if="hiddenConfigJson">
        后端不保证配置 JSON 已脱敏，本页不读取现有配置内容；保存其他字段时会保留原配置。
      </p>
      <UiFormField label="备注">
        <UiInput
          v-model="syncJobDraft.remark"
          maxlength="512"
          aria-label="同步任务备注"
        />
      </UiFormField>
    </section>

    <section v-else-if="dialogKind?.endsWith('skip')">
      <UiFormField
        label="处置原因"
        required
      >
        <UiTextarea
          v-model="skipReason"
          rows="4"
          maxlength="512"
          aria-label="跳过原因"
        />
      </UiFormField>
    </section>

    <section v-else>
      <p>{{ dialogDescription }}</p>
    </section>

    <p
      v-if="dialogError"
      role="alert"
    >
      {{ dialogError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="saving || exporting"
        @click="closeWhenIdle"
        >取消</UiButton
      >
      <UiButton
        v-if="dialogKind !== 'detail'"
        variant="primary"
        :loading="saving || exporting"
        @click="submitDialog"
      >
        {{ confirmLabel }}
      </UiButton>
    </template>
  </UiDialog>
</template>
