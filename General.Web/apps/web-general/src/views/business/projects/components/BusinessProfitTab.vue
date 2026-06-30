<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import {
  Button,
  Card,
  Col,
  Form,
  Input,
  Popconfirm,
  Row,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useBusinessProjectsContext } from '../context';

const {
  acceptanceForm,
  detail,
  editingAcceptanceId,
  editAcceptance,
  formatMoney,
  getFocusedRowClass,
  handleDeleteAcceptance,
  handleSaveAcceptance,
  historyColumns,
  savingAcceptance,
  showAcceptanceForm,
  toggleAcceptanceForm,
} = useBusinessProjectsContext();

function handleEditAcceptanceRecord(record: Record<string, any>) {
  editAcceptance(record as BusinessApi.AcceptanceItem);
}

function handleDeleteAcceptanceRecord(record: Record<string, any>) {
  return handleDeleteAcceptance(record as BusinessApi.AcceptanceItem);
}

function getAcceptanceRowClass(record: Record<string, any>) {
  return getFocusedRowClass('acceptance', record);
}

const acceptanceColumns = [
  { field: 'acceptanceCode', title: '验收编号', width: 160 },
  { align: 'left', field: 'title', title: '验收标题', width: 240 },
  { field: 'acceptanceDate', title: '验收日期', width: 130 },
  { field: 'linkedContractCode', title: '关联合同', width: 160 },
  { field: 'acceptanceResult', title: '结果', width: 110 },
  { field: 'status', title: '状态', width: 110 },
  {
    fixed: 'right',
    slots: { default: 'operation' },
    title: '操作',
    width: 150,
  },
] satisfies VxeGridProps<BusinessApi.AcceptanceItem>['columns'];

const historyRows = computed(() => detail.value.forecastHistories);
const acceptanceRows = computed(() => detail.value.acceptances);
const historyPagination = useLocalVxePagination(historyRows, {
  defaultPageSize: 10,
});
const acceptancePagination = useLocalVxePagination(acceptanceRows, {
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

const acceptanceGridOptions = computed<
  VxeGridProps<BusinessApi.AcceptanceItem>
>(() => ({
  columns: acceptanceColumns,
  data: acceptancePagination.pagedRows.value,
  pagerConfig: acceptancePagination.pagerConfig.value,
  rowClassName: ({ row }) => getAcceptanceRowClass(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const historyGridEvents: NativeVxeGridEvents<BusinessApi.ForecastHistoryItem> =
  {
    'page-change': historyPagination.handlePageChange,
  };
const acceptanceGridEvents: NativeVxeGridEvents<BusinessApi.AcceptanceItem> = {
  'page-change': acceptancePagination.handlePageChange,
};

const [HistoryGrid] = useVbenVxeGrid<BusinessApi.ForecastHistoryItem>({
  gridEvents: historyGridEvents,
  gridOptions: {},
});
const [AcceptanceGrid] = useVbenVxeGrid<BusinessApi.AcceptanceItem>({
  gridEvents: acceptanceGridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-grid">
    <Card class="rounded-2xl" :bordered="false" title="利润结果">
      <div class="business-projects__summary-grid">
        <div class="business-projects__summary-item">
          <span>合同收入</span
          ><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>经营成本</span
          ><strong>{{ formatMoney(detail.summary.costAmount) }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>毛利额</span
          ><strong>{{ formatMoney(detail.summary.grossProfitAmount) }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>毛利率</span
          ><strong>{{
            typeof detail.summary.grossProfitRate === 'number'
              ? `${detail.summary.grossProfitRate.toFixed(1)}%`
              : '--'
          }}</strong>
        </div>
      </div>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="预测与历史">
      <HistoryGrid
        class="app-vxe-card-grid business-projects__table"
        :grid-events="historyGridEvents"
        :grid-options="historyGridOptions"
      />
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="验收记录">
      <template #extra>
        <Button size="small" type="primary" @click="toggleAcceptanceForm">
          {{ showAcceptanceForm ? '收起' : '新增验收' }}
        </Button>
      </template>
      <Form
        v-if="showAcceptanceForm"
        class="business-projects__inline-form"
        layout="vertical"
      >
        <Row :gutter="12">
          <Col :lg="5" :span="24"
            ><Form.Item label="验收编号"
              ><Input
                v-model:value="acceptanceForm.acceptanceCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="验收标题"
              ><Input
                v-model:value="acceptanceForm.title"
                :maxlength="128" /></Form.Item
          ></Col>
          <Col :lg="5" :span="12"
            ><Form.Item label="验收日期"
              ><Input
                v-model:value="acceptanceForm.acceptanceDate"
                type="date" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="结果"
              ><Input
                v-model:value="acceptanceForm.acceptanceResult"
                :maxlength="32" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="状态"
              ><Input
                v-model:value="acceptanceForm.status"
                :maxlength="32" /></Form.Item
          ></Col>
        </Row>
        <Row :gutter="12">
          <Col :lg="8" :span="24"
            ><Form.Item label="关联合同"
              ><Input
                v-model:value="acceptanceForm.linkedContractCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="8" :span="24"
            ><Form.Item label="质量检查编号"
              ><Input
                v-model:value="acceptanceForm.qualityInspectionCode"
                :maxlength="64" /></Form.Item
          ></Col>
        </Row>
        <Button
          :loading="savingAcceptance"
          type="primary"
          @click="handleSaveAcceptance"
          >{{ editingAcceptanceId ? '更新验收记录' : '保存验收记录' }}</Button
        >
      </Form>
      <AcceptanceGrid
        class="app-vxe-card-grid business-projects__table"
        :grid-events="acceptanceGridEvents"
        :grid-options="acceptanceGridOptions"
      >
        <template #operation="{ row }">
          <Button
            size="small"
            type="link"
            @click="handleEditAcceptanceRecord(row)"
          >
            编辑</Button
          >
          <Popconfirm
            ok-text="删除"
            title="确认删除该验收记录？"
            @confirm="handleDeleteAcceptanceRecord(row)"
          >
            <Button danger size="small" type="link">删除</Button>
          </Popconfirm>
        </template>
      </AcceptanceGrid>
    </Card>
  </div>
</template>
