<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import { Card } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';
import { useBusinessProjectsContext } from '../context';

const { closingItems, currentProject, detail, formatMoney, historyColumns } =
  useBusinessProjectsContext();

const historyRows = computed(() => detail.value.forecastHistories);
const historyPagination = useLocalVxePagination(historyRows, {
  defaultPageSize: 10,
});

const historyGridOptions = computed<
  VxeGridProps<BusinessApi.ForecastHistoryItem>
>(() => ({
  columns: historyColumns,
  data: historyPagination.pagedRows.value,
  pagerConfig: historyPagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const historyGridEvents: NativeVxeGridEvents<BusinessApi.ForecastHistoryItem> =
  {
    'page-change': historyPagination.handlePageChange,
  };

const [HistoryGrid] = useVbenVxeGrid<BusinessApi.ForecastHistoryItem>({
  gridEvents: historyGridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-grid">
    <Card class="rounded-2xl" :bordered="false" title="经营摘要">
      <div class="business-projects__summary-grid">
        <div class="business-projects__summary-item">
          <span>当前项目</span
          ><strong>{{ currentProject?.name ?? '--' }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>合同收入</span
          ><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>分包与采购</span
          ><strong>{{ formatMoney(detail.summary.procurementAmount) }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>年底预计回款</span
          ><strong>{{
            formatMoney(detail.summary.yearEndReceivableForecast)
          }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>经营收尾状态</span
          ><strong>{{ detail.summary.businessCloseStatus }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>项目阶段</span><strong>{{ detail.summary.status }}</strong>
        </div>
      </div>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="经营收尾视图">
      <div class="business-projects__closing-grid">
        <article
          v-for="item in closingItems"
          :key="item.key"
          :class="[
            'business-projects__closing-item',
            { 'is-warning': item.status === 'warning' },
          ]"
        >
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </article>
      </div>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="最近变化">
      <HistoryGrid
        class="app-vxe-card-grid business-projects__table"
        :grid-events="historyGridEvents"
        :grid-options="historyGridOptions"
      />
    </Card>
  </div>
</template>
