<script setup lang="ts">
import { Bug, CircleHelp, ExternalLink, Lightbulb, MessageCircle, Send, Sparkles, X } from 'lucide-vue-next';
import { onMounted, onUnmounted, ref, watch } from 'vue';
import { UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ close: [] }>();

type FeedbackType = 'problem' | 'feature' | 'consult' | 'other';
const activeTab = ref('submit');
const type = ref<FeedbackType>('problem');
const summary = ref('');
const detail = ref('');
const attachPage = ref(true);
const error = ref('');
const submitted = ref(false);
const feedbackTypes = [
  { key: 'problem' as const, label: '问题反馈', description: '页面报错、数据异常、功能用不了', icon: Bug },
  { key: 'feature' as const, label: '功能建议', description: '希望我们新增或改进的功能', icon: Lightbulb },
  { key: 'consult' as const, label: '使用咨询', description: '操作流程、字段含义、配置方法', icon: CircleHelp },
  { key: 'other' as const, label: '其他反馈', description: '体验感受、合作建议或其他', icon: MessageCircle },
];
const history = ref([
  { title: '批量导出希望支持自定义字段', status: '处理中', time: '2026-07-10' },
  { title: '建议商品图片支持拖拽排序', status: '已采纳', time: '2026-07-02' },
  { title: '条码打印预览偏移', status: '已解决', time: '2026-06-26' },
]);

function submit() {
  if (!summary.value.trim() || !detail.value.trim()) {
    error.value = '请填写一句话描述和详细描述';
    return;
  }
  history.value.unshift({ title: summary.value.trim(), status: '已提交', time: new Date().toISOString().slice(0, 10) });
  submitted.value = true;
  error.value = '';
}

function reset() {
  summary.value = '';
  detail.value = '';
  type.value = 'problem';
  attachPage.value = true;
  submitted.value = false;
}

function onKeydown(event: KeyboardEvent) {
  if (props.open && event.key === 'Escape') emit('close');
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      activeTab.value = 'submit';
      reset();
    }
  },
);
onMounted(() => document.addEventListener('keydown', onKeydown));
onUnmounted(() => document.removeEventListener('keydown', onKeydown));
</script>

