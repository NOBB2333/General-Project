<script setup lang="ts">
import { PlugZap } from 'lucide-vue-next';
import { ListPageSurface } from '@/components/page';
import { UiButton, UiFormField, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { IntegrationOperationsSection } from './operations.model';
import IntegrationOperationsDialog from './IntegrationOperationsDialog.vue';
import { useIntegrationOperations } from './useIntegrationOperations';

const props = defineProps<{ initialSection: IntegrationOperationsSection }>();
const page = useIntegrationOperations(props);
const {
  activeSection,
  applyDeliveryFilters,
  applyWebhookFilters,
  changePage,
  changeSyncMode,
  deliveryCreatedFrom,
  deliveryCreatedTo,
  deliveryConnectorFilter,
  deliveryMessageTypeFilter,
  deliveryTargetTypeFilter,
  handleAction,
  handleFilterChange,
  handleSearch,
  loadError,
  loading,
  openRow,
  pageModel,
  remoteFilterValues,
  remotePagination,
  resetDeliveryFilters,
  resetWebhookFilters,
  selectSection,
  summaryError,
  syncMode,
  webhookEventTypeFilter,
  webhookProviderFilter,
} = page;
</script>

<template>
  <ListPageSurface
    :key="`${activeSection}:${syncMode}`"
    :model="pageModel"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @filter-change="handleFilterChange"
    @page-change="changePage"
    @row="openRow"
    @search-change="handleSearch"
  >
    <template #hero-icon>
      <PlugZap
        :size="24"
        :stroke-width="1.7"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <section>
        <UiTabs
          :model-value="activeSection"
          @update:model-value="selectSection"
        >
          <UiTabsList label="集成运行管理">
            <UiTabsTrigger value="delivery">投递日志</UiTabsTrigger>
            <UiTabsTrigger value="sync">同步管理</UiTabsTrigger>
            <UiTabsTrigger value="webhook">Webhook 事件</UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <UiTabs
          v-if="activeSection === 'sync'"
          :model-value="syncMode"
          @update:model-value="changeSyncMode"
        >
          <UiTabsList label="同步任务和执行记录">
            <UiTabsTrigger value="jobs">同步任务</UiTabsTrigger>
            <UiTabsTrigger value="records">执行记录</UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <div v-if="activeSection === 'delivery'">
          <p>高级筛选</p>
          <UiFormField label="连接器编码">
            <UiInput
              v-model="deliveryConnectorFilter"
              aria-label="连接器编码"
              placeholder="例如 sandbox"
            />
          </UiFormField>
          <UiFormField label="消息类型">
            <UiInput
              v-model="deliveryMessageTypeFilter"
              aria-label="消息类型"
            />
          </UiFormField>
          <UiFormField label="目标类型">
            <UiInput
              v-model="deliveryTargetTypeFilter"
              aria-label="目标类型"
            />
          </UiFormField>
          <UiFormField label="开始日期">
            <UiInput
              v-model="deliveryCreatedFrom"
              type="date"
              aria-label="投递开始日期"
            />
          </UiFormField>
          <UiFormField label="结束日期">
            <UiInput
              v-model="deliveryCreatedTo"
              type="date"
              aria-label="投递结束日期"
            />
          </UiFormField>
          <UiButton @click="applyDeliveryFilters">应用筛选</UiButton>
          <UiButton @click="resetDeliveryFilters">重置</UiButton>
        </div>

        <div v-if="activeSection === 'webhook'">
          <p>Webhook 高级筛选</p>
          <UiFormField
            label="平台类型"
            hint="填写后端 ProviderType，例如 Feishu、DingTalk 或 WeCom。"
          >
            <UiInput
              v-model="webhookProviderFilter"
              aria-label="Webhook 平台类型"
            />
          </UiFormField>
          <UiFormField label="事件类型">
            <UiInput
              v-model="webhookEventTypeFilter"
              aria-label="Webhook 事件类型"
            />
          </UiFormField>
          <UiButton @click="applyWebhookFilters">应用筛选</UiButton>
          <UiButton @click="resetWebhookFilters">重置</UiButton>
          <p>
            外部回调路由：<code>POST /api/app/integration/webhooks/{connectorCode}/receive</code>。当前后端没有 Webhook
            端点配置或登记接口，此页只查看和处置已接收事件。
          </p>
        </div>

        <p
          v-if="loading"
          role="status"
        >
          正在读取当前列表…
        </p>
        <p
          v-else-if="loadError"
          role="alert"
        >
          {{ loadError }}
        </p>
        <p
          v-if="summaryError"
          role="status"
        >
          {{ summaryError }}
        </p>
      </section>
    </template>
  </ListPageSurface>

  <IntegrationOperationsDialog :page="page" />
</template>
