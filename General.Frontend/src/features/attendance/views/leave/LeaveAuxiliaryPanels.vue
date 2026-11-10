<script setup lang="ts">
import { ref } from 'vue';
import { Check, Clock3, X } from 'lucide-vue-next';
import { UiButton, UiSwitch, toast } from '@/components/ui';
import { compTimeAdjustments, leaveApprovals } from '../../model/leave.data';

defineProps<{ tab: '审批管理' | '调休补假' | '假期配置' }>();

const settings = ref([
  { id: 'auto-calc', label: '自动计算工作日', description: '自动排除周末和法定节假日', enabled: true },
  { id: 'balance-check', label: '校验假期余额', description: '提交申请时检查剩余额度', enabled: true },
  { id: 'attachment', label: '病假证明', description: '病假超过 2 天时要求上传证明', enabled: false },
]);

function resolveApproval(name: string, approved: boolean) {
  toast.success(`${name}的申请已${approved ? '批准' : '驳回'}`);
}
</script>

<template>
  <section
    v-if="tab === '审批管理'"
    :class="$style.panel"
  >
    <header>
      <div>
        <h2>待我审批</h2>
        <p>当前共有 {{ leaveApprovals.length }} 条请假申请等待处理</p>
      </div>
    </header>
    <div :class="$style.approvalGrid">
      <article
        v-for="item in leaveApprovals"
        :key="item.id"
      >
        <div :class="$style.avatar">{{ item.applicant.slice(0, 1) }}</div>
        <div :class="$style.approvalBody">
          <strong>{{ item.applicant }} · {{ item.type }}</strong>
          <p>{{ item.department }} · {{ item.period }} · {{ item.days }} 天</p>
          <small>{{ item.reason }}</small>
        </div>
        <div :class="$style.approvalActions">
          <UiButton
            size="xs"
            variant="ghost"
            @click="resolveApproval(item.applicant, false)"
            ><X
              :size="13"
              :stroke-width="1.8"
            />驳回</UiButton
          >
          <UiButton
            size="xs"
            variant="primary"
            @click="resolveApproval(item.applicant, true)"
            ><Check
              :size="13"
              :stroke-width="1.8"
            />批准</UiButton
          >
        </div>
      </article>
    </div>
  </section>

  <section
    v-else-if="tab === '调休补假'"
    :class="$style.panel"
  >
    <header>
      <div>
        <h2>待确认调休额度</h2>
        <p>核对加班记录后计入可用调休余额</p>
      </div>
    </header>
    <div :class="$style.adjustments">
      <article
        v-for="item in compTimeAdjustments"
        :key="item.id"
      >
        <span :class="$style.clock"
          ><Clock3
            :size="17"
            :stroke-width="1.8"
        /></span>
        <div>
          <strong>{{ item.source }}</strong>
          <p>有效期至 {{ item.expiresAt }}</p>
        </div>
        <strong>+{{ item.earned }} 天</strong>
        <UiButton
          size="xs"
          variant="primary"
          @click="toast.success('调休额度已确认入账')"
          >确认入账</UiButton
        >
      </article>
    </div>
  </section>

  <section
    v-else
    :class="$style.panel"
  >
    <header>
      <div>
        <h2>假期配置</h2>
        <p>配置当前企业的请假与额度校验规则</p>
      </div>
    </header>
    <div :class="$style.settings">
      <label
        v-for="setting in settings"
        :key="setting.id"
      >
        <span
          ><strong>{{ setting.label }}</strong
          ><small>{{ setting.description }}</small></span
        >
        <UiSwitch
          v-model="setting.enabled"
          :label="setting.label"
        />
      </label>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply border-b px-5 py-4;
  border-color: var(--ui-border-subtle);
}
.panel h2 {
  @apply text-[15px] font-semibold;
}
.panel header p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.approvalGrid {
  @apply grid grid-cols-3 gap-3 p-4;
}
.approvalGrid article {
  @apply flex min-w-0 items-start gap-3 rounded-lg border p-4;
  border-color: var(--ui-border);
}
.avatar {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-[12px] font-semibold text-white;
  background: var(--app-primary);
}
.approvalBody {
  @apply min-w-0 flex-1;
}
.approvalBody strong {
  @apply text-[12px];
}
.approvalBody p,
.approvalBody small {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-subtle);
}
.approvalActions {
  @apply flex flex-col gap-1;
}
.adjustments {
  @apply divide-y;
}
.adjustments article {
  @apply flex items-center gap-3 px-5 py-4;
}
.clock {
  @apply flex h-8 w-8 items-center justify-center rounded bg-[#DEEBFF] text-[#0052CC];
}
.adjustments article > div {
  @apply flex-1;
}
.adjustments p {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.settings {
  @apply divide-y;
}
.settings label {
  @apply flex items-center justify-between px-5 py-4;
}
.settings span {
  @apply flex flex-col;
}
.settings strong {
  @apply text-[12px];
}
.settings small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
</style>
