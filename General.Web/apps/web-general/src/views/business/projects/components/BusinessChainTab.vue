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
  Select,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useBusinessProjectsContext } from '../context';

const {
  businessChainForm,
  businessColumns,
  detail,
  editingBusinessChainId,
  editingProcurementId,
  editBusinessChain,
  editProcurement,
  formatMoney,
  getFocusedRowClass,
  handleDeleteBusinessChain,
  handleDeleteProcurement,
  handleSaveBusinessChain,
  handleSaveProcurement,
  ownerSelectOptions,
  procurementForm,
  procurementColumns,
  savingBusinessChain,
  savingProcurement,
  showBusinessChainForm,
  showProcurementForm,
  toggleBusinessChainForm,
  toggleProcurementForm,
} = useBusinessProjectsContext();

function handleEditBusinessChainRecord(record: Record<string, any>) {
  editBusinessChain(record as BusinessApi.BusinessChainItem);
}

function handleDeleteBusinessChainRecord(record: Record<string, any>) {
  return handleDeleteBusinessChain(record as BusinessApi.BusinessChainItem);
}

function handleEditProcurementRecord(record: Record<string, any>) {
  editProcurement(record as BusinessApi.ProcurementItem);
}

function handleDeleteProcurementRecord(record: Record<string, any>) {
  return handleDeleteProcurement(record as BusinessApi.ProcurementItem);
}

function getBusinessChainRowClass(record: Record<string, any>) {
  return getFocusedRowClass('businessChain', record);
}

function getProcurementRowClass(record: Record<string, any>) {
  return getFocusedRowClass('procurement', record);
}

const businessRows = computed(() => detail.value.businessChains);
const procurementRows = computed(() => detail.value.procurements);
const businessPagination = useLocalVxePagination(businessRows, {
  defaultPageSize: 10,
});
const procurementPagination = useLocalVxePagination(procurementRows, {
  defaultPageSize: 10,
});

const businessGridOptions = computed<
  VxeGridProps<BusinessApi.BusinessChainItem>
