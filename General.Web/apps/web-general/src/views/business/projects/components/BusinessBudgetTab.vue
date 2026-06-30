<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import {
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Row,
  Col,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useBusinessProjectsContext } from '../context';

const {
  budgetForm,
  budgetColumns,
  detail,
  editingBudgetId,
  editBudget,
  formatMoney,
  getFocusedRowClass,
  handleDeleteBudget,
  handleSaveBudget,
  savingBudget,
  showBudgetForm,
  toggleBudgetForm,
} = useBusinessProjectsContext();

function handleEditBudgetRecord(record: Record<string, any>) {
  editBudget(record as BusinessApi.BudgetExecutionItem);
}

function handleDeleteBudgetRecord(record: Record<string, any>) {
  return handleDeleteBudget(record as BusinessApi.BudgetExecutionItem);
}

function getBudgetRowClass(record: Record<string, any>) {
  return getFocusedRowClass('budget', record);
}

const budgetRows = computed(() => detail.value.budgetExecutions);
const budgetPagination = useLocalVxePagination(budgetRows, {
  defaultPageSize: 10,
});

const gridOptions = computed<VxeGridProps<BusinessApi.BudgetExecutionItem>>(
  () => ({
    columns: budgetColumns,
    data: budgetPagination.pagedRows.value,
    pagerConfig: budgetPagination.pagerConfig.value,
    rowClassName: ({ row }) => getBudgetRowClass(row),
    rowConfig: { keyField: 'id' },
    scrollX: { enabled: true },
    size: 'medium',
  }),
);

const gridEvents: NativeVxeGridEvents<BusinessApi.BudgetExecutionItem> = {
  'page-change': budgetPagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<BusinessApi.BudgetExecutionItem>({
  gridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-panel">
    <Card
      :bordered="false"
      class="business-projects__form-card rounded-2xl"
      title="预算执行维护"
    >
      <template #extra>
        <Button size="small" type="primary" @click="toggleBudgetForm">{{
          showBudgetForm ? '收起' : '新增预算项'
        }}</Button>
      </template>
      <Form v-if="showBudgetForm" layout="vertical">
        <Row :gutter="12">
          <Col :lg="6" :span="24"
            ><Form.Item label="预算编号"
              ><Input
                v-model:value="budgetForm.budgetCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="预算项"
              ><Input
                v-model:value="budgetForm.category"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="调整后预算"
              ><InputNumber
                v-model:value="budgetForm.adjustedAmount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="已执行金额"
              ><InputNumber
                v-model:value="budgetForm.executedAmount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="排序"
              ><InputNumber
                v-model:value="budgetForm.sortOrder"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
        </Row>
        <Button
          :loading="savingBudget"
          type="primary"
          @click="handleSaveBudget"
          >{{ editingBudgetId ? '更新预算项' : '保存预算项' }}</Button
        >
      </Form>
    </Card>

    <Grid
      class="business-projects__table"
      :grid-events="gridEvents"
      :grid-options="gridOptions"
    >
      <template #adjustedAmount="{ row }">
        {{ formatMoney(row.adjustedAmount) }}
      </template>
      <template #executedAmount="{ row }">
        {{ formatMoney(row.executedAmount) }}
      </template>
      <template #varianceAmount="{ row }">
        <Tag
          :color="
            typeof row.varianceAmount === 'number' && row.varianceAmount > 0
              ? 'red'
              : 'blue'
          "
        >
          {{ formatMoney(row.varianceAmount) }}
        </Tag>
      </template>
      <template #operation="{ row }">
        <Button size="small" type="link" @click="handleEditBudgetRecord(row)">
          编辑</Button
        >
        <Popconfirm
          ok-text="删除"
          title="确认删除该预算执行项？"
          @confirm="handleDeleteBudgetRecord(row)"
        >
          <Button danger size="small" type="link">删除</Button>
        </Popconfirm>
      </template>
    </Grid>
  </div>
</template>
