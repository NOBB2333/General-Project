<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { ArrivalRecord } from '../../model/arrival-register.types';
defineProps<{ rows: readonly ArrivalRecord[] }>();
const emit = defineEmits<{ view: [row: ArrivalRecord] }>();
const money = new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY', maximumFractionDigits: 0 });
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>到货单号</th>
          <th>类型</th>
          <th>到货日期</th>
          <th>仓库</th>
          <th>对方</th>
          <th>关联单号</th>
          <th>数量</th>
          <th>金额</th>
          <th>状态</th>
          <th>检验单</th>
          <th>入库单</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong>{{ row.id }}</strong>
          </td>
          <td>
            <span
              :class="$style.type"
              :data-type="row.type"
              >{{ row.type }}</span
            >
          </td>
          <td>{{ row.date }}</td>
          <td>{{ row.warehouse }}</td>
          <td>{{ row.party }}</td>
          <td>
            <a
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.relatedCode }}</a
            >
          </td>
          <td>
            <b>{{ row.quantity }}</b>
          </td>
          <td>
            <b>{{ money.format(row.amount) }}</b>
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <a
              v-if="row.inspectionCode !== '-'"
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.inspectionCode }}</a
            ><span v-else>-</span>
          </td>
          <td>
            <a
              v-if="row.inboundCode !== '-'"
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.inboundCode }}</a
            ><span v-else>-</span>
          </td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="查看到货详情"
              title="查看到货详情"
              @click="emit('view', row)"
              ><Eye
                :size="14"
                :stroke-width="1.8"
            /></UiButton>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的到货记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply min-w-[1380px] w-full border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: #6b778c;
  border-color: var(--ui-border);
  background: #f4f5f7;
}
.scroller td {
  @apply h-12 whitespace-nowrap border-b px-3;
  color: #42526e;
  border-color: var(--ui-border-subtle);
}
.scroller td:first-child strong,
.scroller a {
  color: #0052cc;
}
.type,
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #0052cc;
  background: #deebff;
}
.type[data-type='生产到货'] {
  color: #00875a;
  background: #e3fcef;
}
.type[data-type='退货到货'] {
  color: #de350b;
  background: #ffebe6;
}
.type[data-type='其他到货'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-status='待检'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='检验中'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已入库'],
.status[data-status='免检-已入库'],
.status[data-status='已检验-合格'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已检验-不合格'] {
  color: #de350b;
  background: #ffebe6;
}
.empty {
  @apply py-16 text-center text-[12px];
  color: #6b778c;
}
</style>