<template>
  <Teleport to="body">
    <UiTabs v-model="activeTab">
      <aside
        v-if="open"
        role="dialog"
        aria-label="问题反馈"
        :class="$style.dialog"
      >
        <header :class="$style.header">
          <div
            aria-hidden="true"
            :class="[$style.decoration, $style.heroBackground]"
          />
          <div
            aria-hidden="true"
            :class="$style.decorationFloating"
          />
          <div
            aria-hidden="true"
            :class="$style.decorationFloatingAlternative"
          />
          <div :class="$style.div">
            <div :class="$style.row">
              <div>
                <div :class="$style.rowGap2">
                  <Sparkles
                    :size="16"
                    :class="$style.icon"
                  />
                  <h2 :class="$style.title">我们在听 · 问题反馈</h2>
                </div>
                <p :class="$style.description">遇到问题或想给我们建议？一句话告诉我们，1 个工作日内回复。</p>
              </div>
              <button
                title="关闭反馈"
                @click="emit('close')"
                :class="$style.closeButton"
              >
                <X :size="16" />
              </button>
            </div>
            <div :class="$style.rowMt25Gap15">
              <span :class="$style.label"><i :class="$style.decorationFloatingRound" /><i :class="$style.i" /></span
              ><span :class="$style.labelCompact">李婷 · 王建国 · 苏雯 在线 · 平均 23 分钟响应</span>
            </div>
            <UiTabsList
              label="反馈视图"
              :class="$style.navigation"
            >
              <UiTabsTrigger
                value="submit"
                variant="inverted"
                :class="$style.button"
                >提交反馈</UiTabsTrigger
              >
              <UiTabsTrigger
                value="mine"
                variant="inverted"
                :class="$style.button"
              >
                我的反馈<span :class="$style.badge">{{ history.length }}</span
                ><i
                  v-if="activeTab !== 'mine'"
                  :class="$style.decorationFloatingRoundH2W2"
                />
              </UiTabsTrigger>
            </UiTabsList>
          </div>
        </header>

        <div :class="$style.rowScrollable">
          <UiTabPanel
            value="submit"
            :class="$style.tabPanel"
          >
            <div
              v-if="!submitted"
              :class="$style.list"
            >
              <div>
                <strong :class="$style.strongLabel">反馈类型</strong>
                <div :class="$style.grid">
                  <button
                    v-for="item in feedbackTypes"
                    :key="item.key"
                    :title="item.description"
                    :class="[$style.itemButton, type === item.key ? $style.stateDanger : $style.stateDefault]"
                    @click="type = item.key"
                  >
                    <span
                      :class="[$style.labelCentered, type === item.key ? $style.stateDangerDanger : $style.stateMuted]"
                      ><component
                        :is="item.icon"
                        :size="18" /></span
                    ><span
                      :class="[
                        $style.labelCompactAlternative,
                        type === item.key ? $style.stateDangerDangerStrong : $style.stateDefaultAlternative,
                      ]"
                      >{{ item.label }}</span
                    >
                  </button>
                </div>
              </div>
              <label :class="$style.field"
                ><span :class="$style.labelStrong"
                  ><span>一句话描述 <b :class="$style.b">*</b></span
                  ><small :class="$style.caption">{{ summary.length }}/60</small></span
                ><input
                  v-model="summary"
                  maxlength="60"
                  placeholder="例如：批量导出按钮点击没反应"
                  :class="$style.exportInput"
              /></label>
              <label :class="$style.field"
                ><span :class="$style.labelStrong"
                  ><span>详细描述 <b :class="$style.b">*</b></span
                  ><small :class="$style.caption">{{ detail.length }}/500</small></span
                ><textarea
                  v-model="detail"
                  maxlength="500"
                  rows="3"
                  placeholder="请描述：操作步骤 → 期望结果 → 实际结果。"
                  :class="$style.textarea"
                />
              </label>
              <div :class="$style.rowMuted">
                <div :class="$style.rowGap2Gap2">
                  <ExternalLink
                    :size="14"
                    :class="$style.iconMuted"
                  /><span :class="$style.labelAlternative"
                    >附带当前页面信息<small :class="$style.captionCompact">/base/products</small></span
                  >
                </div>
                <button
                  role="switch"
                  :aria-checked="attachPage"
                  :class="[$style.iconButton, attachPage ? $style.stateSuccess : $style.stateMutedMuted]"
                  @click="attachPage = !attachPage"
                >
                  <span
                    :class="[
                      $style.labelFloating,
                      attachPage ? $style.stateTransformed : $style.stateTransformedAlternative,
                    ]"
                  />
                </button>
              </div>
              <p
                v-if="error"
                :class="$style.descriptionDanger"
              >
                {{ error }}
              </p>
              <button
                @click="submit"
                :class="$style.primaryButton"
              >
                <Send :size="14" />立即提交反馈
              </button>
            </div>
            <div
              v-else
              :class="$style.rowCentered"
            >
              <span :class="$style.labelSuccess"><Send :size="28" /></span>
              <h3 :class="$style.titleStrong">反馈已提交</h3>
              <p :class="$style.descriptionMuted">感谢你的反馈，我们会在 1 个工作日内回复。</p>
              <button
                @click="activeTab = 'mine'"
                :class="$style.buttonMt5Px4Py2"
              >
                查看我的反馈
              </button>
            </div>
          </UiTabPanel>

          <UiTabPanel
            value="mine"
            :class="$style.tabPanel"
          >
            <div :class="$style.listPx4Py3">
              <article
                v-for="item in history"
                :key="`${item.title}-${item.time}`"
                :class="$style.itemCard"
              >
                <div :class="$style.rowAlternative">
                  <strong :class="$style.strongLabelWide">{{ item.title }}</strong
                  ><span
                    :class="[
                      $style.badgeCompact,
                      item.status === '已解决' || item.status === '已采纳'
                        ? $style.stateSuccessSuccess
                        : $style.stateSelected,
                    ]"
                    >{{ item.status }}</span
                  >
                </div>
                <small :class="$style.captionCompactMuted">{{ item.time }} · 商品管理</small>
              </article>
            </div>
          </UiTabPanel>
        </div>
        <footer :class="$style.footer">
          感谢您让产品变得更好 💙
          <button
            @click="activeTab = 'mine'"
            :class="$style.buttonSuccess"
          >
            查看全部
            <ExternalLink
              :size="11"
              :class="$style.iconAlternative"
            />
          </button>
        </footer>
      </aside>
    </UiTabs>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";
