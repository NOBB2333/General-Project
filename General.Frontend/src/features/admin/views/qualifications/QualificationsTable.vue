<script setup lang="ts">
import { Eye, Pencil, ShieldCheck, Trash2 } from 'lucide-vue-next';
import type { Qualification } from '../../model/qualifications.types';
defineProps<{ rows: readonly Qualification[] }>();
const emit = defineEmits<{ view: [item: Qualification]; edit: [item: Qualification]; remove: [item: Qualification] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>资质名称</th>
          <th>类别</th>
          <th>证书编号</th>
          <th>发证机构</th>
          <th>有效期</th>
          <th>责任人</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <span
              :class="$style.icon"
              :data-status="row.status"
              ><ShieldCheck
                :size="15"
                :stroke-width="1.8"
            /></span>
            <div>
              <strong>{{ row.name }}</strong
              ><small>{{ row.archiveCode }}</small>
            </div>
          </td>
          <td>
            <span
              :class="$style.category"
              :data-category="row.category"
              >{{ row.category }}</span
            >
          </td>
          <td>{{ row.certificateNo }}</td>
          <td>
            <span :class="$style.issuer">{{ row.issuer }}</span>
          </td>
          <td>
            <strong>{{ row.validFrom }} ~ {{ row.validTo }}</strong
            ><small
              v-if="row.expiryHint"
              :data-expired="row.expiryHint.includes('已过期')"
              >{{ row.expiryHint }}</small
            >
          </td>
          <td>{{ row.owner }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <button
              aria-label="查看资质"
              title="查看资质"
              @click="emit('view', row)"
            >
              <Eye
                :size="14"
                :stroke-width="1.8"
              /></button
            ><button
              aria-label="编辑资质"
              title="编辑资质"
              @click="emit('edit', row)"
            >
              <Pencil
                :size="14"
                :stroke-width="1.8"
              /></button
            ><button
              aria-label="删除资质"
              title="删除资质"
              @click="emit('remove', row)"
            >
              <Trash2
                :size="14"
                :stroke-width="1.8"
              />
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有资质记录
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
  @apply w-full min-w-[1320px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.scroller td {
  @apply h-[60px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td:first-child {
  @apply flex items-center gap-2;
}
.icon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.icon[data-status='即将到期'] {
  color: #de350b;
  background: #ffebe6;
}
.scroller td > div {
  @apply min-w-0;
}
.scroller td strong {
  @apply block font-semibold;
  color: var(--ui-text);
}
.scroller td small {
  @apply mt-1 block text-[8px];
  color: var(--ui-text-subtle);
}
.scroller td:nth-child(5) small {
  color: #ff8b00;
}
.scroller td:nth-child(5) small[data-expired='true'] {
  color: #de350b;
}
.issuer {
  @apply block max-w-[210px] truncate;
}
.category,
.status {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #0052cc;
  background: #deebff;
}
.category[data-category='安全资质'] {
  color: #de350b;
  background: #ffebe6;
}
.category[data-category='政府认定类资质'] {
  color: #8d5b00;
  background: #fff0b3;
}
.category[data-category='产品认证'] {
  color: #00875a;
  background: #e3fcef;
}
.status {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='即将到期'] {
  color: #ff8b00;
  background: #fff0b3;
}
.scroller td:last-child button {
  @apply rounded p-1.5;
  color: var(--ui-text-subtle);
}
.scroller td:last-child button:hover {
  color: var(--app-primary);
  background: var(--ui-surface-hover);
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
