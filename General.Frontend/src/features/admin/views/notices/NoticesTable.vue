<script setup lang="ts">
import { Eye, Paperclip, RotateCcw } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { CompanyNotice } from '../../model/notices.types';

defineProps<{ rows: readonly CompanyNotice[] }>();
const emit = defineEmits<{
  view: [notice: CompanyNotice];
  withdraw: [notice: CompanyNotice];
}>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>编号</th>
          <th>主题</th>
          <th>类别 / 优先级</th>
          <th>范围</th>
          <th>发布部门 / 发布人</th>
          <th>发布时间</th>
          <th>阅读情况</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="notice in rows"
          :key="notice.id"
        >
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('view', notice)"
            >
              {{ notice.code }}
            </button>
          </td>
          <td>
            <button
              type="button"
              :class="$style.subject"
              @click="emit('view', notice)"
            >
              <span>
                <em v-if="notice.pinned">置顶</em>
                <strong>{{ notice.title }}</strong>
                <small v-if="notice.attachments"
                  ><Paperclip
                    :size="11"
                    :stroke-width="1.8"
                  />{{ notice.attachments }}</small
                >
              </span>
              <p>{{ notice.summary }}</p>
            </button>
          </td>
          <td>
            <span
              :class="$style.category"
              :data-category="notice.category"
              >{{ notice.category }}</span
            >
            <span
              :class="$style.priority"
              :data-priority="notice.priority"
              >{{ notice.priority }}</span
            >
          </td>
          <td>
            <span
              :class="$style.scope"
              :data-scope="notice.scope"
              >{{ notice.scope }}</span
            >
            <small v-if="notice.scope !== '全公司'">{{ notice.scopeDetail }}</small>
          </td>
          <td>
            <strong>{{ notice.department }}</strong>
            <span :class="$style.publisher"
              ><i>{{ notice.publisherInitial }}</i
              >{{ notice.publisher }}</span
            >
          </td>
          <td>{{ notice.publishedAt }}</td>
          <td>
            <div :class="$style.reading">
              <span
                ><Eye
                  :size="12"
                  :stroke-width="1.8"
                /><strong>{{ notice.readCount }}</strong> / {{ notice.totalReaders }}</span
              >
              <progress
                :value="notice.readCount"
                :max="notice.totalReaders"
              />
            </div>
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="notice.status"
              >{{ notice.status }}</span
            >
          </td>
          <td>
            <UiButton
              v-if="notice.status === '已发布'"
              variant="ghost"
              size="sm"
              @click="emit('withdraw', notice)"
            >
              <template #leading
                ><RotateCcw
                  :size="13"
                  :stroke-width="1.8"
              /></template>
              撤回
            </UiButton>
            <span
              v-else
              :class="$style.muted"
              >—</span
            >
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有公司通知
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto rounded border bg-white;
  border-color: var(--ui-border);
}

.scroller table {
  @apply w-full min-w-[1370px] border-collapse text-left text-[10px];
}

.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: #f4f5f7;
}

.scroller th:nth-child(1) {
  width: 150px;
}

.scroller th:nth-child(2) {
  width: 400px;
}

.scroller th:nth-child(3) {
  width: 115px;
}

.scroller th:nth-child(4) {
  width: 110px;
}

.scroller th:nth-child(5) {
  width: 150px;
}

.scroller th:nth-child(6) {
  width: 145px;
}

.scroller th:nth-child(7) {
  width: 115px;
}

.scroller th:nth-child(8) {
  width: 85px;
}

.scroller td {
  @apply h-[64px] border-b px-3 align-middle;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.code {
  @apply font-semibold;
  color: var(--app-primary);
}

.subject {
  @apply block w-full min-w-0 text-left;
}

.subject > span {
  @apply flex min-w-0 items-center gap-1.5;
}

.subject em {
  @apply shrink-0 rounded px-1 py-0.5 text-[9px] not-italic;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.subject strong {
  @apply truncate text-[11px] font-semibold;
  color: var(--ui-text);
}

.subject small {
  @apply flex shrink-0 items-center gap-0.5;
  color: var(--ui-text-subtle);
}

.subject p {
  @apply mt-1 truncate;
  color: var(--ui-text-subtle);
}

.category,
.priority,
.scope,
.status {
  @apply block w-fit rounded px-1.5 py-0.5 text-[9px] font-semibold;
}

.category {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.category[data-category='人事变动'] {
  color: #6554c0;
  background: #eae6ff;
}

.category[data-category='活动通知'] {
  color: #00875a;
  background: #e3fcef;
}

.category[data-category='紧急通知'] {
  color: #de350b;
  background: #ffebe6;
}

.category[data-category='节假日安排'] {
  color: #ff8b00;
  background: #fff0b3;
}

.priority {
  @apply mt-1;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}

.priority[data-priority='重要'],
.priority[data-priority='紧急'] {
  color: #ff8b00;
  background: #fff7e6;
}

.priority[data-priority='紧急'] {
  color: #de350b;
  background: #ffebe6;
}

.scope {
  color: #0052cc;
  background: #deebff;
}

.scope[data-scope='指定部门'],
.scope[data-scope='指定人员'] {
  color: #00875a;
  background: #e3fcef;
}

.scroller td:nth-child(4) small {
  @apply mt-1 block;
  color: var(--ui-text-subtle);
}

.scroller td:nth-child(5) > strong {
  @apply block text-[10px] font-semibold;
  color: var(--ui-text);
}

.publisher {
  @apply mt-1 flex items-center gap-1;
}

.publisher i {
  @apply flex h-4 w-4 items-center justify-center rounded-full text-[8px] not-italic text-white;
  background: var(--app-primary);
}

.reading span {
  @apply flex items-center gap-1;
}

.reading strong {
  color: var(--ui-text);
}

.reading progress {
  @apply mt-1 block h-1 w-[70px] overflow-hidden rounded;
  color: var(--ui-success);
  background: #ebecf0;
}

.reading progress::-webkit-progress-bar {
  background: #ebecf0;
}

.reading progress::-webkit-progress-value {
  background: var(--ui-success);
}

.status[data-status='已发布'] {
  color: #00875a;
  background: #e3fcef;
}

.status[data-status='已过期'] {
  color: #6554c0;
  background: #eae6ff;
}

.status[data-status='已撤回'] {
  color: #de350b;
  background: #ffebe6;
}

.status[data-status='草稿'] {
  color: #ff8b00;
  background: #fff0b3;
}

.muted {
  color: var(--ui-text-subtle);
}

.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
