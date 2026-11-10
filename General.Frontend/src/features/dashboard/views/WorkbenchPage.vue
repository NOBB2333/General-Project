<script setup lang="ts">
import {
  Bell,
  CheckCircle2,
  ChevronRight,
  ClipboardList,
  Package,
  Plus,
  Rocket,
  ShoppingCart,
  Sparkles,
  Upload,
  Users,
  Wallet,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { toast, UiButton } from '@/components/ui';

const router = useRouter();
const mottoIndex = ref(0);
const mottos = ['登顶 · 把目标踩在脚下', '协作 · 让流程替你跑腿', '专注 · 把复杂留给系统'];
const approvals = ref([
  {
    id: 'a1',
    title: '采购申请 PR-2026-0035',
    type: '采购审批',
    applicant: '李工',
    amount: '¥42,800',
    status: '待审批',
    urgent: true,
  },
  {
    id: 'a2',
    title: '销售合同 SC-2026-0112',
    type: '合同审批',
    applicant: '王芳',
    amount: '¥186,000',
    status: '待审批',
    urgent: false,
  },
  {
    id: 'a3',
    title: '费用报销 EX-2026-0088',
    type: '费用审批',
    applicant: '赵主管',
    amount: '¥1,260',
    status: '催办',
    urgent: true,
  },
  {
    id: 'a4',
    title: '请假申请 LV-2026-0021',
    type: '人事审批',
    applicant: '陈会计',
    amount: '—',
    status: '待审批',
    urgent: false,
  },
]);
const notices = ref([
  { id: 'n1', title: '关于 Q3 库存盘点安排的通知', time: '今天 09:20', tag: '行政' },
  { id: 'n2', title: '新版销售合同模板已上线', time: '昨天 16:40', tag: '销售' },
  { id: 'n3', title: '供应商对账截止提醒', time: '07-11', tag: '财务' },
]);
const services = [
  { label: '请假', icon: ClipboardList },
  { label: '报销', icon: Wallet },
  { label: '用章', icon: CheckCircle2 },
  { label: '会议室', icon: Users },
  { label: '访客', icon: Bell },
  { label: '资产领用', icon: Package },
];
const shortcuts = [
  { label: '采购申请', path: '/purchase/requests', icon: ShoppingCart },
  { label: '销售合同', path: '/sales/contracts', icon: ClipboardList },
  { label: '客户管理', path: '/base/customers', icon: Users },
  { label: '库存总览', path: '/inventory/overview', icon: Package },
  { label: '我的待办', path: '/workflow/todo', icon: Bell },
  { label: '应收应付', path: '/finance/accounts', icon: Wallet },
  { label: '系统初始化', path: '/system-init/system-init', icon: Rocket },
];
const today = computed(() =>
  new Date().toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric', weekday: 'long' }),
);
function notify(message: string) {
  toast.success(message);
}

function approve(id: string) {
  approvals.value = approvals.value.filter((item) => item.id !== id);
  notify('已同意审批');
}

function reject(id: string) {
  approvals.value = approvals.value.filter((item) => item.id !== id);
  notify('已驳回');
}

function rotateMotto() {
  mottoIndex.value = (mottoIndex.value + 1) % mottos.length;
  notify('已切换企业文化标语');
}

function go(path: string) {
  void router.push(path);
}
</script>

<template>
  <div
    :class="$style.page"
    data-module-page
    data-page-label="工作台"
    data-page-module="工作台"
    data-page-layout="workbench"
  >
    <section :class="$style.welcome">
      <div>
        <h1 :class="$style.welcomeTitle">欢迎回来，张伟！</h1>
        <p :class="$style.welcomeSub">今天是 {{ today }}，祝您工作愉快。以下是今日业务全局概览。</p>
        <div :class="$style.motto">
          <Sparkles
            :size="14"
            :stroke-width="1.8"
          />
          <span>「{{ mottos[mottoIndex] }}」</span>
          <button
            type="button"
            @click="rotateMotto"
          >
            换一张
          </button>
          <button
            type="button"
            @click="notify('已打开上传')"
          >
            <Upload
              :size="13"
              :stroke-width="1.8"
            />
            上传
          </button>
          <button
            type="button"
            @click="notify('已打开管理')"
          >
            管理
          </button>
        </div>
      </div>
      <article :class="$style.initCard">
        <header>
          <Rocket
            :size="16"
            :stroke-width="1.8"
          />
          <strong>完成系统初始化</strong>
        </header>
        <p>当前阶段：公司主体与组织架构 · 必填 0 / 14 · 总进度 0 / 27</p>
        <UiButton
          size="sm"
          variant="primary"
          @click="go('/system-init/system-init')"
          >进入引导中心</UiButton
        >
      </article>
    </section>

    <section :class="$style.grid">
      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <h2>员工服务</h2>
        </header>
        <div :class="$style.serviceGrid">
          <button
            v-for="item in services"
            :key="item.label"
            type="button"
            :class="$style.serviceItem"
            @click="notify(`已打开「${item.label}」`)"
          >
            <component
              :is="item.icon"
              :size="18"
              :stroke-width="1.8"
            />
            <span>{{ item.label }}</span>
          </button>
        </div>
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <h2>快捷操作</h2>
        </header>
        <div :class="$style.shortcutList">
          <button
            v-for="item in shortcuts"
            :key="item.label"
            type="button"
            :class="$style.shortcutItem"
            @click="go(item.path)"
          >
            <component
              :is="item.icon"
              :size="16"
              :stroke-width="1.8"
            />
            <span>{{ item.label }}</span>
            <ChevronRight
              :size="14"
              :stroke-width="1.8"
            />
          </button>
        </div>
      </article>

      <article :class="[$style.panel, $style.panelWide]">
        <header :class="$style.panelHeader">
          <h2>待我审批</h2>
          <span>{{ approvals.length }} 项</span>
        </header>
        <div :class="$style.approvalList">
          <div
            v-for="item in approvals"
            :key="item.id"
            :class="$style.approvalItem"
          >
            <div>
              <strong>{{ item.title }}</strong>
              <p>{{ item.type }} · {{ item.applicant }} · {{ item.amount }}</p>
            </div>
            <div :class="$style.approvalActions">
              <span
                v-if="item.urgent"
                :class="$style.urgent"
                >催办</span
              >
              <UiButton
                size="xs"
                variant="secondary"
                @click="reject(item.id)"
                >驳回</UiButton
              >
              <UiButton
                size="xs"
                variant="primary"
                @click="approve(item.id)"
                >同意</UiButton
              >
            </div>
          </div>
          <p
            v-if="!approvals.length"
            :class="$style.empty"
          >
            暂无待审批事项
          </p>
        </div>
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <h2>企业通知</h2>
        </header>
        <ul :class="$style.noticeList">
          <li
            v-for="item in notices"
            :key="item.id"
          >
            <button
              type="button"
              @click="notify(`已打开通知：${item.title}`)"
            >
              <span>{{ item.title }}</span>
              <small>{{ item.tag }} · {{ item.time }}</small>
            </button>
          </li>
        </ul>
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <h2>企业文化合影</h2>
          <UiButton
            size="xs"
            variant="secondary"
            @click="notify('已打开合影管理')"
          >
            <template #leading>
              <Plus
                :size="12"
                :stroke-width="1.8"
              />
            </template>
            管理
          </UiButton>
        </header>
        <div :class="$style.photoGrid">
          <div
            v-for="n in 4"
            :key="n"
            :class="$style.photo"
          >
            团队活动 {{ n }}
          </div>
        </div>
      </article>
    </section>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply flex h-full min-h-0 flex-col gap-3 overflow-auto p-4;
  background: #fafbfc;
}
.welcome {
  @apply flex flex-wrap items-stretch justify-between gap-4 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.welcomeTitle {
  @apply m-0 text-[22px] font-semibold;
  color: var(--ui-text);
}
.welcomeSub {
  @apply m-0 mt-1 text-[13px];
  color: var(--ui-text-muted);
}
.motto {
  @apply mt-3 flex flex-wrap items-center gap-2 text-[13px];
  color: var(--ui-text);
}
.motto button {
  @apply rounded border bg-white px-2 py-1 text-[12px] transition-colors;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}
.motto button:hover {
  background: var(--ui-surface-hover);
}
.initCard {
  @apply w-full max-w-sm rounded-lg border p-3;
  border-color: #b3d4ff;
  background: #deebff;
}
.initCard header {
  @apply mb-2 flex items-center gap-2 text-[13px] font-semibold;
  color: var(--app-primary);
}
.initCard p {
  @apply m-0 mb-3 text-[12px];
  color: var(--ui-text-muted);
}
.grid {
  @apply grid gap-3;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}
.panel {
  @apply rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}
.panelWide {
  grid-column: span 2;
}
.panelHeader {
  @apply mb-3 flex items-center justify-between;
}
.panelHeader h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}
.panelHeader span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.serviceGrid {
  @apply grid grid-cols-3 gap-2;
}
.serviceItem {
  @apply flex flex-col items-center gap-2 rounded border px-2 py-3 text-[12px] transition-colors;
  border-color: var(--ui-border);
  color: var(--ui-text);
  background: #fff;
}
.serviceItem:hover {
  background: var(--ui-surface-hover);
}
.shortcutList {
  @apply flex flex-col gap-1;
}
.shortcutItem {
  @apply flex w-full items-center gap-2 rounded px-2 py-2 text-left text-[13px] transition-colors;
  color: var(--ui-text);
}
.shortcutItem:hover {
  background: var(--ui-surface-hover);
}
.shortcutItem span {
  @apply flex-1;
}
.approvalList {
  @apply flex flex-col gap-2;
}
.approvalItem {
  @apply flex flex-wrap items-center justify-between gap-3 rounded border px-3 py-2;
  border-color: var(--ui-border);
}
.approvalItem strong {
  @apply block text-[13px];
  color: var(--ui-text);
}
.approvalItem p {
  @apply m-0 text-[12px];
  color: var(--ui-text-subtle);
}
.approvalActions {
  @apply flex items-center gap-2;
}
.urgent {
  @apply rounded px-1.5 py-0.5 text-[11px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.noticeList {
  @apply m-0 flex list-none flex-col gap-1 p-0;
}
.noticeList button {
  @apply flex w-full flex-col items-start gap-0.5 rounded border-0 bg-transparent px-2 py-2 text-left transition-colors;
}
.noticeList button:hover {
  background: var(--ui-surface-hover);
}
.noticeList span {
  @apply text-[13px];
  color: var(--ui-text);
}
.noticeList small {
  color: var(--ui-text-subtle);
}
.photoGrid {
  @apply grid grid-cols-2 gap-2;
}
.photo {
  @apply flex h-20 items-center justify-center rounded text-[12px];
  background: #f4f5f7;
  color: var(--ui-text-subtle);
}
.empty {
  @apply m-0 py-6 text-center text-[13px];
  color: var(--ui-text-subtle);
}
@media (max-width: 1200px) {
  .grid,
  .panelWide {
    grid-column: auto;
    grid-template-columns: 1fr;
  }
}
</style>
