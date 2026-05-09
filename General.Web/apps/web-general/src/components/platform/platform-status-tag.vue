<script lang="ts" setup>
import { computed } from 'vue';

import { Tag } from 'ant-design-vue';

const props = withDefaults(
  defineProps<{
    falseLabel?: string;
    falseValue?: boolean | number | string;
    trueLabel?: string;
    trueValue?: boolean | number | string;
    value?: boolean | number | string;
  }>(),
  {
    falseLabel: '停用',
    falseValue: false,
    trueLabel: '启用',
    trueValue: true,
    value: false,
  },
);

const active = computed(() => props.value === props.trueValue);
</script>

<template>
  <Tag :color="active ? 'success' : 'default'">
    {{ active ? trueLabel : falseLabel }}
  </Tag>
</template>
