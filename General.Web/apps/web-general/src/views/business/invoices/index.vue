<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '经营节点', value: '开票' },
  { label: '一期模式', value: '台账记录' },
  { label: '目标角色', value: 'PMO / 财务' },
  { label: '后续补强', value: '金额联动' },
];

const highlights = ['记录开票时间、金额与状态', '后续接合同回款联动校验', '为经营预警提供输入数据'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="开票记录页在一期先提供经营跟踪台账能力，确保合同执行后的开票节点能够留痕、查询和追溯。"
    title="开票记录"
  />
</template>
