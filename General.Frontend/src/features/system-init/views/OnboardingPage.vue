<script setup lang="ts">
import { Check, ChevronRight, Circle, Plus, Settings2 } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { toast, UiButton, UiSelect } from '@/components/ui';

type StepStatus = '未完成' | '已完成' | '进行中';
interface Step {
  id: string;
  name: string;
  required: boolean;
  role: string;
  status: StepStatus;
  estimate: string;
}
interface Stage {
  id: string;
  name: string;
  description: string;
  steps: Step[];
}

const stages = ref<Stage[]>([
  {
    id: 's1',
    name: '公司主体与组织架构',
    description: '维护企业主体、组织树与部门负责人',
    steps: [
      { id: 's1-1', name: '填写企业主体信息', required: true, role: '管理层', status: '未完成', estimate: '20 分钟' },
      { id: 's1-2', name: '配置组织架构', required: true, role: '管理层', status: '未完成', estimate: '30 分钟' },
      { id: 's1-3', name: '设置部门负责人', required: true, role: '管理层', status: '未完成', estimate: '15 分钟' },
      { id: 's1-4', name: '上传企业证照', required: false, role: '行政', status: '未完成', estimate: '10 分钟' },
    ],
  },
  {
    id: 's2',
    name: '用户与权限',
    description: '创建账号、角色与数据权限边界',
    steps: [
      { id: 's2-1', name: '邀请核心用户', required: true, role: '管理员', status: '未完成', estimate: '25 分钟' },
      { id: 's2-2', name: '配置角色权限', required: true, role: '管理员', status: '未完成', estimate: '40 分钟' },
      { id: 's2-3', name: '设置数据权限', required: true, role: '管理员', status: '未完成', estimate: '30 分钟' },
    ],
  },
  {
    id: 's3',
    name: '业务基础数据',
    description: '商品、客户、供应商、仓库等主数据',
    steps: [
      { id: 's3-1', name: '导入商品主数据', required: true, role: '采购', status: '未完成', estimate: '45 分钟' },
      { id: 's3-2', name: '导入客户档案', required: true, role: '销售', status: '未完成', estimate: '35 分钟' },
      { id: 's3-3', name: '配置仓库与库位', required: true, role: '仓储', status: '未完成', estimate: '30 分钟' },
      { id: 's3-4', name: '维护供应商名录', required: false, role: '采购', status: '未完成', estimate: '25 分钟' },
    ],
  },
  {
    id: 's4',
    name: '期初数据建账',
    description: '库存、应收应付与账户期初',
    steps: [
      { id: 's4-1', name: '录入期初库存', required: true, role: '仓储', status: '未完成', estimate: '50 分钟' },
      { id: 's4-2', name: '录入期初应收', required: true, role: '财务', status: '未完成', estimate: '40 分钟' },
      { id: 's4-3', name: '录入期初应付', required: true, role: '财务', status: '未完成', estimate: '40 分钟' },
    ],
  },
  {
    id: 's5',
    name: '流程与模板',
    description: '审批流、打印模板与通知策略',
    steps: [
      { id: 's5-1', name: '配置采购审批流', required: false, role: '管理员', status: '未完成', estimate: '30 分钟' },
      { id: 's5-2', name: '配置销售审批流', required: false, role: '管理员', status: '未完成', estimate: '30 分钟' },
      { id: 's5-3', name: '上传打印模板', required: false, role: '行政', status: '未完成', estimate: '20 分钟' },
    ],
  },
  {
    id: 's6',
    name: '上线验收',
    description: '试运行检查与正式启用确认',
    steps: [
      { id: 's6-1', name: '完成试运行清单', required: true, role: '管理层', status: '未完成', estimate: '60 分钟' },
      { id: 's6-2', name: '确认正式上线', required: true, role: '管理层', status: '未完成', estimate: '15 分钟' },
    ],
  },
]);

const activeStageId = ref('s1');
const filter = ref<'全部' | '必填' | '未完成' | '已完成'>('全部');
const roleView = ref('全部角色');
const roleOptions = [
  '全部角色',
  '以 管理层 角色查看',
  '以 管理员 角色查看',
  '以 财务 角色查看',
  '以 采购 角色查看',
  '以 销售 角色查看',
].map((value) => ({ value, label: value }));

