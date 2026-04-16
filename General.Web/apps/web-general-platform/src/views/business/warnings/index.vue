<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '目标角色', value: 'PMO / 管理层' },
  { label: '一期定位', value: '异常提醒' },
  { label: '数据来源', value: '预算 + 合同 + 回款' },
  { label: '后续补强', value: '趋势分层' },
];

const highlights = ['承接经营预警统一入口', '后续接预算偏差、回款逾期、关键节点异常', '作为管理层经营视角入口之一'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="经营预警页在一期先作为异常聚合入口，后续把预算偏差、合同节点、开票与回款逾期统一汇总到这里。"
    title="经营预警"
  />
</template>