>(() => ({
  columns: businessColumns,
  data: businessPagination.pagedRows.value,
  pagerConfig: businessPagination.pagerConfig.value,
  rowClassName: ({ row }) => getBusinessChainRowClass(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const procurementGridOptions = computed<
  VxeGridProps<BusinessApi.ProcurementItem>
>(() => ({
  columns: procurementColumns,
  data: procurementPagination.pagedRows.value,
  pagerConfig: procurementPagination.pagerConfig.value,
  rowClassName: ({ row }) => getProcurementRowClass(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const businessGridEvents: NativeVxeGridEvents<BusinessApi.BusinessChainItem> = {
  'page-change': businessPagination.handlePageChange,
};
const procurementGridEvents: NativeVxeGridEvents<BusinessApi.ProcurementItem> =
  {
    'page-change': procurementPagination.handlePageChange,
  };

const [BusinessGrid] = useVbenVxeGrid<BusinessApi.BusinessChainItem>({
  gridEvents: businessGridEvents,
  gridOptions: {},
});
const [ProcurementGrid] = useVbenVxeGrid<BusinessApi.ProcurementItem>({
  gridEvents: procurementGridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-grid">
    <Card class="rounded-2xl" :bordered="false" title="商务链与分包链">
      <template #extra>
        <Button size="small" type="primary" @click="toggleBusinessChainForm">
          {{ showBusinessChainForm ? '收起' : '新增链路' }}
        </Button>
      </template>
      <div v-if="showBusinessChainForm" class="business-projects__inline-form">
        <Form layout="vertical">
          <Row :gutter="12">
            <Col :lg="5" :span="24"
              ><Form.Item label="链路编号"
                ><Input
                  v-model:value="businessChainForm.chainCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="标题"
                ><Input
                  v-model:value="businessChainForm.title"
                  :maxlength="128" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="阶段"
                ><Input
                  v-model:value="businessChainForm.stage"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="类型"
                ><Input
                  v-model:value="businessChainForm.type"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="负责人"
                ><Select
                  v-model:value="businessChainForm.ownerUserId"
                  :options="ownerSelectOptions"
                  show-search /></Form.Item
            ></Col>
          </Row>
          <Row :gutter="12">
            <Col :lg="6" :span="24"
              ><Form.Item label="状态"
                ><Input
                  v-model:value="businessChainForm.status"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="关联合同"
                ><Input
                  v-model:value="businessChainForm.linkedContractCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="变更来源"
                ><Input
                  v-model:value="businessChainForm.sourceChangeCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="摘要"
                ><Input
                  v-model:value="businessChainForm.summary"
                  :maxlength="512" /></Form.Item
            ></Col>
          </Row>
          <Button
            :loading="savingBusinessChain"
            type="primary"
            @click="handleSaveBusinessChain"
            >{{ editingBusinessChainId ? '更新链路' : '保存链路' }}</Button
          >
        </Form>
      </div>
      <BusinessGrid
        class="app-vxe-card-grid business-projects__table"
        :grid-events="businessGridEvents"
        :grid-options="businessGridOptions"
      >
        <template #status="{ row }">
          <Tag color="blue">{{ row.status }}</Tag>
        </template>
        <template #operation="{ row }">
          <Button
            size="small"
            type="link"
            @click="handleEditBusinessChainRecord(row)"
          >
            编辑</Button
          >
          <Popconfirm
            ok-text="删除"
            title="确认删除该经营链事项？"
            @confirm="handleDeleteBusinessChainRecord(row)"
          >
            <Button danger size="small" type="link">删除</Button>
          </Popconfirm>
        </template>
      </BusinessGrid>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="分包与采购台账">
      <template #extra>
        <Button size="small" type="primary" @click="toggleProcurementForm">
          {{ showProcurementForm ? '收起' : '新增采购' }}
        </Button>
      </template>
      <div v-if="showProcurementForm" class="business-projects__inline-form">
        <Form layout="vertical">
          <Row :gutter="12">
            <Col :lg="5" :span="24"
              ><Form.Item label="采购编号"
                ><Input
                  v-model:value="procurementForm.procurementCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="事项标题"
                ><Input
                  v-model:value="procurementForm.title"
                  :maxlength="128" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="供应商"
                ><Input
                  v-model:value="procurementForm.supplierName"
                  :maxlength="128" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="金额"
                ><InputNumber
                  v-model:value="procurementForm.amount"
                  class="business-projects__number-input"
                  :min="0" /></Form.Item
            ></Col>
            <Col :lg="5" :span="12"
              ><Form.Item label="签订日期"
                ><Input
                  v-model:value="procurementForm.signDate"
                  type="date" /></Form.Item
            ></Col>
          </Row>
          <Row :gutter="12">
            <Col :lg="6" :span="24"
              ><Form.Item label="阶段"
                ><Input
                  v-model:value="procurementForm.stage"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="状态"
                ><Input
                  v-model:value="procurementForm.status"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="来源合同"
                ><Input
                  v-model:value="procurementForm.linkedContractCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="变更来源"
                ><Input
                  v-model:value="procurementForm.sourceChangeCode"
                  :maxlength="64" /></Form.Item
            ></Col>
          </Row>
          <Button
            :loading="savingProcurement"
            type="primary"
            @click="handleSaveProcurement"
            >{{
              editingProcurementId ? '更新采购事项' : '保存采购事项'
            }}</Button
          >
        </Form>
      </div>
      <ProcurementGrid
        class="app-vxe-card-grid business-projects__table"
        :grid-events="procurementGridEvents"
        :grid-options="procurementGridOptions"
      >
        <template #amount="{ row }">
          {{ formatMoney(row.amount) }}
        </template>
        <template #status="{ row }">
          <Tag color="blue">{{ row.status }}</Tag>
        </template>
        <template #operation="{ row }">
          <Button
            size="small"
            type="link"
            @click="handleEditProcurementRecord(row)"
          >
            编辑</Button
          >
          <Popconfirm
            ok-text="删除"
            title="确认删除该采购事项？"
            @confirm="handleDeleteProcurementRecord(row)"
          >
            <Button danger size="small" type="link">删除</Button>
          </Popconfirm>
        </template>
      </ProcurementGrid>
    </Card>
  </div>
</template>
