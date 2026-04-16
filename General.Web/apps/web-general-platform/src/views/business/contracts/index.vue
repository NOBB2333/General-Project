<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '一期定位', value: '合同台账' },
  { label: '信息类型', value: '节点跟踪' },
  { label: '目标角色', value: 'PMO / 财务接口' },
  { label: '后续补强', value: '资料关联' },
];

const highlights = ['先做合同主数据与节点留痕', '后续接验收、开票、回款串联', '为资料归档和经营预警提供基础'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="合同台账页在一期只承担合同执行跟踪角色，先把合同金额、关键节点和责任归属纳入统一平台。"
    title="合同台账"
  />
</template>
