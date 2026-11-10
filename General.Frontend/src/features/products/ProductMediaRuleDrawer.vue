<script setup lang="ts">
import { Check, GripVertical, RotateCcw, SlidersHorizontal } from 'lucide-vue-next';
import { reactive, ref, watch } from 'vue';
import { UiButton, UiDrawer, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { MediaRule } from './product.media.types';

const props = defineProps<{ open: boolean; rule: MediaRule }>();
const emit = defineEmits<{ close: []; save: [rule: MediaRule] }>();

const draft = reactive<MediaRule>({ ...props.rule });
const error = ref('');
const presets: Array<MediaRule & { description: string }> = [
  {
    name: '编码 + 序号',
    separator: '_',
    primary: 'either',
    classifier: 'sequence',
    description: '最简：编码或型号 + 数字序号（1=主图，≥2=副图）',
  },
  {
    name: '型号 + 类型',
    separator: '-',
    primary: 'model',
    classifier: 'role',
    description: '产品型号 + 主图/副图/附件关键词',
  },
  {
    name: '编码或型号 + 类型',
    separator: '_',
    primary: 'either',
    classifier: 'role',
    description: '同时兼容编码和型号，适合多来源文件',
  },
];

const separatorOptions = [
  { value: '_', label: '下划线 _' },
  { value: '-', label: '短横线 -' },
  { value: ' ', label: '空格' },
] satisfies readonly UiSelectOption[];

const primaryOptions = [
  { value: 'code', label: '物料编码' },
  { value: 'model', label: '产品型号' },
  { value: 'either', label: '编码或型号' },
] satisfies readonly UiSelectOption[];

const classifierOptions = [
  { value: 'sequence', label: '数字序号' },
  { value: 'role', label: '类型关键词' },
] satisfies readonly UiSelectOption[];

function applyPreset(preset: MediaRule) {
  Object.assign(draft, preset);
  error.value = '';
}

function resetDefault() {
  applyPreset(presets[0]!);
}

function save() {
  if (!draft.name.trim()) {
    error.value = '规则名称不能为空';
    return;
  }
  emit('save', { ...draft });
}

function primaryLabel(value: MediaRule['primary']) {
  return value === 'code' ? '物料编码' : value === 'model' ? '产品型号' : '编码或型号';
}

function classifierLabel(value: MediaRule['classifier']) {
  return value === 'sequence' ? '序号' : '类型关键词';
}

function updateOpen(open: boolean) {
  if (!open) emit('close');
}

function updateSeparator(value: string) {
  draft.separator = value as MediaRule['separator'];
}

function updatePrimary(value: string) {
  draft.primary = value as MediaRule['primary'];
}

function updateClassifier(value: string) {
  draft.classifier = value as MediaRule['classifier'];
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      Object.assign(draft, props.rule);
      error.value = '';
    }
  },
);
</script>

<template>
  <UiDrawer
    :open="open"
    title="编辑命名规则"
    size="lg"
    @update:open="updateOpen"
  >
    <template #icon>
      <span :class="$style.drawerIcon">
        <SlidersHorizontal
          :size="16"
          :stroke-width="1.8"
        />
      </span>
    </template>

    <template #description> 系统只维护 <b>一条</b> 命名规则。不符合规则的文件可在预览页手动改派。 </template>

    <template #actions>
      <UiButton
        variant="secondary"
        size="sm"
        @click="resetDefault"
      >
        <template #leading>
          <RotateCcw
            :size="12"
            :stroke-width="1.8"
          />
        </template>
        重置默认
      </UiButton>
    </template>

    <div :class="$style.list">
      <section :class="$style.section">
        <h3 :class="$style.titleCompact">从预设开始（点击套用作为起点，仍可继续编辑）</h3>
        <div :class="$style.grid">
          <button
            v-for="preset in presets"
            :key="preset.name"
            type="button"
            :class="[
              $style.presetButton,
              draft.primary === preset.primary &&
              draft.classifier === preset.classifier &&
              draft.separator === preset.separator
                ? $style.stateSelected
                : $style.stateSelectedPrimary,
            ]"
            @click="applyPreset(preset)"
          >
            <span :class="$style.label">
              <Check
                v-if="
                  draft.primary === preset.primary &&
                  draft.classifier === preset.classifier &&
                  draft.separator === preset.separator
                "
                :size="12"
                :stroke-width="1.8"
                :class="$style.iconPrimary"
              />
              {{ preset.name }}
            </span>
            <span :class="$style.labelCompact">
              <b :class="$style.b">{{ primaryLabel(preset.primary) }}</b>
              <i :class="$style.i">{{ preset.separator }}</i>
              <b :class="$style.bSuccess">{{ classifierLabel(preset.classifier) }}</b>
            </span>
            <span :class="$style.labelCompactMuted">{{ preset.description }}</span>
          </button>
        </div>
      </section>

      <section :class="$style.sectionPrimary">
        <header :class="$style.headerPrimary">
          <SlidersHorizontal
            :size="12"
            :stroke-width="1.8"
          />
          <strong :class="$style.strongLabel">规则配置</strong>
        </header>

        <div :class="$style.formFields">
          <label :class="$style.field">
            <span :class="$style.fieldLabel">规则名称</span>
            <UiInput
              v-model="draft.name"
              :invalid="Boolean(error)"
              placeholder="请输入规则名称"
            />
            <small
              v-if="error"
              :class="$style.caption"
              >{{ error }}</small
            >
          </label>

          <div :class="$style.field">
            <span :class="$style.fieldLabel">分隔符</span>
            <UiSelect
              :model-value="draft.separator"
              :options="separatorOptions"
              label="分隔符"
              @update:model-value="updateSeparator"
            />
          </div>

          <div>
            <span :class="$style.fieldLabel">按顺序解析，必须含 1 个主字段</span>
            <div :class="$style.ruleSequence">
              <span :class="$style.dragIcon">
                <GripVertical
                  :size="15"
                  :stroke-width="1.8"
                />
              </span>
              <UiSelect
                :model-value="draft.primary"
                :options="primaryOptions"
                label="主字段"
                :class="$style.ruleSelect"
                size="sm"
                @update:model-value="updatePrimary"
              />
              <span :class="$style.separatorPreview">{{ draft.separator }}</span>
              <UiSelect
                :model-value="draft.classifier"
                :options="classifierOptions"
                label="分类字段"
                :class="$style.ruleSelect"
                size="sm"
                @update:model-value="updateClassifier"
              />
            </div>
          </div>
        </div>
      </section>

      <section :class="$style.sectionMuted">
        <div :class="$style.previewHeader">
          <strong :class="$style.strongLabel">解析预览</strong>
          <span :class="$style.previewHint">（示例值：编码=SP-2025-00001，型号=MT8102iP，类型=副图）</span>
        </div>
        <code :class="$style.code">
          {{ draft.primary === 'model' ? 'MT8102iP' : 'SP-2025-00001' }}{{ draft.separator
          }}{{ draft.classifier === 'sequence' ? '2' : '副图' }}.jpg
        </code>
      </section>
    </div>

    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('close')"
      >
        取消
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="save"
      >
        保存规则
      </UiButton>
    </template>
  </UiDrawer>