const allSteps = computed(() => stages.value.flatMap((stage) => stage.steps));
const total = computed(() => allSteps.value.length);
const requiredTotal = computed(() => allSteps.value.filter((step) => step.required).length);
const done = computed(() => allSteps.value.filter((step) => step.status === '已完成').length);
const requiredDone = computed(() => allSteps.value.filter((step) => step.required && step.status === '已完成').length);
const activeStage = computed(() => stages.value.find((stage) => stage.id === activeStageId.value) ?? stages.value[0]!);
const visibleSteps = computed(() => {
  let list = activeStage.value.steps;
  if (filter.value === '必填') list = list.filter((step) => step.required);
  if (filter.value === '未完成') list = list.filter((step) => step.status !== '已完成');
  if (filter.value === '已完成') list = list.filter((step) => step.status === '已完成');
  if (roleView.value !== '全部角色')
    list = list.filter(
      (step) =>
        step.role === roleView.value.replace('以 ', '').replace(' 角色查看', '') || roleView.value === '全部角色',
    );
  if (roleView.value.startsWith('以 ')) {
    const role = roleView.value.replace('以 ', '').replace(' 角色查看', '');
    list = list.filter((step) => step.role === role);
  }
  return list;
});

function notify(message: string) {
  toast.success(message);
}

function completeStep(stepId: string) {
  stages.value = stages.value.map((stage) => ({
    ...stage,
    steps: stage.steps.map((step) => (step.id === stepId ? { ...step, status: '已完成' as const } : step)),
  }));
  notify('已标记完成');
}

function reopenStep(stepId: string) {
  stages.value = stages.value.map((stage) => ({
    ...stage,
    steps: stage.steps.map((step) => (step.id === stepId ? { ...step, status: '未完成' as const } : step)),
  }));
  notify('已重新打开步骤');
}

function stageProgress(stage: Stage) {
  const finished = stage.steps.filter((step) => step.status === '已完成').length;
  return `${finished}/${stage.steps.length}`;
}
</script>

