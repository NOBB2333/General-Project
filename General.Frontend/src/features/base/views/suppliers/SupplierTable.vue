<script setup lang="ts">
import { Eye, Pencil, Phone, Star, Trash2 } from 'lucide-vue-next';
import { UiTooltip } from '@/components/ui';
import type { SupplierRow } from '../../model/suppliers.types';

defineProps<{ rows: readonly SupplierRow[]; followedIds: ReadonlySet<string> }>();
const emit = defineEmits<{
  open: [row: SupplierRow];
  edit: [row: SupplierRow];
  remove: [row: SupplierRow];
  toggleFollow: [id: string];
}>();

const levelClass: Record<SupplierRow['level'], string> = {
  A: 'levelA',
  B: 'levelB',
  C: 'levelC',
  D: 'levelD',
};
const statusClass: Record<SupplierRow['status'], string> = {
  已审批: 'statusApproved',
  待审批: 'statusPending',
  草稿: 'statusDraft',
  已驳回: 'statusRejected',
};

function formatMoney(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}
</script>

<template>
  <section :class="$style.frame">
    <div :class="$style.scroll">
      <table :class="$style.table">
        <thead>
          <tr>
            <th aria-label="关注" />
            <th>供应商名称</th>
            <th>级别</th>
            <th>联系人</th>
            <th>联系电话</th>
            <th>负责人</th>
            <th>付款方式</th>
            <th>历史采购额</th>
            <th>应付余额</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td :class="$style.followCell">
              <button
                type="button"
                :class="[$style.iconButton, followedIds.has(row.id) && $style.followed]"
                :aria-label="followedIds.has(row.id) ? `取消关注 ${row.name}` : `关注 ${row.name}`"
                @click="emit('toggleFollow', row.id)"
              >
                <Star
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
            </td>
            <td :class="$style.nameCell">
              <button
                type="button"
                :class="$style.nameButton"
                @click="emit('open', row)"
              >
                {{ row.name }}
              </button>
            </td>
            <td :class="$style.center">
              <span :class="[$style.badge, $style[levelClass[row.level]]]">{{ row.level }}</span>
            </td>
            <td>{{ row.contact }}</td>
            <td>
              <span :class="$style.phone">
                <Phone
                  :size="13"
                  :stroke-width="1.8"
                />
                {{ row.phone }}
              </span>
            </td>
            <td>{{ row.owner }}</td>
            <td>
              <span :class="$style.payment">{{ row.payMethod }}</span>
            </td>
            <td :class="$style.money">{{ formatMoney(row.totalPurchase) }}</td>
            <td :class="[$style.money, row.payable > 100000 && $style.payable]">{{ formatMoney(row.payable) }}</td>
            <td :class="$style.center">
              <span :class="[$style.badge, $style[statusClass[row.status]]]">{{ row.status }}</span>
            </td>
            <td>
              <div :class="$style.actions">
                <UiTooltip content="查看详情">
                  <button
                    type="button"
                    aria-label="查看详情"
                    @click="emit('open', row)"
                  >
                    <Eye
                      :size="14"
                      :stroke-width="1.8"
                    />
                  </button>
                </UiTooltip>
                <UiTooltip content="编辑">
                  <button
                    type="button"
                    aria-label="编辑供应商"
                    @click="emit('edit', row)"
                  >
                    <Pencil
                      :size="14"
                      :stroke-width="1.8"
                    />
                  </button>
                </UiTooltip>
                <UiTooltip content="删除">
                  <button
                    type="button"
                    aria-label="删除供应商"
                    @click="emit('remove', row)"
                  >
                    <Trash2
                      :size="14"
                      :stroke-width="1.8"
                    />
                  </button>
                </UiTooltip>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的供应商
    </div>
    <footer :class="$style.footer">
      <span>共 {{ rows.length }} 条记录</span>
      <div :class="$style.pagination">
        <button
          type="button"
          disabled
          aria-label="上一页"
          :class="$style.pageButton"
        >
          ‹
        </button>
        <button
          type="button"
          aria-current="page"
          :class="[$style.pageButton, $style.currentPage]"
        >
          1
        </button>
        <button
          type="button"
          disabled
          aria-label="下一页"
          :class="$style.pageButton"
        >
          ›
        </button>
      </div>
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.frame {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.scroll {
  @apply overflow-x-auto;
}
.table {
  @apply w-full min-w-[1400px] table-fixed border-collapse text-left text-[13px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-[46px] border-b bg-[#F4F5F7] px-3 text-[11px] font-semibold;
  color: #5e6c84;
  border-color: var(--ui-border);
}
.table th:nth-child(1) {
  @apply w-10 px-2;
}
.table th:nth-child(2) {
  @apply w-[200px] text-left;
}
.table th:nth-child(3) {
  @apply w-[60px] text-center;
}
.table th:nth-child(4) {
  @apply w-[100px] text-left;
}
.table th:nth-child(5) {
  @apply w-[140px] text-left;
}
.table th:nth-child(6) {
  @apply w-[100px] text-left;
}
.table th:nth-child(7) {
  @apply w-[110px] text-left;
}
.table th:nth-child(8),
.table th:nth-child(9) {
  @apply w-[120px] text-right;
}
.table th:nth-child(10) {
  @apply w-[80px] text-center;
}
.table th:nth-child(11) {
  @apply w-[120px] text-center;
}
.table td {
  @apply h-[47px] border-b px-3 align-middle;
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-subtle);
}
.table tbody tr:last-child td {
  @apply border-b-0;
}
.followCell,
.center {
  @apply text-center;
}
.iconButton,
.actions button {
  @apply inline-flex h-7 w-7 items-center justify-center rounded transition-colors;
  color: #c1c7d0;
}
.iconButton:hover,
.actions button:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.followed {
  color: #ffab00;
}
.followed svg {
  fill: #ffab00;
}
.nameButton {
  @apply text-left font-medium transition-colors;
  color: var(--app-primary);
}
.nameButton:hover {
  text-decoration: underline;
}
.phone {
  @apply inline-flex items-center gap-1.5;
  color: var(--ui-text);
}
.payment {
  @apply inline-flex rounded bg-[#EBECF0] px-2.5 py-1 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.money {
  @apply text-right font-medium;
  color: var(--ui-text);
}
.payable {
  color: #ff5630;
}
.badge {
  @apply inline-flex rounded px-2 py-0.5 text-[10px] font-semibold;
}
.levelA {
  color: #0052cc;
  background: #deebff;
}
.levelB {
  color: #00875a;
  background: #e3fcef;
}
.levelC {
  color: #ff8b00;
  background: #fffae6;
}
.levelD {
  color: #6b778c;
  background: #ebecf0;
}
.statusApproved {
  color: #00875a;
  background: #e3fcef;
}
.statusPending {
  color: #ff8b00;
  background: #fffae6;
}
.statusDraft {
  color: #6b778c;
  background: #ebecf0;
}
.statusRejected {
  color: #de350b;
  background: #ffebe6;
}
.actions {
  @apply flex items-center justify-center gap-0.5 opacity-0 transition-opacity;
}
.table tr:hover .actions,
.table tr:focus-within .actions {
  @apply opacity-100;
}
.empty {
  @apply px-4 py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex items-center justify-between border-t px-5 py-3 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.pagination {
  @apply flex items-center gap-1;
}
.pageButton {
  @apply flex h-7 w-7 items-center justify-center rounded text-[14px] transition-colors disabled:cursor-not-allowed disabled:opacity-40;
  color: var(--ui-text-subtle);
}
.pageButton:hover:not(:disabled) {
  background: var(--ui-surface-hover);
}
.currentPage {
  color: white;
  background: var(--app-primary);
}
</style>