</template>

<style module>
@reference "../../styles/index.css";

.drawerIcon {
  @apply mt-0.5 inline-flex h-7 w-7 shrink-0 items-center justify-center;
  color: var(--app-primary);
  background: var(--app-primary-soft);
  border-radius: var(--app-control-radius);
}

.list {
  @apply space-y-4 p-5;
}

.section {
  @apply border p-3;
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.titleCompact {
  @apply mb-2 text-[11px] font-semibold;
  color: var(--ui-text-muted);
}

.grid {
  @apply grid grid-cols-3 gap-2;
}

.presetButton {
  @apply border p-2.5 text-left transition-colors;
  color: var(--ui-text);
  border-radius: var(--app-control-radius);
}

.presetButton:focus-visible {
  outline: 2px solid var(--ui-focus);
  outline-offset: 1px;
}

.stateSelected {
  border-color: var(--app-primary);
  background: var(--app-primary-soft);
}

.stateSelectedPrimary {
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.stateSelectedPrimary:hover {
  border-color: var(--ui-focus);
}

.label {
  @apply flex items-center text-[12px] font-semibold;
}

.iconPrimary {
  @apply mr-1;
  color: var(--app-primary);
}

.labelCompact {
  @apply mt-1.5 flex items-center gap-1 text-[10px];
}

.b {
  @apply px-1.5 py-0.5;
  color: var(--app-primary);
  background: var(--app-primary-soft);
  border-radius: var(--app-compact-control-radius);
}

.i {
  @apply not-italic;
  color: var(--ui-text-subtle);
}

.bSuccess {
  @apply px-1.5 py-0.5;
  color: var(--ui-success);
  background: var(--ui-success-soft);
  border-radius: var(--app-compact-control-radius);
}

.labelCompactMuted {
  @apply mt-1.5 block text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.sectionPrimary {
  @apply overflow-hidden border;
  background: var(--ui-surface);
  border-color: var(--app-primary);
  border-radius: var(--app-surface-radius);
}

.headerPrimary {
  @apply flex items-center gap-2 border-b px-4 py-2.5;
  color: var(--app-primary);
  background: color-mix(in srgb, var(--app-primary-soft) 40%, transparent);
  border-color: var(--ui-border);
}

.strongLabel {
  @apply text-[12px];
  color: var(--ui-text);
}

.formFields {
  @apply space-y-4 p-4;
}

.field {
  @apply block;
}

.fieldLabel {
  @apply mb-1.5 block text-[11px] font-medium;
  color: var(--ui-text-muted);
}

.caption {
  @apply mt-1 block text-[11px];
  color: var(--ui-danger);
}

.ruleSequence {
  @apply flex items-center gap-2 border p-3;
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.dragIcon {
  @apply shrink-0;
  color: var(--ui-text-subtle);
}

.ruleSelect {
  @apply min-w-0 flex-1;
}

.separatorPreview {
  @apply shrink-0 text-[14px];
  color: var(--ui-text-subtle);
}

.sectionMuted {
  @apply border p-4;
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.previewHeader {
  @apply flex items-center gap-3;
}

.previewHint {
  @apply ml-auto text-[10px];
  color: var(--ui-text-subtle);
}

.code {
  @apply mt-3 block p-3 text-[13px] font-semibold;
  color: var(--app-primary);
  background: var(--ui-surface);
  border-radius: var(--app-control-radius);
}
</style>