<template>
  <div
    :class="$style.page"
    data-module-page
    data-page-label="系统初始化"
    data-page-module="系统初始化"
    data-page-layout="onboarding"
  >
    <header :class="$style.hero">
      <div>
        <div :class="$style.breadcrumb">工作台 / 系统初始化</div>
        <h1 :class="$style.title">系统初始化引导</h1>
        <p :class="$style.desc">
          首次启用系统建议按 6 个阶段、{{ total }} 个步骤依次完成。每完成一项可点“标记完成”，管理员可新增企业专属步骤。
        </p>
      </div>
      <div :class="$style.heroActions">
        <UiButton
          size="sm"
          variant="secondary"
          @click="notify('已打开引导配置')"
        >
          <template #leading>
            <Settings2
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          引导配置
        </UiButton>
        <UiButton
          size="sm"
          variant="primary"
          @click="notify('已新增企业步骤')"
        >
          <template #leading>
            <Plus
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          新增步骤
        </UiButton>
      </div>
    </header>

    <section :class="$style.stats">
      <article>
        <span>总进度</span><strong>{{ done }} / {{ total }}</strong>
      </article>
      <article>
        <span>必填进度</span><strong>{{ requiredDone }} / {{ requiredTotal }}</strong>
      </article>
      <article><span>预计剩余时间</span><strong>11 小时</strong></article>
      <article>
        <span>当前阶段</span><strong>阶段 {{ stages.findIndex((s) => s.id === activeStageId) + 1 }}</strong>
      </article>
    </section>

    <div :class="$style.body">
      <aside :class="$style.stageList">
        <button
          v-for="(stage, index) in stages"
          :key="stage.id"
          type="button"
          :class="[$style.stageItem, activeStageId === stage.id && $style.stageActive]"
          @click="activeStageId = stage.id"
        >
          <span :class="$style.stageIndex">阶段 {{ index + 1 }}</span>
          <strong>{{ stage.name }}</strong>
          <small>{{ stageProgress(stage) }}</small>
        </button>
      </aside>

      <section :class="$style.stageDetail">
        <header :class="$style.detailHeader">
          <div>
            <h2>{{ activeStage.name }}</h2>
            <p>{{ activeStage.description }}</p>
          </div>
          <div :class="$style.filters">
            <button
              v-for="item in ['全部', '必填', '未完成', '已完成']"
              :key="item"
              type="button"
              :class="[$style.filterChip, filter === item && $style.filterActive]"
              @click="filter = item as typeof filter"
            >
              {{ item }}
            </button>
            <UiSelect
              v-model="roleView"
              :options="roleOptions"
              label="按角色查看初始化步骤"
              size="sm"
            />
          </div>
        </header>

        <div :class="$style.stepList">
          <article
            v-for="step in visibleSteps"
            :key="step.id"
            :class="$style.stepCard"
          >
            <div :class="$style.stepIcon">
              <Check
                v-if="step.status === '已完成'"
                :size="16"
                :stroke-width="1.8"
              />
              <Circle
                v-else
                :size="16"
                :stroke-width="1.8"
              />
            </div>
            <div :class="$style.stepBody">
              <div :class="$style.stepTitle">
                <strong>{{ step.name }}</strong>
                <span
                  v-if="step.required"
                  :class="$style.required"
                  >必填</span
                >
                <span :class="$style.status">{{ step.status }}</span>
              </div>
              <p>{{ step.role }} · 预计 {{ step.estimate }}</p>
            </div>
            <div :class="$style.stepActions">
              <UiButton
                v-if="step.status !== '已完成'"
                size="sm"
                variant="primary"
                @click="completeStep(step.id)"
                >标记完成</UiButton
              >
              <UiButton
                v-else
                size="sm"
                variant="secondary"
                @click="reopenStep(step.id)"
                >重新打开</UiButton
              >
              <UiButton
                size="sm"
                variant="ghost"
                @click="notify(`已进入「${step.name}」`)"
              >
                进入
                <template #trailing>
                  <ChevronRight
                    :size="14"
                    :stroke-width="1.8"
                  />
                </template>
              </UiButton>
            </div>
          </article>
          <p
            v-if="!visibleSteps.length"
            :class="$style.empty"
          >
            当前筛选下没有步骤
          </p>
        </div>
      </section>
    </div>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply flex h-full min-h-0 flex-col gap-3 overflow-auto p-4;
  background: #fafbfc;
}
.hero {
  @apply flex flex-wrap items-start justify-between gap-3 rounded-lg border bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.breadcrumb {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.title {
  @apply m-0 mt-1 text-[20px] font-semibold;
  color: var(--ui-text);
}
.desc {
  @apply m-0 mt-1 max-w-3xl text-[13px] leading-5;
  color: var(--ui-text-muted);
}
.heroActions {
  @apply flex gap-2;
}
.stats {
  @apply grid gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}
.stats article {
  @apply rounded-lg border bg-white px-3 py-3;
  border-color: var(--ui-border);
}
.stats span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.stats strong {
  @apply mt-1 block text-[18px];
  color: var(--ui-text);
}
.body {
  @apply grid min-h-0 flex-1 gap-3;
  grid-template-columns: 280px minmax(0, 1fr);
}
.stageList {
  @apply flex flex-col gap-2 overflow-auto;
}
.stageItem {
  @apply rounded-lg border bg-white px-3 py-3 text-left transition-colors;
  border-color: var(--ui-border);
}
.stageItem:hover,
.stageActive {
  border-color: #b3d4ff;
  background: #deebff;
}
.stageIndex {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}
.stageItem strong {
  @apply mt-1 block text-[13px];
  color: var(--ui-text);
}
.stageItem small {
  @apply text-[12px];
  color: var(--app-primary);
}
.stageDetail {
  @apply min-h-0 overflow-auto rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}
.detailHeader {
  @apply mb-3 flex flex-wrap items-start justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border);
}
.detailHeader h2 {
  @apply m-0 text-[16px];
  color: var(--ui-text);
}
.detailHeader p {
  @apply m-0 mt-1 text-[12px];
  color: var(--ui-text-muted);
}
.filters {
  @apply flex flex-wrap items-center gap-2;
}
.filterChip {
  @apply rounded border px-2 py-1 text-[12px];
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
  background: #fff;
}
.filterActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
  border-color: #b3d4ff;
}
.filters select {
  @apply h-8 rounded border px-2 text-[12px];
  border-color: var(--ui-border);
  color: var(--ui-text);
}
.stepList {
  @apply flex flex-col gap-2;
}
.stepCard {
  @apply flex flex-wrap items-center gap-3 rounded border px-3 py-3;
  border-color: var(--ui-border);
}
.stepIcon {
  @apply flex h-8 w-8 items-center justify-center rounded-full;
  background: #f4f5f7;
  color: var(--ui-text-muted);
}
.stepBody {
  @apply min-w-0 flex-1;
}
.stepTitle {
  @apply flex flex-wrap items-center gap-2;
}
.stepTitle strong {
  @apply text-[13px];
  color: var(--ui-text);
}
.stepBody p {
  @apply m-0 mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.required {
  @apply rounded px-1.5 py-0.5 text-[11px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.status {
  @apply rounded px-1.5 py-0.5 text-[11px];
  color: var(--ui-text-muted);
  background: #f4f5f7;
}
.stepActions {
  @apply flex items-center gap-2;
}
.empty {
  @apply m-0 py-10 text-center text-[13px];
  color: var(--ui-text-subtle);
}
@media (max-width: 1100px) {
  .body,
  .stats {
    grid-template-columns: 1fr;
  }
}
</style>
