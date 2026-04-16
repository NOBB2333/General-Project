<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '经营节点', value: '验收' },
  { label: '一期重点', value: '记录留痕' },
  { label: '目标角色', value: 'PM / 财务接口' },
  { label: '后续补强', value: '附件归档' },
];

const highlights = ['一期先做验收记录和时间节点', '后续接验收资料上传与审批链路', '与合同台账、项目资料联动'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="验收记录页用于把项目交付后的验收节点纳入统一管理，一期先保证记录留痕和后续经营链路可追溯。"
    title="验收记录"
  />
</template>
