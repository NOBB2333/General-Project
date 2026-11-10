<script setup lang="ts">
import { AlertTriangle, BarChart3, CheckCircle2, FileText, ShieldAlert, Sparkles, Upload } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { CompanyPolicyRecord, CompanyPolicyView } from '../../model/company-policies.types';
defineProps<{ view: CompanyPolicyView; rows: readonly CompanyPolicyRecord[] }>();
const emit = defineEmits<{ upload: []; create: []; open: [row: CompanyPolicyRecord] }>();
</script>
<template>
  <section :class="$style.surface">
    <template v-if="view === 'AI 智能上传'"
      ><header>
        <Sparkles
          :size="22"
          :stroke-width="1.8"
        />
        <div>
          <h2>AI 智能制度解析</h2>
          <p>导入 Word 或 PDF，自动提取章节、重要条款和版本差异</p>
        </div>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('upload')"
          ><template #leading
            ><Upload
              :size="14"
              :stroke-width="1.8" /></template
          >上传文件</UiButton
        >
      </header>
      <div :class="$style.steps">
        <article><b>1</b><strong>上传制度文件</strong><span>支持 Word / PDF</span></article>
        <article><b>2</b><strong>AI 结构化解析</strong><span>章节、条款、标签</span></article>
        <article><b>3</b><strong>校对并发布</strong><span>生成签字任务</span></article>
      </div>
      <button
        type="button"
        :class="$style.blank"
        @click="emit('create')"
      >
        <FileText
          :size="20"
          :stroke-width="1.8"
        /><strong>从空白文档新建制度</strong>
      </button></template
    >
    <template v-else-if="view === '合规看板'"
      ><header>
        <BarChart3
          :size="22"
          :stroke-width="1.8"
        />
        <div>
          <h2>制度合规看板</h2>
          <p>阅读、电签、逾期与制度覆盖情况</p>
        </div>
      </header>
      <div :class="$style.metrics">
        <article><span>全员阅读率</span><strong>89%</strong><small>较上月 +6%</small></article>
        <article><span>电签覆盖率</span><strong>88%</strong><small>重要条款 13 项</small></article>
        <article><span>逾期未读</span><strong>4 部</strong><small>涉及 43 人次</small></article>
        <article><span>待修订制度</span><strong>2 部</strong><small>30 天内到期</small></article>
      </div>
      <div :class="$style.compliance">
        <article
          v-for="row in rows.filter((item) => item.status !== '草稿')"
          :key="row.id"
        >
          <CheckCircle2
            :size="15"
            :stroke-width="1.8"
          /><strong>{{ row.title }}</strong
          ><span>阅读 {{ row.coverage }}%</span><span>逾期 {{ row.overdue }}</span
          ><button
            type="button"
            @click="emit('open', row)"
          >
            查看
          </button>
        </article>
      </div></template
    >
    <template v-else
      ><header>
        <ShieldAlert
          :size="22"
          :stroke-width="1.8"
        />
        <div>
          <h2>违规通报</h2>
          <p>违规处理公示与整改跟踪</p>
        </div>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
          >发布通报</UiButton
        >
      </header>
      <div :class="$style.violations">
        <article
          v-for="(item, index) in [
            '项目现场未按规定佩戴防护用品',
            '客户资料通过个人邮箱发送',
            '差旅费用超标准未提前审批',
            '考勤异常连续三日未说明',
            '代码仓库密钥管理不规范',
          ]"
          :key="item"
        >
          <AlertTriangle
            :size="16"
            :stroke-width="1.8"
          />
          <div>
            <strong>{{ item }}</strong>
            <p>案例编号 VR-2026-00{{ index + 1 }} · 责任部门 {{ index % 2 ? '销售部' : '工程部' }}</p>
          </div>
          <span>{{ index === 4 ? '已完成' : '整改中' }}</span>
        </article>
      </div></template
    >
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.surface {
  @apply mt-4 overflow-hidden rounded border bg-white;
  min-height: 480px;
  border-color: var(--ui-border);
}
.surface > header {
  @apply flex h-[80px] items-center gap-4 border-b px-6;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.surface > header > svg {
  @apply box-content rounded p-3;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.surface header div {
  @apply flex-1;
}
.surface h2 {
  @apply m-0 text-[16px];
}
.surface header p {
  @apply m-0 mt-1 text-[10px];
  color: var(--ui-text-muted);
}
.steps {
  @apply grid grid-cols-3 gap-4 p-6;
}
.steps article {
  @apply flex h-[120px] flex-col items-center justify-center rounded border;
  border-color: var(--ui-border-subtle);
}
.steps b {
  @apply flex h-7 w-7 items-center justify-center rounded-full text-[11px] text-white;
  background: var(--app-primary);
}
.steps strong {
  @apply mt-3 text-[11px];
}
.steps span {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.blank {
  @apply mx-6 flex h-16 items-center justify-center gap-3 rounded border border-dashed bg-transparent text-[11px];
  width: calc(100% - 48px);
  border-color: var(--app-primary);
  color: var(--app-primary);
}
.metrics {
  @apply grid grid-cols-4 gap-4 p-6;
}
.metrics article {
  @apply flex h-24 flex-col justify-center rounded border p-4;
  border-color: var(--ui-border-subtle);
}
.metrics span,
.metrics small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply my-2 text-[20px];
  color: var(--app-primary);
}
.compliance,
.violations {
  @apply px-6 pb-6;
}
.compliance article,
.violations article {
  @apply flex h-14 items-center gap-3 border-b;
  border-color: var(--ui-border-subtle);
}
.compliance svg {
  color: var(--ui-success);
}
.compliance strong {
  @apply flex-1 text-[10px];
}
.compliance span {
  @apply w-28 text-[9px];
  color: var(--ui-text-muted);
}
.compliance button {
  @apply border-0 bg-transparent text-[9px];
  color: var(--app-primary);
}
.violations svg {
  color: var(--ui-warning);
}
.violations div {
  @apply flex-1;
}
.violations strong {
  @apply text-[10px];
}
.violations p {
  @apply m-0 mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.violations > article > span {
  @apply rounded px-2 py-1 text-[8px];
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
</style>
