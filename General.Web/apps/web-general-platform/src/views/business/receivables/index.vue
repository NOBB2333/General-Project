<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '经营节点', value: '回款' },
  { label: '一期重点', value: '逾期识别' },
  { label: '目标角色', value: 'PMO / 财务' },
  { label: '后续补强', value: '预警口径' },
];

const highlights = ['先做回款节点跟踪和状态留痕', '后续接逾期预警和经营看板', '为管理层查看重点项目经营状态提供入口'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="回款记录页负责承接一期经营主线中的最后一环，后续会在这里补回款状态、计划日期和逾期预警。"
    title="回款记录"
  />
</template>
