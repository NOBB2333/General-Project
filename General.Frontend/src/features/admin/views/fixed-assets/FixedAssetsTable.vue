<script setup lang="ts">
import type { FixedAsset } from '../../model/fixed-assets.types';
defineProps<{ rows: readonly FixedAsset[] }>();
const emit = defineEmits<{ view: [asset: FixedAsset] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>资产编号</th>
          <th>资产名称</th>
          <th>分类</th>
          <th>原值(¥)</th>
          <th>净值(¥)</th>
          <th>月折旧</th>
          <th>使用人</th>
          <th>状态</th>
          <th>存放地点</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          tabindex="0"
          @click="emit('view', row)"
          @keydown.enter="emit('view', row)"
        >
          <td>
            <strong :class="$style.code">{{ row.code }}</strong>
          </td>
          <td>
            <strong>{{ row.name }}</strong
            ><small>{{ row.brand }} · {{ row.specification }}</small>
          </td>
          <td>
            <span
              :class="$style.category"
              :data-category="row.category"
              >{{ row.category }}</span
            >
          </td>
          <td>
            <b>{{ row.originalValue.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</b>
          </td>
          <td>
            <b>{{ row.netValue.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</b
            ><i :data-status="row.status" />
          </td>
          <td>{{ row.monthlyDepreciation?.toFixed(2) ?? '-' }}</td>
          <td>
            <span :class="$style.avatar">{{ row.user.slice(0, 1) }}</span
            >{{ row.user }}
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.location }}</td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选条件下没有固定资产
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto rounded border bg-white;
  border-color: var(--ui-border);
}
.scroller table {
  @apply w-full min-w-[1380px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.scroller td {
  @apply h-[58px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller tbody tr {
  @apply cursor-pointer outline-none transition-colors;
}
.scroller tbody tr:hover {
  background: var(--ui-surface-hover);
}
.scroller tbody tr:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}
.scroller td strong,
.scroller td b {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller td small {
  @apply mt-1 block text-[8px];
  color: var(--ui-text-subtle);
}
.code {
  color: var(--app-primary) !important;
}
.category,
.status {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #0052cc;
  background: #deebff;
}
.category[data-category='仪器仪表'] {
  color: #de350b;
  background: #ffebe6;
}
.category[data-category='办公家具'] {
  color: #6554c0;
  background: #eae6ff;
}
.status {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已报废'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='待处置'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-status='维修中'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='闲置'] {
  color: #ff8b00;
  background: #fff0b3;
}
.scroller td i {
  @apply mx-auto mt-1 block h-1 w-14 rounded;
  background: #00875a;
}
.scroller td i[data-status='已报废'],
.scroller td i[data-status='待处置'] {
  background: #ff5630;
}
.scroller td i[data-status='闲置'] {
  background: #ff8b00;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[8px] font-semibold text-white;
  background: #0052cc;
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
