<script setup lang="ts">
import { Eye, Pencil, Plus, Trash2 } from 'lucide-vue-next';
import type { MaintEquipment } from '../../model/equipment-maint.types';
defineProps<{ rows: readonly MaintEquipment[] }>();
const emit = defineEmits<{
  view: [item: MaintEquipment];
  edit: [item: MaintEquipment];
  maintain: [item: MaintEquipment];
  remove: [item: MaintEquipment];
}>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>设备编号</th>
          <th>设备名称</th>
          <th>型号</th>
          <th>制造商</th>
          <th>分类</th>
          <th>位置</th>
          <th>状态</th>
          <th>负责人</th>
          <th>上次保养</th>
          <th>下次保养</th>
          <th>资产原值</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong :class="$style.code">{{ row.code }}</strong>
          </td>
          <td>
            <strong>{{ row.name }}</strong>
          </td>
          <td>{{ row.model }}</td>
          <td>{{ row.manufacturer }}</td>
          <td>
            <span :class="$style.category">{{ row.category }}</span>
          </td>
          <td>{{ row.location }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.owner }}</td>
          <td>{{ row.lastMaintenance }}</td>
          <td>
            <strong :data-overdue="row.overdue"
              >{{ row.nextMaintenance }}<template v-if="row.overdue"> !</template></strong
            >
          </td>
          <td>¥{{ row.assetValue.toLocaleString('zh-CN') }}</td>
          <td>
            <button
              title="查看设备"
              @click="emit('view', row)"
            >
              <Eye
                :size="13"
                :stroke-width="1.8"
              /></button
            ><button
              title="编辑设备"
              @click="emit('edit', row)"
            >
              <Pencil
                :size="13"
                :stroke-width="1.8"
              /></button
            ><button
              title="创建维保"
              @click="emit('maintain', row)"
            >
              <Plus
                :size="13"
                :stroke-width="1.8"
              /></button
            ><button
              title="删除设备"
              @click="emit('remove', row)"
            >
              <Trash2
                :size="13"
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
      当前条件下没有设备
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply mx-6 overflow-x-auto rounded border bg-white;
  border-color: var(--ui-border);
}
.scroller table {
  @apply w-full min-w-[1540px] border-collapse text-left text-[9px];
}
.scroller th {
  @apply h-10 border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.scroller td {
  @apply h-[52px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.code {
  color: var(--app-primary) !important;
}
.category {
  @apply rounded px-2 py-1 text-[8px];
  background: var(--ui-surface-hover);
}
.status {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='维修中'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='闲置'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='已报废'] {
  color: #de350b;
  background: #ffebe6;
}
.scroller strong[data-overdue='true'] {
  color: #de350b;
}
.scroller td:last-child button {
  @apply rounded p-1.5;
  color: var(--ui-text-subtle);
}
.scroller td:last-child button:hover {
  color: var(--app-primary);
  background: var(--ui-surface-hover);
}
.scroller td:last-child button:nth-child(3) {
  color: #00875a;
}
.scroller td:last-child button:last-child {
  color: #de350b;
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
