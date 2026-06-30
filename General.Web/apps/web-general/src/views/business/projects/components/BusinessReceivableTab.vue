<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import {
  Button,
  Card,
  Col,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Row,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useBusinessProjectsContext } from '../context';

const {
  detail,
  editingInvoiceId,
  editingReceivableId,
  editInvoice,
  editReceivable,
  formatMoney,
  getFocusedRowClass,
  handleDeleteInvoice,
  handleDeleteReceivable,
  handleSaveInvoice,
  handleSaveReceivable,
  invoiceForm,
  receivableColumns,
  receivableForm,
  savingInvoice,
  savingReceivable,
  showInvoiceForm,
  showReceivableForm,
  toggleInvoiceForm,
  toggleReceivableForm,
} = useBusinessProjectsContext();

function handleEditReceivableRecord(record: Record<string, any>) {
  editReceivable(record as BusinessApi.ReceivableItem);
}

function handleDeleteReceivableRecord(record: Record<string, any>) {
  return handleDeleteReceivable(record as BusinessApi.ReceivableItem);
}

function handleEditInvoiceRecord(record: Record<string, any>) {
  editInvoice(record as BusinessApi.InvoiceItem);
}

function handleDeleteInvoiceRecord(record: Record<string, any>) {
  return handleDeleteInvoice(record as BusinessApi.InvoiceItem);
}

function getReceivableRowClass(record: Record<string, any>) {
  return getFocusedRowClass('receivable', record);
}

function getInvoiceRowClass(record: Record<string, any>) {
  return getFocusedRowClass('invoice', record);
}

const receivableRows = computed(() => detail.value.receivables);
const invoiceRows = computed(() => detail.value.invoices);
const receivablePagination = useLocalVxePagination(receivableRows, {
  defaultPageSize: 10,
});
const invoicePagination = useLocalVxePagination(invoiceRows, {
  defaultPageSize: 10,
});

const invoiceColumns = [
  { field: 'invoiceCode', title: '发票编号', width: 160 },
  { align: 'left', field: 'title', title: '开票标题', width: 240 },
  { field: 'invoiceDate', title: '开票日期', width: 130 },
  { field: 'linkedContractCode', title: '关联合同', width: 160 },
  { field: 'amount', slots: { default: 'amount' }, title: '金额', width: 130 },
  {
    field: 'taxAmount',
    slots: { default: 'taxAmount' },
    title: '税额',
    width: 130,
  },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  {
    fixed: 'right',
    slots: { default: 'operation' },
    title: '操作',
    width: 150,
  },
] satisfies VxeGridProps<BusinessApi.InvoiceItem>['columns'];

const receivableGridOptions = computed<
  VxeGridProps<BusinessApi.ReceivableItem>
