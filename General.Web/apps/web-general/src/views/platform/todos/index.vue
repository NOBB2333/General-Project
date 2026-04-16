<script lang="ts" setup>
import FeatureShell from '#/views/_shared/feature-shell.vue';

const metrics = [
  { label: '高频入口', value: 'PM / PMO' },
  { label: '一期状态', value: '骨架已接入' },
  { label: '待办来源', value: '项目 + 审批' },
  { label: '权限模式', value: '后端鉴权' },
];

const highlights = ['统一收口个人待办', '后续接审批与项目行动项', '支持按角色展示紧急事项'];
</script>

<template>
  <FeatureShell
    eyebrow="平台中心"
    :highlights="highlights"
    :metrics="metrics"
    summary="一期先把个人待办作为统一登录后的通用入口，后续把审批、项目行动项、风险催办和资料审计待处理统一汇总到这里。"
    title="我的待办"
  />
</template>
