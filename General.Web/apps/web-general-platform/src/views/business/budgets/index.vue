<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '一期定位', value: '轻量跟踪' },
  { label: '敏感字段', value: '需控可见' },
  { label: '目标角色', value: 'PMO / PM' },
  { label: '后续补强', value: '偏差预警' },
];

const highlights = ['一期只做项目级预算跟踪', '敏感金额字段后续按权限显隐', '为经营预警和回款主线提供基础数据'];
</script>

<template>
  <FeatureShell
    eyebrow="经营管理区"
    :highlights="highlights"
    :metrics="metrics"
    summary="预算页一期只做轻量跟踪，不做财务核算，重点是把项目预算总额、执行金额和偏差预警纳入统一平台。"
    title="预算"
  />
</template>
