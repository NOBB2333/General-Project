<script lang="ts" setup>
import { Card, Table, Tag } from 'ant-design-vue';

import { useBusinessProjectsContext } from '../context';

const {
  contractColumns,
  contractRelationItems,
  detail,
  detailTableScroll,
  formatMoney,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-grid">
  <Card :bordered="false" title="合同关系链">
    <div class="business-projects__relation-list">
      <div v-for="relation in contractRelationItems" :key="relation.id" class="business-projects__relation-item">
        <div class="business-projects__relation-main">
          <div class="business-projects__relation-primary">
            <span class="business-projects__relation-label">收入合同</span>
            <strong>{{ relation.title }}</strong>
            <p>{{ relation.code }} · {{ relation.type }} · {{ relation.status }}</p>
          </div>
          <Tag v-if="relation.changeCode" color="gold">{{ relation.changeCode }}</Tag>
        </div>

        <div class="business-projects__relation-body">
          <div v-if="relation.childContracts.length > 0" class="business-projects__relation-sublist">
            <div v-for="contract in relation.childContracts" :key="contract.id" class="business-projects__relation-subitem">
              <span>衍生合同</span>
              <strong>{{ contract.title }}</strong>
              <p>{{ contract.contractCode }} · {{ contract.type }}</p>
            </div>
          </div>

          <div v-if="relation.linkedProcurements.length > 0" class="business-projects__relation-sublist">
            <div v-for="procurement in relation.linkedProcurements" :key="procurement.id" class="business-projects__relation-subitem">
              <span>分包/采购</span>
              <strong>{{ procurement.title }}</strong>
              <p>{{ procurement.procurementCode }} · {{ procurement.status }}</p>
            </div>
          </div>

          <div
            v-if="relation.childContracts.length === 0 && relation.linkedProcurements.length === 0"
            class="business-projects__relation-empty"
          >
            当前合同暂无衍生合同或分包/采购关联。
          </div>
        </div>
      </div>
    </div>
  </Card>

  <Card :bordered="false" title="合同台账">
    <Table
      class="business-projects__table"
      :columns="contractColumns"
      :data-source="detail.contracts"
      :pagination="false"
      row-key="id"
      :scroll="detailTableScroll"
      size="middle"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
      </template>
    </Table>
  </Card>
</div>
</template>