>(() => ({
  columns: receivableColumns,
  data: receivablePagination.pagedRows.value,
  pagerConfig: receivablePagination.pagerConfig.value,
  rowClassName: ({ row }) => getReceivableRowClass(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const invoiceGridOptions = computed<VxeGridProps<BusinessApi.InvoiceItem>>(
  () => ({
    columns: invoiceColumns,
    data: invoicePagination.pagedRows.value,
    pagerConfig: invoicePagination.pagerConfig.value,
    rowClassName: ({ row }) => getInvoiceRowClass(row),
    rowConfig: { keyField: 'id' },
    scrollX: { enabled: true },
    size: 'medium',
  }),
);

const receivableGridEvents: NativeVxeGridEvents<BusinessApi.ReceivableItem> = {
  'page-change': receivablePagination.handlePageChange,
};
const invoiceGridEvents: NativeVxeGridEvents<BusinessApi.InvoiceItem> = {
  'page-change': invoicePagination.handlePageChange,
};

const [ReceivableGrid] = useVbenVxeGrid<BusinessApi.ReceivableItem>({
  gridEvents: receivableGridEvents,
  gridOptions: {},
});
const [InvoiceGrid] = useVbenVxeGrid<BusinessApi.InvoiceItem>({
  gridEvents: invoiceGridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-grid">
    <Card class="rounded-2xl" :bordered="false" title="收尾专项视图">
      <div class="business-projects__summary-grid">
        <div class="business-projects__summary-item">
          <span>经营收尾状态</span
          ><strong>{{ detail.summary.businessCloseStatus }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>项目状态</span><strong>{{ detail.summary.status }}</strong>
        </div>
        <div class="business-projects__summary-item">
          <span>待回款事项</span
          ><strong
            >{{
              detail.receivables.filter((item: BusinessApi.ReceivableItem) =>
                item.status.includes('待回款'),
              ).length
            }}
            项</strong
          >
        </div>
        <div class="business-projects__summary-item">
          <span>年底预计回款</span
          ><strong>{{
            formatMoney(detail.summary.yearEndReceivableForecast)
          }}</strong>
        </div>
      </div>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="回款计划维护">
      <template #extra>
        <Button size="small" type="primary" @click="toggleReceivableForm">
          {{ showReceivableForm ? '收起' : '新增回款' }}
        </Button>
      </template>
      <Form
        v-if="showReceivableForm"
        class="business-projects__inline-form"
        layout="vertical"
      >
        <Row :gutter="12">
          <Col :lg="5" :span="24"
            ><Form.Item label="回款编号"
              ><Input
                v-model:value="receivableForm.receivableCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="回款事项"
              ><Input
                v-model:value="receivableForm.title"
                :maxlength="128" /></Form.Item
          ></Col>
          <Col :lg="5" :span="12"
            ><Form.Item label="计划日期"
              ><Input
                v-model:value="receivableForm.plannedDate"
                type="date" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="应回金额"
              ><InputNumber
                v-model:value="receivableForm.totalAmount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="已回金额"
              ><InputNumber
                v-model:value="receivableForm.receivedAmount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
        </Row>
        <Row :gutter="12">
          <Col :lg="6" :span="24"
            ><Form.Item label="状态"
              ><Input
                v-model:value="receivableForm.status"
                :maxlength="32" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="开票编号"
              ><Input
                v-model:value="receivableForm.invoiceCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="关联合同"
              ><Input
                v-model:value="receivableForm.linkedContractCode"
                :maxlength="64" /></Form.Item
          ></Col>
        </Row>
        <Button
          :loading="savingReceivable"
          type="primary"
          @click="handleSaveReceivable"
          >{{ editingReceivableId ? '更新回款计划' : '保存回款计划' }}</Button
        >
      </Form>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="开票记录维护">
      <template #extra>
        <Button size="small" type="primary" @click="toggleInvoiceForm">
          {{ showInvoiceForm ? '收起' : '新增开票' }}
        </Button>
      </template>
      <Form
        v-if="showInvoiceForm"
        class="business-projects__inline-form"
        layout="vertical"
      >
        <Row :gutter="12">
          <Col :lg="5" :span="24"
            ><Form.Item label="发票编号"
              ><Input
                v-model:value="invoiceForm.invoiceCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="开票标题"
              ><Input
                v-model:value="invoiceForm.title"
                :maxlength="128" /></Form.Item
          ></Col>
          <Col :lg="5" :span="12"
            ><Form.Item label="开票日期"
              ><Input
                v-model:value="invoiceForm.invoiceDate"
                type="date" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="金额"
              ><InputNumber
                v-model:value="invoiceForm.amount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
          <Col :lg="4" :span="12"
            ><Form.Item label="税额"
              ><InputNumber
                v-model:value="invoiceForm.taxAmount"
                class="business-projects__number-input"
                :min="0" /></Form.Item
          ></Col>
        </Row>
        <Row :gutter="12">
          <Col :lg="6" :span="24"
            ><Form.Item label="状态"
              ><Input
                v-model:value="invoiceForm.status"
                :maxlength="32" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="关联合同"
              ><Input
                v-model:value="invoiceForm.linkedContractCode"
                :maxlength="64" /></Form.Item
          ></Col>
          <Col :lg="6" :span="24"
            ><Form.Item label="关联回款"
              ><Input
                v-model:value="invoiceForm.receivableCode"
                :maxlength="64" /></Form.Item
          ></Col>
        </Row>
        <Button
          :loading="savingInvoice"
          type="primary"
          @click="handleSaveInvoice"
          >{{ editingInvoiceId ? '更新开票记录' : '保存开票记录' }}</Button
        >
      </Form>
    </Card>

    <ReceivableGrid
      class="app-vxe-card-grid business-projects__table"
      :grid-events="receivableGridEvents"
      :grid-options="receivableGridOptions"
    >
      <template #plannedDate="{ row }">
        {{ row.plannedDate || '-' }}
      </template>
      <template #totalAmount="{ row }">
        {{ formatMoney(row.totalAmount) }}
      </template>
      <template #receivedAmount="{ row }">
        {{ formatMoney(row.receivedAmount) }}
      </template>
      <template #status="{ row }">
        <Tag color="blue">{{ row.status }}</Tag>
      </template>
      <template #operation="{ row }">
        <Button
          size="small"
          type="link"
          @click="handleEditReceivableRecord(row)"
        >
          编辑</Button
        >
        <Popconfirm
          ok-text="删除"
          title="确认删除该回款计划？"
          @confirm="handleDeleteReceivableRecord(row)"
        >
          <Button danger size="small" type="link">删除</Button>
        </Popconfirm>
      </template>
    </ReceivableGrid>

    <InvoiceGrid
      class="app-vxe-card-grid business-projects__table"
      :grid-events="invoiceGridEvents"
      :grid-options="invoiceGridOptions"
    >
      <template #amount="{ row }">
        {{ formatMoney(row.amount) }}
      </template>
      <template #taxAmount="{ row }">
        {{ formatMoney(row.taxAmount) }}
      </template>
      <template #status="{ row }">
        <Tag color="blue">{{ row.status }}</Tag>
      </template>
      <template #operation="{ row }">
        <Button size="small" type="link" @click="handleEditInvoiceRecord(row)">
          编辑</Button
        >
        <Popconfirm
          ok-text="删除"
          title="确认删除该开票记录？"
          @confirm="handleDeleteInvoiceRecord(row)"
        >
          <Button danger size="small" type="link">删除</Button>
        </Popconfirm>
      </template>
    </InvoiceGrid>
  </div>
</template>