.heroBackground {
  background-image: url('https://plus.unsplash.com/premium_photo-1664474667296-4176696eb08b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=900');
}

/* Semantic template classes */
.dialog {
  @apply fixed bottom-[96px] right-[24px] z-[130] flex h-[640px] w-[420px] origin-bottom-right flex-col overflow-hidden rounded-2xl border border-[#DFE1E6] bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply relative h-[143.25px] shrink-0 overflow-hidden text-white;
}
.decoration {
  @apply absolute inset-0 bg-cover bg-[center_35%];
}
.decorationFloating {
  @apply absolute inset-0 bg-[linear-gradient(135deg,rgba(6,182,212,.62)_0%,rgba(20,184,166,.5)_50%,rgba(16,185,129,.62)_100%)];
}
.decorationFloatingAlternative {
  @apply absolute inset-y-0 left-0 right-1/2 bg-[linear-gradient(90deg,rgba(8,47,73,.55)_0%,rgba(8,47,73,0)_100%)];
}
.div {
  @apply relative px-5 py-4;
}
.row {
  @apply flex items-start justify-between gap-3;
}
.rowGap2 {
  @apply flex items-center gap-2;
}
.icon {
  @apply text-[#FFE380];
}
.title {
  @apply text-[15px] font-semibold;
}
.description {
  @apply mt-1 text-[12px] text-white/90;
}
.closeButton {
  @apply shrink-0 rounded-lg p-1.5 text-white/80 transition-colors hover:bg-white/20 hover:text-white;
}
.rowMt25Gap15 {
  @apply mt-2.5 flex items-center gap-1.5;
}
.label {
  @apply relative flex h-1.5 w-1.5;
}
.decorationFloatingRound {
  @apply absolute inset-0 rounded-full bg-[#36B37E] opacity-75;
}
.i {
  @apply relative h-1.5 w-1.5 rounded-full bg-[#36B37E];
}
.labelCompact {
  @apply text-[11px] font-medium text-white/95;
}
.navigation {
  @apply mt-3 flex gap-1.5 rounded-lg bg-black/15 p-1;
}
.button {
  @apply relative flex-1 rounded-md py-1.5 text-[12px] font-semibold;
}
.badge {
  @apply ml-1 rounded bg-white/20 px-1.5 py-px text-[10px] font-bold text-white;
}
.decorationFloatingRoundH2W2 {
  @apply absolute right-1 top-0.5 h-2 w-2 rounded-full bg-[#FACC15] ring-2 ring-[#0E9488];
}
.rowScrollable {
  @apply min-h-0 flex-1 overflow-y-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.tabPanel {
  @apply min-h-full;
}
.list {
  @apply space-y-3 px-4 py-3;
}
.strongLabel {
  @apply mb-1.5 block text-[12px] font-semibold text-[#42526E];
}
.grid {
  @apply grid grid-cols-4 gap-1.5;
}
.itemButton {
  @apply flex flex-col items-center justify-center gap-1 rounded-lg border py-2 text-center transition-colors;
}
.stateDanger {
  @apply border-[#FF5630]/40 bg-[#FFEBE6]/50 shadow-sm;
}
.stateDefault {
  @apply border-[#DFE1E6] hover:border-[#B3D4FF] hover:bg-[#FAFBFC];
}
.labelCentered {
  @apply flex h-6 w-6 items-center justify-center rounded-md;
}
.stateDangerDanger {
  @apply bg-[#FFEBE6] text-[#FF5630];
}
.stateMuted {
  @apply bg-[#F4F5F7] text-[#6B778C];
}
.labelCompactAlternative {
  @apply text-[11px];
}
.stateDangerDangerStrong {
  @apply font-semibold text-[#FF5630];
}
.stateDefaultAlternative {
  @apply font-medium text-[#172B4D];
}
.field {
  @apply block;
}
.labelStrong {
  @apply mb-1 flex items-center justify-between text-[12px] font-semibold text-[#42526E];
}
.b {
  @apply text-[#FF5630];
}
.caption {
  @apply text-[10px] font-normal text-[#97A0AF];
}
.exportInput {
  @apply w-full rounded-lg border border-[#DFE1E6] px-3 py-2 text-[13px] text-[#172B4D] outline-none transition-colors placeholder:text-[#A5ADBA] focus:border-[#14B8A6] focus:ring-2 focus:ring-[#14B8A6]/20;
}
.textarea {
  @apply w-full resize-none rounded-lg border border-[#DFE1E6] px-3 py-2 text-[13px] leading-[1.55] text-[#172B4D] outline-none transition-colors placeholder:text-[#A5ADBA] focus:border-[#14B8A6] focus:ring-2 focus:ring-[#14B8A6]/20;
}
.rowMuted {
  @apply flex items-center justify-between rounded-lg border border-[#EBECF0] bg-[#FAFBFC] p-2.5;
}
.rowGap2Gap2 {
  @apply flex min-w-0 items-center gap-2;
}
.iconMuted {
  @apply shrink-0 text-[#6B778C];
}
.labelAlternative {
  @apply min-w-0 text-[12px] font-medium text-[#172B4D];
}
.captionCompact {
  @apply block truncate text-[10px] font-normal text-[#97A0AF];
}
.iconButton {
  @apply relative h-5 w-9 shrink-0 rounded-full transition-colors;
}
.stateSuccess {
  @apply bg-[#14B8A6];
}
.stateMutedMuted {
  @apply bg-[#C1C7D0];
}
.labelFloating {
  @apply absolute top-0.5 h-4 w-4 rounded-full bg-white shadow transition-transform;
}
.stateTransformed {
  @apply translate-x-[18px];
}
.stateTransformedAlternative {
  @apply translate-x-0.5;
}
.descriptionDanger {
  @apply rounded bg-[#FFEBE6] px-3 py-2 text-[11px] text-[#DE350B];
}
.primaryButton {
  @apply flex w-full items-center justify-center gap-2 rounded-lg bg-[linear-gradient(135deg,#06B6D4_0%,#14B8A6_55%,#10B981_100%)] py-2.5 text-[13px] font-semibold text-white shadow-[0_4px_14px_rgba(20,184,166,.32)];
}
.rowCentered {
  @apply flex flex-col items-center px-4 py-12 text-center;
}
.labelSuccess {
  @apply mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#E3FCEF] text-[#00875A];
}
.titleStrong {
  @apply text-[17px] font-semibold;
}
.descriptionMuted {
  @apply mt-2 text-[12px] leading-5 text-[#6B778C];
}
.buttonMt5Px4Py2 {
  @apply mt-5 rounded border border-[#DFE1E6] px-4 py-2 text-[12px] text-[#42526E];
}
.listPx4Py3 {
  @apply space-y-2 px-4 py-3;
}
.itemCard {
  @apply rounded-lg border border-[#DFE1E6] p-3;
}
.rowAlternative {
  @apply flex items-start;
}
.strongLabelWide {
  @apply min-w-0 flex-1 text-[12px] leading-5;
}
.badgeCompact {
  @apply ml-2 shrink-0 rounded px-2 py-0.5 text-[9px] font-semibold;
}
.stateSuccessSuccess {
  @apply bg-[#E3FCEF] text-[#00875A];
}
.stateSelected {
  @apply bg-app-primary-soft text-app-primary;
}
.captionCompactMuted {
  @apply mt-1 block text-[10px] text-[#97A0AF];
}
.footer {
  @apply flex h-[36.5px] shrink-0 items-center border-t border-[#DFE1E6] bg-[#FAFBFC] px-4 py-2.5 text-[12px] text-[#97A0AF];
}
.buttonSuccess {
  @apply ml-auto w-[62.5px] font-semibold text-[#00875A];
}
.iconAlternative {
  @apply inline;
}
</style>
