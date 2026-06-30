<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import {
  Button,
  Card,
  Checkbox,
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
  contractForm,
  contractColumns,
  contractRelationItems,
  contractTypeOptions,
  detail,
  editingContractId,
  editContract,
  formatMoney,
  getFocusedRowClass,
  handleDeleteContract,
  handleSaveContract,
  savingContract,
  showContractForm,
  toggleContractForm,
} = useBusinessProjectsContext();

function handleEditContractRecord(record: Record<string, any>) {
  editContract(record as BusinessApi.ContractItem);
}

function handleDeleteContractRecord(record: Record<string, any>) {
  return handleDeleteContract(record as BusinessApi.ContractItem);
}

function getContractRowClass(record: Record<string, any>) {
  return getFocusedRowClass('contract', record);
}

const contractRows = computed(() => detail.value.contracts);
const contractPagination = useLocalVxePagination(contractRows, {
  defaultPageSize: 10,
});

const gridOptions = computed<VxeGridProps<BusinessApi.ContractItem>>(() => ({
  columns: contractColumns,
  data: contractPagination.pagedRows.value,
  pagerConfig: contractPagination.pagerConfig.value,
  rowClassName: ({ row }) => getContractRowClass(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const gridEvents: NativeVxeGridEvents<BusinessApi.ContractItem> = {
  'page-change': contractPagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<BusinessApi.ContractItem>({
  gridEvents,
  gridOptions: {},
});
</script>

<template>
  <div class="business-projects__tab-grid">
    <Card class="rounded-2xl" :bordered="false" title="合同关系链">
      <div class="business-projects__relation-list">
        <div
          v-for="relation in contractRelationItems"
          :key="relation.id"
          class="business-projects__relation-item"
        >
          <div class="business-projects__relation-main">
            <div class="business-projects__relation-primary">
              <span class="business-projects__relation-label">收入合同</span>
              <strong>{{ relation.title }}</strong>
              <p>
                {{ relation.code }} · {{ relation.type }} ·
                {{ relation.status }}
              </p>
            </div>
            <Tag v-if="relation.changeCode" color="gold">{{
              relation.changeCode
            }}</Tag>
          </div>

          <div class="business-projects__relation-body">
            <div
              v-if="relation.childContracts.length > 0"
              class="business-projects__relation-sublist"
            >
              <div
                v-for="contract in relation.childContracts"
                :key="contract.id"
                class="business-projects__relation-subitem"
              >
                <span>衍生合同</span>
                <strong>{{ contract.title }}</strong>
                <p>{{ contract.contractCode }} · {{ contract.type }}</p>
              </div>
            </div>

            <div
              v-if="relation.linkedProcurements.length > 0"
              class="business-projects__relation-sublist"
            >
              <div
                v-for="procurement in relation.linkedProcurements"
                :key="procurement.id"
                class="business-projects__relation-subitem"
              >
                <span>分包/采购</span>
                <strong>{{ procurement.title }}</strong>
                <p>
                  {{ procurement.procurementCode }} · {{ procurement.status }}
                </p>
              </div>
            </div>

            <div
              v-if="
                relation.childContracts.length === 0 &&
                relation.linkedProcurements.length === 0
              "
              class="business-projects__relation-empty"
            >
              当前合同暂无衍生合同或分包/采购关联。
            </div>
          </div>
        </div>
      </div>
    </Card>

    <Card class="rounded-2xl" :bordered="false" title="合同台账">
      <template #extra>
        <Button size="small" type="primary" @click="toggleContractForm">
          {{ showContractForm ? '收起' : '新增合同' }}
        </Button>
      </template>
      <div v-if="showContractForm" class="business-projects__inline-form">
        <Form layout="vertical">
          <Row :gutter="12">
            <Col :lg="5" :span="24"
              ><Form.Item label="合同编号"
                ><Input
                  v-model:value="contractForm.contractCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="6" :span="24"
              ><Form.Item label="合同名称"
                ><Input
                  v-model:value="contractForm.title"
                  :maxlength="128" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="类型"
                ><Select
                  v-model:value="contractForm.type"
                  :options="contractTypeOptions" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="金额"
                ><InputNumber
                  v-model:value="contractForm.amount"
                  class="business-projects__number-input"
                  :min="0" /></Form.Item
            ></Col>
            <Col :lg="5" :span="12"
              ><Form.Item label="签订日期"
                ><Input
                  v-model:value="contractForm.signDate"
                  type="date" /></Form.Item
            ></Col>
          </Row>
          <Row :gutter="12">
            <Col :lg="5" :span="24"
              ><Form.Item label="相对方"
                ><Input
                  v-model:value="contractForm.counterpartyName"
                  :maxlength="128" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="状态"
                ><Input
                  v-model:value="contractForm.status"
                  :maxlength="32" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="父合同"
                ><Input
                  v-model:value="contractForm.parentContractCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="5" :span="24"
              ><Form.Item label="变更编号"
                ><Input
                  v-model:value="contractForm.sourceChangeCode"
                  :maxlength="64" /></Form.Item
            ></Col>
            <Col :lg="4" :span="12"
              ><Form.Item label="收入合同"
                ><Checkbox
                  v-model:checked="contractForm.isRevenueContract" /></Form.Item
            ></Col>
          </Row>
          <Button
            :loading="savingContract"
            type="primary"
            @click="handleSaveContract"
            >{{ editingContractId ? '更新合同' : '保存合同' }}</Button
          >
        </Form>
      </div>
      <Grid
        class="business-projects__table"
        :grid-events="gridEvents"
        :grid-options="gridOptions"
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
            @click="handleEditContractRecord(row)"
          >
            编辑</Button
          >
          <Popconfirm
            ok-text="删除"
            title="确认删除该合同？"
            @confirm="handleDeleteContractRecord(row)"
          >
            <Button danger size="small" type="link">删除</Button>
          </Popconfirm>
        </template>
      </Grid>
    </Card>
  </div>
</template>
